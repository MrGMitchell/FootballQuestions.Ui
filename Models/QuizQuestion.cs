using System.Text.Json.Serialization;

namespace FootballQuestions.Ui.Models
{ 
    public class QuizQuestion
    {
        [property: JsonPropertyName("question")]
        public string? Question { get; set; }

        [property: JsonPropertyName("choices")]
        public Choice? Choices { get; set; }
    }
}