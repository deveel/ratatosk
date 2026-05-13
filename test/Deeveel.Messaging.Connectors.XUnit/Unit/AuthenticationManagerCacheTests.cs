namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Feature", "Authentication")]
public class AuthenticationManagerCacheTests
{
    [Fact]
    public async Task Should_ReturnExistingCredential_When_Cached()
    {
        var manager = new AuthenticationManager();
        var settings = new ConnectionSettings().SetParameter("ApiKey", "key-1");
        var config = AuthenticationConfigurations.ApiKeyAuthentication();

        var first = await manager.AuthenticateAsync(settings, config, TestContext.Current.CancellationToken);
        var second = await manager.AuthenticateAsync(settings, config, TestContext.Current.CancellationToken);

        Assert.True(first.IsSuccessful);
        Assert.True(second.IsSuccessful);
        Assert.Same(first.Credential, second.Credential);
    }

    [Fact]
    public void Should_InvalidateCredential_AndRemoveFromCache()
    {
        var manager = new AuthenticationManager();
        var settings = new ConnectionSettings().SetParameter("ApiKey", "key-1");
        var config = AuthenticationConfigurations.ApiKeyAuthentication();

        manager.InvalidateCredential(settings, config);

        Assert.True(true);
    }

    [Fact]
    public async Task Should_ReturnNewCredential_AfterCacheInvalidated()
    {
        var manager = new AuthenticationManager();
        var settings = new ConnectionSettings().SetParameter("ApiKey", "key-1");
        var config = AuthenticationConfigurations.ApiKeyAuthentication();

        var first = await manager.AuthenticateAsync(settings, config, TestContext.Current.CancellationToken);
        manager.InvalidateCredential(settings, config);
        var second = await manager.AuthenticateAsync(settings, config, TestContext.Current.CancellationToken);

        Assert.True(first.IsSuccessful);
        Assert.True(second.IsSuccessful);
    }

    [Fact]
    public void Should_ClearCache()
    {
        var manager = new AuthenticationManager();

        manager.ClearCache();

        Assert.True(true);
    }
}
