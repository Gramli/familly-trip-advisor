namespace familly_trip_advisor.Features.TripPlanner.Models
{
    /// <summary>
    /// Represents an activity place (entertainment, leisure, nature) for family trip planning.
    /// </summary>
    public sealed class ActivityPlaceDto : PlaceDto
    {
        /// <summary>
        /// Whether the activity is indoor or outdoor — derived from PlaceCategoryActivityMap.
        /// Drives filtering based on weather conditions.
        /// </summary>
        public Activity ActivityType { get; init; }

        /// <summary>
        /// Primary Geoapify category key (e.g. entertainment.museum, leisure.park).
        /// Describes the nature of the place.
        /// </summary>
        public string? Category { get; init; }
    }
}
