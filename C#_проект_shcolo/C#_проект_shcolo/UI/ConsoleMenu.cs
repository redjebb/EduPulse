using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spectre.Console;
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
                MenuItem("1", "Ученически портал");
                MenuItem("2", "Преподавателски портал");
                MenuItem("0", "Изход");
                Footer("EDUPULSE  •  ЕЛЕКТРОНЕН ДНЕВНИК");
                int choice = SelectOption("Избор: ", new[] { "Изход", "Ученически портал", "Преподавателски портал" });
                if (choice == 0) return;
                if (choice == 1) ChooseStudent();
                else ChooseTeacher();
            }
        }

        private void ChooseStudent()
        {
            Header("УЧЕНИЧЕСКИ ПОРТАЛ", "Избор на ученически профил");
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
            int choice = SelectOption("Избор: ", new[] { "Назад" }.Concat(database.Students.Select(st => $"{st.Name} · {st.ClassName}")).ToArray());
            if (choice > 0) StudentDashboard(database.Students[choice - 1]);
        }

        private void ChooseTeacher()
        {
            Header("ПРЕПОДАВАТЕЛСКИ ПОРТАЛ", "Избор на преподавател");
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
            int choice = SelectOption("Избор: ", new[] { "Назад" }.Concat(database.Teachers.Select(t => t.Name)).ToArray());
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
                Section("НАВИГАЦИЯ");
                MenuItem("1", "Моите оценки");
                MenuItem("2", "Успех по предмети");
                MenuItem("3", "Grade Simulator");
                MenuItem("4", "Smart Insights");
                MenuItem("0", "Назад");
                int choice = SelectOption("Избор: ", new[] { "Назад", "Моите оценки", "Успех по предмети", "Grade Simulator", "Smart Insights" });
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
                Section("УПРАВЛЕНИЕ");
                MenuItem("1", "Списък с ученици");
                MenuItem("2", "Добави оценка");
                MenuItem("3", "Преглед на оценки");
                MenuItem("0", "Назад");
                int choice = SelectOption("Избор: ", new[] { "Назад", "Списък с ученици", "Добави оценка", "Преглед на оценки" });
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
            var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);
            table.AddColumn("[aqua]ID[/]");
            table.AddColumn("[aqua]Ученик[/]");
            table.AddColumn("[aqua]Клас[/]");
            table.AddColumn("[aqua]Оценки[/]");
            foreach (Student student in database.Students)
            {
                table.AddRow(student.Id.ToString(), Markup.Escape(student.Name),
                    Markup.Escape(student.ClassName), student.Grades.Count.ToString());
            }
            if (database.Students.Count == 0)
                Notice("Няма ученици.", ConsoleColor.Yellow);
            else
                AnsiConsole.Write(table);
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
            int choice = SelectOption("Избор: ", new[] { "Назад" }.Concat(database.Students.Select(st => $"{st.Name} · {st.ClassName}")).ToArray());
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
            int choice = SelectOption("Предмет: ", new[] { "Отказ" }.Concat(options.Select(x => x.Name)).ToArray());
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
            int previousNextId = database.NextGradeId;
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
                // Roll back the uncommitted grade to keep memory in sync with the file.
                student.Grades.RemoveAll(g => g.Id == previousNextId);
                database.NextGradeId = previousNextId;
                Notice("Оценката не е записана. Промяната беше отменена.", ConsoleColor.Red);
                Notice("Причина: " + ex.Message, ConsoleColor.Yellow);
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
            var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);
            table.AddColumn("[aqua]Предмет[/]");
            table.AddColumn("[aqua]Оценка[/]");
            table.AddColumn("[aqua]Дата[/]");
            foreach (Grade grade in student.Grades.OrderByDescending(g => g.Date))
            {
                Subject? subject = grades.FindSubject(grade.SubjectId);
                string color = grade.Value >= 6 ? "springgreen2" :
                    grade.Value >= 5 ? "aqua" : grade.Value >= 4 ? "yellow" : "red";
                table.AddRow(Markup.Escape(subject?.Name ?? "Неизвестен"),
                    $"[bold {color}]{grade.Value}[/]", grade.Date.ToString("dd.MM.yyyy"));
            }
            AnsiConsole.Write(table);
            Metric("ОБЩ УСПЕХ", grades.GetAverage(student.Id).ToString("F2"));
        }

        private void ShowSubjectAverages(Student student)
        {
            Header("УСПЕХ ПО ПРЕДМЕТИ", student.Name);
            var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);
            table.AddColumn("[aqua]Предмет[/]");
            table.AddColumn("[aqua]Среден успех[/]");
            table.AddColumn("[aqua]Прогрес[/]");
            foreach (Subject subject in database.Subjects)
            {
                int count = student.Grades.Count(g => g.SubjectId == subject.Id);
                if (count == 0)
                {
                    table.AddRow(Markup.Escape(subject.Name), "[grey]Няма оценки[/]", "[grey]—[/]");
                    continue;
                }
                double score = grades.GetSubjectAverage(student.Id, subject.Id);
                string color = score >= 5.5 ? "springgreen2" : score >= 4.5 ? "aqua" : "yellow";
                int filled = Math.Clamp((int)Math.Round((score - 2) / 4.0 * 12), 0, 12);
                table.AddRow(Markup.Escape(subject.Name), $"[bold {color}]{score:F2}[/]",
                    $"[{color}]{new string('━', filled)}[/][grey]{new string('─', 12 - filled)}[/]");
            }
            AnsiConsole.Write(table);
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
            double change = existingCount == 0 ? 0 : nextAverage - grades.GetSubjectAverage(student.Id, subject.Id);
            if (existingCount > 0)
            {
                Console.ForegroundColor = change >= 0 ? ConsoleColor.Green : ConsoleColor.Yellow;
                Console.WriteLine($"  ПРОМЯНА: {change:+0.00;-0.00;0.00}");
                Console.ResetColor();
            }
            Notice("Това е симулация — оценката НЕ е записана.", ConsoleColor.Green);
        }

        private void ShowInsights(Student student)
        {
            Header("SMART INSIGHTS", student.Name);
            var items = insights.GetSummary(student.Id)
                .Select(line => "• " + Markup.Escape(line));
            AnsiConsole.Write(new Panel(new Markup(string.Join("\n", items)))
                .Header("[aqua]Анализ на успеха[/]")
                .Border(BoxBorder.Rounded).BorderColor(Color.Teal));
        }

        private static void Header(string title, string subtitle)
        {
            if (!Console.IsOutputRedirected)
            {
                Console.Clear();
            }

            var panel = new Panel(new Markup(
                "[bold aqua]EDUPULSE[/]   [grey]·[/]   [silver]Електронен дневник[/]"))
                .Header("[aqua] SCHOOL PORTAL [/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Teal);
            AnsiConsole.Write(panel);
            AnsiConsole.MarkupLine($"[bold white]{Markup.Escape(title)}[/]");
            AnsiConsole.MarkupLine($"[grey]{Markup.Escape(subtitle)}[/]");
            AnsiConsole.WriteLine();
        }

        private static int SelectOption(string prompt, string[] labels)
        {
            if (labels.Length == 0) return 0;

            // CI, scripted input and redirected terminals keep numeric choices.
            if (Console.IsInputRedirected || Console.IsOutputRedirected ||
                !AnsiConsole.Profile.Capabilities.Interactive)
            {
                return InputValidator.ReadInt(prompt, 0, labels.Length - 1);
            }

            string selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[aqua]Избери опция[/]")
                    .PageSize(Math.Min(10, labels.Length))
                    .HighlightStyle(new Style(Color.Aqua))
                    .AddChoices(labels));
            return Array.IndexOf(labels, selected);
        }

        private static void Section(string title)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  " + title);
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void Footer(string message)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  " + new string('─', 54));
            Console.WriteLine("  " + message);
            Console.ResetColor();
        }

        private static void MenuItem(string index, string title)
        {
            if (Console.IsInputRedirected || Console.IsOutputRedirected ||
                !AnsiConsole.Profile.Capabilities.Interactive)
            {
                AnsiConsole.MarkupLine($"  [aqua][[{Markup.Escape(index)}]][/]  {Markup.Escape(title)}");
            }
        }

        private static void Metric(string label, string value)
        {
            AnsiConsole.MarkupLine($"  [grey]{Markup.Escape(label)}:[/] [bold springgreen2]{Markup.Escape(value)}[/]");
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
