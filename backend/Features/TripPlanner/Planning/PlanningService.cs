using Ardalis.GuardClauses;
using familly_trip_advisor.Features.TripPlanner.Models;
using familly_trip_advisor.Features.TripPlanner.Planning.Prompts;
using familly_trip_advisor.Infrastructure.OllamaClient;
using familly_trip_advisor.Infrastructure.WeatherbitClient;
using familly_trip_advisor.Shared;
using FluentResults;
using Polly;
using Polly.Retry;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace familly_trip_advisor.Features.TripPlanner.Planning
{
    public interface IPlanningService
    {
        Task<Result<TripIntentionDto>> ExtractIntentionAsync(string prompt, CancellationToken cancellationToken);
        Task<Result<Activity>> GetActivityByWeatherAsync(ForecastWeatherDto forecastWeather, CancellationToken cancellationToken);
        Task<Result<TripPlanDto>> GenerateTripPlanAsync(GenerateTripPlanCommand command, CancellationToken cancellationToken);
    }

    internal sealed class PlanningService : IPlanningService
    {
        private readonly IOllamaClient _ollamaClient;
        private readonly IIntentionPromptBuilder _intentionPromptBuilder;
        private readonly IActivityPromptBuilder _activityPromptBuilder;
        private readonly ITripPlanPromptBuilder _tripPlanPromptBuilder;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        private static class RetryPipeline<T>
        {
            public static readonly ResiliencePipeline<Result<T>> Instance = new ResiliencePipelineBuilder<Result<T>>()
                .AddRetry(new RetryStrategyOptions<Result<T>>
                {
                    MaxRetryAttempts = 2,
                    Delay = TimeSpan.FromSeconds(2),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldHandle = new PredicateBuilder<Result<T>>()
                        .Handle<HttpRequestException>()
                        .Handle<JsonException>()
                        .Handle<TaskCanceledException>(ex =>
                            ex is not OperationCanceledException oce ||
                            !oce.CancellationToken.IsCancellationRequested)
                        .HandleResult(r => r.IsFailed)
                })
                .Build();
        }

        public PlanningService(IOllamaClient ollamaClient,
            IIntentionPromptBuilder intentionPromptBuilder,
            IActivityPromptBuilder activityPromptBuilder,
            ITripPlanPromptBuilder tripPlanPromptBuilder)
        {
            _ollamaClient = Guard.Against.Null(ollamaClient);
            _intentionPromptBuilder = Guard.Against.Null(intentionPromptBuilder);
            _activityPromptBuilder = Guard.Against.Null(activityPromptBuilder);
            _tripPlanPromptBuilder = Guard.Against.Null(tripPlanPromptBuilder);
        }

        public Task<Result<Activity>> GetActivityByWeatherAsync(ForecastWeatherDto forecastWeather, CancellationToken cancellationToken)
        {
            var prompt = _activityPromptBuilder.BuildActivityPrompt(forecastWeather);

            return ExecuteWithResilienceAsync<Activity>(async ct =>
            {
                var raw = await _ollamaClient.GetResponseAsync(prompt, ct);
                var trimmed = raw.Trim();

                return Enum.TryParse<Activity>(trimmed, ignoreCase: true, out var activity)
                    ? Result.Ok(activity)
                    : Result.Fail($"Model returned an unrecognised activity value: '{trimmed}'.");

            }, "Failed to determine activity by weather", cancellationToken);
        }

        public Task<Result<TripIntentionDto>> ExtractIntentionAsync(string prompt, CancellationToken cancellationToken)
        {
            var fullPrompt = _intentionPromptBuilder.BuildIntentionPrompt(prompt);

            return ExecuteWithResilienceAsync<TripIntentionDto>(async ct =>
            {
                var raw = await _ollamaClient.GetResponseAsync(fullPrompt, ct);
                var json = JsonExtractor.ExtractJson(raw);
                var result = JsonSerializer.Deserialize<TripIntentionDto>(json, JsonOptions);

                return result is null
                    ? Result.Fail("Failed to deserialize trip intention from model response.")
                    : Result.Ok(result);

            }, "Failed to extract trip intention", cancellationToken);
        }

        public Task<Result<TripPlanDto>> GenerateTripPlanAsync(GenerateTripPlanCommand command, CancellationToken cancellationToken)
        {
            var prompt = _tripPlanPromptBuilder.BuildTripPlanPrompt(new BuildTripPlanRequest
            {
                Intention = command.Intention,
                ActivityType = command.ActivityType,
                Weather = command.Weather,
                Places = command.Places
            });
            var sessionId = command.SessionId ?? Guid.NewGuid().ToString();

            return ExecuteWithResilienceAsync<TripPlanDto>(async ct =>
            {
                var raw = await _ollamaClient.GetPreservedResponseAsync(sessionId, prompt, ct);
                var json = JsonExtractor.ExtractJson(raw);
                var node = JsonNode.Parse(json);

                if (node is null)
                    return Result.Fail("Model returned an empty plan.");

                var activityIds = ParseIds(node["activities"]);
                var restaurantIds = ParseIds(node["restaurants"]);
                var parkingIds = ParseIds(node["parking"]);
                var summary = node["summary"]?.GetValue<string>();

                var plan = new TripPlanDto
                {
                    SessionId = sessionId,
                    Destination = command.Intention.Destination,
                    Date = command.Intention.Date,
                    ActivityType = command.ActivityType.ToString(),
                    SuggestedActivities = ResolveByIds(command.Places.Activities.ToList(), activityIds, "A"),
                    SuggestedRestaurants = ResolveByIds(command.Places.Restaurants.ToList(), restaurantIds, "R"),
                    SuggestedParking = ResolveByIds(command.Places.Parking.ToList(), parkingIds, "P"),
                    PlanSummary = summary
                };

                return Result.Ok(plan);

            }, "Failed to generate trip plan", cancellationToken);
        }

        private static async Task<Result<T>> ExecuteWithResilienceAsync<T>(
            Func<CancellationToken, ValueTask<Result<T>>> action,
            string errorPrefix,
            CancellationToken cancellationToken)
        {
            try
            {
                return await RetryPipeline<T>.Instance.ExecuteAsync(action, cancellationToken);
            }
            catch (Exception ex)
            {
                return Result.Fail($"{errorPrefix}: {ex.Message}");
            }
        }

        private static List<string> ParseIds(JsonNode? node) =>
            node is JsonArray arr
                ? [.. arr.Select(n => n?.GetValue<string>() ?? string.Empty).Where(s => s.Length > 0)]
                : [];

        private static IReadOnlyCollection<T> ResolveByIds<T>(List<T> source, List<string> ids, string prefix)
        {
            var result = new List<T>();
            foreach (var id in ids)
            {
                if (id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(id[prefix.Length..], out var index)
                    && index >= 1 && index <= source.Count)
                {
                    result.Add(source[index - 1]);
                }
            }
            return result;
        }
    }
}
