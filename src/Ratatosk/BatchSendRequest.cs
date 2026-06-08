namespace Ratatosk;

public class BatchSendRequest
{
    public string ChannelName { get; }
    public IMessageBatch Batch { get; }
    public ConnectionSettings? ConnectionSettings { get; init; }
    public Type? ConnectorType { get; init; }
    public MessageContext? Context { get; init; }

    public BatchSendRequest(string channelName, IMessageBatch batch)
    {
        ArgumentNullException.ThrowIfNull(channelName);
        ArgumentNullException.ThrowIfNull(batch);
        ChannelName = channelName;
        Batch = batch;
    }
}
