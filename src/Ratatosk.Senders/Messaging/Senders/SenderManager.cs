//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;

namespace Ratatosk
{
    /// <summary>
    /// Manages sender identities using an underlying repository and
    /// implements <see cref="ISenderRegistry"/>.
    /// </summary>
    public class SenderManager : EntityManager<SenderEntity>, ISenderRegistry
    {
        /// <summary>
        /// Constructs the manager with the given dependencies.
        /// </summary>
        /// <param name="repository">
        /// The repository used to persist sender entities.
        /// </param>
        /// <param name="validator">
        /// An optional validator for sender entities.
        /// </param>
        /// <param name="cache">
        /// An optional entity cache.
        /// </param>
        /// <param name="systemTime">
        /// An optional system time provider.
        /// </param>
        /// <param name="errorFactory">
        /// An optional operation error factory.
        /// </param>
        /// <param name="serviceProvider">
        /// An optional service provider.
        /// </param>
        /// <param name="loggerFactory">
        /// An optional logger factory.
        /// </param>
        public SenderManager(
            IRepository<SenderEntity> repository,
            IEntityValidator<SenderEntity>? validator = null,
            IEntityCache<SenderEntity>? cache = null,
            ISystemTime? systemTime = null,
            IOperationErrorFactory<SenderEntity>? errorFactory = null,
            IServiceProvider? serviceProvider = null,
            ILoggerFactory? loggerFactory = null)
            : base(repository, validator, cache, systemTime, errorFactory, serviceProvider, loggerFactory)
        {
        }

        /// <inheritdoc />
        public async Task<SenderEntity?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await FindFirstAsync(e => e.Name == name, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<SenderEntity?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await FindAsync(id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IList<SenderEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return (await FindAllAsync(cancellationToken: cancellationToken)).ToList();
        }

        /// <inheritdoc />
        public async Task<OperationResult<SenderEntity>> CreateAsync(SenderEntity sender, CancellationToken cancellationToken = default)
        {
            return await AddAsync(sender, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<OperationResult<SenderEntity>> UpdateAsync(SenderEntity sender, CancellationToken cancellationToken = default)
        {
            return await UpdateAsync(sender, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<OperationResult<bool>> DeleteAsync(SenderEntity sender, CancellationToken cancellationToken = default)
        {
            return await RemoveAsync(sender, cancellationToken);
        }
    }
}
