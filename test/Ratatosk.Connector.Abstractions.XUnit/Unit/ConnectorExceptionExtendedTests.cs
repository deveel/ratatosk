namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "ConnectorException")]
public class ConnectorExceptionExtendedTests
{
    [Fact]
    public void Should_CreateWithErrorCodeAndDomain()
    {
        var ex = new ConnectorException("ERR_001", "DOMAIN", "Something went wrong");
        Assert.Equal("ERR_001", ex.ErrorCode);
        Assert.Equal("DOMAIN", ex.ErrorDomain);
        Assert.Equal("Something went wrong", ex.Message);
    }

    [Fact]
    public void Should_CreateWithInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new ConnectorException("ERR_002", "DOMAIN", "Outer error", inner);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void Should_CreateWithCodeAndDomainOnly()
    {
        var ex = new ConnectorException("ERR_003", "DOMAIN");
        Assert.Equal("ERR_003", ex.ErrorCode);
        Assert.Equal("DOMAIN", ex.ErrorDomain);
    }
}
