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
            var forecastLine = string.Join("\n", $"  - Date: {forecastWeather.ValidDate:yyyy-MM-dd}, Temp: {forecastWeather.MinTemp}°C–{forecastWeather.MaxTemp}°C (avg {forecastWeather.Temp}°C), Clouds: {forecastWeather.CloudsPercentage}%, Wind: {forecastWeather.WindSpeed} m/s");

            return "You are a family trip activity advisor. Based on the weather forecast below, decide whether the trip day is better suited for INDOOR or OUTDOOR activities.\n\n" +
                   "Rules:\n" +
                   "- Prefer OUTDOOR when: temperature avg >= 15°C AND cloud coverage <= 50%.\n" +
                   "- Prefer INDOOR when: temperature avg < 15°C OR cloud coverage > 50% OR wind speed > 10 m/s.\n" +
                   "- Prefer BOTH when: conditions are suitable for both indoor and outdoor activities.\n" +
                   "- When conditions are borderline, lean toward INDOOR for safety and comfort.\n" +
                   "- Return ONLY one word: either \"Indoor\" or \"Outdoor\". No explanation, no punctuation.\n\n" +
                   $"Forecast:\n{forecastLine}";
        }
    }
}
