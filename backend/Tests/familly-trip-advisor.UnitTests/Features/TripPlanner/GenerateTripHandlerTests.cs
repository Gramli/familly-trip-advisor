using familly_trip_advisor.Features.TripPlanner;
using familly_trip_advisor.Features.TripPlanner.Models;
using familly_trip_advisor.Features.TripPlanner.Places;
using familly_trip_advisor.Features.TripPlanner.Planning;
using familly_trip_advisor.Features.TripPlanner.Weather;
using familly_trip_advisor.Infrastructure.WeatherbitClient;
using familly_trip_advisor.Shared;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Net;

namespace familly_trip_advisor.UnitTests.Features.TripPlanner;

public class GenerateTripHandlerTests
{
    private readonly IGetTripPlanQueryValidator _validator       = Substitute.For<IGetTripPlanQueryValidator>();
    private readonly IPlanningService           _planningService = Substitute.For<IPlanningService>();
    private readonly IWeatherService            _weatherService  = Substitute.For<IWeatherService>();
    private readonly IPlacesService             _placesService   = Substitute.For<IPlacesService>();
    private readonly ILogger<GenerateTripHandler> _logger        = Substitute.For<ILogger<GenerateTripHandler>>();

    private readonly TripPlannerOptions _options = new()
    {
        HomeLatitude  = 48.8566,
        HomeLongitude = 2.3522,
        HomeName      = "Paris"
    };

    private readonly GenerateTripHandler _sut;

    public GenerateTripHandlerTests()
    {
        _sut = new GenerateTripHandler(
            _validator,
            _planningService,
            _weatherService,
            _placesService,
            Options.Create(_options),
            _logger);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ReturnsBadRequest()
    {
        // Arrange
        _validator.Validate(TripHandlerTestData.ValidCommand).Returns(Result.Fail("Prompt is too short"));

        // Act
        var response = await _sut.HandleAsync(TripHandlerTestData.ValidCommand, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallDownstreamServices()
    {
        // Arrange
        _validator.Validate(TripHandlerTestData.ValidCommand).Returns(Result.Fail("Prompt is too short"));

        // Act
        await _sut.HandleAsync(TripHandlerTestData.ValidCommand, CancellationToken.None);

        // Assert
        await _planningService.DidNotReceive().ExtractIntentionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenIntentionExtractionFails_ReturnsInternalServerError()
    {
        // Arrange
        ArrangeValidCommand();
        _planningService.ExtractIntentionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail("LLM unavailable"));

        // Act
        var response = await _sut.HandleAsync(TripHandlerTestData.ValidCommand, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_WhenCoordinateResolutionFails_ReturnsInternalServerError()
    {
        // Arrange
        ArrangeValidCommand();
        _planningService.ExtractIntentionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.IntentionForExternalDestination));
        _placesService.GetCoordinatesByCityAsync("Lyon", Arg.Any<CancellationToken>())
            .Returns(Result.Fail("City not found"));

        // Act
        var response = await _sut.HandleAsync(TripHandlerTestData.ValidCommand, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_WhenDestinationIsHomeLocation_UsesConfiguredHomeCoordinates()
    {
        // Arrange
        ArrangeValidCommand();
        _planningService.ExtractIntentionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.IntentionForHomeLocation));
        _weatherService.GetWeatherForecastAsync(
                _options.HomeLatitude, _options.HomeLongitude,
                Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.SunnyWeather));
        _placesService.GetTripPlacesAsync(Arg.Any<ActivityPlacesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.EmptyPlaces));
        _planningService.GenerateTripPlanAsync(Arg.Any<GenerateTripPlanCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.SamplePlan));

        // Act
        await _sut.HandleAsync(TripHandlerTestData.ValidCommand, CancellationToken.None);

        // Assert
        await _placesService.DidNotReceive().GetCoordinatesByCityAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _weatherService.Received(1).GetWeatherForecastAsync(
            _options.HomeLatitude,
            _options.HomeLongitude,
            Arg.Any<DateOnly>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenWeatherForecastFails_ReturnsInternalServerError()
    {
        // Arrange
        ArrangeValidCommand();
        ArrangeIntentionForExternalDestination();
        _weatherService.GetWeatherForecastAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail("Weather API error"));

        // Act
        var response = await _sut.HandleAsync(TripHandlerTestData.ValidCommand, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_WhenPreferredActivityIsSet_SkipsWeatherBasedResolution()
    {
        // Arrange
        var intentionWithPreference = new TripIntentionDto
        {
            Date              = new DateOnly(2026, 6, 1),
            Destination       = "Lyon",

            PreferredActivity = Activity.Indoor
        };

        ArrangeValidCommand();
        _planningService.ExtractIntentionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(intentionWithPreference));
        _placesService.GetCoordinatesByCityAsync("Lyon", Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.LyonCoordinates));
        _weatherService.GetWeatherForecastAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.SunnyWeather));
        _placesService.GetTripPlacesAsync(Arg.Any<ActivityPlacesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.EmptyPlaces));
        _planningService.GenerateTripPlanAsync(Arg.Any<GenerateTripPlanCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.SamplePlan));

        // Act
        await _sut.HandleAsync(TripHandlerTestData.ValidCommand, CancellationToken.None);

        // Assert
        await _planningService.Received(1).GenerateTripPlanAsync(
            Arg.Is<GenerateTripPlanCommand>(c => c.ActivityType == Activity.Indoor),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenNoPreferredActivity_ResolvesActivityFromWeather()
    {
        // Arrange — SunnyWeather (temp=22, clouds=10, wind=5) resolves to Outdoor
        ArrangeValidCommand();
        ArrangeIntentionForExternalDestination();
        _weatherService.GetWeatherForecastAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.SunnyWeather));
        _placesService.GetTripPlacesAsync(Arg.Any<ActivityPlacesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.EmptyPlaces));
        _planningService.GenerateTripPlanAsync(Arg.Any<GenerateTripPlanCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.SamplePlan));

        // Act
        await _sut.HandleAsync(TripHandlerTestData.ValidCommand, CancellationToken.None);

        // Assert
        await _planningService.Received(1).GenerateTripPlanAsync(
            Arg.Is<GenerateTripPlanCommand>(c => c.ActivityType == Activity.Outdoor),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPlacesLookupFails_ReturnsInternalServerError()
    {
        // Arrange
        ArrangeValidCommand();
        ArrangeIntentionForExternalDestination();
        _weatherService.GetWeatherForecastAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.SunnyWeather));
        _placesService.GetTripPlacesAsync(Arg.Any<ActivityPlacesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail("Places API error"));

        // Act
        var response = await _sut.HandleAsync(TripHandlerTestData.ValidCommand, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_PassesRadiusAndCoordinatesToPlacesRequest()
    {
        // Arrange
        ArrangeValidCommand();
        ArrangeIntentionForExternalDestination();
        _weatherService.GetWeatherForecastAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.SunnyWeather));
        _placesService.GetTripPlacesAsync(Arg.Any<ActivityPlacesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.EmptyPlaces));
        _planningService.GenerateTripPlanAsync(Arg.Any<GenerateTripPlanCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.SamplePlan));

        // Act
        await _sut.HandleAsync(TripHandlerTestData.ValidCommand, CancellationToken.None);

        // Assert
        await _placesService.Received(1).GetTripPlacesAsync(
            Arg.Is<ActivityPlacesRequest>(r =>
                r.Latitude     == TripHandlerTestData.LyonCoordinates.Latitude  &&
                r.Longitude    == TripHandlerTestData.LyonCoordinates.Longitude &&
                r.RadiusMeters == TripHandlerTestData.ValidCommand.RadiusInMeters),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPlanGenerationFails_ReturnsInternalServerError()
    {
        // Arrange
        ArrangeValidCommand();
        ArrangeIntentionForExternalDestination();
        _weatherService.GetWeatherForecastAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.SunnyWeather));
        _placesService.GetTripPlacesAsync(Arg.Any<ActivityPlacesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.EmptyPlaces));
        _planningService.GenerateTripPlanAsync(Arg.Any<GenerateTripPlanCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail("LLM failed to produce a plan"));

        // Act
        var response = await _sut.HandleAsync(TripHandlerTestData.ValidCommand, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_WhenAllStepsSucceed_ReturnsOkWithTripPlan()
    {
        // Arrange
        ArrangeFullHappyPath(TripHandlerTestData.IntentionForExternalDestination);

        // Act
        var response = await _sut.HandleAsync(TripHandlerTestData.ValidCommand, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(TripHandlerTestData.SamplePlan, response.Data);
    }

    [Fact]
    public async Task HandleAsync_WhenHomeLocationRequested_ReturnsOkWithTripPlan()
    {
        // Arrange
        ArrangeFullHappyPath(TripHandlerTestData.IntentionForHomeLocation);

        // Act
        var response = await _sut.HandleAsync(TripHandlerTestData.ValidCommand, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(TripHandlerTestData.SamplePlan, response.Data);
    }

    private void ArrangeValidCommand() =>
        _validator.Validate(Arg.Any<CreateTripPlanCommand>()).Returns(Result.Ok());

    private void ArrangeIntentionForExternalDestination()
    {
        _planningService.ExtractIntentionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.IntentionForExternalDestination));
        _placesService.GetCoordinatesByCityAsync("Lyon", Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.LyonCoordinates));
    }

    private void ArrangeFullHappyPath(TripIntentionDto intention)
    {
        ArrangeValidCommand();
        _planningService.ExtractIntentionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(intention));

        if (intention.Destination is not null)
        {
            _placesService.GetCoordinatesByCityAsync(intention.Destination, Arg.Any<CancellationToken>())
                .Returns(Result.Ok(TripHandlerTestData.LyonCoordinates));
        }

        _weatherService.GetWeatherForecastAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.SunnyWeather));
        _placesService.GetTripPlacesAsync(Arg.Any<ActivityPlacesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.EmptyPlaces));
        _planningService.GenerateTripPlanAsync(Arg.Any<GenerateTripPlanCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TripHandlerTestData.SamplePlan));
    }
}