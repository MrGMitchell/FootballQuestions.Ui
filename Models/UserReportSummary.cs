namespace FootballQuestions.Ui.Models
{
    public class UserReportSummary
    {
        public int TotalQuizzesTaken { get; set; }
        public double OverallAccuracyPercentage { get; set; }
        public int TotalQuestionsAnswered { get; set; }
        public int TotalCorrectAnswers { get; set; }
        public List<CategoryStats>? CategoryBreakdown { get; set; }
        public List<QuizReport>? RecentQuizzes { get; set; }
    }
}
