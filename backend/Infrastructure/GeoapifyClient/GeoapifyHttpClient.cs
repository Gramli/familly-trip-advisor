using Ardalis.GuardClauses;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System.Text.Json;

namespace familly_trip_advisor.Infrastructure.GeoapifyClient
{
    public interface IGeoapifyHttpClient
    {
        Task<Result<PlacesDataModel>> GetFoodAndDrinkPlaces(PlacesRequest placesRequest, CancellationToken cancellationToken);
        Task<Result<PlacesDataModel>> GetRestaurantPlaces(PlacesRequest placesRequest, CancellationToken cancellationToken);
        Task<Result<PlacesDataModel>> GetEntertainmentPlaces(PlacesWithCategoriesRequest placesRequest, CancellationToken cancellationToken);
        Task<Result<PlacesDataModel>> GetParkingPlaces(PlacesRequest placesRequest, CancellationToken cancellationToken);
    }
    internal sealed class GeoapifyHttpClient : IGeoapifyHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<GeoapifyOptions> _options;
        private readonly ILogger<GeoapifyHttpClient> _logger;
        private readonly ResiliencePipeline _retryPipeline;

        public GeoapifyHttpClient(IOptions<GeoapifyOptions> options, IHttpClientFactory httpClientFactory, ILogger<GeoapifyHttpClient> logger)
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
                    ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                    OnRetry = args =>
                    {
                        _logger.LogWarning(
                            "Geoapify retry attempt {Attempt} of 3 after {Delay:s\\.ff}s. Reason: {Reason}",
                            args.AttemptNumber + 1,
                            args.RetryDelay,
                            args.Outcome.Exception!.Message);
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();
        }

        public Task<Result<PlacesDataModel>> GetFoodAndDrinkPlaces(PlacesRequest placesRequest, CancellationToken cancellationToken)
        {
            const string categories = "commercial.food_and_drink,catering";
            return GetPlacesByRadius(categories, placesRequest.Latitude, placesRequest.Longitude, placesRequest.RadiusMeters, placesRequest.Limit, cancellationToken);
        }

        public Task<Result<PlacesDataModel>> GetRestaurantPlaces(PlacesRequest placesRequest, CancellationToken cancellationToken)
        {
            const string categories = "catering.restaurant,catering.fast_food,catering.cafe";
            return GetPlacesByRadius(categories, placesRequest.Latitude, placesRequest.Longitude, placesRequest.RadiusMeters, placesRequest.Limit, cancellationToken);
        }

        public Task<Result<PlacesDataModel>> GetEntertainmentPlaces(PlacesWithCategoriesRequest placesRequest, CancellationToken cancellationToken)
        {
            var categories = string.Join(',', placesRequest.Categories);
            return GetPlacesByRadius(categories, placesRequest.Latitude, placesRequest.Longitude, placesRequest.RadiusMeters, placesRequest.Limit, cancellationToken);
        }

        public Task<Result<PlacesDataModel>> GetParkingPlaces(PlacesRequest placesRequest, CancellationToken cancellationToken)
        {
            const string categories = "parking.cars";
            return GetPlacesByRadius(categories, placesRequest.Latitude, placesRequest.Longitude, placesRequest.RadiusMeters, placesRequest.Limit, cancellationToken);
        }

        private Task<Result<PlacesDataModel>> GetPlacesByRadius(string categories, double latitude, double longitude, int radiusMeters, int limit, CancellationToken cancellationToken)
        {
            var url = $"{_options.Value.BaseUrl}/v2/places" +
                      $"?categories={categories}" +
                      $"&filter=circle:{longitude},{latitude},{radiusMeters}" +
                      $"&bias=proximity:{longitude},{latitude}" +
                      $"&limit={limit}" +
                      $"&apiKey={_options.Value.ApiKey}";

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };

            return SendAsync<PlacesDataModel>(request, cancellationToken);
        }

        private async Task<Result<T>> SendAsync<T>(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            try
            {
                return await _retryPipeline.ExecuteAsync(async ct =>
                {
                    using var response = await _httpClient.SendAsync(requestMessage, ct);

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
                _logger.LogError(ex, "Geoapify request failed after all retry attempts.");
                return Result.Fail(ex.Message);
            }
        }
    }
}

