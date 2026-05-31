using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Ratatosk.Senders;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "SenderResolver")]
public class DbSenderResolverTests
{
    private static DbContextOptions<SenderDbContext> CreateOptions(string dbName)
    {
        return new DbContextOptionsBuilder<SenderDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
    }

    private static DbSender CreateEntity(string name = "test-sender", EndpointType endpointType = EndpointType.PhoneNumber, string address = "+1234567890", bool isActive = true)
    {
        var entity = new DbSender
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            DisplayName = name,
            Address = address,
            Type = endpointType.ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        if (isActive)
            entity.Activate();
        else
            entity.Deactivate();
            
        return entity;
    }

    private static (SenderDbContext context, SenderResolver<DbSender> resolver) CreateResolver(
        string dbName,
        Mock<ISenderCache>? cacheMock = null,
        params DbSender[] seedEntities)
    {
        var options = CreateOptions(dbName);
        var context = new SenderDbContext(options);

        if (seedEntities.Length > 0)
        {
            context.Senders.AddRange(seedEntities);
            context.SaveChanges();
        }

        var services = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        var repo = new EntitySenderRepository(context, services, NullLogger<EntityRepository<DbSender>>.Instance);
        cacheMock ??= CreateCacheMock();
        var resolver = new SenderResolver<DbSender>(repo, cacheMock.Object, NullLogger<SenderResolver<DbSender>>.Instance);

        return (context, resolver);
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

    private static SenderResolutionContext CreateContext(IEndpoint? sender)
    {
        return new SenderResolutionContext(sender, new ConnectionSettings());
    }

    [Fact]
    public async Task Should_ResolveByName_When_SenderExists()
    {
        var entity = CreateEntity("my-sender");
        var (context, resolver) = CreateResolver("resolve-by-name", seedEntities: entity);

        var result = await resolver.ResolveAsync(CreateContext(new SenderRef("my-sender")));

        Assert.NotNull(result);
        Assert.Equal("my-sender", result.Name);

        context.Dispose();
    }

    [Fact]
    public async Task Should_ResolveByNameFromCache_When_Cached()
    {
        var entity = CreateEntity("my-sender");
        var cacheMock = CreateCacheMock(cachedByName: entity);
        var (context, resolver) = CreateResolver("resolve-cached", cacheMock);

        var result = await resolver.ResolveAsync(CreateContext(new SenderRef("my-sender")));

        Assert.NotNull(result);
        Assert.Equal("my-sender", result.Name);

        context.Dispose();
    }

    [Fact]
    public async Task Should_CacheResult_When_ResolvedFromStore()
    {
        var entity = CreateEntity("cache-me");
        var cacheMock = CreateCacheMock();
        var (context, resolver) = CreateResolver("check-cache", cacheMock, entity);

        await resolver.ResolveAsync(CreateContext(new SenderRef("cache-me")));

        cacheMock.Verify(x => x.SetAsync(It.IsAny<ISender>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);

        context.Dispose();
    }

    [Fact]
    public async Task Should_ReturnNull_When_NameNotFound()
    {
        var (context, resolver) = CreateResolver("not-found");

        var result = await resolver.ResolveAsync(CreateContext(new SenderRef("unknown")));

        Assert.Null(result);

        context.Dispose();
    }

    [Fact]
    public async Task Should_ResolveByEndpoint_When_ConcreteSender()
    {
        var entity = CreateEntity("email-sender", EndpointType.EmailAddress, "test@example.com");
        var (context, resolver) = CreateResolver("resolve-endpoint", seedEntities: entity);

        var result = await resolver.ResolveAsync(CreateContext(new EmailSender("test@example.com")));

        Assert.NotNull(result);
        Assert.Equal("email-sender", result.Name);

        context.Dispose();
    }

    [Fact]
    public async Task Should_ReturnNull_When_ConcreteSenderNotInStore()
    {
        var (context, resolver) = CreateResolver("not-in-store");

        var result = await resolver.ResolveAsync(CreateContext(new EmailSender("unknown@example.com")));

        Assert.Null(result);

        context.Dispose();
    }

    [Fact]
    public async Task Should_ReturnNull_When_SenderIsInactive()
    {
        var entity = CreateEntity("inactive", EndpointType.EmailAddress, "inactive@example.com", isActive: false);
        var (context, resolver) = CreateResolver("inactive", seedEntities: entity);

        var result = await resolver.ResolveAsync(CreateContext(new SenderRef("inactive")));

        Assert.Null(result);

        context.Dispose();
    }

    [Fact]
    public async Task Should_ReturnNull_When_PlainEndpoint()
    {
        var (context, resolver) = CreateResolver("plain-endpoint");

        var result = await resolver.ResolveAsync(CreateContext(new Endpoint(EndpointType.PhoneNumber, "+1234567890")));

        Assert.Null(result);

        context.Dispose();
    }

    [Fact]
    public async Task Should_ReturnNull_When_ConcreteSenderIsInactive()
    {
        var entity = CreateEntity("inactive-endpoint", EndpointType.EmailAddress, "inactive@example.com", isActive: false);
        var (context, resolver) = CreateResolver("inactive-endpoint", seedEntities: entity);

        var result = await resolver.ResolveAsync(CreateContext(new EmailSender("inactive@example.com")));

        Assert.Null(result);

        context.Dispose();
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_RepositoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SenderResolver<DbSender>(null!, CreateCacheMock().Object, NullLogger<SenderResolver<DbSender>>.Instance));
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_CacheIsNull()
    {
        var options = CreateOptions("null-cache-test");
        using var context = new SenderDbContext(options);
        var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        var repo = new EntitySenderRepository(context, services, NullLogger<EntityRepository<DbSender>>.Instance);

        Assert.Throws<ArgumentNullException>(() =>
            new SenderResolver<DbSender>(repo, null!, NullLogger<SenderResolver<DbSender>>.Instance));
    }
}
