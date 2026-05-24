using familly_trip_advisor.Features.TripPlanner.Models;
using familly_trip_advisor.Infrastructure.WeatherbitClient;

namespace familly_trip_advisor.Features.TripPlanner.Planning
{
    internal static class ActivityTypeResolver
    {
        public static Activity Resolve(ForecastWeatherDto forecastWeather)
        {
            var temp = forecastWeather.Temp;
            var clouds = forecastWeather.CloudsPercentage;
            var wind = forecastWeather.WindSpeed;

            if (temp >= 15 && clouds <= 40 && wind <= 10)
                return Activity.Outdoor;

            if (temp < 15 || clouds > 60 || wind > 10)
                return Activity.Indoor;

            return Activity.Both;
        }
    }
}
