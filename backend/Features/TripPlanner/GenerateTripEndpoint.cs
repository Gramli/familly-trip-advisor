using familly_trip_advisor.Features.TripPlanner.Models;
using familly_trip_advisor.Features.TripPlanner.Places;
using familly_trip_advisor.Features.TripPlanner.Planning;
using familly_trip_advisor.Features.TripPlanner.Planning.Prompts;
using familly_trip_advisor.Features.TripPlanner.Weather;
using familly_trip_advisor.Shared;
using Microsoft.AspNetCore.Mvc;
using SmallApiToolkit.Core.Extensions;
using SmallApiToolkit.Core.RequestHandlers;
using SmallApiToolkit.Extensions;

namespace familly_trip_advisor.Features.TripPlanner
{
    public static class GenerateTripEndpoint
    {
        public static IEndpointRouteBuilder BuildTripAdvisorEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapPost("/plan",
                async (CreateTripPlanCommand query, [FromServices] IHttpRequestHandler<TripPlanDto, CreateTripPlanCommand> handler, CancellationToken cancellationToken) =>
                    await handler.SendAsync(query, cancellationToken))
                        .ProducesDataResponse<TripPlanDto>()
                        .WithName("GetTripPlan")
                        .WithTags("Getters");

            return endpointRouteBuilder;
        }
        public static IServiceCollection AddTripAdvisorServices(this IServiceCollection serviceCollection, IConfigurationSection tripPlannerConfiguration)
        {
            serviceCollection.
                Configure<TripPlannerOptions>(tripPlannerConfiguration)
                .AddScoped<IHttpRequestHandler<TripPlanDto, CreateTripPlanCommand>, GenerateTripHandler>()
                .AddSingleton<IGetTripPlanQueryValidator, GetTripPlanQueryValidator>()
                .AddScoped<IPlanningService, PlanningService>()
                .AddSingleton<IIntentionPromptBuilder, IntentionPromptBuilder>()
                .AddSingleton<IActivityPromptBuilder, ActivityPromptBuilder>()
                .AddSingleton<ITripPlanPromptBuilder, TripPlanPromptBuilder>()
                .AddSingleton<IDateTimeProvider, DateTimeProvider>()
                .AddScoped<IWeatherService, WeatherService>()
                .AddScoped<IPlacesService, PlacesService>();
            return serviceCollection;
        }
    }
}
