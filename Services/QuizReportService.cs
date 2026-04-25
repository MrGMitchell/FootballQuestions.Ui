using FootballQuestions.Ui.Models;

namespace FootballQuestions.Ui.Services
{
    public class QuizReportService : IQuizReportService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string? _reportsEndpoint;

        public QuizReportService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _reportsEndpoint = configuration.GetValue<string>("ReportsEndpoint");
        }

        public async Task<UserReportSummary?> GetUserReport(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_reportsEndpoint))
                {
                    Console.Error.WriteLine("ReportsEndpoint configuration is missing");
                    return null;
                }

                var client = _httpClientFactory.CreateClient("LunaApi");
                var response = await client.GetAsync($"{_reportsEndpoint}/user/{userId}/summary");

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.Error.WriteLine($"Error retrieving report: {response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return System.Text.Json.JsonSerializer.Deserialize<UserReportSummary>(content,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching user report: {ex.Message}");
                return null;
            }
        }

        public async Task<List<QuizReport>?> GetQuizHistory(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_reportsEndpoint))
                {
                    Console.Error.WriteLine("ReportsEndpoint configuration is missing");
                    return null;
                }

                var client = _httpClientFactory.CreateClient("LunaApi");
                var response = await client.GetAsync($"{_reportsEndpoint}/user/{userId}/history");

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.Error.WriteLine($"Error retrieving quiz history: {response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return System.Text.Json.JsonSerializer.Deserialize<List<QuizReport>>(content,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching quiz history: {ex.Message}");
                return null;
            }
        }
    }
}
