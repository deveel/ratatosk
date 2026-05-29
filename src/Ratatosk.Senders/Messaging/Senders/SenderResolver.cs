//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ratatosk
{
    /// <summary>
    /// Resolves sender identities for messages by delegating to the
    /// sender registry and caching results.
    /// </summary>
    public class SenderResolver : ISenderResolver
    {
        private readonly ISenderRegistry _registry;
        private readonly ISenderCache _cache;
        private readonly ISenderSelector _selector;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructs the resolver with the given dependencies.
        /// </summary>
        /// <param name="registry">The sender registry to query.</param>
        /// <param name="cache">The sender cache for caching resolution results.</param>
        /// <param name="selector">The selector for choosing a default sender.</param>
        /// <param name="logger">An optional logger.</param>
        public SenderResolver(
            ISenderRegistry registry,
            ISenderCache cache,
            ISenderSelector selector,
            ILogger<SenderResolver>? logger = null)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _selector = selector ?? throw new ArgumentNullException(nameof(selector));
            _logger = logger ?? NullLogger<SenderResolver>.Instance;
        }

        /// <inheritdoc />
        public async ValueTask<ISender?> ResolveSenderAsync(IMessage message, CancellationToken cancellationToken = default)
        {
            if (message.Sender is SenderRef senderRef)
                return await ResolveByNameAsync(senderRef.SenderName, cancellationToken);

            if (message.Sender is ISender sender && sender is not SenderRef)
                return sender;

            if (message.Sender == null)
                return await ResolveDefaultAsync(cancellationToken);

            return null;
        }

        private async ValueTask<ISender?> ResolveByNameAsync(string senderName, CancellationToken cancellationToken)
        {
            var cached = await _cache.GetByNameAsync(senderName, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Sender '{SenderName}' resolved from cache.", senderName);
                return MapToSender(cached);
            }

            var entity = await _registry.FindByNameAsync(senderName, cancellationToken);
            if (entity == null)
            {
                _logger.LogWarning("Sender '{SenderName}' not found in registry.", senderName);
                return null;
            }

            await _cache.SetByNameAsync(senderName, entity, cancellationToken: cancellationToken);

            return MapToSender(entity);
        }

        private async ValueTask<ISender?> ResolveDefaultAsync(CancellationToken cancellationToken)
        {
            var all = await _registry.GetAllAsync(cancellationToken);

            if (all.Count == 0)
            {
                _logger.LogDebug("No senders registered for default resolution.");
                return null;
            }

            var selected = await _selector.SelectAsync(all.AsReadOnly(), cancellationToken);

            if (selected == null)
            {
                _logger.LogDebug("Default sender selector returned no candidate.");
                return null;
            }

            return MapToSender(selected);
        }

        private static ISender MapToSender(SenderEntity entity)
        {
            var endpointType = Endpoint.ParseEndpointType(entity.EndpointType);

            return endpointType switch
            {
                EndpointType.PhoneNumber => new PhoneSender(entity.Address, entity.Name, entity.IsActive, entity.DisplayName),
                EndpointType.Label => new AlphaNumericSender(entity.Address, entity.Name, entity.IsActive, entity.DisplayName),
                EndpointType.EmailAddress => new EmailSender(entity.Address, entity.DisplayName, entity.Name, entity.IsActive),
                _ => new BotSender(entity.Address, entity.Name, entity.IsActive, entity.DisplayName),
            };
        }
    }
}
