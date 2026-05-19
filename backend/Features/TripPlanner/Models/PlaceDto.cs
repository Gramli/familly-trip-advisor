namespace familly_trip_advisor.Features.TripPlanner.Models
{
    /// <summary>
    /// Base class for all trip place DTOs — holds properties common to every place type.
    /// </summary>
    public abstract class PlaceDto
    {
        /// <summary>Display name of the place.</summary>
        public string? Name { get; init; }

        /// <summary>Full formatted address.</summary>
        public string? Address { get; init; }

        /// <summary>Distance in meters from the trip location.</summary>
        public double? DistanceMeters { get; init; }

        public double Latitude { get; init; }
        public double Longitude { get; init; }

        /// <summary>Geoapify place ID for fetching additional details.</summary>
        public string? PlaceId { get; init; }
    }
}
