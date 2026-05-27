using familly_trip_advisor.Features.TripPlanner.Models;
using familly_trip_advisor.Features.TripPlanner.Planning;
using familly_trip_advisor.Features.TripPlanner.Planning.Prompts;
using familly_trip_advisor.Infrastructure.OllamaClient;
using familly_trip_advisor.Infrastructure.WeatherbitClient;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace familly_trip_advisor.UnitTests.Features.TripPlanner.Planning;

public class PlanningServiceTests
{
    private readonly IOllamaClient _ollamaClient = Substitute.For<IOllamaClient>();
    private readonly IIntentionPromptBuilder _intentionPromptBuilder = Substitute.For<IIntentionPromptBuilder>();
    private readonly ITripPlanPromptBuilder _tripPlanPromptBuilder = Substitute.For<ITripPlanPromptBuilder>();
    private readonly PlanningService _sut;

    public PlanningServiceTests()
    {
        _sut = new PlanningService(_ollamaClient, _intentionPromptBuilder, _tripPlanPromptBuilder);
    }

    [Fact]
    public async Task ExtractIntentionAsync_WhenOllamaReturnsValidJson_ReturnsDeserializedIntention()
    {
        // Arrange
        _intentionPromptBuilder
            .BuildIntentionPrompt(Arg.Any<string>())
            .Returns("mocked-intention-prompt");

        _ollamaClient
            .GetResponseAsync("mocked-intention-prompt", Arg.Any<CancellationToken>())
            .Returns("""{"date":"2026-06-01","destination":"Lyon"}""");

        // Act
        var result = await _sut.ExtractIntentionAsync("Plan a trip to Lyon", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Lyon", result.Value.Destination);
        Assert.Equal(new DateOnly(2026, 6, 1), result.Value.Date);
    }

    [Fact]
    public async Task ExtractIntentionAsync_WhenOllamaThrowsUnhandledException_ReturnsFail()
    {
        // Arrange — InvalidOperationException bypasses Polly's retry predicate
        _intentionPromptBuilder
            .BuildIntentionPrompt(Arg.Any<string>())
            .Returns("mocked-intention-prompt");

        _ollamaClient
            .GetResponseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("LLM unavailable"));

        // Act
        var result = await _sut.ExtractIntentionAsync("Plan a trip", CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("LLM unavailable", result.Errors[0].Message);
    }

    [Fact]
    public async Task GenerateTripPlanAsync_WhenOllamaReturnsValidPlanJson_ReturnsMappedPlan()
    {
        // Arrange
        _tripPlanPromptBuilder
            .BuildTripPlanPrompt(Arg.Any<BuildTripPlanRequest>())
            .Returns("mocked-plan-prompt");

        _ollamaClient
            .GetPreservedResponseAsync("session-42", "mocked-plan-prompt", Arg.Any<CancellationToken>())
            .Returns("""{"activities":["A1"],"restaurants":["R1"],"parking":["P1"],"summary":"A lovely day in Lyon!"}""");

        // Act
        var result = await _sut.GenerateTripPlanAsync(TestData.DefaultCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("session-42", result.Value.SessionId);
        Assert.Equal("Lyon", result.Value.Destination);
        Assert.Equal("A lovely day in Lyon!", result.Value.PlanSummary);
        Assert.Equal(TestData.ActivityPlace, result.Value.SuggestedActivities.First());
        Assert.Equal(TestData.Restaurant, result.Value.SuggestedRestaurants.First());
        Assert.Equal(TestData.Parking, result.Value.SuggestedParking.First());
    }

    [Fact]
    public async Task GenerateTripPlanAsync_WhenOllamaThrowsUnhandledException_ReturnsFail()
    {
        // Arrange — InvalidOperationException bypasses Polly's retry predicate
        _tripPlanPromptBuilder
            .BuildTripPlanPrompt(Arg.Any<BuildTripPlanRequest>())
            .Returns("mocked-plan-prompt");

        _ollamaClient
            .GetPreservedResponseAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("LLM unavailable"));

        // Act
        var result = await _sut.GenerateTripPlanAsync(TestData.DefaultCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("LLM unavailable", result.Errors[0].Message);
    }
}

file static class TestData
{
    public static readonly ActivityPlaceDto ActivityPlace = new()
    {
        Name = "Parc du Tête d'Or",
        ActivityType = Activity.Outdoor
    };

    public static readonly RestaurantDto Restaurant = new()
    {
        Name = "Café Lyon"
    };

    public static readonly ParkingDto Parking = new()
    {
        Name = "Parking Bellecour"
    };

    public static readonly GenerateTripPlanCommand DefaultCommand = new()
    {
        Intention = new TripIntentionDto
        {
            Date = new DateOnly(2026, 6, 1),
            Destination = "Lyon"
        },
        Weather = new ForecastWeatherDto { ValidDate = new DateTime(2026, 6, 1) },
        ActivityType = Activity.Outdoor,
        SessionId = "session-42",
        Places = new TripPlacesDto
        {
            Activities  = [ActivityPlace],
            Restaurants = [Restaurant],
            Parking     = [Parking]
        }
    };
}
