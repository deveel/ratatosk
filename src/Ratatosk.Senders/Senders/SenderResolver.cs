using Microsoft.Extensions.Logging;

namespace Ratatosk.Senders
{
    /// <summary>
    /// Resolves sender identities by querying the global sender repository.
    /// </summary>
    public class SenderResolver : SenderResolverBase
    {
        private readonly ISenderRepository<ISender> _repository;

        /// <summary>
        /// Constructs the resolver with the given dependencies.
        /// </summary>
        public SenderResolver(
            ISenderRepository<ISender> repository,
            ISenderCache cache,
            ILogger<SenderResolver>? logger = null)
            : base(cache, logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <inheritdoc />
        protected override async ValueTask<ISender?> ResolveByNameAsync(string name, CancellationToken cancellationToken)
        {
            return await _repository.FindByNameAsync(name, cancellationToken);
        }

        /// <inheritdoc />
        protected override async ValueTask<ISender?> ResolveByEndpointAsync(string address, EndpointType endpointType, CancellationToken cancellationToken)
        {
            return await _repository.FindByEndpointAsync(address, endpointType, cancellationToken);
        }
    }

    /// <summary>
    /// Backward-compatible typed resolver that delegates to the non-generic resolver pipeline.
    /// </summary>
    /// <typeparam name="TSender">The sender entity type.</typeparam>
    public class SenderResolver<TSender> : SenderResolver
        where TSender : class, ISender
    {
        /// <summary>
        /// Constructs a typed compatibility resolver over the non-generic pipeline.
        /// </summary>
        public SenderResolver(
            ISenderRepository<TSender> repository,
            ISenderCache cache,
            ILogger<SenderResolver>? logger = null)
            : base(new SenderRepositoryAdapter<TSender>(repository), cache, logger)
        {
        }
    }
}
