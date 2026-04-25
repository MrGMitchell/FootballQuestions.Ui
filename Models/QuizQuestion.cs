using System.Text.Json.Serialization;

namespace FootballQuestions.Ui.Models
{ 
    public class QuizQuestion
    {
        [property: JsonPropertyName("id")]
        public string? id { get; set; }

        [property: JsonPropertyName("questionId")]
        public string? QuestionId { get; set; }

        [property: JsonPropertyName("question")]
        public string? Question { get; set; }
        
        [property: JsonPropertyName("correctAnswer")]
        public string? CorrectAnswer { get; set; }

        [property: JsonPropertyName("choices")]
        public List<string>? Choices { get; set; }

        [property: JsonPropertyName("categories")]
        public List<string>? Categories { get; set; }

        [property: JsonPropertyName("ruleNumber")]
        public string? RuleNumber { get; set; }
    }
}