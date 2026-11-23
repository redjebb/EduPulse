
using System;
using System.IO;
using C__проект_shcolo.Models;
using C__проект_shcolo.Services;

namespace C__проект_shcolo
{
    class Program
    {
        static void Main(string[] args)
        {
            // Създаваме JSON услугата
            JsonDataService jsonService = new JsonDataService();

            // Зареждаме базата данни
            SchoolDatabase database;

            try
            {
                database = jsonService.Load();
            }
            catch (InvalidDataException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Грешка при зареждане: " + ex.Message);
                return;
            }

            // Създаваме услугите
            GradeService gradeService = new GradeService(database);

            GradeSimulator simulator = new GradeSimulator(database);

            InsightsService insights =
                new InsightsService(database, gradeService);

            // Начален екран
            Console.WriteLine("================================");
            Console.WriteLine("          EDUPULSE");
            Console.WriteLine("================================");
            Console.WriteLine("Програмата стартира успешно!");
            Console.WriteLine();

            Console.WriteLine("Ученици: " + database.Students.Count);
            Console.WriteLine("Учители: " + database.Teachers.Count);
            Console.WriteLine("Предмети: " + database.Subjects.Count);

            Console.WriteLine();

            // Показваме учениците
            Console.WriteLine("Списък с ученици:");

            foreach (Student student in database.Students)
            {
                Console.WriteLine(
                    student.Id + ". " +
                    student.Name + " - " +
                    student.ClassName
                );
            }

            Console.WriteLine();

            // Тест за среден успех
            Console.WriteLine("Среден успех на първия ученик:");

            if (database.Students.Count > 0)
            {
                Student firstStudent = database.Students[0];

                double average =
                    gradeService.GetAverage(firstStudent.Id);

                Console.WriteLine(
                    firstStudent.Name + ": " + average.ToString("F2")
                );

                // Тест на Grade Simulator
                double simulatedAverage =
                    simulator.SimulateNextGrade(
                        firstStudent.Id, 1, 6
                    );

                Console.WriteLine(
                    "Среден успех след пробна шестица по математика: "
                    + simulatedAverage.ToString("F2")
                );

                // Тест на Smart Insights
                Console.WriteLine();
                Console.WriteLine("SMART INSIGHTS:");

                foreach (string insight in insights.GetSummary(firstStudent.Id))
                {
                    Console.WriteLine(insight);
                }
            }

            Console.WriteLine();
            Console.WriteLine("================================");
            Console.WriteLine("Тест за добавяне на оценка");
            Console.WriteLine("================================");

            Console.Write("Въведи ID на ученик: ");
            int studentId;

            if (!int.TryParse(Console.ReadLine(), out studentId))
            {
                Console.WriteLine("Невалидно ID!");
                return;
            }

            Console.Write("Въведи ID на предмет: ");
            int subjectId;

            if (!int.TryParse(Console.ReadLine(), out subjectId))
            {
                Console.WriteLine("Невалидно ID!");
                return;
            }

            Console.Write("Въведи оценка (2-6): ");
            int value;

            if (!int.TryParse(Console.ReadLine(), out value))
            {
                Console.WriteLine("Невалидна оценка!");
                return;
            }

            // Добавяме оценката
            bool result = gradeService.AddGrade(
                studentId, subjectId, value
            );

            if (result)
            {
                try
                {
                    // Записваме промените в JSON
                    jsonService.Save(database);

                    Console.WriteLine();
                    Console.WriteLine("Оценката е добавена успешно!");
                    Console.WriteLine("Данните са записани в school.json.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        "Грешка при записване: " + ex.Message
                    );
                }
            }
            else
            {
                Console.WriteLine(
                    "Оценката не беше добавена. Провери въведените данни."
                );
            }

            Console.WriteLine();
            Console.WriteLine("Натисни Enter за изход.");
            Console.ReadLine();
        }
    }
}
