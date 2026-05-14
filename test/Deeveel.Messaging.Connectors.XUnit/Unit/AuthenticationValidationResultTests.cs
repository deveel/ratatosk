namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Feature", "Authentication")]
public class AuthenticationValidationResultTests
{
    [Fact]
    public void Should_ValidateConnectionSettings_When_ValidSettings()
    {
        var schema = new ChannelSchemaBuilder("Test", "Test", "1.0.0")
            .AddAuthenticationConfiguration(new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key Authentication")
                .WithField("ApiKey", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; }))
            .Build();
        var settings = new ConnectionSettings().SetParameter("ApiKey", "valid-key");

        var results = schema.ValidateConnectionSettings(settings);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_ValidateConnectionSettings_When_MissingRequiredField()
    {
        var schema = new ChannelSchemaBuilder("Test", "Test", "1.0.0")
            .AddAuthenticationConfiguration(new AuthenticationConfiguration(AuthenticationScheme.Basic, "Basic Authentication")
                .WithField("Username", DataType.String, f => f.AuthenticationRole = "principal")
                .WithField("Password", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; }))
            .Build();
        var settings = new ConnectionSettings()
            .SetParameter("Username", "user");

        var results = schema.ValidateConnectionSettings(settings).ToList();

        Assert.NotEmpty(results);
        Assert.Contains("Connection settings do not satisfy any of the supported authentication methods. Supported methods: Basic Authentication.", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_ValidateConnectionSettings_When_MultipleMissing()
    {
        var schema = new ChannelSchemaBuilder("Test", "Test", "1.0.0")
            .AddAuthenticationConfiguration(new AuthenticationConfiguration(AuthenticationScheme.Custom, "Custom")
                .WithField(new AuthenticationField("Key1", DataType.String))
                .WithField(new AuthenticationField("Key2", DataType.String)))
            .Build();
        var settings = new ConnectionSettings();

        var results = schema.ValidateConnectionSettings(settings).ToList();

        Assert.NotEmpty(results);
    }
}
