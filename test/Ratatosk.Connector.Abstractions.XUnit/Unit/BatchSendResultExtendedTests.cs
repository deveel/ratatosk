namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "BatchSendResult")]
public class BatchSendResultExtendedTests
{
    [Fact]
    public void Should_CreateWithBatchIdAndResults()
    {
        var results = new Dictionary<string, SendResult>
        {
            ["msg1"] = new SendResult("msg1", "remote1")
        };
        var batchResult = new BatchSendResult("batch1", "batch1", results);
        Assert.Equal("batch1", batchResult.BatchId);
        Assert.Single(batchResult.MessageResults);
    }

    [Fact]
    public void Should_CreateWithEmptyResults()
    {
        var batchResult = new BatchSendResult("batch1", "batch1", new Dictionary<string, SendResult>());
        Assert.Empty(batchResult.MessageResults);
    }

    [Fact]
    public void Should_CreateWithNullResults()
    {
        var batchResult = new BatchSendResult("batch1", "batch1", null);
        Assert.NotNull(batchResult.MessageResults);
        Assert.Empty(batchResult.MessageResults);
    }
}
