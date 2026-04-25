namespace FootballQuestions.Ui.Models
{
    public class QuizReport
    {
        public string? QuizResultId { get; set; }
        public DateTime CompletedAt { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public double AccuracyPercentage => TotalQuestions > 0 ? (CorrectAnswers * 100.0) / TotalQuestions : 0;
        public List<CategoryStats>? CategoryStats { get; set; }
    }
}
