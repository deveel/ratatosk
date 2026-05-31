using System.ComponentModel.DataAnnotations;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "ChannelSchemaExtensions")]
public class ChannelSchemaExtensionsValidationTests
{
    [Fact]
    public void Should_ValidateStringMaxLength()
    {
        var config = new MessagePropertyConfiguration("Name", DataType.String)
        {
            MaxLength = 5
        };
        var errors = config.Validate("too-long-value").ToList();
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Should_ValidateNumericMaxValue()
    {
        var config = new MessagePropertyConfiguration("Count", DataType.Integer)
        {
            MaxValue = 100
        };
        var errors = config.Validate(200).ToList();
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Should_PassValidation_When_Valid()
    {
        var config = new MessagePropertyConfiguration("Count", DataType.Integer)
        {
            MinValue = 1,
            MaxValue = 100
        };
        var errors = config.Validate(50).ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Should_ValidateRequired()
    {
        var config = new MessagePropertyConfiguration("Name", DataType.String)
        {
            IsRequired = true
        };
        var errors = config.Validate(null).ToList();
        Assert.NotEmpty(errors);
    }
}
