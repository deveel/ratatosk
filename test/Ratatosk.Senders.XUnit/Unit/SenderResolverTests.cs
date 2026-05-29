using Moq;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "SenderResolver")]
public class SenderResolverTests
{
    private static SenderEntity CreateEntity(string name = "test-sender", string endpointType = "phone", string address = "+1234567890", bool isActive = true) => new()
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

    private static Mock<ISenderRegistry> CreateRegistryMock(SenderEntity? byName = null, SenderEntity? byEndpoint = null)
    {
        var mock = new Mock<ISenderRegistry>(MockBehavior.Strict);

        mock.Setup(x => x.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string name, CancellationToken _) =>
                byName != null && name == byName.Name ? byName : null);

        mock.Setup(x => x.FindByEndpointAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string address, string type, CancellationToken _) =>
                byEndpoint != null && address == byEndpoint.Address && type == byEndpoint.EndpointType ? byEndpoint : null);

        return mock;
    }

    private static Mock<ISenderCache> CreateCacheMock(SenderEntity? cached = null)
    {
        var mock = new Mock<ISenderCache>(MockBehavior.Strict);

        if (cached != null)
            mock.Setup(x => x.GetByNameAsync(cached.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cached);
        else
            mock.Setup(x => x.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SenderEntity?)null);

        mock.Setup(x => x.SetByNameAsync(It.IsAny<string>(), It.IsAny<SenderEntity>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        mock.Setup(x => x.RemoveByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        return mock;
    }

    private static Mock<ISenderSelector> CreateSelectorMock()
    {
        return new Mock<ISenderSelector>(MockBehavior.Strict);
    }

    [Fact]
    public async Task Should_ResolveByNameFromCache_When_SenderRefAndCached()
    {
        var entity = CreateEntity("my-sender");
        var registryMock = CreateRegistryMock();
        var cacheMock = CreateCacheMock(entity);
        var selectorMock = CreateSelectorMock();
        var resolver = new SenderResolver(registryMock.Object, cacheMock.Object, selectorMock.Object);

        var result = await resolver.ResolveSenderAsync(new SenderRef("my-sender"));

        Assert.NotNull(result);
        Assert.Equal("my-sender", result.Name);
        registryMock.Verify(x => x.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_ResolveByNameFromRegistry_When_NotCached()
    {
        var entity = CreateEntity("my-sender");
        var registryMock = CreateRegistryMock(byName: entity);
        var cacheMock = CreateCacheMock();
        var selectorMock = CreateSelectorMock();
        var resolver = new SenderResolver(registryMock.Object, cacheMock.Object, selectorMock.Object);

        var result = await resolver.ResolveSenderAsync(new SenderRef("my-sender"));

        Assert.NotNull(result);
        Assert.Equal("my-sender", result.Name);
        registryMock.Verify(x => x.FindByNameAsync("my-sender", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_CacheResult_When_ResolvedByNameFromRegistry()
    {
        var entity = CreateEntity("my-sender");
        var registryMock = CreateRegistryMock(byName: entity);
        var cacheMock = CreateCacheMock();
        var selectorMock = CreateSelectorMock();
        var resolver = new SenderResolver(registryMock.Object, cacheMock.Object, selectorMock.Object);

        await resolver.ResolveSenderAsync(new SenderRef("my-sender"));

        cacheMock.Verify(x => x.SetByNameAsync("my-sender", entity, It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnNull_When_SenderRefNameNotFound()
    {
        var registryMock = CreateRegistryMock();
        var cacheMock = CreateCacheMock();
        var selectorMock = CreateSelectorMock();
        var resolver = new SenderResolver(registryMock.Object, cacheMock.Object, selectorMock.Object);

        var result = await resolver.ResolveSenderAsync(new SenderRef("unknown-sender"));

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ResolveByEndpoint_When_ConcreteSender()
    {
        var entity = CreateEntity("email-sender", endpointType: "email", address: "test@example.com");
        var registryMock = CreateRegistryMock(byEndpoint: entity);
        var cacheMock = CreateCacheMock();
        var selectorMock = CreateSelectorMock();
        var resolver = new SenderResolver(registryMock.Object, cacheMock.Object, selectorMock.Object);

        var result = await resolver.ResolveSenderAsync(new EmailSender("test@example.com"));

        Assert.NotNull(result);
        Assert.Equal("email-sender", result.Name);
        registryMock.Verify(x => x.FindByEndpointAsync("test@example.com", "email", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnNull_When_ConcreteSenderNotInRegistry()
    {
        var registryMock = CreateRegistryMock();
        var cacheMock = CreateCacheMock();
        var selectorMock = CreateSelectorMock();
        var resolver = new SenderResolver(registryMock.Object, cacheMock.Object, selectorMock.Object);

        var result = await resolver.ResolveSenderAsync(new EmailSender("unknown@example.com"));

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ReturnNull_When_ConcreteSenderIsInactive()
    {
        var entity = CreateEntity("inactive-sender", endpointType: "email", address: "inactive@example.com", isActive: false);
        var registryMock = CreateRegistryMock(byEndpoint: entity);
        var cacheMock = CreateCacheMock();
        var selectorMock = CreateSelectorMock();
        var resolver = new SenderResolver(registryMock.Object, cacheMock.Object, selectorMock.Object);

        var result = await resolver.ResolveSenderAsync(new EmailSender("inactive@example.com"));

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_MapToPhoneSender_When_EndpointTypeIsPhone()
    {
        var entity = CreateEntity("phone-sender", endpointType: "phone", address: "+1234567890");
        var registryMock = CreateRegistryMock(byName: entity);
        var cacheMock = CreateCacheMock();
        var selectorMock = CreateSelectorMock();
        var resolver = new SenderResolver(registryMock.Object, cacheMock.Object, selectorMock.Object);

        var result = await resolver.ResolveSenderAsync(new SenderRef("phone-sender"));

        Assert.NotNull(result);
        Assert.IsType<PhoneSender>(result);
        var phoneSender = (PhoneSender)result;
        Assert.Equal("+1234567890", phoneSender.PhoneNumber);
    }

    [Fact]
    public async Task Should_MapToAlphaNumericSender_When_EndpointTypeIsLabel()
    {
        var entity = CreateEntity("brand-sender", endpointType: "label", address: "MyBrand");
        var registryMock = CreateRegistryMock(byName: entity);
        var cacheMock = CreateCacheMock();
        var selectorMock = CreateSelectorMock();
        var resolver = new SenderResolver(registryMock.Object, cacheMock.Object, selectorMock.Object);

        var result = await resolver.ResolveSenderAsync(new SenderRef("brand-sender"));

        Assert.NotNull(result);
        Assert.IsType<AlphaNumericSender>(result);
        var alphaSender = (AlphaNumericSender)result;
        Assert.Equal("MyBrand", alphaSender.BrandName);
    }

    [Fact]
    public async Task Should_MapToEmailSender_When_EndpointTypeIsEmail()
    {
        var entity = CreateEntity("email-sender", endpointType: "email", address: "test@example.com");
        var registryMock = CreateRegistryMock(byName: entity);
        var cacheMock = CreateCacheMock();
        var selectorMock = CreateSelectorMock();
        var resolver = new SenderResolver(registryMock.Object, cacheMock.Object, selectorMock.Object);

        var result = await resolver.ResolveSenderAsync(new SenderRef("email-sender"));

        Assert.NotNull(result);
        Assert.IsType<EmailSender>(result);
        var emailSender = (EmailSender)result;
        Assert.Equal("test@example.com", emailSender.Address);
    }

    [Fact]
    public async Task Should_MapToBotSender_When_EndpointTypeIsId()
    {
        var entity = CreateEntity("bot-sender", endpointType: "id", address: "bot-123");
        var registryMock = CreateRegistryMock(byName: entity);
        var cacheMock = CreateCacheMock();
        var selectorMock = CreateSelectorMock();
        var resolver = new SenderResolver(registryMock.Object, cacheMock.Object, selectorMock.Object);

        var result = await resolver.ResolveSenderAsync(new SenderRef("bot-sender"));

        Assert.NotNull(result);
        Assert.IsType<BotSender>(result);
        var botSender = (BotSender)result;
        Assert.Equal("bot-123", botSender.PlatformId);
    }

    [Fact]
    public async Task Should_LogWarning_When_SenderRefNotFound()
    {
        var registryMock = CreateRegistryMock();
        var cacheMock = CreateCacheMock();
        var selectorMock = CreateSelectorMock();
        var resolver = new SenderResolver(registryMock.Object, cacheMock.Object, selectorMock.Object);

        var result = await resolver.ResolveSenderAsync(new SenderRef("missing"));

        Assert.Null(result);
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_RegistryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SenderResolver(null!, CreateCacheMock().Object, CreateSelectorMock().Object));
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_CacheIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SenderResolver(CreateRegistryMock().Object, null!, CreateSelectorMock().Object));
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_SelectorIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SenderResolver(CreateRegistryMock().Object, CreateCacheMock().Object, null!));
    }
}
