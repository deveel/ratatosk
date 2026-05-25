using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Feature", "Authentication")]
public class AuthenticationManagerEdgeTests
{
    private class ThrowingProvider : AuthenticationProviderBase
    {
        public ThrowingProvider() : base(AuthenticationScheme.ApiKey, "Throwing") { }

        public override Task<AuthenticationResult> ObtainCredentialAsync(
            ConnectionSettings connectionSettings,
            AuthenticationConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Unexpected error");
        }
    }

    [Fact]
    public async Task Should_ReturnFailure_When_NoProviderMatchesScheme()
    {
        var manager = new AuthenticationManager();
        var config = new AuthenticationConfiguration(AuthenticationScheme.Certificate, "Certificate");

        var result = await manager.AuthenticateAsync(
            new ConnectionSettings(),
            config,
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccessful);
        Assert.Equal("NO_PROVIDER", result.ErrorCode);
    }

    [Fact]
    public async Task Should_HandleException_When_ProviderThrows()
    {
        var manager = new AuthenticationManager(new[] { new ThrowingProvider() });
        var settings = new ConnectionSettings().SetParameter("ApiKey", "key-1");
        var config = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key Authentication")
            .WithField("ApiKey", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; });

        var result = await manager.AuthenticateAsync(settings, config, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccessful);
        Assert.Equal("AUTHENTICATION_ERROR", result.ErrorCode);
        Assert.Contains("Unexpected error", result.ErrorMessage);
    }

    [Fact]
    public async Task Should_ThrowArgumentNullException_When_ConnectionSettingsNull()
    {
        var manager = new AuthenticationManager();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            manager.AuthenticateAsync(null!, new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "Test"), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_ThrowArgumentNullException_When_ConfigurationNull()
    {
        var manager = new AuthenticationManager();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            manager.AuthenticateAsync(new ConnectionSettings(), null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_InvalidateWithNullConnectionSettings()
    {
        var manager = new AuthenticationManager();

        Assert.Throws<ArgumentNullException>(() =>
            manager.InvalidateCredential(null!, new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "Test")));
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_InvalidateWithNullConfiguration()
    {
        var manager = new AuthenticationManager();

        Assert.Throws<ArgumentNullException>(() =>
            manager.InvalidateCredential(new ConnectionSettings(), null!));
    }
}
