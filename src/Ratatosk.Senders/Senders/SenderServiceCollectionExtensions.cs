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
        /// Registers the sender identity services (cache, registry, resolver)
        /// in the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddSenders(this IServiceCollection services)
        {
            services.TryAddSingleton<ISenderCache>(sp => new InMemorySenderCache(TimeSpan.FromMinutes(5)));

            services.TryAddScoped<ISenderRepository<ISender>, SenderManager<ISender>>();
            services.TryAddScoped<ISenderResolver, SenderResolver>();

            return services;
        }

        /// <summary>
        /// Registers a custom repository for sender entities.
        /// </summary>
        /// <typeparam name="TRepository">
        /// The type of the repository that implements <see cref="ISenderRepository{TSender}"/>
        /// for the specified sender type.
        /// </typeparam>
        /// <typeparam name="TSender">
        /// The type of sender entity.
        /// </typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddSenderRepository<TRepository, TSender>(this IServiceCollection services)
            where TRepository : class, ISenderRepository<TSender>
            where TSender : class, ISender
        {
            services.TryAddScoped<ISenderRepository<TSender>, TRepository>();
            return services;
        }
    }
}
