using System.ComponentModel.DataAnnotations;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "ChannelSchemaExtensions")]
public class ChannelSchemaExtensionsAdvancedTests
{
    [Fact]
    public void Should_ValidateRequired_When_PropertyIsRequired()
    {
        var config = new MessagePropertyConfiguration("Name", DataType.String)
        {
            IsRequired = true
        };
        var errors = config.Validate(null).ToList();
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Should_PassOptionalValidation()
    {
        var config = new MessagePropertyConfiguration("Name", DataType.String)
        {
            IsRequired = false
        };
        var errors = config.Validate(null).ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Should_ValidatePattern()
    {
        var config = new MessagePropertyConfiguration("Email", DataType.String)
        {
            Pattern = @"^[a-z]+@[a-z]+\.com$"
        };
        var errors = config.Validate("invalid").ToList();
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Should_PassPatternValidation()
    {
        var config = new MessagePropertyConfiguration("Email", DataType.String)
        {
            Pattern = @"^[a-z]+@[a-z]+\.com$"
        };
        var errors = config.Validate("test@example.com").ToList();
        Assert.Empty(errors);
    }
}
