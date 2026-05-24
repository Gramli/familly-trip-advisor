namespace familly_trip_advisor.Features.TripPlanner.Models
{
    public sealed class ActivityPlacesRequest
    {
        public Activity Activity { get; init; }
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public int? RadiusMeters { get; init; }
        public IReadOnlyList<string>? Categories { get; init; }
    }
}
