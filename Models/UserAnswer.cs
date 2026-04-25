namespace FootballQuestions.Ui.Models
{
    public class UserAnswer
    {
        public string? QuestionId { get; set; }
        public List<string>? Categories { get; set; }
        public string? RuleNumber { get; set; }
        public string? SelectedAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public DateTime? AnsweredAt { get; set; }
    }
}