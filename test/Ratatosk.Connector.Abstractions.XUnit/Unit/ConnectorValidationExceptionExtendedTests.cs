using System.ComponentModel.DataAnnotations;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "ConnectorValidationException")]
public class ConnectorValidationExceptionExtendedTests
{
    [Fact]
    public void Should_CreateWithValidationResults()
    {
        var results = new List<ValidationResult>
        {
            new ValidationResult("Invalid config")
        };
        var ex = new ConnectorValidationException("ERR_001", "DOMAIN", results);
        Assert.Single(ex.ValidationResults);
        Assert.Equal("ERR_001", ex.ErrorCode);
    }

    [Fact]
    public void Should_CreateWithValidationResultsAndMessage()
    {
        var results = new List<ValidationResult>
        {
            new ValidationResult("Error")
        };
        var ex = new ConnectorValidationException("ERR_002", "DOMAIN", "Custom", results);
        Assert.Equal("Custom", ex.Message);
    }
}
