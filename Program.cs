using FootballQuestions.Ui.Components;
using FootballQuestions.Ui.Services;
using Microsoft.Net.Http.Headers;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

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

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<IFootballQuestionService, FootballQuestionService>();
builder.Services.AddScoped<IEmailSubscriptionService, EmailSubscriptionService>();
builder.Services.AddScoped<IQuizAnswerService, QuizAnswerService>();
builder.Services.AddScoped<IQuizReportService, QuizReportService>();

builder.Services.AddAzureAppConfiguration();

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

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options => 
    {
        builder.Configuration.Bind("AzureAd", options);
        
        // Map the correct claim to the Identity.Name property
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name" 
        };
    });

builder.Services.AddAuthorization(options =>
{
    // This forces every single endpoint and page to require a login by default
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAzureAppConfiguration();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

app.Run();