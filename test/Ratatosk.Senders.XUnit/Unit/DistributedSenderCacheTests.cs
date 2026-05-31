using System.Text.Json;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Moq;

namespace Ratatosk.Senders;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "DistributedSenderCache")]
public class DistributedSenderCacheTests
{
    private static Sender CreateValidSender(string name = "test-sender", string address = "+1234567890", EndpointType type = EndpointType.PhoneNumber)
    {
        var sender = new Sender
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            DisplayName = "Test Sender",
            Address = address,
            EndpointType = type,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        sender.Activate();
        return sender;
    }

    private static IOptions<SenderCacheOptions> CreateOptions(TimeSpan? ttl = null)
    {
        return Options.Create(new SenderCacheOptions
        {
            DefaultTtl = ttl ?? TimeSpan.FromMinutes(5)
        });
    }

    [Fact]
    public async Task Should_ReturnNull_When_GetByNameAndNotCached()
    {
        // Arrange
        var cacheMock = new Mock<IDistributedCache>();
        cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
        var cache = new DistributedSenderCache(cacheMock.Object, CreateOptions());

        // Act
        var result = await cache.GetByNameAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ReturnNull_When_GetByEndpointAndNotCached()
    {
        // Arrange
        var cacheMock = new Mock<IDistributedCache>();
        cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
        var cache = new DistributedSenderCache(cacheMock.Object, CreateOptions());

        // Act
        var result = await cache.GetByEndpointAsync("+1234567890", EndpointType.PhoneNumber);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Should_SetBothNameAndEndpointKeys_When_SetAsync()
    {
        // Arrange
        var sender = CreateValidSender("my-sender", "+1234567890", EndpointType.PhoneNumber);
        var cacheMock = new Mock<IDistributedCache>();
        cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var cache = new DistributedSenderCache(cacheMock.Object, CreateOptions());

        // Act
        await cache.SetAsync(sender);

        // Assert
        cacheMock.Verify(c => c.SetAsync("ratatosk:sender:name:my-sender", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        cacheMock.Verify(c => c.SetAsync("ratatosk:sender:endpoint:PhoneNumber:+1234567890", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_UseCustomTtl_When_Provided()
    {
        // Arrange
        var sender = CreateValidSender();
        var customTtl = TimeSpan.FromMinutes(10);
        var cacheMock = new Mock<IDistributedCache>();
        DistributedCacheEntryOptions? capturedOptions = null;
        cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((_, _, opts, _) => capturedOptions = opts)
            .Returns(Task.CompletedTask);
        var cache = new DistributedSenderCache(cacheMock.Object, CreateOptions());

        // Act
        await cache.SetAsync(sender, customTtl);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.Equal(customTtl, capturedOptions!.AbsoluteExpirationRelativeToNow);
    }

    [Fact]
    public async Task Should_UseDefaultTtl_When_NotProvided()
    {
        // Arrange
        var sender = CreateValidSender();
        var defaultTtl = TimeSpan.FromMinutes(15);
        var cacheMock = new Mock<IDistributedCache>();
        DistributedCacheEntryOptions? capturedOptions = null;
        cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((_, _, opts, _) => capturedOptions = opts)
            .Returns(Task.CompletedTask);
        var cache = new DistributedSenderCache(cacheMock.Object, CreateOptions(defaultTtl));

        // Act
        await cache.SetAsync(sender);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.Equal(defaultTtl, capturedOptions!.AbsoluteExpirationRelativeToNow);
    }

    [Fact]
    public async Task Should_RemoveByName_When_RemoveByNameAsync()
    {
        // Arrange
        var cacheMock = new Mock<IDistributedCache>();
        cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var cache = new DistributedSenderCache(cacheMock.Object, CreateOptions());

        // Act
        await cache.RemoveByNameAsync("my-sender");

        // Assert
        cacheMock.Verify(c => c.RemoveAsync("ratatosk:sender:name:my-sender", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_CacheIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DistributedSenderCache(null!, CreateOptions()));
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_OptionsIsNull()
    {
        // Arrange
        var cacheMock = new Mock<IDistributedCache>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DistributedSenderCache(cacheMock.Object, null!));
    }

    [Fact]
    public async Task Should_ThrowArgumentNullException_When_SetAsyncWithNullSender()
    {
        // Arrange
        var cacheMock = new Mock<IDistributedCache>();
        var cache = new DistributedSenderCache(cacheMock.Object, CreateOptions());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await cache.SetAsync(null!));
    }
}
