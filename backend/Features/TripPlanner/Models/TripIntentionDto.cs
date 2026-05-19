using System.Text.Json.Serialization;

namespace familly_trip_advisor.Features.TripPlanner.Models
{
    public sealed class TripIntentionDto
    {
        [JsonPropertyName("date")]
        public DateOnly Date { get; init; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; init; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; init; }

        [JsonPropertyName("destination")]
        public string? Destination { get; init; }

        [JsonPropertyName("isHomeLocation")]
        public bool IsHomeLocation { get; init; }

        [JsonPropertyName("preferredActivity")]
        public Activity? PreferredActivity { get; init; }
    }
}
