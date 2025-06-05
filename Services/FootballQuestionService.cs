using FootballQuestions.Ui.Models;

namespace FootballQuestions.Ui.Services
{
    public class FootballQuestionService : IFootballQuestionService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string? todaysQuestionEndpoint;
        private readonly string? quizQuestionEndpoint;

        public FootballQuestionService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            todaysQuestionEndpoint = configuration.GetValue<string>("TodaysQuestionEndpoint");
            quizQuestionEndpoint = configuration.GetValue<string>("QuizQuestionEndpoint");
        }

        public async Task<FootballQuestion?> GetTodaysQuestion()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("LunaApi");

                var question = await client.GetFromJsonAsync<FootballQuestion>(todaysQuestionEndpoint);

                return question;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving question: {ex.Message}");

                return new FootballQuestion();
            }
        }
        
        public async Task<List<QuizQuestion>?> GetQuizFootballQuestions()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("LunaApi");
                
                var footballQuestions = await client.GetFromJsonAsync<List<QuizQuestion>>(quizQuestionEndpoint);

                return footballQuestions;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving questions: {ex.Message}");

                return [];
            }
        }
    }
}