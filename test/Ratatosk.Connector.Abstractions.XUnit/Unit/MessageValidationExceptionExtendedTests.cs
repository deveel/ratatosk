using System.ComponentModel.DataAnnotations;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "MessageValidationException")]
public class MessageValidationExceptionExtendedTests
{
    [Fact]
    public void Should_CreateWithValidationResults()
    {
        var results = new List<ValidationResult>
        {
            new ValidationResult("Field is required", new[] { "Field1" })
        };
        var ex = new MessageValidationException("MSG_001", "DOMAIN", results);
        Assert.Single(ex.ValidationResults);
        Assert.Equal("MSG_001", ex.ErrorCode);
        Assert.Equal("DOMAIN", ex.ErrorDomain);
    }

    [Fact]
    public void Should_CreateWithValidationResultsAndMessage()
    {
        var results = new List<ValidationResult>
        {
            new ValidationResult("Error")
        };
        var ex = new MessageValidationException("MSG_002", "DOMAIN", "Custom message", results);
        Assert.Single(ex.ValidationResults);
        Assert.Equal("Custom message", ex.Message);
    }
}
