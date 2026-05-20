using familly_trip_advisor.Infrastructure.WeatherbitClient;

namespace familly_trip_advisor.Features.TripPlanner.Planning.Prompts
{
    public interface IActivityPromptBuilder
    {
        string BuildActivityPrompt(ForecastWeatherDto forecastWeather);
    }

    internal sealed class ActivityPromptBuilder : IActivityPromptBuilder
    {
        public string BuildActivityPrompt(ForecastWeatherDto forecastWeather)
        {
            return $"""
                You are a family trip activity advisor. Based on the weather forecast below, decide whether the trip day is better suited for INDOOR activities, OUTDOOR activities, or BOTH.

                Rules:
                - Prefer OUTDOOR when: temperature avg >= 15°C AND cloud coverage <= 40% AND wind speed <= 10 m/s.
                - Prefer INDOOR when: temperature avg < 15°C OR cloud coverage > 60% OR wind speed > 10 m/s.
                - Prefer BOTH when: temperature avg >= 15°C AND cloud coverage between 40–60% AND wind speed <= 10 m/s.
                - Return ONLY one word: either "Indoor", "Outdoor", or "Both". No explanation, no punctuation.

                Forecast:
                  - Date: {forecastWeather.ValidDate:yyyy-MM-dd}, Temp: {forecastWeather.MinTemp}°C–{forecastWeather.MaxTemp}°C (avg {forecastWeather.Temp}°C), Clouds: {forecastWeather.CloudsPercentage}%, Wind: {forecastWeather.WindSpeed} m/s
                """;
        }
    }
}
