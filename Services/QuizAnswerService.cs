using FootballQuestions.Ui.Models;
using System.Text.Json;

namespace FootballQuestions.Ui.Services
{
    public class QuizAnswerService : IQuizAnswerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string? _saveAnswersEndpoint;

        public QuizAnswerService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _saveAnswersEndpoint = configuration.GetValue<string>("SaveAnswersEndpoint");
        }

        public async Task<bool> SaveQuizAnswers(List<UserAnswer> answers, string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_saveAnswersEndpoint))
                {
                    Console.Error.WriteLine("SaveAnswersEndpoint configuration is missing");
                    return false;
                }

                var client = _httpClientFactory.CreateClient("LunaApi");
                
                var quizResult = new
                {
                    id = Guid.NewGuid().ToString(),
                    userId = userId,
                    UserQuizId = Guid.NewGuid().ToString(),
                    completedAt = DateTime.Now,
                    answers = answers
                };

                var response = await client.PostAsync(_saveAnswersEndpoint,
                    new StringContent(JsonSerializer.Serialize(quizResult),
                    System.Text.Encoding.UTF8, "application/json"));

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.Error.WriteLine($"Error saving answers: {response.StatusCode}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error saving to Cosmos DB: {ex.Message}");
                return false;
            }
        }
    }
}
