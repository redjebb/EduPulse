using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using C__проект_shcolo.Models;

namespace C__проект_shcolo.Services
{
    public class JsonDataService
    {
        private string filePath;
        private bool loadedSuccessfully;
        private bool loadFailed;

        private JsonSerializerOptions options;

        public JsonDataService(string filePath = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                filePath = GetDefaultFilePath();
            }

            this.filePath = Path.GetFullPath(filePath);

            this.options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            this.loadedSuccessfully = false;
            this.loadFailed = false;
        }

        private static string GetDefaultFilePath()
        {
            // The app may be started from the solution, project folder,
            // repository root, or its compiled bin/Debug directory.
            string[] startingPoints =
            {
                Directory.GetCurrentDirectory(),
                AppContext.BaseDirectory
            };

            foreach (string startingPoint in startingPoints)
            {
                DirectoryInfo? directory = new DirectoryInfo(startingPoint);
                while (directory != null)
                {
                    string directData = Path.Combine(
                        directory.FullName, "Data", "school.json");
                    if (File.Exists(directData))
                    {
                        return directData;
                    }

                    string repositoryData = Path.Combine(
                        directory.FullName, "C#_проект_shcolo", "Data", "school.json");
                    if (File.Exists(repositoryData))
                    {
                        return repositoryData;
                    }

                    directory = directory.Parent;
                }
            }

            // First run: create a Data folder beside the project file,
            // not in a transient working directory.
            DirectoryInfo? projectDirectory = new DirectoryInfo(AppContext.BaseDirectory);
            while (projectDirectory != null)
            {
                if (Directory.GetFiles(projectDirectory.FullName, "*.csproj").Length > 0)
                {
                    return Path.Combine(projectDirectory.FullName, "Data", "school.json");
                }

                projectDirectory = projectDirectory.Parent;
            }

            return Path.Combine(AppContext.BaseDirectory, "Data", "school.json");
        }

        // Зарежда базата данни
        public SchoolDatabase Load()
        {
            this.loadedSuccessfully = false;
            this.loadFailed = false;

            // Ако файлът не съществува
            if (!File.Exists(this.filePath))
            {
                SchoolDatabase database = CreateSampleDatabase();

                SaveInternal(database);

                this.loadedSuccessfully = true;

                return database;
            }

            try
            {
                string json = File.ReadAllText(this.filePath);

                SchoolDatabase database =
                    JsonSerializer.Deserialize<SchoolDatabase>(
                        json,
                        this.options
                    );

                if (database == null)
                {
                    throw new JsonException(
                        "JSON файлът не съдържа валидна база данни."
                    );
                }

                PrepareDatabase(database);

                this.loadedSuccessfully = true;

                return database;
            }
            catch (JsonException ex)
            {
                this.loadFailed = true;

                throw new InvalidDataException(
                    "Грешка! JSON файлът е повреден или невалиден. " +
                    "Оригиналният файл не е променен.",
                    ex
                );
            }
        }

        // Запазва базата данни
        public void Save(SchoolDatabase database)
        {
            if (this.loadFailed)
            {
                throw new InvalidOperationException(
                    "Не може да се записва, защото JSON файлът е повреден."
                );
            }

            if (!this.loadedSuccessfully)
            {
                throw new InvalidOperationException(
                    "Първо трябва да заредите базата данни чрез Load()."
                );
            }

            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            PrepareDatabase(database);

            SaveInternal(database);
        }

        // Вътрешен метод за запис
        private void SaveInternal(SchoolDatabase database)
        {
            string directory = Path.GetDirectoryName(this.filePath);

            Directory.CreateDirectory(directory);

            string json = JsonSerializer.Serialize(
                database,
                this.options
            );

            string temporaryPath = this.filePath + ".tmp";

            try
            {
                File.WriteAllText(temporaryPath, json);

                // Замества стария файл с новия
                File.Move(temporaryPath, this.filePath, true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }

        // Проверява и подготвя заредената база
        private void PrepareDatabase(SchoolDatabase database)
        {
            if (database.Students == null)
            {
                database.Students = new List<Student>();
            }

            if (database.Teachers == null)
            {
                database.Teachers = new List<Teacher>();
            }

            if (database.Subjects == null)
            {
                database.Subjects = new List<Subject>();
            }

            int maxGradeId = 0;

            foreach (Student student in database.Students)
            {
                if (student.Grades == null)
                {
                    student.Grades = new List<Grade>();
                }

                foreach (Grade grade in student.Grades)
                {
                    if (grade.Id > maxGradeId)
                    {
                        maxGradeId = grade.Id;
                    }
                }
            }

            if (database.NextGradeId <= maxGradeId)
            {
                database.NextGradeId = maxGradeId + 1;
            }
        }

        // Създава примерна база данни
        private SchoolDatabase CreateSampleDatabase()
        {
            SchoolDatabase database = new SchoolDatabase();

            // Предмети
            database.Subjects.Add(new Subject(1, "Математика"));
            database.Subjects.Add(new Subject(2, "Български език"));
            database.Subjects.Add(new Subject(3, "Английски език"));

            // Ученик 1
            Student student1 = new Student();
            student1.Id = 1;
            student1.Name = "Иван Петров";
            student1.ClassName = "10А";

            student1.Grades.Add(new Grade(
                1, 1, 1, 5, DateTime.Today.AddDays(-5)
            ));

            student1.Grades.Add(new Grade(
                2, 1, 1, 6, DateTime.Today.AddDays(-3)
            ));

            student1.Grades.Add(new Grade(
                3, 1, 2, 5, DateTime.Today.AddDays(-2)
            ));

            student1.Grades.Add(new Grade(
                4, 1, 3, 4, DateTime.Today.AddDays(-1)
            ));

            database.Students.Add(student1);

            // Ученик 2
            Student student2 = new Student();
            student2.Id = 2;
            student2.Name = "Мария Георгиева";
            student2.ClassName = "10А";

            student2.Grades.Add(new Grade(
                5, 2, 1, 4, DateTime.Today.AddDays(-4)
            ));

            student2.Grades.Add(new Grade(
                6, 2, 2, 6, DateTime.Today.AddDays(-2)
            ));

            student2.Grades.Add(new Grade(
                7, 2, 3, 5, DateTime.Today.AddDays(-1)
            ));

            database.Students.Add(student2);

            // Ученик 3
            Student student3 = new Student();
            student3.Id = 3;
            student3.Name = "Георги Димитров";
            student3.ClassName = "10Б";

            student3.Grades.Add(new Grade(
                8, 3, 1, 3, DateTime.Today.AddDays(-5)
            ));

            student3.Grades.Add(new Grade(
                9, 3, 2, 5, DateTime.Today.AddDays(-3)
            ));

            student3.Grades.Add(new Grade(
                10, 3, 3, 6, DateTime.Today.AddDays(-1)
            ));

            database.Students.Add(student3);

    
            Teacher teacher = new Teacher();
            teacher.Id = 1;
            teacher.Name = "Петър Иванов";
            teacher.SubjectIds.Add(1);
            teacher.SubjectIds.Add(2);
            teacher.SubjectIds.Add(3);

            database.Teachers.Add(teacher);
            database.NextGradeId = 11;

            return database;
        }
    }
}