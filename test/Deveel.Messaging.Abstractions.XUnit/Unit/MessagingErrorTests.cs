namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Feature", "MessagingError")]
public class MessagingErrorTests
{
    [Fact]
    public void Should_CreateWithCode()
    {
        var error = new MessagingError("ERR_001");
        Assert.Equal("ERR_001", error.ErrorCode);
        Assert.Null(error.ErrorMessage);
    }

    [Fact]
    public void Should_CreateWithCodeAndMessage()
    {
        var error = new MessagingError("ERR_002", "Something went wrong");
        Assert.Equal("ERR_002", error.ErrorCode);
        Assert.Equal("Something went wrong", error.ErrorMessage);
    }

    [Fact]
    public void Should_Throw_When_CodeIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new MessagingError(null!));
    }

    [Fact]
    public void Should_Throw_When_CodeIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new MessagingError(""));
    }
}
