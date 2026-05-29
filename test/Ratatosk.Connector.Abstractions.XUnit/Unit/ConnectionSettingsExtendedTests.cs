namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "ConnectionSettings")]
public class ConnectionSettingsExtendedTests
{
    [Fact]
    public void Should_GetTypedParameter_When_Called()
    {
        var settings = new ConnectionSettings();
        settings.SetParameter("Active", true);
        var result = settings.GetParameter<bool>("Active");
        Assert.True(result);
    }

    [Fact]
    public void Should_ReturnDefault_When_ParameterNotFound()
    {
        var settings = new ConnectionSettings();
        var result = settings.GetParameter<string>("nonexistent");
        Assert.Null(result);
    }

    [Fact]
    public void Should_ConvertTo_When_TypeIsCompatible()
    {
        var settings = new ConnectionSettings();
        settings.SetParameter("count", 42);
        var result = settings.GetParameter<int>("count");
        Assert.Equal(42, result);
    }

    [Fact]
    public void Should_Throw_When_TypeIncompatible()
    {
        var settings = new ConnectionSettings();
        settings.SetParameter("value", "not-a-number");
        Assert.Throws<InvalidCastException>(() => settings.GetParameter<int>("value"));
    }
}
