using Ardalis.GuardClauses;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
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
        private readonly ILogger<WeatherbitHttpClient> _logger;
        private readonly ResiliencePipeline _retryPipeline;

        private const string XRapidAPIKeyHeader = "X-RapidAPI-Key";
        private const string XRapidAPIHostHeader = "X-RapidAPI-Host";

        public WeatherbitHttpClient(IOptions<WeatherbitOptions> options,
            IHttpClientFactory httpClientFactory,
            ILogger<WeatherbitHttpClient> logger)
        {
            Guard.Against.Null(httpClientFactory);
            _httpClient = httpClientFactory.CreateClient();
            _options = Guard.Against.Null(options);
            _logger = Guard.Against.Null(logger);

            _retryPipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(2),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldHandle = new PredicateBuilder()
                        .Handle<HttpRequestException>()
                        .Handle<TaskCanceledException>(ex =>
                            ex is not OperationCanceledException oce ||
                            !oce.CancellationToken.IsCancellationRequested),
                    OnRetry = args =>
                    {
                        _logger.LogWarning(
                            "Weatherbit retry attempt {Attempt} of 3 after {Delay:s\\.ff}s. Reason: {Reason}",
                            args.AttemptNumber + 1,
                            args.RetryDelay,
                            args.Outcome.Exception!.Message);
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();
        }

        public Task<Result<ForecastWeatherDataDto>> GetSixteenDayForecast(double latitude, double longitude, CancellationToken cancellationToken)
        {
            var uri = new Uri($"{_options.Value.BaseUrl}/forecast/daily?lon={longitude}&lat={latitude}");
            return SendAsync<ForecastWeatherDataDto>(uri, cancellationToken);
        }

        public Task<Result<CurrentWeatherDataDto>> GetCurrentWeather(double latitude, double longitude, CancellationToken cancellationToken)
        {
            var uri = new Uri($"{_options.Value.BaseUrl}/current?lat={latitude}&lon={longitude}");
            return SendAsync<CurrentWeatherDataDto>(uri, cancellationToken);
        }

        private async Task<Result<T>> SendAsync<T>(Uri uri, CancellationToken cancellationToken)
        {
            try
            {
                return await _retryPipeline.ExecuteAsync(async ct =>
                {
                    using var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = uri,
                        Headers =
                        {
                            { XRapidAPIHostHeader, _options.Value.XRapidAPIHost },
                            { XRapidAPIKeyHeader, _options.Value.XRapidAPIKey },
                        }
                    };

                    using var response = await _httpClient.SendAsync(request, ct);

                    if (!response.IsSuccessStatusCode)
                    {
                        return Result.Fail($"Failed response to {nameof(SendAsync)}: {response.StatusCode}");
                    }

                    var resultContent = await response.Content.ReadAsStringAsync(ct);
                    var result = JsonSerializer.Deserialize<T>(resultContent);

                    return result is null
                        ? Result.Fail("Failed to deserialize response.")
                        : Result.Ok(result);

                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Weatherbit request failed after all retry attempts.");
                return Result.Fail(ex.Message);
            }
        }
    }
}