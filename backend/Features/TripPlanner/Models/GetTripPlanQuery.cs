namespace familly_trip_advisor.Features.TripPlanner.Models
{
    public class CreateTripPlanCommand
    {
        public string? SessionId { get; init; }
        public required string Prompt { get; init; }
        public int? RadiusInMeters { get; init; }
    }
}
