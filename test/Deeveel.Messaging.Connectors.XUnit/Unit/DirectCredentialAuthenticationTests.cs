using Microsoft.Extensions.Logging.Abstractions;

namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Feature", "AuthenticationMechanism")]
public class DirectCredentialAuthenticationTests
{
    private static AuthenticationConfiguration ApiKeyConfig => new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key Authentication")
        .WithField("ApiKey", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; });
    private static AuthenticationConfiguration BearerConfig => new AuthenticationConfiguration(AuthenticationScheme.Bearer, "Flexible Bearer Token Authentication")
        .WithField("Token", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; })
        .WithField("AccessToken", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; })
        .WithField("BearerToken", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; })
        .WithField("AuthToken", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; });
    private static AuthenticationConfiguration BasicConfig => new AuthenticationConfiguration(AuthenticationScheme.Basic, "Flexible Basic Authentication")
        .WithField("Username", DataType.String, f => f.AuthenticationRole = "principal")
        .WithField("Password", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; })
        .WithField("AccountSid", DataType.String, f => f.AuthenticationRole = "principal")
        .WithField("AuthToken", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; })
        .WithField("User", DataType.String, f => f.AuthenticationRole = "principal")
        .WithField("Pass", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; })
        .WithField("ClientId", DataType.String, f => f.AuthenticationRole = "principal")
        .WithField("ClientSecret", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; });

    [Fact]
    public async Task Should_ObtainApiKeyCredential()
    {
        var provider = new ApiKeyAuthenticationProvider();
        var settings = new ConnectionSettings().SetParameter("ApiKey", "my-api-key");

        var result = await provider.ObtainCredentialAsync(settings, ApiKeyConfig, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Credential);
        Assert.Equal(AuthenticationScheme.ApiKey, result.Credential.Scheme);
        Assert.Equal("my-api-key", result.Credential.Value);
    }

    [Fact]
    public async Task Should_ObtainTokenCredential()
    {
        var provider = new BearerTokenAuthenticationProvider();
        var settings = new ConnectionSettings()
            .SetParameter("AuthToken", "bearer-token")
            .SetParameter("TokenType", "Bearer");

        var result = await provider.ObtainCredentialAsync(settings, BearerConfig, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccessful);
        Assert.Equal(AuthenticationScheme.Bearer, result.Credential!.Scheme);
        Assert.Equal("bearer-token", result.Credential.Value);
        Assert.Equal("Bearer", result.Credential.Properties["TokenType"]);
    }

    [Fact]
    public async Task Should_ObtainBasicCredential()
    {
        var provider = new BasicAuthenticationProvider();
        var settings = new ConnectionSettings()
            .SetParameter("Username", "admin")
            .SetParameter("Password", "secret");

        var result = await provider.ObtainCredentialAsync(settings, BasicConfig, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccessful);
        Assert.Equal(AuthenticationScheme.Basic, result.Credential!.Scheme);
    }

    [Fact]
    public async Task Should_Fail_When_MissingApiKey()
    {
        var provider = new ApiKeyAuthenticationProvider();
        var settings = new ConnectionSettings();

        var result = await provider.ObtainCredentialAsync(settings, ApiKeyConfig, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccessful);
        Assert.Equal("MISSING_API_KEY", result.ErrorCode);
    }

    [Fact]
    public async Task Should_FindApiKey_AlternativeNames()
    {
        var provider = new ApiKeyAuthenticationProvider();
        var config = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "Flexible API Key Authentication")
            .WithField("Key", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; });
        var settings = new ConnectionSettings().SetParameter("Key", "alt-key");

        var result = await provider.ObtainCredentialAsync(settings, config, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccessful);
        Assert.Equal("alt-key", result.Credential!.Value);
    }

    [Fact]
    public async Task Should_FindToken_AlternativeNames()
    {
        var provider = new BearerTokenAuthenticationProvider();
        var config = new AuthenticationConfiguration(AuthenticationScheme.Bearer, "Flexible Bearer Token Authentication")
            .WithField("AccessToken", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; });
        var settings = new ConnectionSettings().SetParameter("AccessToken", "access-tok");

        var result = await provider.ObtainCredentialAsync(settings, config, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccessful);
        Assert.Equal("access-tok", result.Credential!.Value);
    }

    [Fact]
    public async Task Should_FindBasic_AccountSid()
    {
        var provider = new BasicAuthenticationProvider();
        var settings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC123")
            .SetParameter("AuthToken", "tok456");

        var result = await provider.ObtainCredentialAsync(settings, BasicConfig, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccessful);
        Assert.Equal(AuthenticationScheme.Basic, result.Credential!.Scheme);
    }

    [Fact]
    public async Task Should_Fail_When_MissingBasicCredentials()
    {
        var provider = new BasicAuthenticationProvider();
        var settings = new ConnectionSettings();

        var result = await provider.ObtainCredentialAsync(settings, BasicConfig, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccessful);
        Assert.Equal("MISSING_BASIC_CREDENTIALS", result.ErrorCode);
    }

    [Fact]
    public async Task Should_RefreshCredential_ByObtainingNew()
    {
        var provider = new ApiKeyAuthenticationProvider();
        var settings = new ConnectionSettings().SetParameter("ApiKey", "key-1");

        var first = await provider.ObtainCredentialAsync(settings, ApiKeyConfig, TestContext.Current.CancellationToken);
        var second = await provider.RefreshCredentialAsync(first.Credential!, settings, ApiKeyConfig, TestContext.Current.CancellationToken);

        Assert.True(second.IsSuccessful);
    }

    [Fact]
    public async Task Should_HandleTokenWithExpiration()
    {
        var provider = new BearerTokenAuthenticationProvider();
        var expiresAt = DateTime.UtcNow.AddHours(1).ToString("O");
        var settings = new ConnectionSettings()
            .SetParameter("Token", "tok")
            .SetParameter("ExpiresAt", expiresAt);

        var result = await provider.ObtainCredentialAsync(settings, BearerConfig, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Credential!.ExpiresAt);
    }
}
