//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Kista;

namespace Ratatosk
{
    /// <summary>
    /// Manages sender identities using an underlying repository and
    /// implements <see cref="ISenderRepository{TSender}"/>.
    /// </summary>
    /// <typeparam name="TSender">
    /// The type of sender entity, which must implement <see cref="ISender"/>.
    /// </typeparam>
    public class SenderManager<TSender> : ISenderRepository<TSender>
        where TSender : class, ISender
    {
        private readonly IRepository<TSender> _repository;

        /// <summary>
        /// Constructs the manager with the given repository.
        /// </summary>
        /// <param name="repository">
        /// The repository used to persist sender entities.
        /// </param>
        public SenderManager(IRepository<TSender> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <inheritdoc />
        public object? GetEntityKey(TSender entity) => _repository.GetEntityKey(entity);

        /// <inheritdoc />
        public async ValueTask AddAsync(TSender entity, CancellationToken cancellationToken = default)
            => await _repository.AddAsync(entity, cancellationToken);

        /// <inheritdoc />
        public async ValueTask AddRangeAsync(IEnumerable<TSender> entities, CancellationToken cancellationToken = default)
            => await _repository.AddRangeAsync(entities, cancellationToken);

        /// <inheritdoc />
        public async ValueTask<bool> UpdateAsync(TSender entity, CancellationToken cancellationToken = default)
            => await _repository.UpdateAsync(entity, cancellationToken);

        /// <inheritdoc />
        public async ValueTask<bool> RemoveAsync(TSender entity, CancellationToken cancellationToken = default)
            => await _repository.RemoveAsync(entity, cancellationToken);

        /// <inheritdoc />
        public async ValueTask RemoveRangeAsync(IEnumerable<TSender> entities, CancellationToken cancellationToken = default)
            => await _repository.RemoveRangeAsync(entities, cancellationToken);

        /// <inheritdoc />
        public async ValueTask<TSender?> FindAsync(object key, CancellationToken cancellationToken = default)
            => await _repository.FindAsync(key, cancellationToken);

        /// <inheritdoc />
        public async Task<TSender?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (_repository is IQueryableRepository<TSender> queryable)
            {
                return queryable.AsQueryable()
                    .FirstOrDefault(s => s.Name == name);
            }

            throw new InvalidOperationException(
                $"The underlying repository must implement {nameof(IQueryableRepository<TSender>)} " +
                $"to support {nameof(FindByNameAsync)}.");
        }

        /// <inheritdoc />
        public async Task<TSender?> FindByEndpointAsync(string address, EndpointType endpointType, CancellationToken cancellationToken = default)
        {
            if (_repository is IQueryableRepository<TSender> queryable)
            {
                return queryable.AsQueryable()
                    .FirstOrDefault(s => s.Address == address && s.Type == endpointType);
            }

            throw new InvalidOperationException(
                $"The underlying repository must implement {nameof(IQueryableRepository<TSender>)} " +
                $"to support {nameof(FindByEndpointAsync)}.");
        }

        /// <inheritdoc />
        public async Task<IList<TSender>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        {
            if (_repository is IQueryableRepository<TSender> queryable)
            {
                return queryable.AsQueryable()
                    .Where(s => s.IsActive)
                    .ToList();
            }

            throw new InvalidOperationException(
                $"The underlying repository must implement {nameof(IQueryableRepository<TSender>)} " +
                $"to support {nameof(GetAllActiveAsync)}.");
        }
    }
}
