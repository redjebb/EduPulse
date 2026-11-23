using System.Collections.Generic;
using C__проект_shcolo.Models;

namespace C__проект_shcolo.Services
{
    public class InsightsService
    {
        private SchoolDatabase db;
        private GradeService gradeService;

        public InsightsService(SchoolDatabase db, GradeService gradeService)
        {
            this.db = db;
            this.gradeService = gradeService;
        }

        public string GetStrongestSubject(int studentId)
        {
            Subject? bestSubject = null;
            double highestAvg = 0;

            foreach (Subject subject in this.db.Subjects)
            {
                double avg = this.gradeService.GetSubjectAverage(studentId, subject.Id);
                if (avg > highestAvg)
                {
                    highestAvg = avg;
                    bestSubject = subject;
                }
            }

            if (bestSubject == null)
            {
                return "Няма достатъчно оценки за анализ.";
            }

            return $"{bestSubject.Name} ({highestAvg:F2})";
        }

        public string GetWeakestSubject(int studentId)
        {
            Subject? weakestSubject = null;
            double lowestAvg = 7.0;

            foreach (Subject subject in this.db.Subjects)
            {
                double avg = this.gradeService.GetSubjectAverage(studentId, subject.Id);
                if (avg > 0 && avg < lowestAvg)
                {
                    lowestAvg = avg;
                    weakestSubject = subject;
                }
            }

            if (weakestSubject == null)
            {
                return "Няма достатъчно оценки за анализ.";
            }

            return $"{weakestSubject.Name} ({lowestAvg:F2})";
        }

        public List<string> GetSummary(int studentId)
        {
            List<string> insights = new List<string>();
            Student? student = this.gradeService.FindStudent(studentId);

            if (student == null || student.Grades.Count == 0)
            {
                insights.Add("Все още нямаш въведени оценки за анализ.");
                return insights;
            }

            insights.Add("Най-силен предмет: " + this.GetStrongestSubject(studentId));
            insights.Add("Най-слаб предмет: " + this.GetWeakestSubject(studentId));

            foreach (Subject subject in this.db.Subjects)
            {
                double currentAvg = this.gradeService.GetSubjectAverage(studentId, subject.Id);
                if (currentAvg > 0)
                {
                    double sum = 0;
                    int count = 0;
                    foreach (Grade grade in student.Grades)
                    {
                        if (grade.SubjectId == subject.Id)
                        {
                            sum += grade.Value;
                            count++;
                        }
                    }
                    double ifSix = (sum + 6) / (count + 1);
                    insights.Add($"По {subject.Name} имаш {currentAvg:F2}. След оценка 6 ще стане {ifSix:F2}.");
                }
            }

            return insights;
        }
    }
}