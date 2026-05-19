namespace familly_trip_advisor.Features.TripPlanner.Models
{
    public sealed class TripPlacesDto
    {
        public required IReadOnlyCollection<RestaurantDto> Restaurants { get; init; }
        public required IReadOnlyCollection<ActivityPlaceDto> Activities { get; init; }
        public required IReadOnlyCollection<ParkingDto> Parking { get; init; }
    }
}
