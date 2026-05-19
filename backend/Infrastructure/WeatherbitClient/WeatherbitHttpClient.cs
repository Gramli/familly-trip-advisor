using Ardalis.GuardClauses;
using FluentResults;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace familly_trip_advisor.Infrastructure.WeatherbitClient
{
    public interface IWeatherbitHttpClient
    {
        Task<Result<ForecastWeatherDataDto>> GetSixteenDayForecast(double latitude, double longitude, CancellationToken cancellationToken);
        Task<Result<CurrentWeatherDataDto>> GetCurrentWeather(double latitude, double longitude, CancellationToken cancellationToken);
    }
    internal sealed class WeatherbitHttpClient : IWeatherbitHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<WeatherbitOptions> _options;

        private const string XRapidAPIKeyHeader = "X-RapidAPI-Key";
        private const string XRapidAPIHostHeader = "X-RapidAPI-Host";
        public WeatherbitHttpClient(IOptions<WeatherbitOptions> options, 
            IHttpClientFactory httpClientFactory) 
        {
            Guard.Against.Null(httpClientFactory);
            _httpClient = httpClientFactory.CreateClient();
            _options = Guard.Against.Null(options);
        }

        public async Task<Result<ForecastWeatherDataDto>> GetSixteenDayForecast(double latitude, double longitude, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{_options.Value.BaseUrl}/forecast/daily?lon={longitude}&lat={latitude}"),
                Headers = 
                {
                    { XRapidAPIHostHeader, _options.Value.XRapidAPIHost },
                    { XRapidAPIKeyHeader, _options.Value.XRapidAPIKey },
                }
            };

            return await SendAsyncSave<ForecastWeatherDataDto>(request, cancellationToken);
        }

        public async Task<Result<CurrentWeatherDataDto>> GetCurrentWeather(double latitude, double longitude, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{_options.Value.BaseUrl}/current?lat={latitude}&lon={longitude}"),
                Headers =
                {
                    { XRapidAPIHostHeader, _options.Value.XRapidAPIHost },
                    { XRapidAPIKeyHeader, _options.Value.XRapidAPIKey },
                }
            };

            return await SendAsyncSave<CurrentWeatherDataDto>(request, cancellationToken);
        }

        private async Task<Result<T>> SendAsyncSave<T>(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            try
            {
                return await SendAsync<T>(requestMessage, cancellationToken);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        private async Task<Result<T>> SendAsync<T>(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result.Fail($"Failed response to {nameof(SendAsync)}");
            }

            var resultContent = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<T>(resultContent);
            if(result is null)
            {
                return Result.Fail($"Failed to deserialize response.");
            }

            return Result.Ok(result);
        }

    }
}