namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Feature", "ConnectorException")]
public class ConnectorExceptionTests
{
    [Fact]
    public void Should_CreateWithCodeAndDomain()
    {
        var ex = new ConnectorException("ERR_001", "TestDomain");
        Assert.Equal("ERR_001", ex.ErrorCode);
        Assert.Equal("TestDomain", ex.ErrorDomain);
    }

    [Fact]
    public void Should_CreateWithMessage()
    {
        var ex = new ConnectorException("ERR_001", "TestDomain", "Test message");
        Assert.Equal("Test message", ex.Message);
    }

    [Fact]
    public void Should_CreateWithInnerException()
    {
        var inner = new Exception("Inner");
        var ex = new ConnectorException("ERR_001", "TestDomain", "Outer", inner);
        Assert.Same(inner, ex.InnerException);
    }
}
