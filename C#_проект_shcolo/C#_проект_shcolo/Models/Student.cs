using System.Collections.Generic;

namespace C__проект_shcolo.Models
{
    public class Student
    {
        private int id;
        private string name = string.Empty;
        private string className = string.Empty;
        private List<Grade> grades = new List<Grade>();

        public Student()
        {
            this.Grades = new List<Grade>();
        }

        public Student(int id, string name, string className) : this()
        {
            this.Id = id;
            this.Name = name;
            this.ClassName = className;
        }

        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public string ClassName
        {
            get { return this.className; }
            set { this.className = value; }
        }

        public List<Grade> Grades
        {
            get { return this.grades; }
            set { this.grades = value; }
        }
    }
}