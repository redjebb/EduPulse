using C__проект_shcolo.Models;

namespace C__проект_shcolo.Services
{
    public class GradeSimulator
    {
        private SchoolDatabase db;

        public GradeSimulator(SchoolDatabase db)
        {
            this.db = db;
        }

        public double SimulateNextGrade(int studentId, int subjectId, int proposedGrade)
        {
            if (proposedGrade < 2 || proposedGrade > 6)
            {
                return 0;
            }

            Student student = null;
            foreach (Student s in this.db.Students)
            {
                if (s.Id == studentId)
                {
                    student = s;
                    break;
                }
            }

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

            double newAverage = (sum + proposedGrade) / (count + 1);
            return Math.Round(newAverage, 2);
        }
    }
}