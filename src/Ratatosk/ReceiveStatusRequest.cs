namespace Ratatosk;

public class ReceiveStatusRequest
{
    public string ChannelName { get; }
    public MessageSource Source { get; }
    public ConnectionSettings? ConnectionSettings { get; init; }
    public Type? ConnectorType { get; init; }
    public MessageContext? Context { get; init; }

    public ReceiveStatusRequest(string channelName, MessageSource source)
    {
        ArgumentNullException.ThrowIfNull(channelName);
        ChannelName = channelName;
        Source = source;
    }
}
