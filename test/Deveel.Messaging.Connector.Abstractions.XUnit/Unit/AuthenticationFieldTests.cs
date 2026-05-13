namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Feature", "AuthenticationField")]
public class AuthenticationFieldTests
{
    [Fact]
    public void Should_CreateWithNameAndType()
    {
        var field = new AuthenticationField("ApiKey", DataType.String);
        Assert.Equal("ApiKey", field.FieldName);
        Assert.Equal(DataType.String, field.DataType);
        Assert.False(field.IsSensitive);
    }

    [Fact]
    public void Should_Throw_When_NameIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new AuthenticationField(null!, DataType.String));
    }

    [Fact]
    public void Should_Throw_When_NameIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new AuthenticationField("", DataType.String));
    }

    [Fact]
    public void Should_Validate_PresentValue()
    {
        var field = new AuthenticationField("key", DataType.String);
        var settings = new ConnectionSettings().SetParameter("key", "value");
        var errors = field.Validate(settings);
        Assert.Empty(errors);
    }

    [Fact]
    public void Should_Validate_MissingValue()
    {
        var field = new AuthenticationField("missing", DataType.String);
        var settings = new ConnectionSettings();
        var errors = field.Validate(settings);
        Assert.NotEmpty(errors);
        Assert.Contains("missing", errors[0]);
    }

    [Fact]
    public void Should_Validate_TypeCompatibility()
    {
        var field = new AuthenticationField("port", DataType.Integer);
        var settings = new ConnectionSettings().SetParameter("port", 8080);
        var errors = field.Validate(settings);
        Assert.Empty(errors);
    }

    [Fact]
    public void Should_Validate_TypeMismatch()
    {
        var field = new AuthenticationField("flag", DataType.Boolean);
        var settings = new ConnectionSettings().SetParameter("flag", "not-a-bool");
        var errors = field.Validate(settings);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Should_Validate_AllowedValues()
    {
        var field = new AuthenticationField("mode", DataType.String)
        {
            AllowedValues = new List<object> { "live", "test" }
        };
        var settings = new ConnectionSettings().SetParameter("mode", "live");
        var errors = field.Validate(settings);
        Assert.Empty(errors);
    }

    [Fact]
    public void Should_Validate_DisallowedValue()
    {
        var field = new AuthenticationField("mode", DataType.String)
        {
            AllowedValues = new List<object> { "live", "test" }
        };
        var settings = new ConnectionSettings().SetParameter("mode", "invalid");
        var errors = field.Validate(settings);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Should_Throw_When_ValidateWithNullSettings()
    {
        var field = new AuthenticationField("k", DataType.String);
        Assert.Throws<ArgumentNullException>(() => field.Validate(null!));
    }

    [Fact]
    public void Should_ToString_IncludeFieldName()
    {
        var field = new AuthenticationField("ApiKey", DataType.String) { DisplayName = "API Key" };
        var str = field.ToString();
        Assert.Contains("API Key", str);
        Assert.Contains("String", str);
    }

    [Fact]
    public void Should_ToString_IncludeRole()
    {
        var field = new AuthenticationField("username", DataType.String)
        {
            AuthenticationRole = "Username"
        };
        var str = field.ToString();
        Assert.Contains("Username", str);
    }

    [Fact]
    public void Should_SupportSettableProperties()
    {
        var field = new AuthenticationField("f", DataType.String)
        {
            IsSensitive = true,
            DisplayName = "Field",
            Description = "A field",
            AllowedValues = new[] { "a", "b" },
            AuthenticationRole = "Role"
        };
        Assert.True(field.IsSensitive);
        Assert.Equal("Field", field.DisplayName);
        Assert.Equal("A field", field.Description);
        Assert.Equal(2, field.AllowedValues!.Count);
        Assert.Equal("Role", field.AuthenticationRole);
    }
}
