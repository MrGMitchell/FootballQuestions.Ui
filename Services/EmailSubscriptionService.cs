using FootballQuestions.Ui.Models;
using Microsoft.Azure.Cosmos;

namespace FootballQuestions.Ui.Services
{
    public class EmailSubscriptionService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly string? _databaseId;
        private readonly string? _containerId;

        public EmailSubscriptionService(IConfiguration configuration)
        {
            string endpointUri = configuration.GetSection("CosmosDb")["EndpointUri"]!;
            string primaryKey = configuration.GetSection("CosmosDb")["PrimaryKey"]!;

            _databaseId = configuration.GetSection("CosmosDb")["DatabaseId"]!;
            _containerId = configuration.GetSection("CosmosDb")["ContainerId"]!;

            _cosmosClient = new CosmosClient(endpointUri, primaryKey);
        }

        public async Task<bool> Subscribe(string email)
        {
            try
            {
                Database database = _cosmosClient.GetDatabase(_databaseId);
                Container container = database.GetContainer(_containerId);
                
                Random rnd = new Random();
                int r = rnd.Next(short.MaxValue);
                var subscriber = new Subscriber{ id = Guid.NewGuid().ToString(), EmailId = r.ToString(), Email = email, SubscriptionDate = DateTime.UtcNow };
                var response = await container.CreateItemAsync(subscriber);
                return response.StatusCode == System.Net.HttpStatusCode.Created;
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
                Database database = _cosmosClient.GetDatabase(_databaseId);
                Container container = database.GetContainer(_containerId);

                // Query to find the item by email
                var query = new QueryDefinition("SELECT * FROM c WHERE c.Email = @Email")
                    .WithParameter("@Email", email);

                var iterator = container.GetItemQueryIterator<Subscriber>(query);
                
                if (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    if (response.Count == 0)
                    {
                        Console.Error.WriteLine($"No subscriber found with email: {email}");
                        return false;
                    }
                    foreach (var item in response)
                    {
                        await container.DeleteItemAsync<Subscriber>(item.id, new PartitionKey(item.EmailId));
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error deleting from Cosmos DB: {ex.Message}");
            }
            return await Task.FromResult(true);
        }
    }
}