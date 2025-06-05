using System.Text.Json.Serialization;

namespace FootballQuestions.Ui.Models
{
    public class FootballQuestion
    {
        [property: JsonPropertyName("id")]
        public string? id { get; set; }
        [property: JsonPropertyName("QuestionId")]
        public string? QuestionId { get; set; }
        [property: JsonPropertyName("question")]
        public string? Question { get; set; }

        [property: JsonPropertyName("ruling")]
        public string? Ruling { get; set; }

        [property: JsonPropertyName("lastSent")]
        public DateTime? LastSent { get; set; }
    }
}