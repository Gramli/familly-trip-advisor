using Ardalis.GuardClauses;
using familly_trip_advisor.Features.TripPlanner.Models;
using familly_trip_advisor.Features.TripPlanner.Places;
using familly_trip_advisor.Features.TripPlanner.Planning;
using familly_trip_advisor.Features.TripPlanner.Weather;
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
                _logger.LogError("Validation failed for CreateTripPlanCommand: {Errors}", string.Join(", ", validationResult.Errors));
                return HttpDataResponses.AsBadRequest<TripPlanDto>(string.Join(", ", validationResult.Errors));
            }

            var intentionResult = await _planningService.ExtractIntentionAsync(request.Prompt, cancellationToken);

            if (intentionResult.IsFailed) 
            { 
                _logger.LogError("Failed to extract intention: {Errors}", string.Join(", ", intentionResult.Errors));
                return HttpDataResponses.AsInternalServerError<TripPlanDto>(string.Join(", ", intentionResult.Errors));
            }

            var weatherResult = await _weatherService.GetWeatherForecastAsync(intentionResult.Value.Latitude, intentionResult.Value.Longitude, intentionResult.Value.Date, cancellationToken);

            if (weatherResult.IsFailed)
            {
                _logger.LogError("Failed to get weather forecast: {Errors}", string.Join(", ", weatherResult.Errors));
                return HttpDataResponses.AsInternalServerError<TripPlanDto>(string.Join(", ", weatherResult.Errors));
            }

            var activity = intentionResult.Value.PreferredActivity ?? Activity.Indoor;

            if (!intentionResult.Value.PreferredActivity.HasValue)
            {
                var activitiesResult = await _planningService.GetActivityByWeatherAsync(weatherResult.Value, cancellationToken);

                if (activitiesResult.IsFailed)
                {
                    _logger.LogError("Failed to determine activity by weather: {Errors}", string.Join(", ", activitiesResult.Errors));
                    return HttpDataResponses.AsInternalServerError<TripPlanDto>(string.Join(", ", activitiesResult.Errors));
                }

                activity = activitiesResult.Value;
            }

            var placesRequest = new ActivityPlacesRequest
            {
                Latitude = intentionResult.Value.Latitude,
                Longitude = intentionResult.Value.Longitude,
                Activity = activity,
                RadiusMeters = request.RadiusInMeters
            };

            var placesResult = await _placesService.GetTripPlacesAsync(placesRequest, cancellationToken);

            if (placesResult.IsFailed)
            {
                _logger.LogError("Failed to get places for activity: {Errors}", string.Join(", ", placesResult.Errors));
                return HttpDataResponses.AsInternalServerError<TripPlanDto>(string.Join(", ", placesResult.Errors));
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
                _logger.LogError("Failed to generate trip plan: {Errors}", string.Join(", ", planResult.Errors));
                return HttpDataResponses.AsInternalServerError<TripPlanDto>(string.Join(", ", planResult.Errors));
            }

            return HttpDataResponses.AsOK(planResult.Value);
        }
    }
}
