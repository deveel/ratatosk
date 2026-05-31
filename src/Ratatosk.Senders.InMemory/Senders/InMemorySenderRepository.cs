namespace Ratatosk.Senders
{
    /// <summary>
    /// An in-memory implementation of a repository for <see cref="SenderEntity"/>,
    /// used for development and testing scenarios.
    /// </summary>
    public class InMemorySenderRepository : InMemoryRepository<SenderEntity>, ISenderRepository<SenderEntity>
    {
        /// <summary>
        /// Constructs the store with an optional seed list of sender entities.
        /// </summary>
        /// <param name="senders">An initial set of sender entities to populate the store.</param>
        /// <param name="services">An optional service provider.</param>
        public InMemorySenderRepository(IEnumerable<SenderEntity>? senders = null, IServiceProvider? services = null)
            : base(senders ?? Array.Empty<SenderEntity>(), new ReflectionFieldMapper<SenderEntity>(), services!)
        {
        }

        /// <inheritdoc />
        public Task<SenderEntity?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var sender = Entities.FirstOrDefault(x => x.Name == name);
            return Task.FromResult(sender);
        }

        /// <inheritdoc />
        public Task<SenderEntity?> FindByEndpointAsync(string address, EndpointType endpointType, CancellationToken cancellationToken = default)
        {
            var sender = Entities.FirstOrDefault(x => x.Address == address && x.Type == endpointType);
            return Task.FromResult(sender);
        }

        /// <inheritdoc />
        public Task<IList<SenderEntity>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        {
            var senders = Entities.Where(x => x.IsActive).ToList();
            return Task.FromResult<IList<SenderEntity>>(senders);
        }

        /// <inheritdoc />
        public Task SetActiveAsync(SenderEntity sender, bool isActive, CancellationToken cancellationToken = default)
        {
            if (isActive)
            {
                sender.Activate();
            }
            else
            {
                sender.Deactivate();
            }
            return Task.CompletedTask;
        }
    }
}
