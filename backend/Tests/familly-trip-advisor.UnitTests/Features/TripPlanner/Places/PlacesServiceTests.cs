using familly_trip_advisor.Features.TripPlanner.Models;
using familly_trip_advisor.Features.TripPlanner.Places;
using familly_trip_advisor.Infrastructure.GeoapifyClient;
using familly_trip_advisor.Shared;
using FluentResults;
using NSubstitute;

namespace familly_trip_advisor.UnitTests.Features.TripPlanner.Places;

public class PlacesServiceTests
{
    private readonly IGeoapifyHttpClient _geoapifyClient = Substitute.For<IGeoapifyHttpClient>();
    private readonly PlacesService _sut;

    public PlacesServiceTests()
    {
        _sut = new PlacesService(_geoapifyClient);
    }

    [Fact]
    public async Task GetCoordinatesByCityAsync_DelegatesToGeoapifyClient()
    {
        // Arrange
        var expected = new GpsCoordinatesDto { Latitude = 45.76, Longitude = 4.83 };
        _geoapifyClient
            .GetCoordinatesByCity("Lyon", Arg.Any<CancellationToken>())
            .Returns(Result.Ok(expected));

        // Act
        var result = await _sut.GetCoordinatesByCityAsync("Lyon", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public async Task GetCoordinatesByCityAsync_WhenGeoapifyFails_ReturnsFail()
    {
        // Arrange
        _geoapifyClient
            .GetCoordinatesByCity(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail("Geocoding failed"));

        // Act
        var result = await _sut.GetCoordinatesByCityAsync("Lyon", CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task GetTripPlacesAsync_WhenActivitiesFetchFails_ReturnsFail()
    {
        // Arrange
        _geoapifyClient
            .GetEntertainmentPlaces(Arg.Any<PlacesWithCategoriesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail("Activities unavailable"));
        _geoapifyClient
            .GetParkingPlaces(Arg.Any<PlacesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TestData.EmptyPlaces));
        _geoapifyClient
            .GetRestaurantPlaces(Arg.Any<PlacesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TestData.EmptyPlaces));

        // Act
        var result = await _sut.GetTripPlacesAsync(TestData.DefaultRequest, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task GetTripPlacesAsync_WhenParkingFetchFails_ReturnsFail()
    {
        // Arrange
        _geoapifyClient
            .GetEntertainmentPlaces(Arg.Any<PlacesWithCategoriesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TestData.ActivityPlaces));
        _geoapifyClient
            .GetParkingPlaces(Arg.Any<PlacesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail("Parking unavailable"));
        _geoapifyClient
            .GetRestaurantPlaces(Arg.Any<PlacesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TestData.EmptyPlaces));

        // Act
        var result = await _sut.GetTripPlacesAsync(TestData.DefaultRequest, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task GetTripPlacesAsync_WhenRestaurantsFetchFails_ReturnsFail()
    {
        // Arrange
        _geoapifyClient
            .GetEntertainmentPlaces(Arg.Any<PlacesWithCategoriesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TestData.ActivityPlaces));
        _geoapifyClient
            .GetParkingPlaces(Arg.Any<PlacesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TestData.EmptyPlaces));
        _geoapifyClient
            .GetRestaurantPlaces(Arg.Any<PlacesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail("Restaurants unavailable"));

        // Act
        var result = await _sut.GetTripPlacesAsync(TestData.DefaultRequest, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task GetTripPlacesAsync_WhenAllSucceed_ReturnsMappedPlaces()
    {
        // Arrange
        _geoapifyClient
            .GetEntertainmentPlaces(Arg.Any<PlacesWithCategoriesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TestData.ActivityPlaces));
        _geoapifyClient
            .GetParkingPlaces(Arg.Any<PlacesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TestData.ParkingPlaces));
        _geoapifyClient
            .GetRestaurantPlaces(Arg.Any<PlacesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TestData.RestaurantPlaces));

        // Act
        var result = await _sut.GetTripPlacesAsync(TestData.DefaultRequest, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Activities);
        Assert.Equal("Parc du Tête d'Or", result.Value.Activities.First().Name);
        Assert.Equal(Activity.Outdoor, result.Value.Activities.First().ActivityType);
        Assert.Single(result.Value.Restaurants);
        Assert.Equal("Café Lyon", result.Value.Restaurants.First().Name);
        Assert.Single(result.Value.Parking);
        Assert.Equal("parking.cars.underground", result.Value.Parking.First().ParkingType);
    }

    [Fact]
    public async Task GetTripPlacesAsync_FiltersActivitiesAndRestaurantsWithNullName()
    {
        // Arrange — features with null names
        var namelessPlaces = TestData.PlacesWithNullNames;

        _geoapifyClient
            .GetEntertainmentPlaces(Arg.Any<PlacesWithCategoriesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(namelessPlaces));
        _geoapifyClient
            .GetParkingPlaces(Arg.Any<PlacesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(namelessPlaces));
        _geoapifyClient
            .GetRestaurantPlaces(Arg.Any<PlacesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(namelessPlaces));

        // Act
        var result = await _sut.GetTripPlacesAsync(TestData.DefaultRequest, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Activities);
        Assert.Empty(result.Value.Restaurants);
        Assert.Single(result.Value.Parking); // parking is NOT filtered by name
    }

    [Fact]
    public async Task GetTripPlacesAsync_WhenNoRadiusProvided_UsesDefaultRadiusOf5000()
    {
        // Arrange
        var requestWithoutRadius = new ActivityPlacesRequest
        {
            Activity = Activity.Outdoor,
            Latitude  = 45.76,
            Longitude = 4.83,
            RadiusMeters = null
        };
        _geoapifyClient
            .GetEntertainmentPlaces(Arg.Any<PlacesWithCategoriesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TestData.ActivityPlaces));
        _geoapifyClient
            .GetParkingPlaces(Arg.Any<PlacesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TestData.EmptyPlaces));
        _geoapifyClient
            .GetRestaurantPlaces(Arg.Any<PlacesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(TestData.EmptyPlaces));

        // Act
        await _sut.GetTripPlacesAsync(requestWithoutRadius, CancellationToken.None);

        // Assert
        await _geoapifyClient.Received(1)
            .GetParkingPlaces(
                Arg.Is<PlacesRequest>(r => r.RadiusMeters == 5000),
                Arg.Any<CancellationToken>());
    }
}

file static class TestData
{
    public static readonly ActivityPlacesRequest DefaultRequest = new()
    {
        Activity = Activity.Outdoor,
        Latitude  = 45.76,
        Longitude = 4.83,
        RadiusMeters = 1000
    };

    public static readonly PlacesDataModel EmptyPlaces = new() { Features = [] };

    public static readonly PlacesDataModel ActivityPlaces = new()
    {
        Features =
        [
            new PlaceFeatureDto
            {
                Properties = new PlacePropertiesDto
                {
                    Name = "Parc du Tête d'Or",
                    Formatted = "Parc du Tête d'Or, Lyon",
                    Categories = ["leisure.park"],
                    Lat = 45.77,
                    Lon = 4.85
                }
            }
        ]
    };

    public static readonly PlacesDataModel ParkingPlaces = new()
    {
        Features =
        [
            new PlaceFeatureDto
            {
                Properties = new PlacePropertiesDto
                {
                    Name = "Parking Bellecour",
                    Categories = ["parking.cars.underground"],
                    Lat = 45.75,
                    Lon = 4.83
                }
            }
        ]
    };

    public static readonly PlacesDataModel RestaurantPlaces = new()
    {
        Features =
        [
            new PlaceFeatureDto
            {
                Properties = new PlacePropertiesDto
                {
                    Name = "Café Lyon",
                    Categories = ["catering.restaurant"],
                    Lat = 45.76,
                    Lon = 4.83
                }
            }
        ]
    };

    // Features with null names — activities and restaurants should filter these out; parking should not
    public static readonly PlacesDataModel PlacesWithNullNames = new()
    {
        Features =
        [
            new PlaceFeatureDto
            {
                Properties = new PlacePropertiesDto
                {
                    Name = null,
                    Categories = ["parking.cars.underground"]
                }
            }
        ]
    };
}
