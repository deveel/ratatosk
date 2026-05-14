namespace Deveel.Messaging
{
    public interface IChannelConnectorResolver
    {
        Task<IChannelConnector?> ResolveAsync(string channelName, CancellationToken cancellationToken = default);
    }
}
