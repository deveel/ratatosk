namespace Ratatosk.Senders
{
    /// <summary>
    /// Adapts a typed sender repository to an <see cref="ISenderRepository{TSender}"/> for <see cref="ISender"/>.
    /// </summary>
    /// <typeparam name="TSender">The concrete sender entity type.</typeparam>
    public sealed class SenderRepositoryAdapter<TSender> : ISenderRepository<ISender>
        where TSender : class, ISender
    {
        private readonly ISenderRepository<TSender> _inner;
        private ISenderRepository<ISender> _senderRepositoryImplementation;

        /// <summary>
        /// Initializes a new adapter around a typed sender repository.
        /// </summary>
        public SenderRepositoryAdapter(ISenderRepository<TSender> inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        /// <inheritdoc />
        public object GetEntityKey(ISender entity)
        {
            return _inner.GetEntityKey(ToTyped(entity))
                   ?? throw new InvalidOperationException("The repository returned a null entity key.");
        }

        /// <inheritdoc />
        public ValueTask AddAsync(ISender entity, CancellationToken cancellationToken = default)
        {
            return _inner.AddAsync(ToTyped(entity), cancellationToken);
        }

        /// <inheritdoc />
        public ValueTask AddRangeAsync(IEnumerable<ISender> entities, CancellationToken cancellationToken = default)
        {
            return _inner.AddRangeAsync(entities.Select(ToTyped), cancellationToken);
        }

        /// <inheritdoc />
        public ValueTask<bool> UpdateAsync(ISender entity, CancellationToken cancellationToken = default)
        {
            return _inner.UpdateAsync(ToTyped(entity), cancellationToken);
        }

        /// <inheritdoc />
        public ValueTask<bool> RemoveAsync(ISender entity, CancellationToken cancellationToken = default)
        {
            return _inner.RemoveAsync(ToTyped(entity), cancellationToken);
        }

        /// <inheritdoc />
        public ValueTask RemoveRangeAsync(IEnumerable<ISender> entities, CancellationToken cancellationToken = default)
        {
            return _inner.RemoveRangeAsync(entities.Select(ToTyped), cancellationToken);
        }

        /// <inheritdoc />
        public async ValueTask<ISender?> FindAsync(object key, CancellationToken cancellationToken = default)
            => await _inner.FindAsync(key, cancellationToken);

        /// <inheritdoc/>
        public ValueTask<PageResult<ISender>> GetPageAsync(PageRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return _senderRepositoryImplementation.GetPageAsync(request, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<ISender?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
            => await _inner.FindByNameAsync(name, cancellationToken);

        /// <inheritdoc />
        public async Task<ISender?> FindByEndpointAsync(string address, EndpointType endpointType, CancellationToken cancellationToken = default)
            => await _inner.FindByEndpointAsync(address, endpointType, cancellationToken);

        /// <inheritdoc />
        public async Task<IList<ISender>> GetAllActiveAsync(CancellationToken cancellationToken = default)
            => (await _inner.GetAllActiveAsync(cancellationToken)).Cast<ISender>().ToList();

        /// <inheritdoc />
        public Task SetActiveAsync(ISender sender, bool isActive, CancellationToken cancellationToken = default)
        {
            return _inner.SetActiveAsync(ToTyped(sender), isActive, cancellationToken);
        }

        private static TSender ToTyped(ISender sender)
        {
            if (sender is not TSender typedSender)
                throw new ArgumentException($"The sender must be assignable to {typeof(TSender).Name}.", nameof(sender));

            return typedSender;
        }
    }
}




