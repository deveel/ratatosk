//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.DependencyInjection;

namespace Ratatosk
{
    /// <summary>
    /// Provides extension methods for registering messaging services
    /// into an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the messaging services to the specified
        /// <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">
        /// The <see cref="IServiceCollection"/> to add the services to.
        /// </param>
        /// <returns>
        /// Returns a <see cref="MessagingBuilder"/> that can be used to
        /// further configure the messaging services.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="services"/> is <c>null</c>.
        /// </exception>
        public static MessagingBuilder AddMessaging(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));

            return new MessagingBuilder(services);
        }
    }
}
