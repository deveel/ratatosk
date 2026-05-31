namespace Ratatosk.Senders;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "SenderEntity")]
public class SenderEntityTests
{
    [Fact]
    public void Should_ImplementISender()
    {
        var entity = new SenderEntity();

        Assert.IsAssignableFrom<ISender>(entity);
    }

    [Fact]
    public void Should_BeActive_ByDefault()
    {
        var entity = new SenderEntity();

        Assert.True(entity.IsActive);
    }

    [Fact]
    public void Should_HaveEmptyStrings_ByDefault()
    {
        var entity = new SenderEntity();

        Assert.Equal(string.Empty, entity.Id);
        Assert.Equal(string.Empty, entity.Name);
        Assert.Equal(string.Empty, entity.DisplayName);
        Assert.Equal(string.Empty, entity.Address);
        Assert.Equal(EndpointType.Any, entity.Type);
    }

    [Fact]
    public void Should_SetAndGet_Id()
    {
        var entity = new SenderEntity { Id = "test-id" };

        Assert.Equal("test-id", entity.Id);
    }

    [Fact]
    public void Should_SetAndGet_Name()
    {
        var entity = new SenderEntity { Name = "test-sender" };

        Assert.Equal("test-sender", entity.Name);
    }

    [Fact]
    public void Should_SetAndGet_DisplayName()
    {
        var entity = new SenderEntity { DisplayName = "Test Sender" };

        Assert.Equal("Test Sender", entity.DisplayName);
    }

    [Fact]
    public void Should_SetAndGet_Address()
    {
        var entity = new SenderEntity { Address = "test@example.com" };

        Assert.Equal("test@example.com", entity.Address);
    }

    [Fact]
    public void Should_SetAndGet_EndpointType()
    {
        var entity = new SenderEntity { Type = EndpointType.EmailAddress };

        Assert.Equal(EndpointType.EmailAddress, entity.Type);
    }

    [Fact]
    public void Should_SetAndGet_IsActive()
    {
        var entity = new SenderEntity();
        entity.Deactivate();

        Assert.False(entity.IsActive);
    }

    [Fact]
    public void Should_ExposeEndpointType_ThroughInterface()
    {
        ISender entity = new SenderEntity { Type = EndpointType.PhoneNumber };

        Assert.Equal(EndpointType.PhoneNumber, entity.Type);
    }

    [Fact]
    public void Should_SetAndGet_CreatedAt()
    {
        var now = DateTime.UtcNow;
        var entity = new SenderEntity { CreatedAt = now };

        Assert.Equal(now, entity.CreatedAt);
    }

    [Fact]
    public void Should_SetAndGet_UpdatedAt()
    {
        var now = DateTime.UtcNow;
        var entity = new SenderEntity { UpdatedAt = now };

        Assert.Equal(now, entity.UpdatedAt);
    }

    [Fact]
    public void Should_SetIsActiveToTrue_When_Activate()
    {
        // Arrange
        var entity = new SenderEntity();
        entity.Deactivate();
        Assert.False(entity.IsActive);

        // Act
        entity.Activate();

        // Assert
        Assert.True(entity.IsActive);
    }

    [Fact]
    public void Should_SetUpdatedAt_When_Activate()
    {
        // Arrange
        var entity = new SenderEntity { UpdatedAt = DateTime.UtcNow.AddDays(-1) };
        var beforeUpdate = entity.UpdatedAt;

        // Act
        entity.Activate();

        // Assert
        Assert.True(entity.UpdatedAt > beforeUpdate);
    }

    [Fact]
    public void Should_SetIsActiveToFalse_When_Deactivate()
    {
        // Arrange
        var entity = new SenderEntity();
        Assert.True(entity.IsActive);

        // Act
        entity.Deactivate();

        // Assert
        Assert.False(entity.IsActive);
    }

    [Fact]
    public void Should_SetUpdatedAt_When_Deactivate()
    {
        // Arrange
        var entity = new SenderEntity { UpdatedAt = DateTime.UtcNow.AddDays(-1) };
        var beforeUpdate = entity.UpdatedAt;

        // Act
        entity.Deactivate();

        // Assert
        Assert.True(entity.UpdatedAt > beforeUpdate);
    }

    [Fact]
    public void Should_UpdateDisplayName_When_Provided()
    {
        // Arrange
        var entity = new SenderEntity { DisplayName = "Old Name" };

        // Act
        entity.Update(displayName: "New Name");

        // Assert
        Assert.Equal("New Name", entity.DisplayName);
    }

    [Fact]
    public void Should_UpdateAddress_When_Provided()
    {
        // Arrange
        var entity = new SenderEntity { Address = "+1111111111" };

        // Act
        entity.Update(address: "+2222222222");

        // Assert
        Assert.Equal("+2222222222", entity.Address);
    }

    [Fact]
    public void Should_UpdateType_When_Provided()
    {
        // Arrange
        var entity = new SenderEntity { Type = EndpointType.PhoneNumber };

        // Act
        entity.Update(type: EndpointType.EmailAddress);

        // Assert
        Assert.Equal(EndpointType.EmailAddress, entity.Type);
    }

    [Fact]
    public void Should_NotChangeDisplayName_When_Null()
    {
        // Arrange
        var entity = new SenderEntity { DisplayName = "Original Name" };

        // Act
        entity.Update(displayName: null);

        // Assert
        Assert.Equal("Original Name", entity.DisplayName);
    }

    [Fact]
    public void Should_NotChangeAddress_When_Null()
    {
        // Arrange
        var entity = new SenderEntity { Address = "+1111111111" };

        // Act
        entity.Update(address: null);

        // Assert
        Assert.Equal("+1111111111", entity.Address);
    }

    [Fact]
    public void Should_NotChangeType_When_Null()
    {
        // Arrange
        var entity = new SenderEntity { Type = EndpointType.PhoneNumber };

        // Act
        entity.Update(type: null);

        // Assert
        Assert.Equal(EndpointType.PhoneNumber, entity.Type);
    }

    [Fact]
    public void Should_SetUpdatedAt_When_Update()
    {
        // Arrange
        var entity = new SenderEntity { UpdatedAt = DateTime.UtcNow.AddDays(-1) };
        var beforeUpdate = entity.UpdatedAt;

        // Act
        entity.Update(displayName: "New Name");

        // Assert
        Assert.True(entity.UpdatedAt > beforeUpdate);
    }

    [Fact]
    public void Should_UpdateMultipleProperties_When_Provided()
    {
        // Arrange
        var entity = new SenderEntity
        {
            DisplayName = "Old Name",
            Address = "+1111111111",
            Type = EndpointType.PhoneNumber
        };

        // Act
        entity.Update(displayName: "New Name", address: "+2222222222", type: EndpointType.EmailAddress);

        // Assert
        Assert.Equal("New Name", entity.DisplayName);
        Assert.Equal("+2222222222", entity.Address);
        Assert.Equal(EndpointType.EmailAddress, entity.Type);
    }
}
