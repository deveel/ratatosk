using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ratatosk.Senders
{
    /// <summary>
    /// Provides the common sender resolution pipeline that storage-specific
    /// resolvers extend by implementing the lookup abstractions.
    /// </summary>
    public abstract class SenderResolverBase : ISenderResolver
    {
        private readonly ISenderCache _cache;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructs the base resolver with the given dependencies.
        /// </summary>
        protected SenderResolverBase(ISenderCache cache, ILogger? logger = null)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? NullLogger<SenderResolverBase>.Instance;
        }

        /// <summary>
        /// Looks up a sender by its logical name in storage.
        /// </summary>
        protected abstract ValueTask<ISender?> ResolveByNameAsync(string name, CancellationToken cancellationToken);

        /// <summary>
        /// Looks up a sender by its endpoint address and type in storage.
        /// </summary>
        protected abstract ValueTask<ISender?> ResolveByEndpointAsync(string address, EndpointType endpointType, CancellationToken cancellationToken);

        /// <inheritdoc />
        public async ValueTask<ISender?> ResolveAsync(SenderResolutionContext context, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);

            var sender = context.Sender;

            if (sender != null)
            {
                if (sender is IUnresolvedSender unresolved)
                    return await ResolveByNameAsyncCached(unresolved.Address, cancellationToken);

                if (sender is ISender typedSender)
                    return await ResolveBySenderAsync(typedSender, cancellationToken);

                return null;
            }

            var defaultSender = context.Settings.GetDefaultSender();
            if (defaultSender == null)
                return null;

            return await ResolveBySenderAsync(defaultSender, cancellationToken);
        }

        private async ValueTask<ISender?> ResolveByNameAsyncCached(string senderName, CancellationToken cancellationToken)
        {
            var cached = await _cache.GetByNameAsync(senderName, cancellationToken);
            if (cached != null)
            {
                _logger.LogSenderResolvedFromCache(senderName);
                return cached;
            }

            var entity = await ResolveByNameAsync(senderName, cancellationToken);
            if (entity != null)
            {
                if (!entity.IsActive)
                {
                    _logger.LogSenderFoundButInactive(entity.Name);
                    return null;
                }

                await _cache.SetAsync(entity, cancellationToken: cancellationToken);
                return entity;
            }

            _logger.LogSenderNotFoundInRegistry(senderName);
            return null;
        }

        private async ValueTask<ISender?> ResolveBySenderAsync(ISender sender, CancellationToken cancellationToken)
        {
            var cached = await _cache.GetByEndpointAsync(sender.Address, sender.Type, cancellationToken);
            if (cached != null)
            {
                _logger.LogSenderResolvedFromCacheByEndpoint(sender.Address, sender.Type);
                return cached;
            }

            var entity = await ResolveByEndpointAsync(sender.Address, sender.Type, cancellationToken);
            if (entity != null)
            {
                if (!entity.IsActive)
                {
                    _logger.LogSenderFoundButInactive(entity.Name);
                    return null;
                }

                await _cache.SetAsync(entity, cancellationToken: cancellationToken);
                return entity;
            }

            _logger.LogNoSenderFoundForEndpoint(sender.Address, sender.Type);
            return null;
        }
    }
}
