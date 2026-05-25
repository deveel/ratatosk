namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Feature", "MessagePropertyConfiguration")]
public class MessagePropertyConfigurationTests
{
    [Fact]
    public void Should_CreateWithNameAndType()
    {
        var config = new MessagePropertyConfiguration("subject", DataType.String);
        Assert.Equal("subject", config.Name);
        Assert.Equal(DataType.String, config.DataType);
    }

    [Fact]
    public void Should_Throw_When_NameIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new MessagePropertyConfiguration(null!, DataType.String));
    }

    [Fact]
    public void Should_Validate_When_RequiredAndMissing()
    {
        var config = new MessagePropertyConfiguration("required", DataType.String) { IsRequired = true };
        var errors = config.Validate(null).ToList();
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Should_Pass_When_NotRequiredAndNull()
    {
        var config = new MessagePropertyConfiguration("optional", DataType.String) { IsRequired = false };
        var errors = config.Validate(null).ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Should_Validate_StringMinLength()
    {
        var config = new MessagePropertyConfiguration("name", DataType.String) { MinLength = 3 };
        var errors = config.Validate("ab").ToList();
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Should_Pass_StringLength_When_WithinLimits()
    {
        var config = new MessagePropertyConfiguration("name", DataType.String) { MinLength = 2, MaxLength = 10 };
        var errors = config.Validate("hello").ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Should_Validate_StringMaxLength()
    {
        var config = new MessagePropertyConfiguration("name", DataType.String) { MaxLength = 5 };
        var errors = config.Validate("too-long").ToList();
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Should_Validate_StringPattern()
    {
        var config = new MessagePropertyConfiguration("email", DataType.String) { Pattern = @"^.+@.+\..+$" };
        var errors = config.Validate("not-an-email").ToList();
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Should_Pass_StringPattern_WhenMatches()
    {
        var config = new MessagePropertyConfiguration("email", DataType.String) { Pattern = @"^.+@.+\..+$" };
        var errors = config.Validate("user@example.com").ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Should_Validate_NumericMinValue()
    {
        var config = new MessagePropertyConfiguration("age", DataType.Integer) { MinValue = 18 };
        var errors = config.Validate(15).ToList();
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Should_Pass_NumericRange_WhenWithinLimits()
    {
        var config = new MessagePropertyConfiguration("age", DataType.Integer) { MinValue = 18, MaxValue = 99 };
        var errors = config.Validate(25).ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Should_Validate_NumericMaxValue()
    {
        var config = new MessagePropertyConfiguration("age", DataType.Integer) { MaxValue = 99 };
        var errors = config.Validate(100).ToList();
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Should_Validate_AllowedValues()
    {
        var config = new MessagePropertyConfiguration("priority", DataType.String)
        {
            AllowedValues = new List<object> { "low", "high" }
        };
        var errors = config.Validate("high").ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Should_Validate_DisallowedValue()
    {
        var config = new MessagePropertyConfiguration("priority", DataType.String)
        {
            AllowedValues = new List<object> { "low", "high" }
        };
        var errors = config.Validate("urgent").ToList();
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Should_Validate_TypeMismatch()
    {
        var config = new MessagePropertyConfiguration("flag", DataType.Boolean);
        var errors = config.Validate("not-bool").ToList();
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Should_RunCustomValidator()
    {
        var config = new MessagePropertyConfiguration("custom", DataType.String)
        {
            CustomValidator = val => {
                if (val?.ToString() == "ok")
                    return Enumerable.Empty<System.ComponentModel.DataAnnotations.ValidationResult>();
                return new[] { new System.ComponentModel.DataAnnotations.ValidationResult("Custom error") };
            }
        };
        var ok = config.Validate("ok").ToList();
        Assert.Empty(ok);

        var fail = config.Validate("bad").ToList();
        Assert.NotEmpty(fail);
    }

    [Fact]
    public void Should_Validate_EmptyString_WhenMinLengthPositive()
    {
        var config = new MessagePropertyConfiguration("name", DataType.String) { MinLength = 1 };
        var errors = config.Validate("").ToList();
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Should_SupportProperties()
    {
        var config = new MessagePropertyConfiguration("x", DataType.String)
        {
            DisplayName = "Display",
            Description = "Desc",
            IsRequired = true,
            IsSensitive = true,
            MinLength = 1,
            MaxLength = 100,
            MinValue = 0,
            MaxValue = 99,
            Pattern = ".*",
            AllowedValues = new[] { "a", "b" },
            CustomValidator = _ => Enumerable.Empty<System.ComponentModel.DataAnnotations.ValidationResult>()
        };

        Assert.Equal("Display", config.DisplayName);
        Assert.Equal("Desc", config.Description);
        Assert.True(config.IsRequired);
        Assert.True(config.IsSensitive);
        Assert.Equal(1, config.MinLength);
        Assert.Equal(100, config.MaxLength);
        Assert.Equal(0, config.MinValue);
        Assert.Equal(99, config.MaxValue);
        Assert.Equal(".*", config.Pattern);
        Assert.Equal(2, config.AllowedValues!.Count);
        Assert.NotNull(config.CustomValidator);
    }
}
