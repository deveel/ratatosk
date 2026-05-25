namespace Ratatosk
{
    /// <summary>
    /// Provides a mechanism to resolve a channel connector by its name.
    /// </summary>
    public interface IChannelConnectorResolver
    {
        /// <summary>
        /// Resolves the <see cref="IChannelConnector"/> associated with the given channel name.
        /// </summary>
        /// <param name="channelName">The name of the channel to resolve.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the resolution operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation, returning the
        /// <see cref="IChannelConnector"/> instance, or <c>null</c> if no
        /// connector is registered for the given name.
        /// </returns>
        Task<IChannelConnector?> ResolveAsync(string channelName, CancellationToken cancellationToken = default);
    }
}
