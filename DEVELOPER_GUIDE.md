# Developer Quick Start - User Access Management

## What's New

Your Blazor app now has:
- ✅ Azure AD authentication via App Service Easy Auth
- ✅ Role-based access control (Admin and Player)
- ✅ User management dashboard for admins
- ✅ All pages require authentication
- ✅ Secure API endpoints for user management

## Key Files Added

| File | Purpose |
|------|---------|
| `Services/IUserIdentityService.cs` | Interface for user identity operations |
| `Services/UserIdentityService.cs` | Service for managing user roles in Azure Table Storage |
| `Services/AppServiceAuthenticationStateProvider.cs` | Blazor authentication provider |
| `Services/AppServiceAuthenticationHandler.cs` | ASP.NET Core authentication handler |
| `Controllers/AuthController.cs` | API endpoints for auth operations |
| `Components/UserAuthDisplay.razor` | User info and logout button |
| `Components/Pages/UserManagement.razor` | Admin page for role management |

## Key Files Modified

| File | Changes |
|------|---------|
| `Program.cs` | Added auth services and middleware |
| `App.razor` | Wrapped Routes with CascadingAuthenticationState |
| `NavMenu.razor` | Added UserAuthDisplay and Admin User Management link |
| All page components | Added @attribute [Authorize] |
| `FootballQuestions.Ui.csproj` | Added Azure.Data.Tables package |

## Quick Testing

### Local Development (without App Service)

When developing locally without App Service Easy Auth:
1. The app will show "Not authenticated" in the navbar
2. API calls to auth endpoints will fail gracefully
3. For full testing, deploy to Azure App Service first

### After Azure Deployment

1. Navigate to your app URL
2. Automatically redirected to Azure AD login
3. Default role: **Player**
4. As admin user, see **User Management** in nav menu

## Adding More Roles

To add more roles (e.g., "Coach", "Manager"):

1. **Update AuthController.cs** - Modify validRoles array
2. **Update UserManagement.razor** - Add role options to dropdown
3. **Update role checks** - Use `@if (context.User.IsInRole("NewRole"))`

## Protecting Admin Features

To restrict a page to Admin role only:

```razor
@attribute [Authorize(Roles = "Admin")]
```

To show content only to admins:

```razor
<AuthorizeView Roles="Admin">
    <Authorized>
        <div>Admin content</div>
    </Authorized>
</AuthorizeView>
```

## API Integration

To call protected API endpoints from JavaScript/Blazor:

```csharp
// Get current user
var response = await HttpClient.GetAsync("api/auth/user");
var user = await response.Content.ReadAsAsync<UserInfo>();

// Assign role (admin only)
var request = new { userEmail = "user@example.com", role = "Admin" };
var response = await HttpClient.PostAsJsonAsync("api/auth/assign-role", request);
```

## Troubleshooting Development

### "No AuthenticationStateProvider found"
- Check that App.razor has `<CascadingAuthenticationState>`
- Verify Program.cs calls `builder.Services.AddCascadingAuthenticationState()`

### API returns 401 Unauthorized
- Verify authentication headers are present
- Check controller `[Authorize]` attributes
- Review Application Insights logs

### Cannot see User Management page
- User needs Admin role
- Check role is assigned via `api/auth/assign-role`
- Clear browser cache

## Environment Variables

Add to `launchSettings.json` for local development:
```json
"Endpoints:AppConfiguration": "your_endpoint",
"Endpoints:TableStorageEndpoint": "https://yourstorageaccount.table.core.windows.net/"
```

## Next Steps

1. See **SETUP_AUTH.md** for production Azure configuration
2. See **TROUBLESHOOTING.md** for common issues
3. Add custom claims to enhance role-based logic
4. Implement role refresh and caching strategies
