using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Messaging
{
    public class ServiceProviderConnectorResolver : IChannelConnectorResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderConnectorResolver(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            _serviceProvider = serviceProvider;
        }

        public Task<IChannelConnector?> ResolveAsync(string channelName, CancellationToken cancellationToken = default)
        {
            var connector = _serviceProvider.GetKeyedService<IChannelConnector>(channelName);
            return Task.FromResult(connector);
        }
    }
}
