namespace Ratatosk;

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
