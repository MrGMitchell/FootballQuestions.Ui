using FootballQuestions.Ui.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballQuestions.Ui.Controllers;

/// <summary>
/// API controller for authentication and user identity management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserIdentityService _userIdentityService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserIdentityService userIdentityService, ILogger<AuthController> logger)
    {
        _userIdentityService = userIdentityService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current user's information (email and role)
    /// Called by the AuthenticationStateProvider
    /// </summary>
    [AllowAnonymous]
    [HttpGet("user")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userEmail = _userIdentityService.GetUserEmail(HttpContext);

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                _logger.LogWarning("Request to /user endpoint without authentication");
                return Unauthorized();
            }

            var userRole = await _userIdentityService.GetUserRoleAsync(userEmail);

            return Ok(new
            {
                email = userEmail,
                role = userRole
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetCurrentUser");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Checks if a user is authenticated
    /// </summary>
    [AllowAnonymous]
    [HttpGet("check")]
    public IActionResult CheckAuth()
    {
        var userEmail = _userIdentityService.GetUserEmail(HttpContext);
        return Ok(new { isAuthenticated = !string.IsNullOrWhiteSpace(userEmail) });
    }

    /// <summary>
    /// Gets all users and their roles (Admin only)
    /// </summary>
    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _userIdentityService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Assigns a role to a user (Admin only)
    /// </summary>
    [HttpPost("assign-role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.UserEmail) || string.IsNullOrWhiteSpace(request?.Role))
        {
            return BadRequest("UserEmail and Role are required");
        }

        var validRoles = new[] { "Admin", "Player" };
        if (!validRoles.Contains(request.Role))
        {
            return BadRequest($"Invalid role. Valid roles are: {string.Join(", ", validRoles)}");
        }

        try
        {
            await _userIdentityService.AssignRoleAsync(request.UserEmail, request.Role);
            _logger.LogInformation("Admin assigned role {Role} to user {UserEmail}", 
                request.Role, request.UserEmail);
            
            return Ok(new { message = "Role assigned successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user {UserEmail}", request.UserEmail);
            return StatusCode(500, "Internal server error");
        }
    }

    public class AssignRoleRequest
    {
        public string? UserEmail { get; set; }
        public string? Role { get; set; }
    }
}
