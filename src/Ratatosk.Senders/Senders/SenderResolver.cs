//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ratatosk
{
    /// <summary>
    /// Resolves sender identities from a repository, with optional caching
    /// and per-connector configuration.
    /// </summary>
    public class SenderResolver : ISenderResolver
    {
        private readonly ISenderRepository<ISender> _repository;
        private readonly ISenderCache _cache;
        private readonly ISender? _defaultSender;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructs the resolver with the given dependencies.
        /// </summary>
        /// <param name="repository">The sender repository.</param>
        /// <param name="cache">The sender cache for resolution results.</param>
        /// <param name="defaultSender">
        /// Optional default sender. Takes precedence over connection settings-based defaults.
        /// </param>
        /// <param name="logger">Optional logger.</param>
        public SenderResolver(
            ISenderRepository<ISender> repository,
            ISenderCache cache,
            ISender? defaultSender = null,
            ILogger<SenderResolver>? logger = null)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _defaultSender = defaultSender;
            _logger = logger ?? NullLogger<SenderResolver>.Instance;
        }

        /// <summary>
        /// Gets the default sender for this connector, if configured.
        /// </summary>
        public ISender? DefaultSender => _defaultSender;

        /// <inheritdoc />
        public async ValueTask<ISender?> ResolveSenderAsync(IEndpoint endpoint, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(endpoint);

            if (endpoint is IUnresolvedSender unresolved)
                return await ResolveByNameAsync(unresolved.Address, cancellationToken);

            if (endpoint is ISender sender)
                return await ResolveByEndpointAsync(sender, cancellationToken);

            return null;
        }

        private async ValueTask<ISender?> ResolveByNameAsync(string senderName, CancellationToken cancellationToken)
        {
            var cached = await _cache.GetByNameAsync(senderName, cancellationToken);
            if (cached != null)
            {
                _logger.LogSenderResolvedFromCache(senderName);
                return cached;
            }

            var entity = await _repository.FindByNameAsync(senderName, cancellationToken);
            if (entity == null)
            {
                _logger.LogSenderNotFoundInRegistry(senderName);
                return null;
            }

            if (!entity.IsActive)
            {
                _logger.LogSenderFoundButInactive(entity.Name);
                return null;
            }

            await _cache.SetByNameAsync(senderName, entity, cancellationToken: cancellationToken);

            return entity;
        }

        private async ValueTask<ISender?> ResolveByEndpointAsync(ISender sender, CancellationToken cancellationToken)
        {
            var entity = await _repository.FindByEndpointAsync(sender.Address, sender.Type, cancellationToken);
            if (entity == null)
            {
                _logger.LogNoSenderFoundForEndpoint(sender.Address, sender.Type);
                return null;
            }

            if (!entity.IsActive)
            {
                _logger.LogSenderFoundButInactive(entity.Name);
                return null;
            }

            return entity;
        }
    }
}
