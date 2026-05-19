namespace familly_trip_advisor.Features.TripPlanner.Models
{
    public sealed class TripPlanDto
    {
        public required string SessionId { get; init; }
        public string? Destination { get; init; }
        public DateOnly Date { get; init; }
        public string? ActivityType { get; init; }
        public IReadOnlyCollection<ActivityPlaceDto> SuggestedActivities { get; init; } = [];
        public IReadOnlyCollection<RestaurantDto> SuggestedRestaurants { get; init; } = [];
        public IReadOnlyCollection<ParkingDto> SuggestedParking { get; init; } = [];
        public string? PlanSummary { get; init; }
    }
}
