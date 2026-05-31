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

    [Fact]
    public async Task FindByNameAsync_ShouldReturnSuccess_WhenSenderExists()
    {
        // Arrange
        var sender = CreateValidSender();
        var repositoryMock = new Mock<ISenderRepository<SenderEntity>>();
        repositoryMock.Setup(r => r.FindByNameAsync("test-sender", It.IsAny<CancellationToken>()))
            .ReturnsAsync(sender);
        
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
        var repositoryMock = new Mock<ISenderRepository<SenderEntity>>();
        repositoryMock.Setup(r => r.FindByNameAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((SenderEntity?)null);
        
        var manager = new SenderManager<SenderEntity>(repositoryMock.Object);

        // Act
        var result = await manager.FindByNameAsync("nonexistent");

        // Assert
        Assert.False(result.IsSuccess());
        Assert.Equal(SenderErrorCodes.SenderNotFound, result.Error?.Code);
    }

    [Fact]
    public async Task FindByEndpointAsync_ShouldReturnSuccess_WhenSenderExists()
    {
        // Arrange
        var sender = CreateValidSender();
        var repositoryMock = new Mock<ISenderRepository<SenderEntity>>();
        repositoryMock.Setup(r => r.FindByEndpointAsync("+1234567890", EndpointType.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sender);
        
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
        var repositoryMock = new Mock<ISenderRepository<SenderEntity>>();
        repositoryMock.Setup(r => r.FindByEndpointAsync("+9999999999", EndpointType.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SenderEntity?)null);
        
        var manager = new SenderManager<SenderEntity>(repositoryMock.Object);

        // Act
        var result = await manager.FindByEndpointAsync("+9999999999", EndpointType.PhoneNumber);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.Equal(SenderErrorCodes.SenderNotFound, result.Error?.Code);
    }

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnActiveSenders()
    {
        // Arrange
        var activeSender = CreateValidSender(id: "1", name: "active");
        
        var repositoryMock = new Mock<ISenderRepository<SenderEntity>>();
        repositoryMock.Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SenderEntity> { activeSender });
        
        var manager = new SenderManager<SenderEntity>(repositoryMock.Object);

        // Act
        var result = await manager.GetAllActiveAsync();

        // Assert
        Assert.True(result.IsSuccess());
        Assert.Single(result.Value!);
        Assert.Equal("active", result.Value[0].Name);
    }
}
