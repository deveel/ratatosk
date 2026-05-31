using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Ratatosk.Senders;

namespace Ratatosk
{
    /// <summary>
    /// Provides a fluent builder for configuring sender resolution
    /// for a specific connector type.
    /// </summary>
    public sealed class SenderRegistrationBuilder<TConnector>
        where TConnector : class, IChannelConnector
    {
        private readonly MessagingBuilder _messagingBuilder;

        internal SenderRegistrationBuilder(MessagingBuilder messagingBuilder)
        {
            _messagingBuilder = messagingBuilder;
            _messagingBuilder.AddSenders();
        }

        /// <summary>
        /// Gets the service collection used to register sender store dependencies.
        /// </summary>
        public IServiceCollection Services => _messagingBuilder.Services;

        /// <summary>
        /// Configures the cache options used by the distributed sender cache.
        /// </summary>
        public SenderRegistrationBuilder<TConnector> WithCacheOptions(Action<SenderCacheOptions> configure)
        {
            Services.PostConfigure(configure);
            return this;
        }

        /// <summary>
        /// Sets the default time-to-live for cached sender entries.
        /// </summary>
        public SenderRegistrationBuilder<TConnector> WithCacheTtl(TimeSpan ttl)
        {
            Services.PostConfigure<SenderCacheOptions>(o => o.DefaultTtl = ttl);
            return this;
        }

        /// <summary>
        /// Replaces the default <see cref="ISenderCache"/> implementation.
        /// </summary>
        public SenderRegistrationBuilder<TConnector> WithCache(ISenderCache cache)
        {
            Services.AddSingleton(cache);
            return this;
        }
    }
}
