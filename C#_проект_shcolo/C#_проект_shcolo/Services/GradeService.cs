using System;
using System.Collections.Generic;
using C__проект_shcolo.Models;

namespace C__проект_shcolo.Services
{
    public class GradeService
    {
        private SchoolDatabase db;

        public GradeService(SchoolDatabase db)
        {
            this.db = db;
        }

        public Student? FindStudent(int studentId)
        {
            foreach (Student student in this.db.Students)
            {
                if (student.Id == studentId)
                {
                    return student;
                }
            }
            return null;
        }

        public Subject? FindSubject(int subjectId)
        {
            foreach (Subject subject in this.db.Subjects)
            {
                if (subject.Id == subjectId)
                {
                    return subject;
                }
            }
            return null;
        }

        public bool AddGrade(int studentId, int subjectId, int value)
        {
            if (value < 2 || value > 6)
            {
                return false;
            }

            Student? student = this.FindStudent(studentId);
            Subject? subject = this.FindSubject(subjectId);

            if (student == null || subject == null)
            {
                return false;
            }

            Grade newGrade = new Grade(this.db.NextGradeId, studentId, subjectId, value, DateTime.Now);
            this.db.NextGradeId++;
            student.Grades.Add(newGrade);

            return true;
        }

        public List<Grade> GetGrades(int studentId)
        {
            Student student = this.FindStudent(studentId);
            if (student == null)
            {
                return new List<Grade>();
            }
            return student.Grades;
        }

        public double GetAverage(int studentId)
        {
            Student student = this.FindStudent(studentId);
            if (student == null || student.Grades.Count == 0)
            {
                return 0;
            }

            double sum = 0;
            foreach (Grade grade in student.Grades)
            {
                sum += grade.Value;
            }

            return Math.Round(sum / student.Grades.Count, 2);
        }

        public double GetSubjectAverage(int studentId, int subjectId)
        {
            Student student = this.FindStudent(studentId);
            if (student == null)
            {
                return 0;
            }

            double sum = 0;
            int count = 0;

            foreach (Grade grade in student.Grades)
            {
                if (grade.SubjectId == subjectId)
                {
                    sum += grade.Value;
                    count++;
                }
            }

            if (count == 0)
            {
                return 0;
            }

            return Math.Round(sum / count, 2);
        }
    }
}