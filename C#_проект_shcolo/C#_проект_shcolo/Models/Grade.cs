using System;

namespace C__проект_shcolo.Models
{
    public class Grade
    {
        private int id;
        private int studentId;
        private int subjectId;
        private int value;
        private DateTime date;

        public Grade()
        {
        }

        public Grade(int id, int studentId, int subjectId, int value, DateTime date) : this()
        {
            this.Id = id;
            this.StudentId = studentId;
            this.SubjectId = subjectId;
            this.Value = value;
            this.Date = date;
        }

        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public int StudentId
        {
            get { return this.studentId; }
            set { this.studentId = value; }
        }

        public int SubjectId
        {
            get { return this.subjectId; }
            set { this.subjectId = value; }
        }

        public int Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public DateTime Date
        {
            get { return this.date; }
            set { this.date = value; }
        }
    }
}