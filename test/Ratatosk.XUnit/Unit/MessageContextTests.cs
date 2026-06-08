namespace Ratatosk.XUnit.Unit;

[Trait("Category", "Unit")]
[Trait("Feature", "MessageContext")]
public class MessageContextTests
{
    [Fact]
    public void Should_StoreAndRetrieveValues()
    {
        var ctx = new MessageContext();
        ctx["tenant_id"] = "tenant-123";
        ctx["user_id"] = "user-456";

        Assert.Equal("tenant-123", ctx["tenant_id"]);
        Assert.Equal("user-456", ctx["user_id"]);
    }

    [Fact]
    public void Should_ReturnNull_ForMissingKey()
    {
        var ctx = new MessageContext();
        Assert.Null(ctx["nonexistent"]);
    }

    [Fact]
    public void Should_BuildWithInitialTuples()
    {
        var ctx = new MessageContext(
            ("tenant_id", "tenant-123"),
            ("user_id", "user-456"),
            ("env", "production")
        );

        Assert.Equal("tenant-123", ctx["tenant_id"]);
        Assert.Equal("user-456", ctx["user_id"]);
        Assert.Equal("production", ctx["env"]);
    }

    [Fact]
    public void Should_SupportFluentWithPattern()
    {
        var ctx = new MessageContext()
            .With("tenant_id", "tenant-123")
            .With("user_id", "user-456");

        Assert.Equal("tenant-123", ctx["tenant_id"]);
        Assert.Equal("user-456", ctx["user_id"]);
    }

    [Fact]
    public void Should_EnumerateAllEntries()
    {
        var ctx = new MessageContext(
            ("key1", "value1"),
            ("key2", "value2")
        );

        Assert.Equal(2, ctx.Data.Count);
        Assert.Contains(ctx.Data, kvp => kvp.Key == "key1" && (string)kvp.Value! == "value1");
        Assert.Contains(ctx.Data, kvp => kvp.Key == "key2" && (string)kvp.Value! == "value2");
    }

    [Fact]
    public void Should_Throw_WhenKeyIsNull()
    {
        var ctx = new MessageContext();
        Assert.Throws<ArgumentNullException>(() => ctx[null!] = "value");
    }

    [Fact]
    public void Should_Throw_WhenKeyIsWhitespace()
    {
        var ctx = new MessageContext();
        Assert.Throws<ArgumentException>(() => ctx["  "] = "value");
    }

    [Fact]
    public void Should_Throw_WhenKeyContainsSpaces()
    {
        var ctx = new MessageContext();
        var ex = Assert.Throws<ArgumentException>(() => ctx["my key"] = "value");
        Assert.Contains("must not contain spaces", ex.Message);
    }

    [Fact]
    public void Should_Throw_WhenFluentWithReceivesSpaces()
    {
        var ctx = new MessageContext();
        var ex = Assert.Throws<ArgumentException>(() => ctx.With("bad key", "value"));
        Assert.Contains("must not contain spaces", ex.Message);
    }

    [Fact]
    public void Should_Throw_WhenInitialTupleKeyContainsSpaces()
    {
        Assert.Throws<ArgumentException>(() =>
            new MessageContext(("bad key", "value")));
    }

    [Fact]
    public void Should_UseOrdinalIgnoreCase_ForKeyComparisons()
    {
        var ctx = new MessageContext();
        ctx["Tenant_ID"] = "tenant-123";

        Assert.Equal("tenant-123", ctx["tenant_id"]);
        Assert.Equal("tenant-123", ctx["TENANT_ID"]);
        Assert.Equal("tenant-123", ctx["Tenant_Id"]);
    }

    [Fact]
    public void Should_AllowNullValues()
    {
        var ctx = new MessageContext();
        ctx["nullable"] = null;

        Assert.Null(ctx["nullable"]);
        Assert.True(ctx.Data.ContainsKey("nullable"));
    }

    [Fact]
    public void Should_OverwriteExistingKey()
    {
        var ctx = new MessageContext(("key", "original"));
        ctx["key"] = "overwritten";

        Assert.Equal("overwritten", ctx["key"]);
        Assert.Single(ctx.Data);
    }

    [Fact]
    public void StaticValidateKey_ShouldNotThrow_ForValidKey()
    {
        MessageContext.ValidateKey("valid_key");
        MessageContext.ValidateKey("another-valid.key");
    }

    [Fact]
    public void StaticValidateKey_ShouldThrow_ForNullKey()
    {
        Assert.Throws<ArgumentNullException>(() => MessageContext.ValidateKey(null!));
    }

    [Fact]
    public void StaticValidateKey_ShouldThrow_ForSpaces()
    {
        Assert.Throws<ArgumentException>(() => MessageContext.ValidateKey("has space"));
    }

    [Fact]
    public void Data_ShouldBePubliclyAccessible_AndMutableViaIndexer()
    {
        var ctx = new MessageContext();
        ctx["a"] = 1;
        ctx["b"] = "two";

        Assert.Equal(2, ctx.Data.Count);
        Assert.Equal(1, ctx.Data["a"]);
        Assert.Equal("two", ctx.Data["b"]);
    }
}
