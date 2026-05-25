using Microsoft.Extensions.DependencyInjection;

namespace Ratatosk
{
    /// <summary>
    /// An implementation of <see cref="IChannelConnectorResolver"/> that resolves
    /// connectors from the dependency injection container using keyed services.
    /// </summary>
    public class ServiceProviderConnectorResolver : IChannelConnectorResolver
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Constructs the resolver with the given service provider.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider used to resolve connector instances.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="serviceProvider"/> is <c>null</c>.
        /// </exception>
        public ServiceProviderConnectorResolver(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public Task<IChannelConnector?> ResolveAsync(string channelName, CancellationToken cancellationToken = default)
        {
            var connector = _serviceProvider.GetKeyedService<IChannelConnector>(channelName);
            return Task.FromResult(connector);
        }
    }
}
