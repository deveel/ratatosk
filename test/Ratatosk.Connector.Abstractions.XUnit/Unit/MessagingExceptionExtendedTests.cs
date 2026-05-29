namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "MessagingException")]
public class MessagingExceptionExtendedTests
{
    [Fact]
    public void Should_CreateWithErrorCodeAndDomain()
    {
        var ex = new MessagingException("ERR_001", "DOMAIN", "Error");
        Assert.Equal("ERR_001", ex.ErrorCode);
        Assert.Equal("DOMAIN", ex.ErrorDomain);
    }

    [Fact]
    public void Should_CreateWithInnerException()
    {
        var inner = new Exception("inner");
        var ex = new MessagingException("ERR_002", "DOMAIN", "Error", inner);
        Assert.Same(inner, ex.InnerException);
    }
}
