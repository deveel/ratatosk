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
}
