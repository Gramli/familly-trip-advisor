using Ardalis.GuardClauses;
using familly_trip_advisor.Features.TripPlanner.Models;
using familly_trip_advisor.Features.TripPlanner.Places;
using familly_trip_advisor.Features.TripPlanner.Planning;
using familly_trip_advisor.Features.TripPlanner.Weather;
using familly_trip_advisor.Shared;
using FluentResults;
using Microsoft.Extensions.Options;
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
        private readonly IOptions<TripPlannerOptions> _tripPlannerOptions;
        private readonly ILogger<GenerateTripHandler> _logger;

        public GenerateTripHandler(
            IGetTripPlanQueryValidator getTripPlanQueryValidator, 
            IPlanningService planningService, 
            IWeatherService weatherService,
            IPlacesService placesService,
            IOptions<TripPlannerOptions> tripPlannerOptions,
            ILogger<GenerateTripHandler> logger)
        {
            _getTripPlanQueryValidator = Guard.Against.Null(getTripPlanQueryValidator);
            _planningService = Guard.Against.Null(planningService);
            _weatherService = Guard.Against.Null(weatherService);
            _placesService = Guard.Against.Null(placesService);
            _tripPlannerOptions = Guard.Against.Null(tripPlannerOptions);
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

            var intention = intentionResult.Value;
            var coordinatesResult = await ResolveCoordinatesAsync(intention, cancellationToken);
            if (coordinatesResult.IsFailed)
            {
                _logger.LogError("Failed to resolve coordinates for '{Destination}': {Errors}", intention.Destination, coordinatesResult.ToErrorString());
                return HttpDataResponses.AsInternalServerError<TripPlanDto>(coordinatesResult.ToErrorString());
            }

            var (lat, lon) = (coordinatesResult.Value.Latitude, coordinatesResult.Value.Longitude);

            var weatherResult = await _weatherService.GetWeatherForecastAsync(lat, lon, intention.Date, cancellationToken);

            if (weatherResult.IsFailed)
            {
                _logger.LogError("Failed to get weather forecast: {Errors}", weatherResult.ToErrorString());
                return HttpDataResponses.AsInternalServerError<TripPlanDto>(weatherResult.ToErrorString());
            }

            var activity = intention.PreferredActivity
                ?? ActivityTypeResolver.Resolve(weatherResult.Value);

            var placesRequest = new ActivityPlacesRequest
            {
                Latitude = lat,
                Longitude = lon,
                Activity = activity,
                RadiusMeters = request.RadiusInMeters,
                Categories = intention.Categories
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
                    Intention = intention,
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

        private async Task<Result<GpsCoordinatesDto>> ResolveCoordinatesAsync(TripIntentionDto intention, CancellationToken cancellationToken)
        {
            if (intention.Destination is not null)
                return await _placesService.GetCoordinatesByCityAsync(intention.Destination, cancellationToken);

            return Result.Ok(new GpsCoordinatesDto
            {
                Latitude = _tripPlannerOptions.Value.HomeLatitude,
                Longitude = _tripPlannerOptions.Value.HomeLongitude
            });
        }
    }
}
