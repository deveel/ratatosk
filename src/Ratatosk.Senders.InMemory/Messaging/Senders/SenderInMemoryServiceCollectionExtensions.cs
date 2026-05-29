//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.DependencyInjection;

namespace Ratatosk
{
    /// <summary>
    /// Provides extension methods for registering the in-memory sender store
    /// in a <see cref="IServiceCollection"/>.
    /// </summary>
    public static class SenderInMemoryServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the in-memory sender store for development and testing.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="seedSenders">
        /// An optional list of sender entities to seed the store.
        /// </param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddSenderInMemoryStore(
            this IServiceCollection services,
            IEnumerable<SenderEntity>? seedSenders = null)
        {
            services.AddSingleton<IRepository<SenderEntity>>(
                sp => new InMemorySenderStore(seedSenders));

            return services;
        }
    }
}
