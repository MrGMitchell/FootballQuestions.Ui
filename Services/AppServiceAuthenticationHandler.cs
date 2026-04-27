using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace FootballQuestions.Ui.Services;

/// <summary>
/// Authentication handler that reads user identity from App Service Easy Auth headers
/// </summary>
public class AppServiceAuthenticationHandler : AuthenticationHandler<AppServiceAuthenticationHandler.AppServiceAuthenticationOptions>
{
    private readonly IUserIdentityService _userIdentityService;

    public AppServiceAuthenticationHandler(
        IOptionsMonitor<AppServiceAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IUserIdentityService userIdentityService)
        : base(options, logger, encoder)
    {
        _userIdentityService = userIdentityService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var userEmail = _userIdentityService.GetUserEmail(Context);

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return AuthenticateResult.NoResult();
            }

            var userRole = await _userIdentityService.GetUserRoleAsync(userEmail);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, userEmail),
                new Claim(ClaimTypes.NameIdentifier, userEmail),
            };

            if (!string.IsNullOrWhiteSpace(userRole))
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            Logger.LogInformation("User {UserEmail} authenticated with role: {Role}", userEmail, userRole);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error authenticating user");
            return AuthenticateResult.Fail("Authentication failed");
        }
    }

    public class AppServiceAuthenticationOptions : AuthenticationSchemeOptions
    {
    }
}
