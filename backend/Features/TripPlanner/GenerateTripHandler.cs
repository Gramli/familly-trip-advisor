using Ardalis.GuardClauses;
using familly_trip_advisor.Features.TripPlanner.Models;
using familly_trip_advisor.Features.TripPlanner.Places;
using familly_trip_advisor.Features.TripPlanner.Planning;
using familly_trip_advisor.Features.TripPlanner.Weather;
using familly_trip_advisor.Shared;
using SmallApiToolkit.Core.Extensions;
using SmallApiToolkit.Core.RequestHandlers;
using SmallApiToolkit.Core.Response;

namespace familly_trip_advisor.Features.TripPlanner
{
    public class GenerateTripHandler : IHttpRequestHandler<TripPlanDto, CreateTripPlanCommand>
    {
        private readonly IGetTripPlanQueryValidator _getTripPlanQueryValidator;
        private readonly IPlanningService _planningService;
        private readonly IWeatherService _weatherService;
        private readonly IPlacesService _placesService;
        private readonly ILogger<GenerateTripHandler> _logger;

        public GenerateTripHandler(
            IGetTripPlanQueryValidator getTripPlanQueryValidator, 
            IPlanningService planningService, 
            IWeatherService weatherService,
            IPlacesService placesService,
            ILogger<GenerateTripHandler> logger)
        {
            _getTripPlanQueryValidator = Guard.Against.Null(getTripPlanQueryValidator);
            _planningService = Guard.Against.Null(planningService);
            _weatherService = Guard.Against.Null(weatherService);
            _placesService = Guard.Against.Null(placesService);
            _logger = Guard.Against.Null(logger);
        }

        public async Task<HttpDataResponse<TripPlanDto>> HandleAsync(CreateTripPlanCommand request, CancellationToken cancellationToken)
        {
            var validationResult = _getTripPlanQueryValidator.Validate(request);
            if (validationResult.IsFailed)
            {
                _logger.LogError("Validation failed for CreateTripPlanCommand: {Errors}", validationResult.ToErrorString());
                return HttpDataResponses.AsBadRequest<TripPlanDto>(validationResult.ToErrorString());
            }

            var intentionResult = await _planningService.ExtractIntentionAsync(request.Prompt, cancellationToken);

            if (intentionResult.IsFailed) 
            { 
                _logger.LogError("Failed to extract intention: {Errors}", intentionResult.ToErrorString());
                return HttpDataResponses.AsInternalServerError<TripPlanDto>(intentionResult.ToErrorString());
            }

            var weatherResult = await _weatherService.GetWeatherForecastAsync(intentionResult.Value.Latitude, intentionResult.Value.Longitude, intentionResult.Value.Date, cancellationToken);

            if (weatherResult.IsFailed)
            {
                _logger.LogError("Failed to get weather forecast: {Errors}", weatherResult.ToErrorString());
                return HttpDataResponses.AsInternalServerError<TripPlanDto>(weatherResult.ToErrorString());
            }

            var activity = intentionResult.Value.PreferredActivity
                ?? ActivityTypeResolver.Resolve(weatherResult.Value);

            var placesRequest = new ActivityPlacesRequest
            {
                Latitude = intentionResult.Value.Latitude,
                Longitude = intentionResult.Value.Longitude,
                Activity = activity,
                RadiusMeters = request.RadiusInMeters,
                Categories = intentionResult.Value.Categories
            };

            var placesResult = await _placesService.GetTripPlacesAsync(placesRequest, cancellationToken);

            if (placesResult.IsFailed)
            {
                _logger.LogError("Failed to get places for activity: {Errors}", placesResult.ToErrorString());
                return HttpDataResponses.AsInternalServerError<TripPlanDto>(placesResult.ToErrorString());
            }

            var planResult = await _planningService.GenerateTripPlanAsync(
                new GenerateTripPlanCommand
                {
                    Intention = intentionResult.Value,
                    Weather = weatherResult.Value,
                    ActivityType = activity,
                    Places = placesResult.Value,
                    SessionId = request.SessionId
                },
                cancellationToken);

            if (planResult.IsFailed)
            {
                _logger.LogError("Failed to generate trip plan: {Errors}", planResult.ToErrorString());
                return HttpDataResponses.AsInternalServerError<TripPlanDto>(planResult.ToErrorString());
            }

            return HttpDataResponses.AsOK(planResult.Value);
        }
    }
}
