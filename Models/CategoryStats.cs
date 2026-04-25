namespace FootballQuestions.Ui.Models
{
    public class CategoryStats
    {
        public string? Category { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public double AccuracyPercentage => TotalQuestions > 0 ? (CorrectAnswers * 100.0) / TotalQuestions : 0;
    }
}
