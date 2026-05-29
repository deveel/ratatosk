//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ratatosk
{
    public class SenderResolver : ISenderResolver
    {
        private readonly ISenderRegistry _registry;
        private readonly ISenderCache _cache;
        private readonly ISenderSelector _selector;
        private readonly ILogger _logger;

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

        public async ValueTask<ISender?> ResolveSenderAsync(ISender sender, CancellationToken cancellationToken = default)
        {
            if (sender is SenderRef senderRef)
                return await ResolveByNameAsync(senderRef.SenderName, cancellationToken);

            return await ResolveByEndpointAsync(sender, cancellationToken);
        }

        private async ValueTask<ISender?> ResolveByNameAsync(string senderName, CancellationToken cancellationToken)
        {
            var cached = await _cache.GetByNameAsync(senderName, cancellationToken);
            if (cached != null)
            {
                _logger.LogSenderResolvedFromCache(senderName);
                return MapToSender(cached);
            }

            var entity = await _registry.FindByNameAsync(senderName, cancellationToken);
            if (entity == null)
            {
                _logger.LogSenderNotFoundInRegistry(senderName);
                return null;
            }

            await _cache.SetByNameAsync(senderName, entity, cancellationToken: cancellationToken);

            return MapToSender(entity);
        }

        private async ValueTask<ISender?> ResolveByEndpointAsync(ISender sender, CancellationToken cancellationToken)
        {
            var endpointType = GetEndpointTypeString(sender.Type);

            var entity = await _registry.FindByEndpointAsync(sender.Address, endpointType, cancellationToken);
            if (entity == null)
            {
                _logger.LogNoSenderFoundForEndpoint(sender.Address, endpointType);
                return null;
            }

            if (!entity.IsActive)
            {
                _logger.LogSenderFoundButInactive(entity.Name);
                return null;
            }

            return MapToSender(entity);
        }

        private static ISender MapToSender(SenderEntity entity)
        {
            var endpointType = Endpoint.ParseEndpointType(entity.EndpointType);

            return endpointType switch
            {
                EndpointType.PhoneNumber => new PhoneSender(entity.Address, entity.Name, entity.IsActive, entity.DisplayName),
                EndpointType.Label => new AlphaNumericSender(entity.Address, entity.Name, entity.IsActive, entity.DisplayName),
                EndpointType.EmailAddress => new EmailSender(entity.Address, entity.DisplayName, entity.Name, entity.IsActive),
                EndpointType.Id => new BotSender(entity.Address, entity.Name, entity.IsActive, entity.DisplayName),
                EndpointType.ApplicationId => new BotSender(entity.Address, entity.Name, entity.IsActive, entity.DisplayName),
                _ => throw new NotSupportedException($"Unsupported endpoint type '{entity.EndpointType}' for sender '{entity.Name}'.")
            };
        }

        private static string GetEndpointTypeString(EndpointType type)
        {
            return type switch
            {
                EndpointType.EmailAddress => "email",
                EndpointType.PhoneNumber => "phone",
                EndpointType.Label => "label",
                EndpointType.Id => "id",
                EndpointType.Url => "url",
                EndpointType.Topic => "topic",
                EndpointType.UserId => "userid",
                EndpointType.ApplicationId => "applicationid",
                EndpointType.DeviceId => "deviceid",
                EndpointType.Any => "any",
                _ => type.ToString().ToLowerInvariant()
            };
        }
    }
}
