using Microsoft.Extensions.DependencyInjection;

namespace Ratatosk.Senders;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "InMemorySenderStore")]
public class InMemorySenderStoreTests
{
    [Fact]
    public void Should_RegisterInMemoryRepository_When_UseInMemoryStoreCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = services.AddMessaging().AddSenders<SenderEntity>();
        
        // Act
        builder.UseInMemoryStore();

        // Assert
        var provider = services.BuildServiceProvider();
        var scope = provider.CreateScope();
        var manager = scope.ServiceProvider.GetService<SenderManager<SenderEntity>>();
        Assert.NotNull(manager);
    }

    [Fact]
    public void Should_ReturnBuilder_ForChaining_When_UseInMemoryStoreCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = services.AddMessaging()
            .AddSenders<SenderEntity>();

        // Act
        var result = builder.UseInMemoryStore();

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_BuilderIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((SenderServiceBuilder)null!).UseInMemoryStore());
    }

    [Fact]
    public void Should_RegisterSeedData_When_Provided()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = services.AddMessaging().AddSenders<SenderEntity>();
        
        var seedSender = new SenderEntity
        {
            Id = "seed-id",
            Name = "seed-sender",
            DisplayName = "Seed Sender",
            Address = "+1234567890",
            Type = EndpointType.PhoneNumber
        };

        // Act
        builder.UseInMemoryStore(new[] { seedSender });

        // Assert
        var provider = services.BuildServiceProvider();
        var seedData = provider.GetService<IEnumerable<SenderEntity>>();
        Assert.NotNull(seedData);
        Assert.Single(seedData!);
        Assert.Equal("seed-sender", seedData!.First().Name);
    }

    [Fact]
    public void Should_WorkWithFullConfiguration_When_Chained()
    {
        // Arrange
        var services = new ServiceCollection();
        var customTtl = TimeSpan.FromMinutes(10);

        // Act
        var builder = services.AddMessaging().AddSenders<SenderEntity>()
            .ConfigureCacheOptions(opts => opts.DefaultTtl = customTtl)
            .UseInMemoryStore();

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SenderCacheOptions>>();
        Assert.Equal(customTtl, options.Value.DefaultTtl);
        Assert.NotNull(provider.GetService<ISenderCache>());
    }
}
