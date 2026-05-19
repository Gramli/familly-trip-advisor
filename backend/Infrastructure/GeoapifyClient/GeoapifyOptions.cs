namespace familly_trip_advisor.Infrastructure.GeoapifyClient
{
    public class GeoapifyOptions
    {
        public const string Geoapify = "Geoapify";

        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
}
