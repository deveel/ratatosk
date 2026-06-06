namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Feature", "RetryPolicy")]
public class ResultRetryExtensionsTests
{
    [Fact]
    public void Should_ReturnOne_When_SendResultHasNoRetryAttempts()
    {
        var result = new SendResult("msg-1", "remote-1");
        Assert.Equal(1, result.GetRetryAttempts());
    }

    [Fact]
    public void Should_ReturnCorrectAttempts_When_SendResultHasRetryAttempts()
    {
        var result = new SendResult("msg-1", "remote-1")
        {
            AdditionalData =
            {
                ["RetryAttempts"] = 3
            }
        };
        Assert.Equal(3, result.GetRetryAttempts());
    }

    [Fact]
    public void Should_ReturnOne_When_StatusUpdateResultHasNoRetryAttempts()
    {
        var result = new StatusUpdateResult("msg-1", MessageStatus.Delivered);
        Assert.Equal(1, result.GetRetryAttempts());
    }

    [Fact]
    public void Should_ReturnCorrectAttempts_When_StatusUpdateResultHasRetryAttempts()
    {
        var result = new StatusUpdateResult("msg-1", MessageStatus.Delivered)
        {
            AdditionalData =
            {
                ["RetryAttempts"] = 2
            }
        };
        Assert.Equal(2, result.GetRetryAttempts());
    }

    [Fact]
    public void Should_ReturnOne_When_StatusInfoHasNoRetryAttempts()
    {
        var info = new StatusInfo("OK");
        Assert.Equal(1, info.GetRetryAttempts());
    }

    [Fact]
    public void Should_ReturnCorrectAttempts_When_StatusInfoHasRetryAttempts()
    {
        var info = new StatusInfo("OK")
        {
            AdditionalData =
            {
                ["RetryAttempts"] = 1
            }
        };
        Assert.Equal(1, info.GetRetryAttempts());
    }

    [Fact]
    public void Should_ReturnOne_When_RetryAttemptsIsWrongType()
    {
        var result = new SendResult("msg-1", "remote-1")
        {
            AdditionalData =
            {
                ["RetryAttempts"] = "not-an-int"
            }
        };
        Assert.Equal(1, result.GetRetryAttempts());
    }
}
