using System.Collections.Generic;

namespace C__проект_shcolo.Models
{
    public class Teacher
    {
        private int id;
        private string name = string.Empty;
        private List<int> subjectIds = new List<int>();

        public Teacher()
        {
            this.SubjectIds = new List<int>();
        }

        public Teacher(int id, string name, List<int> subjectIds) : this()
        {
            this.Id = id;
            this.Name = name;
            this.SubjectIds = subjectIds;
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

        public List<int> SubjectIds
        {
            get { return this.subjectIds; }
            set { this.subjectIds = value; }
        }
    }
}