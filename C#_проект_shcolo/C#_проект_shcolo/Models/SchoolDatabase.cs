using System.Collections.Generic;

namespace C__проект_shcolo.Models
{
    public class SchoolDatabase
    {
        private List<Student> students = new List<Student>();
        private List<Teacher> teachers = new List<Teacher>();
        private List<Subject> subjects = new List<Subject>();
        private int nextGradeId;

        public SchoolDatabase()
        {
            this.Students = new List<Student>();
            this.Teachers = new List<Teacher>();
            this.Subjects = new List<Subject>();
            this.NextGradeId = 1;
        }

        public List<Student> Students
        {
            get { return this.students; }
            set { this.students = value; }
        }

        public List<Teacher> Teachers
        {
            get { return this.teachers; }
            set { this.teachers = value; }
        }

        public List<Subject> Subjects
        {
            get { return this.subjects; }
            set { this.subjects = value; }
        }

        public int NextGradeId
        {
            get { return this.nextGradeId; }
            set { this.nextGradeId = value; }
        }
    }
}