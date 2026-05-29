namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "AuthenticationConfiguration")]
public class AuthenticationConfigurationExtendedTests
{
    [Fact]
    public void Should_ReturnAllFieldNames()
    {
        var config = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key")
            .WithField("Field1", DataType.String, f => f.AuthenticationRole = "principal")
            .WithField("Field2", DataType.Integer, f => f.AuthenticationRole = "credential");

        var names = config.GetAllFieldNames();
        Assert.Equal(2, names.Count());
        Assert.Contains("Field1", names);
        Assert.Contains("Field2", names);
    }

    [Fact]
    public void Should_ReturnEmptyFieldNames_When_NoFields()
    {
        var config = new AuthenticationConfiguration(AuthenticationScheme.None, "None");
        var names = config.GetAllFieldNames();
        Assert.Empty(names);
    }

    [Fact]
    public void Should_BeSatisfied_When_ConnectionHasMatchingFields()
    {
        var config = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key")
            .WithField("ApiKey", DataType.String, f => f.AuthenticationRole = "principal");

        var settings = new ConnectionSettings();
        settings.SetParameter("ApiKey", "my-key");

        Assert.True(config.IsSatisfiedBy(settings));
    }

    [Fact]
    public void Should_NotBeSatisfied_When_MissingRequiredField()
    {
        var config = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key")
            .WithField("ApiKey", DataType.String, f => f.AuthenticationRole = "principal");

        var settings = new ConnectionSettings();
        Assert.False(config.IsSatisfiedBy(settings));
    }

    [Fact]
    public void Should_NotBeSatisfied_When_NullSettings()
    {
        var config = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key");
        Assert.False(config.IsSatisfiedBy(null!));
    }

    [Fact]
    public void Should_NotBeSatisfied_When_NoAuthenticationFields()
    {
        var config = new AuthenticationConfiguration(AuthenticationScheme.None, "None");
        var settings = new ConnectionSettings();
        Assert.False(config.IsSatisfiedBy(settings));
    }
}
