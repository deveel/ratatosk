using System.ComponentModel.DataAnnotations;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Feature", "ConnectorValidationException")]
public class ConnectorValidationExceptionTests
{
    [Fact]
    public void Should_CreateWithDefaultMessage()
    {
        var results = new[] { new ValidationResult("Error 1") };
        var ex = new ConnectorValidationException("ERR", "Domain", results);
        Assert.Equal("ERR", ex.ErrorCode);
        Assert.Equal("Domain", ex.ErrorDomain);
        Assert.Single(ex.ValidationResults);
    }

    [Fact]
    public void Should_CreateWithCustomMessage()
    {
        var results = new[] { new ValidationResult("Error 1") };
        var ex = new ConnectorValidationException("ERR", "Domain", "Custom message", results);
        Assert.Equal("Custom message", ex.Message);
        Assert.Single(ex.ValidationResults);
    }

    [Fact]
    public void Should_ImplementIValidationError()
    {
        var results = new[] { new ValidationResult("Test") };
        var ex = new ConnectorValidationException("ERR", "Domain", results);
        Assert.IsAssignableFrom<IValidationError>(ex);
    }
}
