using System.Text.Json.Serialization;

namespace familly_trip_advisor.Infrastructure.GeoapifyClient
{
    public class PlacesRequest
    {
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public int RadiusMeters { get; init; }
        public int Limit { get; init; } = 10;
    }

    public sealed class PlacesWithCategoriesRequest : PlacesRequest
    {
        public IReadOnlyCollection<string> Categories { get; set; } = [];
    }

    public sealed class PlacesDataModel
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("features")]
        public List<PlaceFeatureDto> Features { get; set; } = [];
    }

    public sealed class PlaceFeatureDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("properties")]
        public PlacePropertiesDto Properties { get; set; } = new();

        [JsonPropertyName("geometry")]
        public PlaceGeometryDto Geometry { get; set; } = new();
    }

    public sealed class PlacePropertiesDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("street")]
        public string? Street { get; set; }

        [JsonPropertyName("housenumber")]
        public string? HouseNumber { get; set; }

        [JsonPropertyName("postcode")]
        public string? Postcode { get; set; }

        [JsonPropertyName("formatted")]
        public string? Formatted { get; set; }

        [JsonPropertyName("address_line1")]
        public string? AddressLine1 { get; set; }

        [JsonPropertyName("address_line2")]
        public string? AddressLine2 { get; set; }

        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; } = [];

        [JsonPropertyName("distance")]
        public double? Distance { get; set; }

        [JsonPropertyName("place_id")]
        public string? PlaceId { get; set; }

        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lon")]
        public double Lon { get; set; }
    }

    public sealed class PlaceGeometryDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("coordinates")]
        public double[] Coordinates { get; set; } = [];
    }
}
