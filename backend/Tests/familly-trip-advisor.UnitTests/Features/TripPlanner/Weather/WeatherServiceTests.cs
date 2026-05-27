using familly_trip_advisor.Features.TripPlanner.Weather;
using familly_trip_advisor.Infrastructure.WeatherbitClient;
using FluentResults;
using NSubstitute;

namespace familly_trip_advisor.UnitTests.Features.TripPlanner.Weather;

public class WeatherServiceTests
{
    private readonly IWeatherbitHttpClient _weatherbitClient = Substitute.For<IWeatherbitHttpClient>();
    private readonly WeatherService _sut;

    public WeatherServiceTests()
    {
        _sut = new WeatherService(_weatherbitClient);
    }

    [Fact]
    public async Task GetWeatherForecastAsync_WhenApiFails_ReturnsFail()
    {
        // Arrange
        _weatherbitClient
            .GetSixteenDayForecast(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail("Upstream API error"));

        // Act
        var result = await _sut.GetWeatherForecastAsync(48.8, 2.3, new DateOnly(2026, 6, 1), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task GetWeatherForecastAsync_WhenNoDataExistsForRequestedDate_ReturnsFail()
    {
        // Arrange
        var forecastData = new ForecastWeatherDataDto
        {
            Data = [new ForecastWeatherDto { ValidDate = new DateTime(2026, 6, 2) }]
        };
        _weatherbitClient
            .GetSixteenDayForecast(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(forecastData));

        // Act — request date 2026-06-01, but only 2026-06-02 is in the response
        var result = await _sut.GetWeatherForecastAsync(48.8, 2.3, new DateOnly(2026, 6, 1), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("No weather data available", result.Errors[0].Message);
    }

    [Fact]
    public async Task GetWeatherForecastAsync_WhenForecastContainsRequestedDate_ReturnsMatchingForecast()
    {
        // Arrange
        var targetDate   = new DateOnly(2026, 6, 1);
        var targetEntry  = new ForecastWeatherDto { ValidDate = new DateTime(2026, 6, 1), Temp = 22, CloudsPercentage = 10 };
        var forecastData = new ForecastWeatherDataDto { Data = [targetEntry] };

        _weatherbitClient
            .GetSixteenDayForecast(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(forecastData));

        // Act
        var result = await _sut.GetWeatherForecastAsync(48.8, 2.3, targetDate, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(targetEntry, result.Value);
    }

    [Fact]
    public async Task GetWeatherForecastAsync_WhenMultipleDatesAvailable_ReturnsOnlyTheMatchingDate()
    {
        // Arrange
        var targetDate   = new DateOnly(2026, 6, 1);
        var targetEntry  = new ForecastWeatherDto { ValidDate = new DateTime(2026, 6, 1), Temp = 22 };
        var otherEntry   = new ForecastWeatherDto { ValidDate = new DateTime(2026, 6, 2), Temp = 10 };
        var forecastData = new ForecastWeatherDataDto { Data = [otherEntry, targetEntry] };

        _weatherbitClient
            .GetSixteenDayForecast(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(forecastData));

        // Act
        var result = await _sut.GetWeatherForecastAsync(48.8, 2.3, targetDate, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(22, result.Value.Temp);
    }

    [Fact]
    public async Task GetWeatherForecastAsync_ForwardsCoordinatesToClient()
    {
        // Arrange
        _weatherbitClient
            .GetSixteenDayForecast(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail("irrelevant"));

        // Act
        await _sut.GetWeatherForecastAsync(45.76, 4.83, new DateOnly(2026, 6, 1), CancellationToken.None);

        // Assert
        await _weatherbitClient.Received(1).GetSixteenDayForecast(45.76, 4.83, Arg.Any<CancellationToken>());
    }
}
