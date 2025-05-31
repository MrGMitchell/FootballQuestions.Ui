using FootballQuestions.Ui.Models;

namespace FootballQuestions.Ui.Services
{
    public class EmailSubscriptionService : IEmailSubscriptionService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string? subcribeEndpoint;
        private readonly string? unsubscribeEndpoint;

        public EmailSubscriptionService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            subcribeEndpoint = configuration.GetValue<string>("SubscribeEndpoint");
            unsubscribeEndpoint = configuration.GetValue<string>("UnsubscribeEndpoint");
        }

        public async Task<bool> Subscribe(string email)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("LunaApi");
                Random rnd = new Random();
                int r = rnd.Next(short.MaxValue);
                var subscriber = new Subscriber{ id = Guid.NewGuid().ToString(), EmailId = r.ToString(), Email = email, SubscriptionDate = DateTime.UtcNow };
                var response = await client.PostAsync(subcribeEndpoint, 
                    new StringContent(System.Text.Json.JsonSerializer.Serialize(subscriber), 
                    System.Text.Encoding.UTF8, "application/json"));
                return response.StatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error saving to Cosmos DB: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> Unsubscribe(string email)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("LunaApi");
                var response = await client.DeleteAsync(unsubscribeEndpoint + $"?email={email}");
                return response.StatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error deleting from Cosmos DB: {ex.Message}");
                return false;
            }
        }
    }
}