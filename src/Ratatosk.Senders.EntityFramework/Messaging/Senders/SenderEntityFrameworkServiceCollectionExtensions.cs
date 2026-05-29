//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ratatosk
{
    /// <summary>
    /// Provides extension methods for registering the Entity Framework Core
    /// sender store in a <see cref="IServiceCollection"/>.
    /// </summary>
    public static class SenderEntityFrameworkServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the Entity Framework Core sender store, including the
        /// <see cref="SenderDbContext"/> and the <see cref="EfSenderRepository"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="optionsAction">
        /// An action to configure the <see cref="DbContextOptionsBuilder"/>
        /// for the <see cref="SenderDbContext"/>.
        /// </param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddSenderEntityFrameworkStore(
            this IServiceCollection services,
            Action<DbContextOptionsBuilder> optionsAction)
        {
            services.AddDbContext<SenderDbContext>(optionsAction);
            services.AddSenderStore<EfSenderRepository>();
            return services;
        }
    }
}
