using Ardalis.GuardClauses;
using familly_trip_advisor.Features.TripPlanner.Models;
using familly_trip_advisor.Infrastructure.GeoapifyClient;
using FluentResults;

namespace familly_trip_advisor.Features.TripPlanner.Places
{
    public interface IPlacesService
    {
        Task<Result<TripPlacesDto>> GetTripPlacesAsync(ActivityPlacesRequest activityPlacesRequest, CancellationToken cancellationToken);
    }

    internal sealed class PlacesService : IPlacesService
    {
        private readonly IGeoapifyHttpClient _geoapifyHttpClient;
        private static readonly int defaultRadiusMeters = 5000;

        public PlacesService(IGeoapifyHttpClient geoapifyHttpClient)
        {
            _geoapifyHttpClient = Guard.Against.Null(geoapifyHttpClient);
        }

        public async Task<Result<TripPlacesDto>> GetTripPlacesAsync(ActivityPlacesRequest activityPlacesRequest, CancellationToken cancellationToken)
        {
            var placesRequest = new PlacesRequest
            {
                Latitude = activityPlacesRequest.Latitude,
                Longitude = activityPlacesRequest.Longitude,
                RadiusMeters = activityPlacesRequest.RadiusMeters ?? defaultRadiusMeters
            };

            var results = await Task.WhenAll(
                GetActivityPlacesAsync(activityPlacesRequest, cancellationToken),
                _geoapifyHttpClient.GetParkingPlaces(placesRequest, cancellationToken),
                _geoapifyHttpClient.GetRestaurantPlaces(placesRequest, cancellationToken)
            );

            var activitiesResult  = results[0];
            var parkingResult     = results[1];
            var restaurantsResult = results[2];

            var errors = new[] { activitiesResult, parkingResult, restaurantsResult }
                .Where(r => r.IsFailed)
                .SelectMany(r => r.Errors)
                .ToList();

            if (errors.Count > 0)
            {
                return Result.Fail(errors);
            }

            var tripPlaces = new TripPlacesDto
            {
                Activities = MapToActivityPlaces(activitiesResult.Value, activityPlacesRequest.Activity),
                Parking = MapToParkingPlaces(parkingResult.Value),
                Restaurants = MapToRestaurants(restaurantsResult.Value)
            };

            return Result.Ok(tripPlaces);
        }

        private async Task<Result<PlacesDataModel>> GetActivityPlacesAsync(ActivityPlacesRequest activityPlacesRequest, CancellationToken cancellationToken)
        {
            var placesRequest = new PlacesWithCategoriesRequest
            {
                Latitude = activityPlacesRequest.Latitude,
                Longitude = activityPlacesRequest.Longitude,
                RadiusMeters = activityPlacesRequest.RadiusMeters ?? defaultRadiusMeters,
                Categories = [.. ResolveCategories(activityPlacesRequest)],
                Limit = 15
            };

            return await _geoapifyHttpClient.GetEntertainmentPlaces(placesRequest, cancellationToken);
        }

        private static IReadOnlyList<string> ResolveCategories(ActivityPlacesRequest request)
        {
            if (request.Categories is { Count: > 0 })
            {
                var valid = request.Categories
                    .Where(PlaceCategoryActivityMap.TopLevelCategories.Contains)
                    .ToList();

                if (valid.Count > 0)
                    return valid;
            }

            return PlaceCategoryActivityMap.ApiCategories[request.Activity];
        }

        private static IReadOnlyCollection<RestaurantDto> MapToRestaurants(PlacesDataModel model) =>
            model.Features
                .Where(f => !string.IsNullOrWhiteSpace(f.Properties.Name))
                .Select(f => new RestaurantDto
                {
                    Name = f.Properties.Name!,
                    Address = f.Properties.Formatted,
                    DistanceMeters = f.Properties.Distance,
                    Latitude = f.Properties.Lat,
                    Longitude = f.Properties.Lon,
                    Categories = f.Properties.Categories,
                    PlaceId = f.Properties.PlaceId
                })
                .ToList();

        private static IReadOnlyCollection<ActivityPlaceDto> MapToActivityPlaces(PlacesDataModel model, Activity activityType) =>
            model.Features
                .Where(f => !string.IsNullOrWhiteSpace(f.Properties.Name))
                .Select(f =>
                {
                    var primaryCategory = f.Properties.Categories
                        .FirstOrDefault(c => PlaceCategoryActivityMap.Categories.ContainsKey(c));

                    return new ActivityPlaceDto
                    {
                        Name = f.Properties.Name!,
                        Address = f.Properties.Formatted,
                        DistanceMeters = f.Properties.Distance,
                        Latitude = f.Properties.Lat,
                        Longitude = f.Properties.Lon,
                        ActivityType = activityType,
                        Category = primaryCategory,
                        PlaceId = f.Properties.PlaceId
                    };
                })
                .ToList();

        private static IReadOnlyCollection<ParkingDto> MapToParkingPlaces(PlacesDataModel model) =>
            model.Features
                .Select(f =>
                {
                    var parkingType = f.Properties.Categories
                        .FirstOrDefault(c => c.StartsWith("parking.cars."))
                        ?? f.Properties.Categories.FirstOrDefault();

                    return new ParkingDto
                    {
                        Name = f.Properties.Name,
                        Address = f.Properties.Formatted,
                        DistanceMeters = f.Properties.Distance,
                        Latitude = f.Properties.Lat,
                        Longitude = f.Properties.Lon,
                        ParkingType = parkingType,
                        PlaceId = f.Properties.PlaceId
                    };
                })
                .ToList();
    }
}
