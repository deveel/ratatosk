namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Feature", "MessageProperty")]
public class MessagePropertyTests
{
    [Fact]
    public void Should_CreateWithDefaultConstructor()
    {
        var prop = new MessageProperty();
        Assert.Equal("", prop.Name);
        Assert.Null(prop.Value);
        Assert.False(prop.IsSensitive);
    }

    [Fact]
    public void Should_CreateWithNameAndValue()
    {
        var prop = new MessageProperty("key", "value");
        Assert.Equal("key", prop.Name);
        Assert.Equal("value", prop.Value);
        Assert.False(prop.IsSensitive);
    }

    [Fact]
    public void Should_CreateFromInterface()
    {
        var source = new MessageProperty("src", 42) { IsSensitive = true };
        var prop = new MessageProperty((IMessageProperty)source);

        Assert.Equal("src", prop.Name);
        Assert.Equal(42, prop.Value);
        Assert.True(prop.IsSensitive);
    }

    [Fact]
    public void Should_CreateFromInterface_WhenNull()
    {
        var prop = new MessageProperty((IMessageProperty)null!);
        Assert.Equal("", prop.Name);
        Assert.Null(prop.Value);
        Assert.False(prop.IsSensitive);
    }

    [Fact]
    public void Should_SupportSettableProperties()
    {
        var prop = new MessageProperty();
        prop.Name = "updated";
        prop.Value = 99;
        prop.IsSensitive = true;

        Assert.Equal("updated", prop.Name);
        Assert.Equal(99, prop.Value);
        Assert.True(prop.IsSensitive);
    }

    [Fact]
    public void Should_CreateSensitiveProperty()
    {
        var prop = MessageProperty.Sensitive("secret", "s3cr3t");
        Assert.Equal("secret", prop.Name);
        Assert.Equal("s3cr3t", prop.Value);
        Assert.True(prop.IsSensitive);
    }
}
