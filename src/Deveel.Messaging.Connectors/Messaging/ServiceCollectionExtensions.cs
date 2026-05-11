//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Messaging
{
    public static class ServiceCollectionExtensions
    {
        public static MessagingBuilder AddMessaging(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));

            return new MessagingBuilder(services);
        }
    }
}
