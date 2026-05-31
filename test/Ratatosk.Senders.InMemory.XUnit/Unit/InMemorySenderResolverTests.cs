using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Ratatosk.Senders;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "SenderResolver")]
public class SenderEntityResolverTests
{
    private static SenderEntity CreateEntity(string name = "test-sender", EndpointType endpointType = EndpointType.PhoneNumber, string address = "+1234567890", bool isActive = true)
    {
        var entity = new SenderEntity
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            DisplayName = name,
            Address = address,
            Type = endpointType,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        if (isActive)
            entity.Activate();
        else
            entity.Deactivate();
            
        return entity;
    }

    private static InMemorySenderRepository CreateStore(params SenderEntity[] entities)
    {
        return new InMemorySenderRepository(entities);
    }

    private static Mock<ISenderCache> CreateCacheMock(ISender? cachedByName = null, ISender? cachedByEndpoint = null)
    {
        var mock = new Mock<ISenderCache>(MockBehavior.Strict);

        if (cachedByName != null)
            mock.Setup(x => x.GetByNameAsync(cachedByName.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedByName);
        else
            mock.Setup(x => x.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ISender?)null);

        if (cachedByEndpoint != null)
            mock.Setup(x => x.GetByEndpointAsync(cachedByEndpoint.Address, cachedByEndpoint.Type, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedByEndpoint);
        else
            mock.Setup(x => x.GetByEndpointAsync(It.IsAny<string>(), It.IsAny<EndpointType>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ISender?)null);

        mock.Setup(x => x.SetAsync(It.IsAny<ISender>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        mock.Setup(x => x.RemoveByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        return mock;
    }

    private static SenderResolver<SenderEntity> CreateResolver(InMemorySenderRepository repository, Mock<ISenderCache>? cacheMock = null)
    {
        cacheMock ??= CreateCacheMock();
        return new SenderResolver<SenderEntity>(repository, cacheMock.Object, NullLogger<SenderResolver<SenderEntity>>.Instance);
    }

    private static SenderResolutionContext CreateContext(IEndpoint? sender)
    {
        return new SenderResolutionContext(sender, new ConnectionSettings());
    }

    [Fact]
    public async Task Should_ResolveByName_When_SenderExists()
    {
        var entity = CreateEntity("my-sender");
        var store = CreateStore(entity);
        var cacheMock = CreateCacheMock();
        var resolver = CreateResolver(store, cacheMock);

        var result = await resolver.ResolveAsync(CreateContext(new SenderRef("my-sender")));

        Assert.NotNull(result);
        Assert.Equal("my-sender", result.Name);
    }

    [Fact]
    public async Task Should_ResolveByNameFromCache_When_SenderRefAndCached()
    {
        var entity = CreateEntity("my-sender");
        var store = CreateStore();
        var cacheMock = CreateCacheMock(cachedByName: entity);
        var resolver = CreateResolver(store, cacheMock);

        var result = await resolver.ResolveAsync(CreateContext(new SenderRef("my-sender")));

        Assert.NotNull(result);
        Assert.Equal("my-sender", result.Name);
    }

    [Fact]
    public async Task Should_CacheResult_When_ResolvedByNameFromStore()
    {
        var entity = CreateEntity("my-sender");
        var store = CreateStore(entity);
        var cacheMock = CreateCacheMock();
        var resolver = CreateResolver(store, cacheMock);

        await resolver.ResolveAsync(CreateContext(new SenderRef("my-sender")));

        cacheMock.Verify(x => x.SetAsync(entity, It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnNull_When_SenderRefNameNotFound()
    {
        var store = CreateStore();
        var cacheMock = CreateCacheMock();
        var resolver = CreateResolver(store, cacheMock);

        var result = await resolver.ResolveAsync(CreateContext(new SenderRef("unknown-sender")));

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ResolveByEndpoint_When_ConcreteSender()
    {
        var entity = CreateEntity("email-sender", EndpointType.EmailAddress, "test@example.com");
        var store = CreateStore(entity);
        var cacheMock = CreateCacheMock();
        var resolver = CreateResolver(store, cacheMock);

        var result = await resolver.ResolveAsync(CreateContext(new EmailSender("test@example.com")));

        Assert.NotNull(result);
        Assert.Equal("email-sender", result.Name);
    }

    [Fact]
    public async Task Should_ReturnNull_When_ConcreteSenderNotInStore()
    {
        var store = CreateStore();
        var cacheMock = CreateCacheMock();
        var resolver = CreateResolver(store, cacheMock);

        var result = await resolver.ResolveAsync(CreateContext(new EmailSender("unknown@example.com")));

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ReturnNull_When_SenderIsInactive()
    {
        var entity = CreateEntity("inactive-sender", EndpointType.EmailAddress, "inactive@example.com", isActive: false);
        var store = CreateStore(entity);
        var cacheMock = CreateCacheMock();
        var resolver = CreateResolver(store, cacheMock);

        var result = await resolver.ResolveAsync(CreateContext(new SenderRef("inactive-sender")));

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ReturnNull_When_PlainEndpoint()
    {
        var store = CreateStore();
        var cacheMock = CreateCacheMock();
        var resolver = CreateResolver(store, cacheMock);

        var result = await resolver.ResolveAsync(CreateContext(new Endpoint(EndpointType.PhoneNumber, "+1234567890")));

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ReturnNull_When_ConcreteSenderIsInactive()
    {
        var entity = CreateEntity("inactive-sender", EndpointType.EmailAddress, "inactive@example.com", isActive: false);
        var store = CreateStore(entity);
        var cacheMock = CreateCacheMock();
        var resolver = CreateResolver(store, cacheMock);

        var result = await resolver.ResolveAsync(CreateContext(new EmailSender("inactive@example.com")));

        Assert.Null(result);
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_RepositoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SenderResolver<SenderEntity>(null!, CreateCacheMock().Object, NullLogger<SenderResolver<SenderEntity>>.Instance));
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_CacheIsNull()
    {
        var store = CreateStore();
        Assert.Throws<ArgumentNullException>(() =>
            new SenderResolver<SenderEntity>(store, null!, NullLogger<SenderResolver<SenderEntity>>.Instance));
    }
}
