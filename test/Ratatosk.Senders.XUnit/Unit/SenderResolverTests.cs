using Deveel;
using Moq;

namespace Ratatosk.Senders;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "SenderResolver")]
public class SenderResolverTests
{
    private static Ratatosk.Sender CreateSender(string name = "test-sender", EndpointType endpointType = EndpointType.PhoneNumber, string address = "+1234567890", bool isActive = true)
    {
        var sender = new Ratatosk.Sender
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            DisplayName = name,
            Address = address,
            EndpointType = endpointType,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        if (isActive)
            sender.Activate();
        else
            sender.Deactivate();
            
        return sender;
    }

    private static Mock<ISenderRepository<Ratatosk.Sender>> CreateRepositoryMock(Ratatosk.Sender? byName = null, Ratatosk.Sender? byEndpoint = null)
    {
        var mock = new Mock<ISenderRepository<Ratatosk.Sender>>(MockBehavior.Strict);

        mock.Setup(x => x.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string name, CancellationToken _) =>
            {
                return byName != null && name == byName.Name ? byName : null;
            });

        mock.Setup(x => x.FindByEndpointAsync(It.IsAny<string>(), It.IsAny<EndpointType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string address, EndpointType type, CancellationToken _) =>
            {
                return byEndpoint != null && address == byEndpoint.Address && type == ((IEndpoint)byEndpoint).Type ? byEndpoint : null;
            });

        return mock;
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
    public async Task Should_ResolveByNameFromCache_When_SenderRefAndCached()
    {
        var sender = CreateSender("my-sender");
        var repositoryMock = CreateRepositoryMock();
        var cacheMock = CreateCacheMock(cachedByName: sender);
        var resolver = new SenderResolver<Ratatosk.Sender>(repositoryMock.Object, cacheMock.Object);

        var result = await resolver.ResolveAsync(CreateContext(new SenderRef("my-sender")));

        Assert.NotNull(result);
        Assert.Equal("my-sender", result.Name);
        repositoryMock.Verify(x => x.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_ResolveByNameFromRepository_When_NotCached()
    {
        var sender = CreateSender("my-sender");
        var repositoryMock = CreateRepositoryMock(byName: sender);
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver<Ratatosk.Sender>(repositoryMock.Object, cacheMock.Object);

        var result = await resolver.ResolveAsync(CreateContext(new SenderRef("my-sender")));

        Assert.NotNull(result);
        Assert.Equal("my-sender", result.Name);
        repositoryMock.Verify(x => x.FindByNameAsync("my-sender", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_CacheResult_When_ResolvedByNameFromRepository()
    {
        var sender = CreateSender("my-sender");
        var repositoryMock = CreateRepositoryMock(byName: sender);
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver<Ratatosk.Sender>(repositoryMock.Object, cacheMock.Object);

        await resolver.ResolveAsync(CreateContext(new SenderRef("my-sender")));

        cacheMock.Verify(x => x.SetAsync(sender, It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnNull_When_SenderRefNameNotFound()
    {
        var repositoryMock = CreateRepositoryMock();
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver<Ratatosk.Sender>(repositoryMock.Object, cacheMock.Object);

        var result = await resolver.ResolveAsync(CreateContext(new SenderRef("unknown-sender")));

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ResolveByEndpoint_When_ConcreteSender()
    {
        var sender = CreateSender("email-sender", endpointType: EndpointType.EmailAddress, address: "test@example.com");
        var repositoryMock = CreateRepositoryMock(byEndpoint: sender);
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver<Ratatosk.Sender>(repositoryMock.Object, cacheMock.Object);

        var result = await resolver.ResolveAsync(CreateContext(new EmailSender("test@example.com")));

        Assert.NotNull(result);
        Assert.Equal("email-sender", result.Name);
        repositoryMock.Verify(x => x.FindByEndpointAsync("test@example.com", EndpointType.EmailAddress, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnNull_When_ConcreteSenderNotInRepository()
    {
        var repositoryMock = CreateRepositoryMock();
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver<Ratatosk.Sender>(repositoryMock.Object, cacheMock.Object);

        var result = await resolver.ResolveAsync(CreateContext(new EmailSender("unknown@example.com")));

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ReturnNull_When_ConcreteSenderIsInactive()
    {
        var sender = CreateSender("inactive-sender", endpointType: EndpointType.EmailAddress, address: "inactive@example.com", isActive: false);
        var repositoryMock = CreateRepositoryMock(byEndpoint: sender);
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver<Ratatosk.Sender>(repositoryMock.Object, cacheMock.Object);

        var result = await resolver.ResolveAsync(CreateContext(new EmailSender("inactive@example.com")));

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ReturnNull_When_PlainEndpoint()
    {
        var repositoryMock = CreateRepositoryMock();
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver<Ratatosk.Sender>(repositoryMock.Object, cacheMock.Object);

        var result = await resolver.ResolveAsync(CreateContext(new Endpoint(EndpointType.PhoneNumber, "+1234567890")));

        Assert.Null(result);
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_RepositoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SenderResolver<Ratatosk.Sender>(null!, CreateCacheMock().Object));
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_CacheIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SenderResolver<Ratatosk.Sender>(CreateRepositoryMock().Object, null!));
    }

    [Fact]
    public async Task Should_ResolveDefaultSender_When_ContextSenderIsNull()
    {
        var defaultSender = CreateSender("default-sender");
        var repositoryMock = CreateRepositoryMock(byEndpoint: defaultSender);
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver<Ratatosk.Sender>(repositoryMock.Object, cacheMock.Object);

        var settings = new ConnectionSettings();
        settings.SetParameter("DefaultSenderName", "default-sender");
        settings.SetParameter("DefaultSenderAddress", defaultSender.Address);
        settings.SetParameter("DefaultSenderType", ((IEndpoint)defaultSender).Type.ToString());
        var context = new SenderResolutionContext(null, settings);

        var result = await resolver.ResolveAsync(context);

        Assert.NotNull(result);
        Assert.Equal("default-sender", result.Name);
    }

    [Fact]
    public async Task Should_ReturnNull_When_ContextSenderIsNullAndNoDefaultSender()
    {
        var repositoryMock = CreateRepositoryMock();
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver<Ratatosk.Sender>(repositoryMock.Object, cacheMock.Object);

        var context = new SenderResolutionContext(null, new ConnectionSettings());

        var result = await resolver.ResolveAsync(context);

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ReturnNull_When_ContextIsNull()
    {
        var repositoryMock = CreateRepositoryMock();
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver<Ratatosk.Sender>(repositoryMock.Object, cacheMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => resolver.ResolveAsync(null!).AsTask());
    }

    [Fact]
    public async Task Should_ReturnNull_When_DefaultSenderIsInactive()
    {
        var defaultSender = CreateSender("inactive-default", isActive: false);
        var repositoryMock = CreateRepositoryMock(byEndpoint: defaultSender);
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver<Ratatosk.Sender>(repositoryMock.Object, cacheMock.Object);

        var settings = new ConnectionSettings();
        settings.SetParameter("DefaultSenderName", "inactive-default");
        settings.SetParameter("DefaultSenderAddress", defaultSender.Address);
        settings.SetParameter("DefaultSenderType", ((IEndpoint)defaultSender).Type.ToString());
        var context = new SenderResolutionContext(null, settings);

        var result = await resolver.ResolveAsync(context);

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_CacheResult_When_ResolvedByEndpointFromRepository()
    {
        var sender = CreateSender("email-sender", endpointType: EndpointType.EmailAddress, address: "test@example.com");
        var repositoryMock = CreateRepositoryMock(byEndpoint: sender);
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver<Ratatosk.Sender>(repositoryMock.Object, cacheMock.Object);

        await resolver.ResolveAsync(CreateContext(new EmailSender("test@example.com")));

        cacheMock.Verify(x => x.SetAsync(sender, It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ResolveByEndpointFromCache_When_Cached()
    {
        var sender = CreateSender("email-sender", endpointType: EndpointType.EmailAddress, address: "test@example.com");
        var repositoryMock = CreateRepositoryMock();
        var cacheMock = CreateCacheMock(cachedByEndpoint: sender);
        var resolver = new SenderResolver<Ratatosk.Sender>(repositoryMock.Object, cacheMock.Object);

        var result = await resolver.ResolveAsync(CreateContext(new EmailSender("test@example.com")));

        Assert.NotNull(result);
        Assert.Equal("email-sender", result.Name);
        repositoryMock.Verify(x => x.FindByEndpointAsync(It.IsAny<string>(), It.IsAny<EndpointType>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
