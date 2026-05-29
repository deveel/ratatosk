//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;

namespace Ratatosk
{
    /// <summary>
    /// An Entity Framework Core implementation of <see cref="IRepository{T}"/>
    /// for <see cref="SenderEntity"/>.
    /// </summary>
    public class EfSenderRepository : EntityRepository<SenderEntity>
    {
        /// <summary>
        /// Constructs the repository with the given context and logger.
        /// </summary>
        /// <param name="context">The sender database context.</param>
        /// <param name="logger">The logger for the repository.</param>
        public EfSenderRepository(SenderDbContext context, ILogger<EntityRepository<SenderEntity>> logger)
            : base(context, logger)
        {
        }
    }
}
