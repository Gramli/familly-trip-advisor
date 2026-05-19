using Ardalis.GuardClauses;
using familly_trip_advisor.Infrastructure.WeatherbitClient;
using FluentResults;

namespace familly_trip_advisor.Features.TripPlanner.Weather
{
    public interface IWeatherService
    {
        Task<Result<ForecastWeatherDto>> GetWeatherForecastAsync(double latitude, double longitude, DateOnly date, CancellationToken cancellationToken);
    }
    internal sealed class WeatherService : IWeatherService
    {
        private readonly IWeatherbitHttpClient _weatherbitHttpClient;
        public WeatherService(IWeatherbitHttpClient weatherbitHttpClient) 
        { 
            _weatherbitHttpClient = Guard.Against.Null(weatherbitHttpClient);
        }

        public async Task<Result<ForecastWeatherDto>> GetWeatherForecastAsync(double latitude, double longitude, DateOnly date, CancellationToken cancellationToken)
        {
            var weatherResult = await _weatherbitHttpClient.GetSixteenDayForecast(latitude, longitude, cancellationToken);

            if (weatherResult.IsFailed)
            {
                return Result.Fail(weatherResult.Errors);
            }

            return weatherResult.Value.Data.FirstOrDefault(w => DateOnly.FromDateTime(w.ValidDate) == date)
                is ForecastWeatherDto forecastForDate
                ? Result.Ok(forecastForDate)
                : Result.Fail($"No weather data available for the specified date: {date}");
        }
    }
}
