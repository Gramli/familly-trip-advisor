namespace familly_trip_advisor.Features.TripPlanner.Models
{
    public class TripPlannerOptions
    {
        public const string TripPlanner = "TripPlanner";

        public double HomeLatitude { get; set; }
        public double HomeLongitude { get; set; }
        public string? HomeName { get; set; }
    }
}
