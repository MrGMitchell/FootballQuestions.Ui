# Authentication & Authorization Troubleshooting

## Common Issues and Solutions

### 1. "Cannot POST /api/auth/assign-role" - 404 Error

**Problem**: The API endpoint returns 404 Not Found

**Solutions**:
- ✅ Ensure `app.MapControllers()` is called in Program.cs after line 50
- ✅ Verify AuthController.cs is in the Controllers folder
- ✅ Check project builds without errors: `dotnet build`
- ✅ Restart the application after code changes
- ✅ Verify the full URL: `https://yourapp.azurewebsites.net/api/auth/assign-role`

**To verify**:
```bash
dotnet build
# Check compilation output for any errors
```

---

### 2. "Unauthorized" (401) on /api/auth/users

**Problem**: Admin cannot access user management API

**Possible Causes**:
- ❌ User doesn't have Admin role assigned yet
- ❌ Authentication headers not being sent from App Service
- ❌ Role cache is stale

**Solutions**:
1. **Check if user has Admin role**:
   - Manually add first admin to Azure Table Storage `UserRoles` table
   - RowKey: `user@example.com`
   - Role: `Admin`

2. **Verify App Service Authentication is enabled**:
   - Azure Portal → App Service → Authentication
   - Check provider is configured correctly
   - Restart App Service

3. **Clear browser cache and logout/login again**:
   - Browser cache may contain old authentication state
   - Try in private/incognito window

---

### 3. "Not authenticated" appears for all users

**Problem**: All users see "Not authenticated" in the navbar even after login

**Possible Causes**:
- ❌ App Service Authentication not configured
- ❌ X-MS-CLIENT-PRINCIPAL-NAME header not present
- ❌ Table Storage connection failed silently

**Solutions**:

1. **Check App Service Authentication**:
   ```
   Azure Portal → App Service → Configuration
   Look for X-MS-CLIENT-PRINCIPAL-* app settings
   ```

2. **Verify Managed Identity has correct roles**:
   - Azure Portal → Storage Account → Access Control (IAM)
   - App Service has "Storage Table Data Contributor" role
   - Can take up to 5 minutes to take effect

3. **Check Application Insights for errors**:
   - Azure Portal → App Service → Application Insights
   - Review logs in Trace and Custom Metrics
   - Search for "GetUserEmail" or "UserIdentityService"

4. **Test manually with Postman/cURL**:
   ```bash
   curl -H "X-MS-CLIENT-PRINCIPAL-NAME: test@example.com" \
        https://yourapp.azurewebsites.net/api/auth/check
   ```

---

### 4. Role change doesn't take effect immediately

**Problem**: After assigning a role, user still has old role until logout

**Explanation**: This is **expected behavior**. Roles are cached in the authentication state.

**Solution**:
- User must log out and log back in for new role to take effect
- Or refresh the page (Ctrl+F5)

---

### 5. "Cannot find api/auth/user" error in browser console

**Problem**: API endpoint returns 404 or CORS error

**Possible Causes**:
- ❌ AuthController not deployed
- ❌ CORS headers blocking the request
- ❌ App Service URL not matching

**Solutions**:
1. **Check deployment includes Controllers folder**:
   ```bash
   dotnet publish -c Release
   # Verify Controllers\ folder is in publish output
   ```

2. **Check CORS configuration in Program.cs**:
   ```csharp
   // If needed, add CORS:
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("default", builder =>
           builder.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader());
   });
   ```

3. **Verify the endpoint exists**:
   - Visit `https://yourapp.azurewebsites.net/api/auth/check`
   - Should return JSON, not 404

---

### 6. Table Storage "Cannot create container" error

**Problem**: Application crashes with "Cannot create container" on startup

**Possible Causes**:
- ❌ Table Storage endpoint is wrong in app settings
- ❌ Managed Identity doesn't have "Storage Table Data Contributor" role
- ❌ Storage account has firewall blocking the App Service

**Solutions**:

1. **Verify app settings in Azure Portal**:
   ```
   App Service → Configuration → Application settings
   
   Check:
   - Endpoints:TableStorageEndpoint = https://yourstorage.table.core.windows.net/
   - Name is exactly spelled correctly
   ```

2. **Grant Storage role to App Service**:
   ```
   Storage Account → Access Control (IAM)
   + Add → Add role assignment
   Select "Storage Table Data Contributor"
   Members: System-assigned managed identity
   Select your App Service
   ```

3. **Check storage firewall**:
   ```
   Storage Account → Networking
   If using firewall, add App Service's outbound IP to allow list
   ```

---

### 7. "The request path /api/auth/user contains invalid characters"

**Problem**: Request to auth API fails with 400 Bad Request

**Solution**:
- Check HTTP client is sending request correctly
- Verify URL is properly formatted: `/api/auth/user`
- No special characters in header values

---

### 8. Authorize attribute not working on pages

**Problem**: Unauthorized users can still access protected pages

**Possible Causes**:
- ❌ Page doesn't have `@attribute [Authorize]`
- ❌ Components not wrapped in CascadingAuthenticationState
- ❌ Blazor server rendering not configured correctly

**Solution**:
1. **Add Authorize attribute to page**:
   ```razor
   @page "/dashboard"
   @attribute [Microsoft.AspNetCore.Authorization.Authorize]
   ```

2. **Verify App.razor has CascadingAuthenticationState**:
   ```razor
   <CascadingAuthenticationState>
       <Routes />
   </CascadingAuthenticationState>
   ```

3. **Check Program.cs**:
   ```csharp
   builder.Services.AddCascadingAuthenticationState();
   ```

---

### 9. Admin cannot see "User Management" in nav menu

**Problem**: User assigned Admin role cannot see the admin menu link

**Possible Causes**:
- ❌ Role assignment failed
- ❌ User hasn't logged out/back in
- ❌ AuthorizeView directive not working

**Solution**:
1. **Verify role in database**:
   - Azure Portal → Storage Account → Tables
   - Open `UserRoles` table
   - Check user has row with Role = "Admin"

2. **Force re-authentication**:
   - Clear browser cookies for the domain
   - Logout: Navigate to `/.auth/logout`
   - Login again

3. **Check NavMenu.razor** has:
   ```razor
   <AuthorizeView Roles="Admin">
       <div class="nav-item px-3">
           <NavLink class="nav-link" href="admin/users">
               <span class="bi bi-people-fill" aria-hidden="true"></span> User Management
           </NavLink>
       </div>
   </AuthorizeView>
   ```

---

### 10. "Cannot resolve service of type IUserIdentityService"

**Problem**: Dependency injection error when starting app

**Possible Causes**:
- ❌ Service not registered in Program.cs
- ❌ Table Storage configuration missing

**Solution**:
1. **Check Program.cs services registration**:
   ```csharp
   // Must include:
   builder.Services.AddScoped<IUserIdentityService, UserIdentityService>();
   builder.Services.AddScoped<AuthenticationStateProvider, AppServiceAuthenticationStateProvider>();
   ```

2. **Verify configuration values**:
   ```csharp
   var tableStorageEndpoint = builder.Configuration.GetValue<string>("Endpoints:TableStorageEndpoint");
   // Should not be null or empty
   ```

---

## Debugging Tips

### Enable verbose logging

Add to Program.cs:
```csharp
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

### Check logs in Azure Portal

1. Go to App Service → Application Insights
2. Expand Logs section
3. Run custom query:
   ```kusto
   traces
   | where message contains "UserIdentity" or message contains "auth"
   | order by timestamp desc
   | limit 100
   ```

### Test endpoints with curl

```bash
# Check if authenticated
curl -v https://yourapp.azurewebsites.net/api/auth/check

# Test with mock header
curl -v -H "X-MS-CLIENT-PRINCIPAL-NAME: test@example.com" \
     https://yourapp.azurewebsites.net/api/auth/user
```

### Check Application Insights from Visual Studio

1. View → Application Insights Search
2. Search for exceptions or traces
3. Filter by request path `/api/auth/*`

---

## Getting Help

If issues persist:
1. Collect logs from Application Insights
2. Check Azure Service Health
3. Verify all prerequisites are met (see SETUP_AUTH.md)
4. Run `dotnet build` and `dotnet test` locally
5. Deploy fresh build to App Service
