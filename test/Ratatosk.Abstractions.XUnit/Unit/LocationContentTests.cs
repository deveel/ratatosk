namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Feature", "LocationContent")]
public class LocationContentTests
{
    [Fact]
    public void Should_CreateWithCoordinates()
    {
        var loc = new LocationContent(45.5, 9.2);
        Assert.Equal(45.5, loc.Latitude);
        Assert.Equal(9.2, loc.Longitude);
        Assert.Equal(MessageContentType.Location, loc.ContentType);
    }

    [Fact]
    public void Should_CreateWithOptionalProperties()
    {
        var loc = new LocationContent(45.5, 9.2)
        {
            HorizontalAccuracy = 10.0,
            Heading = 90,
            LivePeriod = 60,
            ProximityAlertRadius = 100
        };
        Assert.Equal(10.0, loc.HorizontalAccuracy);
        Assert.Equal(90, loc.Heading);
        Assert.Equal(60, loc.LivePeriod);
        Assert.Equal(100, loc.ProximityAlertRadius);
    }

    [Fact]
    public void Should_CreateFromInterface()
    {
        var source = new LocationContent(10.0, 20.0) { Heading = 45 };
        var copy = new LocationContent((ILocationContent)source);
        Assert.Equal(10.0, copy.Latitude);
        Assert.Equal(20.0, copy.Longitude);
        Assert.Equal(45, copy.Heading);
    }

    [Fact]
    public void Should_Throw_When_LatitudeOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LocationContent(100, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new LocationContent(-100, 0));
    }

    [Fact]
    public void Should_Throw_When_LongitudeOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LocationContent(0, 200));
        Assert.Throws<ArgumentOutOfRangeException>(() => new LocationContent(0, -200));
    }

    [Fact]
    public void Should_Accept_BoundaryValues()
    {
        var latMax = new LocationContent(90, 0);
        Assert.Equal(90, latMax.Latitude);

        var latMin = new LocationContent(-90, 0);
        Assert.Equal(-90, latMin.Latitude);

        var lonMax = new LocationContent(0, 180);
        Assert.Equal(180, lonMax.Longitude);

        var lonMin = new LocationContent(0, -180);
        Assert.Equal(-180, lonMin.Longitude);
    }

    [Fact]
    public void Should_Throw_When_InterfaceContentIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new LocationContent((ILocationContent)null!));
    }

    [Fact]
    public void Should_UseBuilderMethods()
    {
        var loc = new LocationContent(45.0, 9.0)
            .WithHorizontalAccuracy(10.0)
            .WithLivePeriod(60)
            .WithHeading(90)
            .WithProximityAlertRadius(500);

        Assert.Equal(10.0, loc.HorizontalAccuracy);
        Assert.Equal(60, loc.LivePeriod);
        Assert.Equal(90, loc.Heading);
        Assert.Equal(500, loc.ProximityAlertRadius);
    }

    [Fact]
    public void Should_Throw_When_AccuracyNegative()
    {
        var loc = new LocationContent(0, 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => loc.WithHorizontalAccuracy(-1));
    }

    [Fact]
    public void Should_Throw_When_LivePeriodOutOfRange()
    {
        var loc = new LocationContent(0, 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => loc.WithLivePeriod(30));
        Assert.Throws<ArgumentOutOfRangeException>(() => loc.WithLivePeriod(90000));
    }

    [Fact]
    public void Should_Throw_When_HeadingOutOfRange()
    {
        var loc = new LocationContent(0, 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => loc.WithHeading(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => loc.WithHeading(361));
    }

    [Fact]
    public void Should_Throw_When_RadiusOutOfRange()
    {
        var loc = new LocationContent(0, 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => loc.WithProximityAlertRadius(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => loc.WithProximityAlertRadius(100001));
    }

    [Fact]
    public void Should_ToString_Format()
    {
        var loc = new LocationContent(45.5, 9.2);
        var str = loc.ToString();
        Assert.Contains("45.5", str);
        Assert.Contains("9.2", str);
    }

    [Fact]
    public void Should_Equals_ByCoordinates()
    {
        var a = new LocationContent(45.0, 9.0);
        var b = new LocationContent(45.0, 9.0);
        Assert.Equal(a, b);
    }

    [Fact]
    public void Should_NotEqual_DifferentCoordinates()
    {
        var a = new LocationContent(45.0, 9.0);
        var b = new LocationContent(46.0, 9.0);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Should_NotEqual_Null()
    {
        var loc = new LocationContent(0, 0);
        Assert.False(loc.Equals(null));
    }

    [Fact]
    public void Should_NotEqual_DifferentType()
    {
        var loc = new LocationContent(0, 0);
        Assert.False(loc.Equals("not-a-location"));
    }

    [Fact]
    public void Should_GetHashCode_Consistent()
    {
        var a = new LocationContent(45.0, 9.0);
        var b = new LocationContent(45.0, 9.0);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Should_NotEqual_ByAccuracy()
    {
        var a = new LocationContent(0, 0) { HorizontalAccuracy = 5.0 };
        var b = new LocationContent(0, 0);
        Assert.NotEqual(a, b);
    }
}
