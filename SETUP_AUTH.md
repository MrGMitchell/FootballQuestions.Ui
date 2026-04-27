# Azure App Service User Access Management Setup

This Blazor app now includes user access management with:
- **Azure AD/Entra ID authentication** via App Service Easy Auth
- **Role-based access control** (Admin and Player roles)
- **User management dashboard** for admins
- **Secure API endpoints** for role assignment

## Architecture Overview

The implementation uses:
1. **App Service Authentication (Easy Auth)** - Handles Azure AD login
2. **Custom AuthenticationStateProvider** - Integrates Blazor with App Service auth
3. **Role management** - Stored in Azure Table Storage
4. **Protected Components** - Using `[Authorize]` attributes

## Prerequisites

- Azure App Service with .NET 9.0 runtime
- **Existing** Azure Cosmos DB account (already in use by your app)
- Azure Entra ID (Microsoft Entra) application registration

## Configuration Steps

### Step 1: Configure Cosmos DB Role Storage

Your app already uses Cosmos DB. The user roles will be stored in a new container called `UserRoles` in your existing Cosmos DB database.

**Key points**:
- Container name: `UserRoles`
- Partition key: `/userId`
- Automatically created on first run
- **No additional cost** - uses your existing Cosmos DB throughput

### Step 2: Configure App Service Authentication

1. Go to **Azure Portal** → Your App Service → **Authentication**
2. Click **Add identity provider**
3. Select **Microsoft Entra ID**
4. Choose **Express** setup (easiest option)
5. Or **Advanced** setup if you need custom configuration
6. Ensure **Unauthenticated requests** is set to **Return HTTP 401 Unauthorized**

### Step 3: Add Application Settings

In Azure Portal → App Service → **Configuration** → **Application settings**, verify/add:

```
Endpoints:CosmosDB=https://yourcosmosaccount.documents.azure.com:443/
Databases:CosmosDbName=YourDatabaseName
```

(Your `Endpoints:AppConfiguration` setting should already exist)

### No Additional Role Assignments Required ✅

Your App Service's Managed Identity already has access to Cosmos DB through your existing configuration.

### Step 4: Deploy the Updated Application

```bash
dotnet build
dotnet publish -c Release
# Deploy the publish folder to your App Service
```

## API Endpoints

### Get Current User
- **Endpoint**: `GET /api/auth/user`
- **Authentication**: App Service headers
- **Response**: 
```json
{
  "email": "user@example.com",
  "role": "Player"
}
```

### Get All Users (Admin only)
- **Endpoint**: `GET /api/auth/users`
- **Authorization**: Requires Admin role
- **Response**:
```json
{
  "user1@example.com": "Admin",
  "user2@example.com": "Player"
}
```

### Assign Role to User (Admin only)
- **Endpoint**: `POST /api/auth/assign-role`
- **Authorization**: Requires Admin role
- **Body**:
```json
{
  "userEmail": "newuser@example.com",
  "role": "Admin"
}
```

## Usage

### For End Users

1. Navigate to the app
2. Redirected to Azure AD login (automatic)
3. Log in with Microsoft account
4. Default role: **Player**
5. Click **Logout** button to sign out

### For Admins

1. Only users with **Admin** role can access user management
2. Navigate to **User Management** (visible in nav menu only for admins)
3. Add or update user roles
4. Changes take effect on next user login

## Components

### `UserAuthDisplay.razor`
Displays current user info and logout button in navbar

### `UserManagement.razor` (`/admin/users`)
Admin page for managing user roles

### `AuthController.cs`
API endpoints for authentication and user management

### `UserIdentityService.cs`
Service for retrieving and managing user roles in Table Storage

### `AppServiceAuthenticationStateProvider.cs`
Blazor provider integrating with App Service auth

### `AppServiceAuthenticationHandler.cs`
ASP.NET Core authentication handler

## Security Notes

- All API endpoints validate authentication headers from App Service
- Role-based access control via `[Authorize(Roles = "Admin")]` attribute
- Default role is **Player** for new users
- Admin operations require explicit role assignment
- Managed Identity used for secure Azure resource access (no connection strings in config)

### Troubleshooting

### Users seeing "Not authenticated"
- Check App Service Authentication is enabled
- Verify `X-MS-CLIENT-PRINCIPAL-NAME` header is being sent
- Clear browser cache and try again

### Role assignment not working
- Verify Cosmos DB connection string/endpoint in app settings
- Check your App Service Managed Identity has access to Cosmos DB
- Review Application Insights logs for errors

### "Cannot find api/auth/user" error
- Ensure `app.MapControllers()` is called in Program.cs
- Verify CORS is not blocking the request

## No Additional Azure Resources Required ✅

This solution reuses your existing Cosmos DB account. The `UserRoles` container will be created automatically.

**Cost**: $0 additional (uses your existing Cosmos DB throughput)

### Step 5: Bootstrap First Admin

After deploying to Azure, manually add the first admin user to Cosmos DB:

1. **Azure Portal** → Your Cosmos DB account → **Data Explorer**
2. Select your database → **UserRoles** container
3. Click **New Item** and add:
```json
{
  "id": "admin@example.com",
  "userId": "admin@example.com",
  "Role": "Admin",
  "AssignedAt": "2026-04-25T00:00:00Z"
}
```

Replace `admin@example.com` with your actual admin email (lowercase).

### Step 6: Test

1. Navigate to your app URL
2. You'll be redirected to Azure AD login
3. Log in with your admin account
4. You should see **User Management** in the nav menu
5. Promote other users to Admin role as needed
