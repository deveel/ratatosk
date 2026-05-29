//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ratatosk
{
    /// <summary>
    /// Provides extension methods for registering sender identity services
    /// in a <see cref="IServiceCollection"/>.
    /// </summary>
    public static class SenderServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the sender identity services (cache, registry, resolver,
        /// selector, and validator) in the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddSenders(this IServiceCollection services)
        {
            services.TryAddSingleton<ISenderCache>(sp => new InMemorySenderCache(TimeSpan.FromMinutes(5)));
            services.TryAddSingleton<ISenderSelector, FirstMatchSenderSelector>();
            services.TryAddScoped<IEntityValidator<SenderEntity>, SenderValidator>();

            services.TryAddScoped<ISenderRegistry, SenderManager>();
            services.TryAddScoped<SenderManager>();

            services.TryAddScoped<ISenderResolver, SenderResolver>();

            return services;
        }

        /// <summary>
        /// Registers a custom store for sender entities.
        /// </summary>
        /// <typeparam name="TStore">
        /// The type of the repository that implements <see cref="IRepository{T}"/>
        /// for <see cref="SenderEntity"/>.
        /// </typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddSenderStore<TStore>(this IServiceCollection services)
            where TStore : class, IRepository<SenderEntity>
        {
            services.TryAddScoped<IRepository<SenderEntity>, TStore>();
            return services;
        }
    }
}
