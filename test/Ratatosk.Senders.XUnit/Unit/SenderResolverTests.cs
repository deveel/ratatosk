using Moq;

namespace Ratatosk.Senders;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "SenderResolver")]
public class SenderResolverTests
{
    private static ISender CreateSender(string name = "test-sender", EndpointType endpointType = EndpointType.PhoneNumber, string address = "+1234567890", bool isActive = true) => new Sender
    {
        Id = Guid.NewGuid().ToString(),
        Name = name,
        DisplayName = name,
        Address = address,
        EndpointType = endpointType,
        IsActive = isActive,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    private static Mock<ISenderRepository<ISender>> CreateRepositoryMock(ISender? byName = null, ISender? byEndpoint = null)
    {
        var mock = new Mock<ISenderRepository<ISender>>(MockBehavior.Strict);

        mock.Setup(x => x.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string name, CancellationToken _) =>
                byName != null && name == byName.Name ? byName : null);

        mock.Setup(x => x.FindByEndpointAsync(It.IsAny<string>(), It.IsAny<EndpointType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string address, EndpointType type, CancellationToken _) =>
                byEndpoint != null && address == byEndpoint.Address && type == byEndpoint.Type ? byEndpoint : null);

        return mock;
    }

    private static Mock<ISenderCache> CreateCacheMock(ISender? cached = null)
    {
        var mock = new Mock<ISenderCache>(MockBehavior.Strict);

        if (cached != null)
            mock.Setup(x => x.GetByNameAsync(cached.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cached);
        else
            mock.Setup(x => x.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ISender?)null);

        mock.Setup(x => x.SetByNameAsync(It.IsAny<string>(), It.IsAny<ISender>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        mock.Setup(x => x.RemoveByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        return mock;
    }

    [Fact]
    public async Task Should_ResolveByNameFromCache_When_SenderRefAndCached()
    {
        var sender = CreateSender("my-sender");
        var repositoryMock = CreateRepositoryMock();
        var cacheMock = CreateCacheMock(sender);
        var resolver = new SenderResolver(repositoryMock.Object, cacheMock.Object);

        var result = await resolver.ResolveSenderAsync(new SenderRef("my-sender"));

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
        var resolver = new SenderResolver(repositoryMock.Object, cacheMock.Object);

        var result = await resolver.ResolveSenderAsync(new SenderRef("my-sender"));

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
        var resolver = new SenderResolver(repositoryMock.Object, cacheMock.Object);

        await resolver.ResolveSenderAsync(new SenderRef("my-sender"));

        cacheMock.Verify(x => x.SetByNameAsync("my-sender", sender, It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnNull_When_SenderRefNameNotFound()
    {
        var repositoryMock = CreateRepositoryMock();
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver(repositoryMock.Object, cacheMock.Object);

        var result = await resolver.ResolveSenderAsync(new SenderRef("unknown-sender"));

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ResolveByEndpoint_When_ConcreteSender()
    {
        var sender = CreateSender("email-sender", endpointType: EndpointType.EmailAddress, address: "test@example.com");
        var repositoryMock = CreateRepositoryMock(byEndpoint: sender);
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver(repositoryMock.Object, cacheMock.Object);

        var result = await resolver.ResolveSenderAsync(new EmailSender("test@example.com"));

        Assert.NotNull(result);
        Assert.Equal("email-sender", result.Name);
        repositoryMock.Verify(x => x.FindByEndpointAsync("test@example.com", EndpointType.EmailAddress, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnNull_When_ConcreteSenderNotInRepository()
    {
        var repositoryMock = CreateRepositoryMock();
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver(repositoryMock.Object, cacheMock.Object);

        var result = await resolver.ResolveSenderAsync(new EmailSender("unknown@example.com"));

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ReturnNull_When_ConcreteSenderIsInactive()
    {
        var sender = CreateSender("inactive-sender", endpointType: EndpointType.EmailAddress, address: "inactive@example.com", isActive: false);
        var repositoryMock = CreateRepositoryMock(byEndpoint: sender);
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver(repositoryMock.Object, cacheMock.Object);

        var result = await resolver.ResolveSenderAsync(new EmailSender("inactive@example.com"));

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ReturnSender_When_EndpointTypeIsPhone()
    {
        var sender = CreateSender("phone-sender", endpointType: EndpointType.PhoneNumber, address: "+1234567890");
        var repositoryMock = CreateRepositoryMock(byName: sender);
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver(repositoryMock.Object, cacheMock.Object);

        var result = await resolver.ResolveSenderAsync(new SenderRef("phone-sender"));

        Assert.NotNull(result);
        Assert.Equal("+1234567890", result.Address);
        Assert.Equal(EndpointType.PhoneNumber, result.Type);
    }

    [Fact]
    public async Task Should_ReturnSender_When_EndpointTypeIsLabel()
    {
        var sender = CreateSender("brand-sender", endpointType: EndpointType.Label, address: "MyBrand");
        var repositoryMock = CreateRepositoryMock(byName: sender);
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver(repositoryMock.Object, cacheMock.Object);

        var result = await resolver.ResolveSenderAsync(new SenderRef("brand-sender"));

        Assert.NotNull(result);
        Assert.Equal("MyBrand", result.Address);
        Assert.Equal(EndpointType.Label, result.Type);
    }

    [Fact]
    public async Task Should_ReturnSender_When_EndpointTypeIsEmail()
    {
        var sender = CreateSender("email-sender", endpointType: EndpointType.EmailAddress, address: "test@example.com");
        var repositoryMock = CreateRepositoryMock(byName: sender);
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver(repositoryMock.Object, cacheMock.Object);

        var result = await resolver.ResolveSenderAsync(new SenderRef("email-sender"));

        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Address);
        Assert.Equal(EndpointType.EmailAddress, result.Type);
    }

    [Fact]
    public async Task Should_ReturnSender_When_EndpointTypeIsId()
    {
        var sender = CreateSender("bot-sender", endpointType: EndpointType.Id, address: "bot-123");
        var repositoryMock = CreateRepositoryMock(byName: sender);
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver(repositoryMock.Object, cacheMock.Object);

        var result = await resolver.ResolveSenderAsync(new SenderRef("bot-sender"));

        Assert.NotNull(result);
        Assert.Equal("bot-123", result.Address);
        Assert.Equal(EndpointType.Id, result.Type);
    }

    [Fact]
    public async Task Should_LogWarning_When_SenderRefNotFound()
    {
        var repositoryMock = CreateRepositoryMock();
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver(repositoryMock.Object, cacheMock.Object);

        var result = await resolver.ResolveSenderAsync(new SenderRef("missing"));

        Assert.Null(result);
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_RepositoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SenderResolver(null!, CreateCacheMock().Object));
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_CacheIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SenderResolver(CreateRepositoryMock().Object, null!));
    }

    [Fact]
    public async Task Should_ReturnNull_When_PlainEndpoint()
    {
        var repositoryMock = CreateRepositoryMock();
        var cacheMock = CreateCacheMock();
        var resolver = new SenderResolver(repositoryMock.Object, cacheMock.Object);

        var result = await resolver.ResolveSenderAsync(new Endpoint(EndpointType.PhoneNumber, "+1234567890"));

        Assert.Null(result);
    }
}
