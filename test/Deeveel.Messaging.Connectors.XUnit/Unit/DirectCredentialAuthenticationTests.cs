using Microsoft.Extensions.Logging.Abstractions;

namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Feature", "AuthenticationMechanism")]
public class DirectCredentialAuthenticationTests
{
    [Fact]
    public async Task Should_ObtainApiKeyCredential()
    {
        var provider = DirectCredentialAuthenticationProvider.CreateApiKeyProvider();
        var settings = new ConnectionSettings().SetParameter("ApiKey", "my-api-key");

        var result = await provider.ObtainCredentialAsync(settings, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Credential);
        Assert.Equal(AuthenticationType.ApiKey, result.Credential.AuthenticationType);
        Assert.Equal("my-api-key", result.Credential.CredentialValue);
    }

    [Fact]
    public async Task Should_ObtainTokenCredential()
    {
        var provider = DirectCredentialAuthenticationProvider.CreateTokenProvider();
        var settings = new ConnectionSettings()
            .SetParameter("AuthToken", "bearer-token")
            .SetParameter("TokenType", "Bearer");

        var result = await provider.ObtainCredentialAsync(settings, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccessful);
        Assert.Equal(AuthenticationType.Token, result.Credential!.AuthenticationType);
        Assert.Equal("bearer-token", result.Credential.CredentialValue);
        Assert.Equal("Bearer", result.Credential.Properties["TokenType"]);
    }

    [Fact]
    public async Task Should_ObtainBasicCredential()
    {
        var provider = DirectCredentialAuthenticationProvider.CreateBasicProvider();
        var settings = new ConnectionSettings()
            .SetParameter("Username", "admin")
            .SetParameter("Password", "secret");

        var result = await provider.ObtainCredentialAsync(settings, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccessful);
        Assert.Equal(AuthenticationType.Basic, result.Credential!.AuthenticationType);
    }

    [Fact]
    public async Task Should_Fail_When_MissingApiKey()
    {
        var provider = DirectCredentialAuthenticationProvider.CreateApiKeyProvider();
        var settings = new ConnectionSettings();

        var result = await provider.ObtainCredentialAsync(settings, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccessful);
        Assert.Equal("MISSING_API_KEY", result.ErrorCode);
    }

    [Fact]
    public async Task Should_FindApiKey_AlternativeNames()
    {
        var provider = DirectCredentialAuthenticationProvider.CreateApiKeyProvider();
        var settings = new ConnectionSettings().SetParameter("Key", "alt-key");

        var result = await provider.ObtainCredentialAsync(settings, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccessful);
        Assert.Equal("alt-key", result.Credential!.CredentialValue);
    }

    [Fact]
    public async Task Should_FindToken_AlternativeNames()
    {
        var provider = DirectCredentialAuthenticationProvider.CreateTokenProvider();
        var settings = new ConnectionSettings().SetParameter("AccessToken", "access-tok");

        var result = await provider.ObtainCredentialAsync(settings, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccessful);
        Assert.Equal("access-tok", result.Credential!.CredentialValue);
    }

    [Fact]
    public async Task Should_FindBasic_AccountSid()
    {
        var provider = DirectCredentialAuthenticationProvider.CreateBasicProvider();
        var settings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC123")
            .SetParameter("AuthToken", "tok456");

        var result = await provider.ObtainCredentialAsync(settings, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccessful);
        Assert.Equal(AuthenticationType.Basic, result.Credential!.AuthenticationType);
    }

    [Fact]
    public async Task Should_Fail_When_MissingBasicCredentials()
    {
        var provider = DirectCredentialAuthenticationProvider.CreateBasicProvider();
        var settings = new ConnectionSettings();

        var result = await provider.ObtainCredentialAsync(settings, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccessful);
        Assert.Equal("MISSING_BASIC_CREDENTIALS", result.ErrorCode);
    }

    [Fact]
    public async Task Should_RefreshCredential_ByObtainingNew()
    {
        var provider = DirectCredentialAuthenticationProvider.CreateApiKeyProvider();
        var settings = new ConnectionSettings().SetParameter("ApiKey", "key-1");

        var first = await provider.ObtainCredentialAsync(settings, TestContext.Current.CancellationToken);
        var second = await provider.RefreshCredentialAsync(first.Credential!, settings, TestContext.Current.CancellationToken);

        Assert.True(second.IsSuccessful);
    }

    [Fact]
    public async Task Should_HandleTokenWithExpiration()
    {
        var provider = DirectCredentialAuthenticationProvider.CreateTokenProvider();
        var expiresAt = DateTime.UtcNow.AddHours(1).ToString("O");
        var settings = new ConnectionSettings()
            .SetParameter("Token", "tok")
            .SetParameter("ExpiresAt", expiresAt);

        var result = await provider.ObtainCredentialAsync(settings, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Credential!.ExpiresAt);
    }

    [Fact]
    public async Task Should_ReturnFailure_ForUnsupportedType()
    {
        var provider = new DirectCredentialAuthenticationProvider(AuthenticationType.Certificate, "Cert", NullLogger<DirectCredentialAuthenticationProvider>.Instance);
        var settings = new ConnectionSettings().SetParameter("Cert", "data");

        var result = await provider.ObtainCredentialAsync(settings, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccessful);
        Assert.Equal("UNSUPPORTED_TYPE", result.ErrorCode);
    }
}
