namespace familly_trip_advisor.Features.TripPlanner.Models
{
    /// <summary>
    /// Represents a parking place relevant for family trip logistics.
    /// </summary>
    public sealed class ParkingDto : PlaceDto
    {
        /// <summary>
        /// Parking sub-type (e.g. parking.cars.multistorey, parking.cars.underground).
        /// Helps families choose a covered option on rainy days.
        /// </summary>
        public string? ParkingType { get; init; }
    }
}
