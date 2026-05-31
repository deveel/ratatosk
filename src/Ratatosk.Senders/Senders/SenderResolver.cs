using Microsoft.Extensions.Logging;

namespace Ratatosk.Senders
{
    /// <summary>
    /// Resolves sender identities by querying the global sender repository.
    /// </summary>
    /// <typeparam name="TSender">
    /// The type of the sender entity, which must implement <see cref="ISender"/>.
    /// </typeparam>
    public class SenderResolver<TSender> : SenderResolverBase
        where TSender : class, ISender
    {
        private readonly ISenderRepository<TSender> _repository;

        /// <summary>
        /// Constructs the resolver with the given dependencies.
        /// </summary>
        public SenderResolver(
            ISenderRepository<TSender> repository,
            ISenderCache cache,
            ILogger<SenderResolver<TSender>>? logger = null)
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
}
