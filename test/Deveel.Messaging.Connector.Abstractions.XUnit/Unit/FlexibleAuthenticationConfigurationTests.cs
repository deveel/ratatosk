namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Feature", "AuthenticationConfiguration")]
public class FlexibleAuthenticationConfigurationTests
{
    [Fact]
    public void Should_CreateWithType()
    {
        var config = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key");
        Assert.Equal(AuthenticationScheme.ApiKey, config.Scheme);
        Assert.Equal("API Key", config.DisplayName);
    }

    [Fact]
    public void Should_AddField_Required()
    {
        var config = new AuthenticationConfiguration(AuthenticationScheme.Bearer, "Token");
        config.WithField("Token", DataType.String, field => field.IsSensitive = true);

        Assert.Single(config.Fields);
        Assert.Equal("Token", config.Fields[0].FieldName);
        Assert.True(config.Fields[0].IsSensitive);
    }

    [Fact]
    public void Should_AddField_Optional()
    {
        var config = new AuthenticationConfiguration(AuthenticationScheme.Basic, "Basic");
        config.WithField("Username", DataType.String);

        Assert.Single(config.Fields);
    }

    [Fact]
    public void Should_AddField_Chained()
    {
        var config = new AuthenticationConfiguration(AuthenticationScheme.Custom, "Custom")
            .WithField("Key", DataType.String)
            .WithField("Region", DataType.String);

        Assert.Equal(2, config.Fields.Count);
    }

    [Fact]
    public void Should_RequireSettings_When_FieldIsRequired()
    {
        var config = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key")
            .WithField("ApiKey", DataType.String);

        var complete = new ConnectionSettings().SetParameter("ApiKey", "value");
        var missing = new ConnectionSettings();

        Assert.True(config.Fields.All(f => complete.GetParameter(f.FieldName) != null));
        Assert.False(config.Fields.All(f => missing.GetParameter(f.FieldName) != null));
    }

    [Fact]
    public void Should_BeSatisfied_WhenOptionalFieldHasValue()
    {
        var config = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key")
            .WithField("Region", DataType.String);

        var settings = new ConnectionSettings().SetParameter("Region", "us-east");
        Assert.True(config.Fields.All(f => settings.GetParameter(f.FieldName) != null));
    }

    [Fact]
    public void Should_NotBeSatisfied_WhenNoOptionalFieldProvided()
    {
        var config = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key")
            .WithField("Region", DataType.String);

        var empty = new ConnectionSettings();
        Assert.False(config.Fields.All(f => empty.GetParameter(f.FieldName) != null));
    }

    [Fact]
    public void Should_Satisfy_WhenAllRequiredPresent()
    {
        var config = new AuthenticationConfiguration(AuthenticationScheme.Custom, "Custom")
            .WithField("Key", DataType.String)
            .WithField("Secret", DataType.String);

        var settings = new ConnectionSettings()
            .SetParameter("Key", "abc")
            .SetParameter("Secret", "xyz");

        Assert.True(config.Fields.All(f => settings.GetParameter(f.FieldName) != null));
    }

    [Fact]
    public void Should_NotSatisfy_WhenRequiredMissing()
    {
        var config = new AuthenticationConfiguration(AuthenticationScheme.Custom, "Custom")
            .WithField("Key", DataType.String)
            .WithField("Secret", DataType.String);

        var settings = new ConnectionSettings().SetParameter("Key", "abc");

        Assert.False(config.Fields.All(f => settings.GetParameter(f.FieldName) != null));
    }
}
