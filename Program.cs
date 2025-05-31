using FootballQuestions.Ui.Components;
using FootballQuestions.Ui.Services;
using Microsoft.Net.Http.Headers;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

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

app.Run();