using System.Security.Claims;

namespace FootballQuestions.Ui.Services;

/// <summary>
/// Service for managing user identity and role assignments
/// </summary>
public interface IUserIdentityService
{
    /// <summary>
    /// Gets the current user's email from App Service authentication headers
    /// </summary>
    string? GetUserEmail(HttpContext? context);

    /// <summary>
    /// Gets the user's assigned role (Admin or Player)
    /// </summary>
    Task<string> GetUserRoleAsync(string userEmail);

    /// <summary>
    /// Checks if the user has a specific role
    /// </summary>
    Task<bool> HasRoleAsync(string userEmail, string role);

    /// <summary>
    /// Assigns a role to a user
    /// </summary>
    Task AssignRoleAsync(string userEmail, string role);

    /// <summary>
    /// Gets all users and their roles (Admin only)
    /// </summary>
    Task<Dictionary<string, string>> GetAllUsersAsync();
}
