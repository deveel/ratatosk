namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "AuthenticationField")]
public class AuthenticationFieldExtendedTests
{
    [Fact]
    public void Should_ToString_ReturnFormattedString()
    {
        var field = new AuthenticationField("ApiKey", DataType.String) { AuthenticationRole = "principal" };
        var str = field.ToString();
        Assert.Contains("ApiKey", str);
        Assert.Contains("principal", str);
    }
}
