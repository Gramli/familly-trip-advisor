namespace familly_trip_advisor.Features.TripPlanner.Models
{
    /// <summary>
    /// Represents a restaurant or food place relevant for family trip meal planning.
    /// </summary>
    public sealed class RestaurantDto : PlaceDto
    {
        /// <summary>
        /// Geoapify category keys (e.g. catering.restaurant.italian, catering.fast_food).
        /// Useful for showing cuisine type to the family.
        /// </summary>
        public IReadOnlyCollection<string> Categories { get; init; } = [];
    }
}
