namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Feature", "FlexibleAuthenticationConfiguration")]
public class FlexibleAuthenticationConfigurationTests
{
    [Fact]
    public void Should_CreateWithType()
    {
        var config = new FlexibleAuthenticationConfiguration(AuthenticationType.ApiKey, "API Key");
        Assert.Equal(AuthenticationType.ApiKey, config.AuthenticationType);
        Assert.Equal("API Key", config.DisplayName);
    }

    [Fact]
    public void Should_AddField_Required()
    {
        var config = new FlexibleAuthenticationConfiguration(AuthenticationType.Token, "Token");
        config.WithRequiredField("Token", DataType.String, field => field.IsSensitive = true);

        Assert.Single(config.RequiredFields);
        Assert.Equal("Token", config.RequiredFields[0].FieldName);
        Assert.True(config.RequiredFields[0].IsSensitive);
    }

    [Fact]
    public void Should_AddField_Optional()
    {
        var config = new FlexibleAuthenticationConfiguration(AuthenticationType.Basic, "Basic");
        config.WithOptionalField("Username", DataType.String);

        Assert.Empty(config.RequiredFields);
        Assert.Single(config.OptionalFields);
    }

    [Fact]
    public void Should_AddField_Chained()
    {
        var config = new FlexibleAuthenticationConfiguration(AuthenticationType.Custom, "Custom")
            .WithRequiredField("Key", DataType.String)
            .WithOptionalField("Region", DataType.String);

        Assert.Single(config.RequiredFields);
        Assert.Single(config.OptionalFields);
    }

    [Fact]
    public void Should_RequireSettings_When_FieldIsRequired()
    {
        var config = new FlexibleAuthenticationConfiguration(AuthenticationType.ApiKey, "API Key")
            .WithRequiredField("ApiKey", DataType.String);

        var complete = new ConnectionSettings().SetParameter("ApiKey", "value");
        var missing = new ConnectionSettings();

        Assert.True(config.IsSatisfiedBy(complete));
        Assert.False(config.IsSatisfiedBy(missing));
    }

    [Fact]
    public void Should_BeSatisfied_WhenOptionalFieldHasValue()
    {
        var config = new FlexibleAuthenticationConfiguration(AuthenticationType.ApiKey, "API Key")
            .WithOptionalField("Region", DataType.String);

        var settings = new ConnectionSettings().SetParameter("Region", "us-east");
        Assert.True(config.IsSatisfiedBy(settings));
    }

    [Fact]
    public void Should_NotBeSatisfied_WhenNoOptionalFieldProvided()
    {
        var config = new FlexibleAuthenticationConfiguration(AuthenticationType.ApiKey, "API Key")
            .WithOptionalField("Region", DataType.String);

        var empty = new ConnectionSettings();
        Assert.False(config.IsSatisfiedBy(empty));
    }

    [Fact]
    public void Should_Satisfy_WhenAllRequiredPresent()
    {
        var config = new FlexibleAuthenticationConfiguration(AuthenticationType.Custom, "Custom")
            .WithRequiredField("Key", DataType.String)
            .WithRequiredField("Secret", DataType.String);

        var settings = new ConnectionSettings()
            .SetParameter("Key", "abc")
            .SetParameter("Secret", "xyz");

        Assert.True(config.IsSatisfiedBy(settings));
    }

    [Fact]
    public void Should_NotSatisfy_WhenRequiredMissing()
    {
        var config = new FlexibleAuthenticationConfiguration(AuthenticationType.Custom, "Custom")
            .WithRequiredField("Key", DataType.String)
            .WithRequiredField("Secret", DataType.String);

        var settings = new ConnectionSettings().SetParameter("Key", "abc");

        Assert.False(config.IsSatisfiedBy(settings));
    }
}
