namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Feature", "Authentication")]
public class AuthenticationValidationResultTests
{
    [Fact]
    public void Should_CreateValid()
    {
        var result = new AuthenticationValidationResult(true, new List<string>());
        Assert.True(result.IsValid);
        Assert.Empty(result.MissingParameters);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Should_CreateInvalid_WithOneMissing()
    {
        var result = new AuthenticationValidationResult(false, new List<string> { "ApiKey" });
        Assert.False(result.IsValid);
        Assert.Single(result.MissingParameters);
        Assert.Contains("ApiKey", result.ErrorMessage);
    }

    [Fact]
    public void Should_CreateInvalid_WithMultipleMissing()
    {
        var result = new AuthenticationValidationResult(false, new List<string> { "Key1", "Key2" });
        Assert.False(result.IsValid);
        Assert.Equal(2, result.MissingParameters.Count);
        Assert.Contains("Key1", result.ErrorMessage);
        Assert.Contains("Key2", result.ErrorMessage);
    }

    [Fact]
    public void Should_CreateInvalid_WithEmptyList()
    {
        var result = new AuthenticationValidationResult(false, new List<string>());
        Assert.False(result.IsValid);
        Assert.Equal("Unknown validation error", result.ErrorMessage);
    }
}
