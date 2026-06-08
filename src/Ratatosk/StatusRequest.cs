namespace Ratatosk;

public class StatusRequest
{
    public string ChannelName { get; }
    public ConnectionSettings? ConnectionSettings { get; init; }
    public Type? ConnectorType { get; init; }
    public MessageContext? Context { get; init; }

    public StatusRequest(string channelName)
    {
        ArgumentNullException.ThrowIfNull(channelName);
        ChannelName = channelName;
    }
}
