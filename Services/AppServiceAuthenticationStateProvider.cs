using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace FootballQuestions.Ui.Services;

/// <summary>
/// Custom AuthenticationStateProvider that integrates with Azure App Service Easy Auth
/// </summary>
public class AppServiceAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AppServiceAuthenticationStateProvider> _logger;

    public AppServiceAuthenticationStateProvider(
        HttpClient httpClient, 
        ILogger<AppServiceAuthenticationStateProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // Call the server to get user info from App Service auth headers
            var response = await _httpClient.GetAsync("api/auth/user");

            if (response.IsSuccessStatusCode)
            {
                var userInfoJson = await response.Content.ReadAsStringAsync();
                var userInfo = System.Text.Json.JsonSerializer.Deserialize<UserAuthInfo>(userInfoJson);

                if (!string.IsNullOrWhiteSpace(userInfo?.Email))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email, userInfo.Email),
                        new Claim(ClaimTypes.NameIdentifier, userInfo.Email),
                    };

                    if (!string.IsNullOrWhiteSpace(userInfo.Role))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, userInfo.Role));
                    }

                    var identity = new ClaimsIdentity(claims, "AppServiceAuth");
                    var user = new ClaimsPrincipal(identity);

                    _logger.LogInformation("User authenticated: {Email} with role: {Role}", 
                        userInfo.Email, userInfo.Role ?? "Player");

                    return new AuthenticationState(user);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving authentication state");
        }

        // Return unauthenticated state
        return new AuthenticationState(new ClaimsPrincipal());
    }

    public class UserAuthInfo
    {
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}
