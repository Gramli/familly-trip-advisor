using familly_trip_advisor.Features.TripPlanner.Models;
using familly_trip_advisor.Infrastructure.WeatherbitClient;
using familly_trip_advisor.Shared;

public static class TripHandlerTestData
{
    public static readonly CreateTripPlanCommand ValidCommand = new()
    {
        Prompt         = "Plan a family trip to Lyon next Saturday",
        RadiusInMeters = 5000,
        SessionId      = "session-1"
    };

    public static readonly TripIntentionDto IntentionForExternalDestination = new()
    {
        Date        = new DateOnly(2026, 6, 1),
        Destination = "Lyon"
    };

    public static readonly TripIntentionDto IntentionForHomeLocation = new()
    {
        Date = new DateOnly(2026, 6, 1)
    };

    public static readonly GpsCoordinatesDto LyonCoordinates = new()
    {
        Latitude  = 45.7640,
        Longitude = 4.8357
    };

    public static readonly ForecastWeatherDto SunnyWeather = new()
    {
        Temp             = 22,
        CloudsPercentage = 10,
        WindSpeed        = 5
    };

    public static readonly TripPlacesDto EmptyPlaces = new()
    {
        Activities  = [],
        Restaurants = [],
        Parking     = []
    };

    public static readonly TripPlanDto SamplePlan = new()
    {
        SessionId    = "session-1",
        Destination  = "Lyon",
        Date         = new DateOnly(2026, 6, 1),
        ActivityType = "Outdoor",
        PlanSummary  = "Great day in Lyon!"
    };
}
