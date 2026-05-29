namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "MessagePropertyConfiguration")]
public class MessagePropertyConfigurationExtendedTests
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
    public void Should_ValidateStringMinLength()
    {
        var config = new MessagePropertyConfiguration("Name", DataType.String)
        {
            MinLength = 3
        };
        var errors = config.Validate("ab").ToList();
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Should_PassValidation_When_StringWithinConstraints()
    {
        var config = new MessagePropertyConfiguration("Name", DataType.String)
        {
            MinLength = 1,
            MaxLength = 10
        };
        var errors = config.Validate("hello").ToList();
        Assert.Empty(errors);
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
    public void Should_ValidateNumericMinValue()
    {
        var config = new MessagePropertyConfiguration("Count", DataType.Integer)
        {
            MinValue = 10
        };
        var errors = config.Validate(5).ToList();
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Should_PassValidation_When_NumericWithinConstraints()
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

    [Fact]
    public void Should_ReturnTypeName()
    {
        Assert.Equal("String", DataType.String.ToString());
        Assert.Equal("Integer", DataType.Integer.ToString());
        Assert.Equal("Boolean", DataType.Boolean.ToString());
    }


}
