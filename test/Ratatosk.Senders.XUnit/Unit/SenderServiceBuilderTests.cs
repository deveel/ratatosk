using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Ratatosk.Senders;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "SenderServiceBuilder")]
public class SenderServiceBuilderTests
{
    private static IServiceCollection CreateServices() => new ServiceCollection();

    [Fact]
    public void Should_SenderTypeBeCorrect_When_AddSendersCalled()
    {
        // Arrange
        var services = CreateServices();

        // Act
        var builder = services.AddMessaging().AddSenders<SenderEntity>();

        // Assert
        Assert.Equal(typeof(SenderEntity), builder.SenderType);
    }

    [Fact]
    public void Should_ServicesBeSame_When_AddSendersCalled()
    {
        // Arrange
        var services = CreateServices();

        // Act
        var builder = services.AddMessaging().AddSenders<SenderEntity>();

        // Assert
        Assert.Same(services, builder.Services);
    }

    [Fact]
    public void Should_ReplaceCache_When_WithCacheGenericType()
    {
        // Arrange
        var services = CreateServices();

        // Act
        services.AddMessaging().AddSenders<SenderEntity>().WithCache<CustomSenderCache>();

        // Assert
        var provider = services.BuildServiceProvider();
        var cache = provider.GetService<ISenderCache>();
        Assert.IsType<CustomSenderCache>(cache);
    }

    [Fact]
    public void Should_ReplaceCache_When_WithCacheFactory()
    {
        // Arrange
        var services = CreateServices();
        var customCache = new CustomSenderCache();

        // Act
        services.AddMessaging().AddSenders<SenderEntity>().WithCache(_ => customCache);

        // Assert
        var provider = services.BuildServiceProvider();
        var cache = provider.GetService<ISenderCache>();
        Assert.Same(customCache, cache);
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_WithCacheFactoryIsNull()
    {
        // Arrange
        var services = CreateServices();
        var builder = services.AddMessaging().AddSenders<SenderEntity>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithCache((Func<IServiceProvider, ISenderCache>)null!));
    }

    [Fact]
    public void Should_ConfigureCacheOptions_When_ConfigureCacheOptionsCalled()
    {
        // Arrange
        var services = CreateServices();
        var customTtl = TimeSpan.FromMinutes(10);

        // Act
        services.AddMessaging().AddSenders<SenderEntity>().ConfigureCacheOptions(opts => opts.DefaultTtl = customTtl);

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SenderCacheOptions>>();
        Assert.Equal(customTtl, options.Value.DefaultTtl);
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_ConfigureCacheOptionsIsNull()
    {
        // Arrange
        var services = CreateServices();
        var builder = services.AddMessaging().AddSenders<SenderEntity>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureCacheOptions(null!));
    }

    [Fact]
    public void Should_ReplaceResolver_When_WithResolverGenericType()
    {
        // Arrange
        var services = CreateServices();

        // Act
        services.AddMessaging().AddSenders<SenderEntity>().WithResolver<CustomSenderResolver>();

        // Assert
        var provider = services.BuildServiceProvider();
        var scope = provider.CreateScope();
        var resolver = scope.ServiceProvider.GetService<ISenderResolver>();
        Assert.IsType<CustomSenderResolver>(resolver);
    }

    [Fact]
    public void Should_ReplaceResolver_When_WithResolverFactory()
    {
        // Arrange
        var services = CreateServices();
        var customResolver = new CustomSenderResolver();

        // Act
        services.AddMessaging().AddSenders<SenderEntity>().WithResolver(_ => customResolver);

        // Assert
        var provider = services.BuildServiceProvider();
        var scope = provider.CreateScope();
        var resolver = scope.ServiceProvider.GetService<ISenderResolver>();
        Assert.Same(customResolver, resolver);
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_WithResolverFactoryIsNull()
    {
        // Arrange
        var services = CreateServices();
        var builder = services.AddMessaging().AddSenders<SenderEntity>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithResolver((Func<IServiceProvider, ISenderResolver>)null!));
    }

    [Fact]
    public void Should_ReturnSelf_ForChaining_When_WithCacheGenericType()
    {
        // Arrange
        var services = CreateServices();
        var builder = services.AddMessaging().AddSenders<SenderEntity>();

        // Act
        var result = builder.WithCache<CustomSenderCache>();

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void Should_ReturnSelf_ForChaining_When_WithCacheFactory()
    {
        // Arrange
        var services = CreateServices();
        var builder = services.AddMessaging().AddSenders<SenderEntity>();

        // Act
        var result = builder.WithCache(_ => new CustomSenderCache());

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void Should_ReturnSelf_ForChaining_When_WithResolverGenericType()
    {
        // Arrange
        var services = CreateServices();
        var builder = services.AddMessaging().AddSenders<SenderEntity>();

        // Act
        var result = builder.WithResolver<CustomSenderResolver>();

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void Should_ReturnSelf_ForChaining_When_WithResolverFactory()
    {
        // Arrange
        var services = CreateServices();
        var builder = services.AddMessaging().AddSenders<SenderEntity>();

        // Act
        var result = builder.WithResolver(_ => new CustomSenderResolver());

        // Assert
        Assert.Same(builder, result);
    }

    private class CustomSenderCache : ISenderCache
    {
        public ValueTask<ISender?> GetByNameAsync(string senderName, CancellationToken cancellationToken = default) => ValueTask.FromResult<ISender?>(null);
        public ValueTask<ISender?> GetByEndpointAsync(string address, EndpointType endpointType, CancellationToken cancellationToken = default) => ValueTask.FromResult<ISender?>(null);
        public ValueTask SetAsync(ISender sender, TimeSpan? ttl = null, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask RemoveByNameAsync(string senderName, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
    }

    private class CustomSenderResolver : ISenderResolver
    {
        public ValueTask<ISender?> ResolveAsync(SenderResolutionContext context, CancellationToken cancellationToken = default) => ValueTask.FromResult<ISender?>(null);
    }
}
