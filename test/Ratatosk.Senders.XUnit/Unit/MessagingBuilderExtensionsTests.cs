using Microsoft.Extensions.DependencyInjection;

using Ratatosk.Senders;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "MessagingBuilderExtensions")]
public class MessagingBuilderExtensionsTests
{
    [Fact]
    public void Should_ReturnSenderServiceBuilder_When_AddSendersCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = services.AddMessaging();

        // Act
        var result = builder.AddSenders();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<SenderServiceBuilder>(result);
    }

    [Fact]
    public void Should_ReturnMessagingBuilder_When_AddSendersWithConfigureCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = services.AddMessaging();

        // Act
        var result = builder.AddSenders(_ => { });

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_AddSendersWithConfigureAndConfigureIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = services.AddMessaging();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddSenders(null!));
    }

    [Fact]
    public void Should_ApplyConfiguration_When_AddSendersWithConfigureCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = services.AddMessaging();
        var customTtl = TimeSpan.FromMinutes(10);

        // Act
        builder.AddSenders(b => b.ConfigureCacheOptions(opts => opts.DefaultTtl = customTtl));

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SenderCacheOptions>>();
        Assert.Equal(customTtl, options.Value.DefaultTtl);
    }

    [Fact]
    public void Should_ReturnBuilder_ForChaining_When_AddSendersWithConfigureCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = services.AddMessaging();

        // Act
        var result = builder.AddSenders(_ => { });

        // Assert
        Assert.Same(builder, result);
    }
}
