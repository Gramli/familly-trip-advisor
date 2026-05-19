using System.Text.Json.Serialization;

namespace familly_trip_advisor.Infrastructure.WeatherbitClient
{
    public sealed class CurrentWeatherDto
    {
        [JsonPropertyName("temp")]
        public double Temp { get; init; }

        [JsonPropertyName("city_name")]
        public string CityName { get; init; } = string.Empty;

        [JsonPropertyName("ob_time")]
        public DateTime ObTime { get; init; }

        [JsonPropertyName("sunset")]
        public string Sunset { get; init; } = string.Empty;

        [JsonPropertyName("sunrise")]
        public string Sunrise { get; init; } = string.Empty;
    }

    public sealed class CurrentWeatherDataDto
    {
        [JsonPropertyName("data")]
        public IReadOnlyCollection<CurrentWeatherDto> Data { get; init; } = new List<CurrentWeatherDto>();
    }

    public sealed class ForecastWeatherDto
    {
        /// <summary>
        /// Average total cloud coverage (%)
        /// </summary>
        [JsonPropertyName("clouds")]
        public double CloudsPercentage { get; init;  }
        /// <summary>
        /// Wind speed (Default m/s)
        /// </summary>
        [JsonPropertyName("wind_spd")]
        public double WindSpeed { get; init; }
        /// <summary>
        /// Minimum Temperature - Calculated from Midnight to Midnight local time (default Celsius)
        /// </summary>
        [JsonPropertyName("min_temp")]
        public double MinTemp { get; init; }
        /// <summary>
        /// Maximum Temperature - Calculated from Midnight to Midnight local time (default Celsius)
        /// </summary>
        [JsonPropertyName("max_temp")]
        public double MaxTemp { get; init; }
        /// <summary>
        /// Average Temperature (default Celsius)
        /// </summary>
        [JsonPropertyName("temp")]  
        public double Temp { get; init; }
        /// <summary>
        /// Local date the forecast is valid for in format YYYY-MM-DD
        /// </summary>
        [JsonPropertyName("valid_date")]
        public DateTime ValidDate { get; init; }
    }

    public sealed class ForecastWeatherDataDto
    {
        [JsonPropertyName("data")]
        public IReadOnlyCollection<ForecastWeatherDto> Data { get; init; } = new List<ForecastWeatherDto>();

        /// <summary>
        /// Nearest city name
        /// </summary>
        [JsonPropertyName("city_name")]
        public string CityName { get; init; } = string.Empty;
    }
}
