using FootballQuestions.Ui.Components;
using FootballQuestions.Ui.Services;
using Microsoft.Net.Http.Headers;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Azure.Cosmos;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

string endpoint = builder.Configuration.GetValue<string>("Endpoints:AppConfiguration")
    ?? throw new InvalidOperationException("The setting `Endpoints:AppConfiguration` was not found.");

builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(new Uri(endpoint), new DefaultAzureCredential())
           .Select(KeyFilter.Any, LabelFilter.Null)
           .ConfigureRefresh(refreshOptions =>
               refreshOptions.RegisterAll());
});

// Add authentication and authorization
builder.Services.AddAuthentication("AppServiceAuth")
    .AddScheme<AppServiceAuthenticationHandler.AppServiceAuthenticationOptions, AppServiceAuthenticationHandler>(
        "AppServiceAuth", options => { });

builder.Services.AddAuthorization();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<AuthenticationStateProvider, AppServiceAuthenticationStateProvider>();

// User identity service - uses existing Cosmos DB
builder.Services.AddScoped<IUserIdentityService>(serviceProvider =>
{
    var cosmosEndpoint = builder.Configuration.GetValue<string>("CosmosDbEndPoint")
        ?? throw new InvalidOperationException("The setting `CosmosDbEndPoint` was not found.");
    
    var cosmosDatabaseKey = builder.Configuration.GetValue<string>("CosmosDbKey")
        ?? throw new InvalidOperationException("The setting `CosmosDbKey` was not found.");

    var userDatabaseId = builder.Configuration.GetValue<string>("FootballQuestionsDatabaseId")
        ?? throw new InvalidOperationException("The setting `FootballQuestions` was not found.");

    var userContainerId = builder.Configuration.GetValue<string>("FootballUserRolesContainerId")
        ?? throw new InvalidOperationException("The setting `CosmosDbName` was not found.");

    var cosmosClient = new CosmosClient(cosmosEndpoint, cosmosDatabaseKey);

    var container = cosmosClient.GetContainer(userDatabaseId, userContainerId);
    
    var logger = serviceProvider.GetRequiredService<ILogger<UserIdentityService>>();

    return new UserIdentityService(container, logger);
});

builder.Services.AddScoped<IFootballQuestionService, FootballQuestionService>();
builder.Services.AddScoped<IEmailSubscriptionService, EmailSubscriptionService>();
builder.Services.AddScoped<IQuizAnswerService, QuizAnswerService>();
builder.Services.AddScoped<IQuizReportService, QuizReportService>();

builder.Services.AddAzureAppConfiguration();

builder.Services.AddControllers();

builder.Services.AddHttpClient("LunaApi", httpClient =>
{
    var endpointUri = builder.Configuration.GetValue<string>("LunaApi");

    if (string.IsNullOrWhiteSpace(endpointUri))
    {
        throw new InvalidOperationException("LunaApi configuration value is missing or empty.");
    }
    
    httpClient.BaseAddress = new Uri(endpointUri);
    httpClient.DefaultRequestHeaders.Add(
        HeaderNames.Accept, "application/json");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAzureAppConfiguration();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();