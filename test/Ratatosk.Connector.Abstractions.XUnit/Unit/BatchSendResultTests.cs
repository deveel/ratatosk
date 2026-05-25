namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Feature", "BatchSendResult")]
public class BatchSendResultTests
{
    [Fact]
    public void Should_CreateWithBatchId()
    {
        var result = new BatchSendResult("batch-1", null);
        Assert.Equal("batch-1", result.BatchId);
        Assert.Null(result.RemoteBatchId);
        Assert.Empty(result.MessageResults);
    }

    [Fact]
    public void Should_CreateWithRemoteBatchId()
    {
        var result = new BatchSendResult("b1", "remote-1");
        Assert.Equal("remote-1", result.RemoteBatchId);
    }

    [Fact]
    public void Should_CreateWithMessageResults()
    {
        var results = new Dictionary<string, SendResult>
        {
            ["msg-1"] = new SendResult("msg-1", "r-msg-1")
        };
        var result = new BatchSendResult("b1", "r1", results);
        Assert.Single(result.MessageResults);
        Assert.Equal("r-msg-1", result.MessageResults["msg-1"].RemoteMessageId);
    }

    [Fact]
    public void Should_Throw_When_BatchIdIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new BatchSendResult(null!, null));
    }

    [Fact]
    public void Should_Throw_When_BatchIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new BatchSendResult("", null));
    }

    [Fact]
    public void Should_UseEmptyResults_When_NotProvided()
    {
        var result = new BatchSendResult("b1", null);
        Assert.NotNull(result.MessageResults);
        Assert.Empty(result.MessageResults);
    }
}
