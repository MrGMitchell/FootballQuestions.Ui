using Microsoft.Azure.Cosmos;
using System.Security.Claims;

namespace FootballQuestions.Ui.Services;

/// <summary>
/// Service for managing user identity and roles using Azure Cosmos DB
/// </summary>
public class UserIdentityService : IUserIdentityService
{
    private const string DEFAULT_ROLE = "Player";
    private const string CONTAINER_NAME = "UserRoles";
    private const string PARTITION_KEY = "/userId";

    private readonly Container _container;
    private readonly ILogger<UserIdentityService> _logger;

    public UserIdentityService(Container container, ILogger<UserIdentityService> logger)
    {
        _container = container;
        _logger = logger;
    }

    public string? GetUserEmail(HttpContext? context)
    {
        if (context == null)
            return null;

        // App Service Easy Auth sets this header
        var principalIdHeader = context.Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"].FirstOrDefault();
        var nameHeader = context.Request.Headers["X-MS-CLIENT-PRINCIPAL-NAME"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(nameHeader))
        {
            _logger.LogInformation("User authenticated: {UserName}", nameHeader);
            return nameHeader;
        }

        if (!string.IsNullOrWhiteSpace(principalIdHeader))
        {
            _logger.LogInformation("User authenticated with principal ID: {PrincipalId}", principalIdHeader);
            return principalIdHeader;
        }

        _logger.LogWarning("No authentication headers found");
        return null;
    }

    public async Task<string> GetUserRoleAsync(string userEmail)
    {
        try
        {
            var userId = userEmail.ToLower();
            var response = await _container.ReadItemAsync<UserRoleEntity>(userId, new PartitionKey(userId));
            return response.Resource.Role ?? DEFAULT_ROLE;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // User doesn't have a role assigned yet, return default
            _logger.LogInformation("User {UserEmail} has no assigned role, using default: {DefaultRole}", 
                userEmail, DEFAULT_ROLE);
            return DEFAULT_ROLE;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role for user {UserEmail}", userEmail);
            return DEFAULT_ROLE;
        }
    }

    public async Task<bool> HasRoleAsync(string userEmail, string role)
    {
        var userRole = await GetUserRoleAsync(userEmail);
        return userRole.Equals(role, StringComparison.OrdinalIgnoreCase);
    }

    public async Task AssignRoleAsync(string userEmail, string role)
    {
        try
        {
            var userId = userEmail.ToLower();
            var entity = new UserRoleEntity
            {
                id = userId,
                userId = userId,
                Role = role,
                AssignedAt = DateTime.UtcNow
            };

            await _container.UpsertItemAsync(entity, new PartitionKey(userId));
            _logger.LogInformation("Assigned role {Role} to user {UserEmail}", role, userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {Role} to user {UserEmail}", role, userEmail);
            throw;
        }
    }

    public async Task<Dictionary<string, string>> GetAllUsersAsync()
    {
        var users = new Dictionary<string, string>();

        try
        {
            var query = "SELECT c.userId, c.Role FROM c";
            var iterator = _container.GetItemQueryIterator<UserRoleEntity>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                foreach (var entity in response)
                {
                    if (!string.IsNullOrWhiteSpace(entity.userId))
                    {
                        users[entity.userId] = entity.Role ?? DEFAULT_ROLE;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
        }

        return users;
    }

    /// <summary>
    /// Entity model for storing user roles in Azure Cosmos DB
    /// </summary>
    private class UserRoleEntity
    {
        public string? id { get; set; }
        public string? userId { get; set; }        public Azure.ETag ETag { get; set; }

        public string? Role { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}

