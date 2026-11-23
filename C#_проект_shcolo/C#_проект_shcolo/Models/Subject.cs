namespace C__проект_shcolo.Models
{
    public class Subject
    {
        private int id;
        private string name;

        public Subject()
        {
        }

        public Subject(int id, string name) : this()
        {
            this.Id = id;
            this.Name = name;
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
    }
}