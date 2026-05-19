using familly_trip_advisor.Features.TripPlanner;
using familly_trip_advisor.Features.TripPlanner.Models;
using familly_trip_advisor.Infrastructure.GeoapifyClient;
using familly_trip_advisor.Infrastructure.OllamaClient;
using familly_trip_advisor.Infrastructure.WeatherbitClient;
using SmallApiToolkit.Extensions;
using SmallApiToolkit.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddHttpClient();

builder.Services
    .AddOllama(builder.Configuration.GetSection(OllamaOptions.Ollama))
    .AddWeatherbit(builder.Configuration.GetSection(WeatherbitOptions.Weatherbit))
    .AddGeoapify(builder.Configuration.GetSection(GeoapifyOptions.Geoapify));

builder.Services.AddTripAdvisorServices(builder.Configuration.GetSection(TripPlannerOptions.TripPlanner));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRequestTimeouts(options =>
    options.DefaultPolicy = new Microsoft.AspNetCore.Http.Timeouts.RequestTimeoutPolicy
    {
        Timeout = TimeSpan.FromSeconds(300)
    });

var corsPolicyName = builder.Services.AddCorsByConfiguration(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(corsPolicyName);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseRequestTimeouts();

app
    .MapGroup("trip")
    .MapVersionGroup(1)
    .BuildTripAdvisorEndpoints();

app.Run();
