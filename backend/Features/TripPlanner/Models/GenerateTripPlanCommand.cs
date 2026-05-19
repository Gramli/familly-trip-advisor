using familly_trip_advisor.Infrastructure.WeatherbitClient;

namespace familly_trip_advisor.Features.TripPlanner.Models
{
    public class GenerateTripPlanCommand
    {
        public required TripIntentionDto Intention { get; init; }
        public required ForecastWeatherDto Weather { get; init; }
        public required Activity ActivityType { get; init; }
        public required TripPlacesDto Places { get; init; }
        public string? SessionId { get; init; }
    }
}
