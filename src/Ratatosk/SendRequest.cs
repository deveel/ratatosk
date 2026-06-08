namespace Ratatosk;

public class SendRequest
{
    public string ChannelName { get; }
    public IMessage Message { get; }
    public ConnectionSettings? ConnectionSettings { get; init; }
    public Type? ConnectorType { get; init; }
    public MessageContext? Context { get; init; }

    public SendRequest(string channelName, IMessage message)
    {
        ArgumentNullException.ThrowIfNull(channelName);
        ArgumentNullException.ThrowIfNull(message);
        ChannelName = channelName;
        Message = message;
    }
}
