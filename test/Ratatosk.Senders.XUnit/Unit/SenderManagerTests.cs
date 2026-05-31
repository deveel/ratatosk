using Deveel;
using Kista;
using Moq;

namespace Ratatosk.Senders;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "SenderManager")]
public class SenderManagerTests
{
    private static SenderEntity CreateValidSender(string id = "test-id", string name = "test-sender")
    {
        var sender = new SenderEntity
        {
            Id = id,
            Name = name,
            DisplayName = "Test Sender",
            Address = "+1234567890",
            Type = EndpointType.PhoneNumber,
            CreatedAt = DateTime.UtcNow
        };
        sender.Activate();
        return sender;
    }

    private static Mock<ISenderRepository<SenderEntity>> CreateRepositoryMock(SenderEntity? byName = null, SenderEntity? byEndpoint = null, IList<SenderEntity>? activeSenders = null, SenderEntity? findById = null)
    {
        var mock = new Mock<ISenderRepository<SenderEntity>>(MockBehavior.Loose);

        mock.Setup(r => r.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string name, CancellationToken _) => byName != null && name == byName.Name ? byName : null);

        mock.Setup(r => r.FindByEndpointAsync(It.IsAny<string>(), It.IsAny<EndpointType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string address, EndpointType type, CancellationToken _) =>
                byEndpoint != null && address == byEndpoint.Address && type == byEndpoint.Type ? byEndpoint : null);

        mock.Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeSenders ?? new List<SenderEntity>());

        mock.Setup(r => r.SetActiveAsync(It.IsAny<SenderEntity>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Callback<SenderEntity, bool, CancellationToken>((sender, isActive, _) =>
            {
                if (isActive) sender.Activate(); else sender.Deactivate();
            })
            .Returns(Task.CompletedTask);

        mock.Setup(r => r.FindAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken _) => findById ?? byName);

        return mock;
    }
    public async Task FindByNameAsync_ShouldReturnSuccess_WhenSenderExists()
    {
        // Arrange
        var sender = CreateValidSender();
        var repositoryMock = CreateRepositoryMock(byName: sender);
        var manager = new SenderManager<SenderEntity>(repositoryMock.Object);

        // Act
        var result = await manager.FindByNameAsync("test-sender");

        // Assert
        Assert.True(result.IsSuccess());
        Assert.Equal("test-sender", result.Value!.Name);
    }

    [Fact]
    public async Task FindByNameAsync_ShouldReturnFail_WhenSenderNotFound()
    {
        // Arrange
        var repositoryMock = CreateRepositoryMock();
        var manager = new SenderManager<SenderEntity>(repositoryMock.Object);

        // Act
        var result = await manager.FindByNameAsync("nonexistent");

        // Assert
        Assert.False(result.IsSuccess());
        Assert.Equal(SenderErrorCodes.SenderNotFound, result.Error?.Code);
    }

    [Fact]
    public async Task FindByNameAsync_ShouldReturnFail_WhenRepositoryThrows()
    {
        // Arrange
        var repositoryMock = new Mock<ISenderRepository<SenderEntity>>(MockBehavior.Strict);
        repositoryMock.Setup(r => r.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));
        var manager = new SenderManager<SenderEntity>(repositoryMock.Object);

        // Act
        var result = await manager.FindByNameAsync("test-sender");

        // Assert
        Assert.False(result.IsSuccess());
        Assert.Equal(SenderErrorCodes.SenderError, result.Error?.Code);
    }

    [Fact]
    public async Task FindByEndpointAsync_ShouldReturnSuccess_WhenSenderExists()
    {
        // Arrange
        var sender = CreateValidSender();
        var repositoryMock = CreateRepositoryMock(byEndpoint: sender);
        var manager = new SenderManager<SenderEntity>(repositoryMock.Object);

        // Act
        var result = await manager.FindByEndpointAsync("+1234567890", EndpointType.PhoneNumber);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.Equal("+1234567890", result.Value!.Address);
    }

    [Fact]
    public async Task FindByEndpointAsync_ShouldReturnFail_WhenSenderNotFound()
    {
        // Arrange
        var repositoryMock = CreateRepositoryMock();
        var manager = new SenderManager<SenderEntity>(repositoryMock.Object);

        // Act
        var result = await manager.FindByEndpointAsync("+9999999999", EndpointType.PhoneNumber);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.Equal(SenderErrorCodes.SenderNotFound, result.Error?.Code);
    }

    [Fact]
    public async Task FindByEndpointAsync_ShouldReturnFail_WhenRepositoryThrows()
    {
        // Arrange
        var repositoryMock = new Mock<ISenderRepository<SenderEntity>>(MockBehavior.Strict);
        repositoryMock.Setup(r => r.FindByEndpointAsync(It.IsAny<string>(), It.IsAny<EndpointType>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));
        var manager = new SenderManager<SenderEntity>(repositoryMock.Object);

        // Act
        var result = await manager.FindByEndpointAsync("+1234567890", EndpointType.PhoneNumber);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.Equal(SenderErrorCodes.SenderError, result.Error?.Code);
    }

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnActiveSenders()
    {
        // Arrange
        var activeSender = CreateValidSender(id: "1", name: "active");
        var repositoryMock = CreateRepositoryMock(activeSenders: new List<SenderEntity> { activeSender });
        var manager = new SenderManager<SenderEntity>(repositoryMock.Object);

        // Act
        var result = await manager.GetAllActiveAsync();

        // Assert
        Assert.True(result.IsSuccess());
        Assert.Single(result.Value!);
        Assert.Equal("active", result.Value[0].Name);
    }

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnFail_WhenRepositoryThrows()
    {
        // Arrange
        var repositoryMock = new Mock<ISenderRepository<SenderEntity>>(MockBehavior.Strict);
        repositoryMock.Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));
        var manager = new SenderManager<SenderEntity>(repositoryMock.Object);

        // Act
        var result = await manager.GetAllActiveAsync();

        // Assert
        Assert.False(result.IsSuccess());
        Assert.Equal(SenderErrorCodes.SenderError, result.Error?.Code);
    }

    [Fact]
    public async Task Should_ReturnFail_WhenRepositoryNotISenderRepository()
    {
        // Arrange
        var genericRepoMock = new Mock<IRepository<SenderEntity>>(MockBehavior.Loose);
        var manager = new SenderManager<SenderEntity>(genericRepoMock.Object);

        // Act
        var result = await manager.FindByNameAsync("test");

        // Assert
        Assert.False(result.IsSuccess());
        Assert.Equal(SenderErrorCodes.SenderError, result.Error?.Code);
    }
}
