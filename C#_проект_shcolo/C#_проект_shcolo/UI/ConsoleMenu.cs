using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C__проект_shcolo.Models;
using C__проект_shcolo.Services;

namespace C__проект_shcolo.UI
{
    public class ConsoleMenu
    {
        private readonly SchoolDatabase database;
        private readonly JsonDataService jsonService;
        private readonly GradeService grades;
        private readonly GradeSimulator simulator;
        private readonly InsightsService insights;

        public ConsoleMenu(SchoolDatabase database, JsonDataService jsonService)
        {
            this.database = database;
            this.jsonService = jsonService;
            this.grades = new GradeService(database);
            this.simulator = new GradeSimulator(database);
            this.insights = new InsightsService(database, this.grades);
        }

        public void Run()
        {
            Console.OutputEncoding = Encoding.UTF8;
            while (true)
            {
                Header("ГЛАВНО МЕНЮ", "Електронният дневник, който показва напредъка");
                MenuItem("1", "Вход като ученик");
                MenuItem("2", "Вход като учител");
                MenuItem("0", "Изход");
                int choice = InputValidator.ReadInt("Избор: ", 0, 2);
                if (choice == 0) return;
                if (choice == 1) ChooseStudent();
                else ChooseTeacher();
            }
        }

        private void ChooseStudent()
        {
            Header("ИЗБОР НА УЧЕНИК", "Демо профили");
            if (database.Students.Count == 0)
            {
                Notice("Няма въведени ученици.", ConsoleColor.Yellow);
                InputValidator.Pause();
                return;
            }
            for (int i = 0; i < database.Students.Count; i++)
            {
                Student student = database.Students[i];
                MenuItem((i + 1).ToString(), $"{student.Name} · {student.ClassName}");
            }
            MenuItem("0", "Назад");
            int choice = InputValidator.ReadInt("Избор: ", 0, database.Students.Count);
            if (choice > 0) StudentDashboard(database.Students[choice - 1]);
        }

        private void ChooseTeacher()
        {
            Header("ИЗБОР НА УЧИТЕЛ", "Демо профили");
            if (database.Teachers.Count == 0)
            {
                Notice("Няма въведени учители.", ConsoleColor.Yellow);
                InputValidator.Pause();
                return;
            }
            for (int i = 0; i < database.Teachers.Count; i++)
            {
                Teacher teacher = database.Teachers[i];
                MenuItem((i + 1).ToString(), teacher.Name);
            }
            MenuItem("0", "Назад");
            int choice = InputValidator.ReadInt("Избор: ", 0, database.Teachers.Count);
            if (choice > 0) TeacherDashboard(database.Teachers[choice - 1]);
        }

        private void StudentDashboard(Student student)
        {
            while (true)
            {
                Header("STUDENT DASHBOARD", $"{student.Name} · {student.ClassName}");
                Metric("ОБЩ УСПЕХ", student.Grades.Count == 0 ? "Няма оценки" : grades.GetAverage(student.Id).ToString("F2"));
                Metric("ОЦЕНКИ", student.Grades.Count.ToString());
                Console.WriteLine();
                MenuItem("1", "Моите оценки");
                MenuItem("2", "Успех по предмети");
                MenuItem("3", "Grade Simulator");
                MenuItem("4", "Smart Insights");
                MenuItem("0", "Назад");
                int choice = InputValidator.ReadInt("Избор: ", 0, 4);
                if (choice == 0) return;
                switch (choice)
                {
                    case 1: ShowGrades(student); break;
                    case 2: ShowSubjectAverages(student); break;
                    case 3: ShowSimulator(student); break;
                    case 4: ShowInsights(student); break;
                }
                InputValidator.Pause();
            }
        }

        private void TeacherDashboard(Teacher teacher)
        {
            while (true)
            {
                Header("TEACHER DASHBOARD", teacher.Name);
                Metric("УЧЕНИЦИ", database.Students.Count.ToString());
                Metric("ПРЕДМЕТИ", teacher.SubjectIds.Count.ToString());
                Console.WriteLine();
                MenuItem("1", "Списък с ученици");
                MenuItem("2", "Добави оценка");
                MenuItem("3", "Преглед на оценки");
                MenuItem("0", "Назад");
                int choice = InputValidator.ReadInt("Избор: ", 0, 3);
                if (choice == 0) return;
                switch (choice)
                {
                    case 1: ShowStudents(); break;
                    case 2: AddGrade(teacher); break;
                    case 3:
                        Student? student = SelectStudent();
                        if (student != null) ShowGrades(student);
                        break;
                }
                InputValidator.Pause();
            }
        }

        private void ShowStudents()
        {
            Header("СПИСЪК С УЧЕНИЦИ", "Училищна база данни");
            foreach (Student student in database.Students)
            {
                Console.WriteLine($"  {student.Id,-4} {student.Name,-26} {student.ClassName,-8} Оценки: {student.Grades.Count}");
            }
            if (database.Students.Count == 0) Notice("Няма ученици.", ConsoleColor.Yellow);
        }

        private Student? SelectStudent()
        {
            Header("ИЗБЕРИ УЧЕНИК", "0 за отказ");
            for (int i = 0; i < database.Students.Count; i++)
            {
                MenuItem((i + 1).ToString(), database.Students[i].Name);
            }
            if (database.Students.Count == 0)
            {
                Notice("Няма ученици.", ConsoleColor.Yellow);
                return null;
            }
            int choice = InputValidator.ReadInt("Избор: ", 0, database.Students.Count);
            return choice == 0 ? null : database.Students[choice - 1];
        }

        private Subject? SelectSubject(IEnumerable<Subject> subjects)
        {
            List<Subject> options = subjects.ToList();
            if (options.Count == 0)
            {
                Notice("Няма налични предмети.", ConsoleColor.Yellow);
                return null;
            }
            Console.WriteLine();
            for (int i = 0; i < options.Count; i++)
            {
                MenuItem((i + 1).ToString(), options[i].Name);
            }
            MenuItem("0", "Отказ");
            int choice = InputValidator.ReadInt("Предмет: ", 0, options.Count);
            return choice == 0 ? null : options[choice - 1];
        }

        private void AddGrade(Teacher teacher)
        {
            Student? student = SelectStudent();
            if (student == null) return;
            Header("НОВА ОЦЕНКА", student.Name);
            List<Subject> allowedSubjects = database.Subjects
                .Where(s => teacher.SubjectIds.Contains(s.Id)).ToList();
            Subject? subject = SelectSubject(allowedSubjects);
            if (subject == null) return;
            int value = InputValidator.ReadInt("Оценка (2–6): ", 2, 6);
            if (!grades.AddGrade(student.Id, subject.Id, value))
            {
                Notice("Оценката не може да бъде добавена.", ConsoleColor.Red);
                return;
            }
            try
            {
                jsonService.Save(database);
                Notice($"Записана оценка {value} по {subject.Name} за {student.Name}.", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                Notice("Грешка при запис: " + ex.Message, ConsoleColor.Red);
                Notice("Промяната е само в паметта. Излезте и проверете файла, преди да въвеждате още оценки.", ConsoleColor.Yellow);
            }
        }

        private void ShowGrades(Student student)
        {
            Header("МОИТЕ ОЦЕНКИ", student.Name);
            if (student.Grades.Count == 0)
            {
                Notice("Няма въведени оценки.", ConsoleColor.Yellow);
                return;
            }
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  {"ПРЕДМЕТ",-24} {"ОЦЕНКА",-8} ДАТА");
            Console.ResetColor();
            Console.WriteLine("  " + new string('─', 49));
            foreach (Grade grade in student.Grades.OrderByDescending(g => g.Date))
            {
                Subject? subject = grades.FindSubject(grade.SubjectId);
                Console.Write($"  {(subject?.Name ?? "Неизвестен"),-24} ");
                Console.ForegroundColor = GradeColor(grade.Value);
                Console.Write($"{grade.Value,-8}");
                Console.ResetColor();
                Console.WriteLine($" {grade.Date:dd.MM.yyyy}");
            }
            Console.WriteLine();
            Metric("ОБЩ УСПЕХ", grades.GetAverage(student.Id).ToString("F2"));
        }

        private void ShowSubjectAverages(Student student)
        {
            Header("УСПЕХ ПО ПРЕДМЕТИ", student.Name);
            foreach (Subject subject in database.Subjects)
            {
                int count = student.Grades.Count(g => g.SubjectId == subject.Id);
                string avg = count == 0 ? "Няма оценки" : grades.GetSubjectAverage(student.Id, subject.Id).ToString("F2");
                Console.Write($"  {subject.Name,-26} ");
                if (count > 0)
                {
                    double score = grades.GetSubjectAverage(student.Id, subject.Id);
                    Console.ForegroundColor = score >= 5.5 ? ConsoleColor.Green : score >= 4.5 ? ConsoleColor.Cyan : ConsoleColor.Yellow;
                    Console.Write($"{avg,-7} ");
                    Console.ResetColor();
                    int filled = (int)Math.Round((score - 2) / 4.0 * 16);
                    filled = Math.Clamp(filled, 0, 16);
                    Console.WriteLine(new string('█', filled) + new string('░', 16 - filled));
                }
                else
                {
                    Console.WriteLine(avg);
                }
            }
        }

        private void ShowSimulator(Student student)
        {
            Header("GRADE SIMULATOR", "Провери резултата преди следващата оценка");
            Subject? subject = SelectSubject(database.Subjects);
            if (subject == null) return;
            int proposed = InputValidator.ReadInt("Пробна оценка (2–6): ", 2, 6);
            int existingCount = student.Grades.Count(g => g.SubjectId == subject.Id);
            double nextAverage = simulator.SimulateNextGrade(student.Id, subject.Id, proposed);
            string current = existingCount == 0 ? "Няма оценки" : grades.GetSubjectAverage(student.Id, subject.Id).ToString("F2");
            Console.WriteLine();
            Metric("СЕГА", current);
            Metric("СЛЕД ПРОБНАТА ОЦЕНКА", nextAverage.ToString("F2"));
            Notice("Това е симулация — оценката НЕ е записана.", ConsoleColor.Green);
        }

        private void ShowInsights(Student student)
        {
            Header("SMART INSIGHTS", student.Name);
            foreach (string summary in insights.GetSummary(student.Id))
            {
                Console.WriteLine("  • " + summary);
            }
        }

        private static void Header(string title, string subtitle)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        EDUPULSE                            ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  " + title);
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  " + subtitle);
            Console.ResetColor();
            Console.WriteLine("  " + new string('─', 54));
            Console.WriteLine();
        }

        private static void MenuItem(string index, string title)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  [" + index + "] ");
            Console.ResetColor();
            Console.WriteLine(title);
        }

        private static void Metric(string label, string value)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  " + label + ": ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        private static ConsoleColor GradeColor(int value)
        {
            return value >= 6 ? ConsoleColor.Green
                : value >= 5 ? ConsoleColor.Cyan
                : value >= 4 ? ConsoleColor.Yellow
                : ConsoleColor.Red;
        }

        private static void Notice(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine();
            Console.WriteLine("  " + text);
            Console.ResetColor();
        }
    }
}
