using Kista.Caching;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ratatosk.Senders
{
    /// <summary>
    /// Manages sender identities by delegating to an underlying repository
    /// and implementing <see cref="ISenderRepository{TSender}"/> with
    /// validation, caching, and system-time support from Kista.
    /// </summary>
    /// <typeparam name="TSender">
    /// The type of sender entity, which must implement <see cref="ISender"/>.
    /// </typeparam>
    public class SenderManager<TSender> : EntityManager<TSender>
        where TSender : class, ISender
    {
        /// <summary>
        /// Constructs the manager with the given dependencies.
        /// </summary>
        /// <param name="repository">The underlying repository.</param>
        /// <param name="validator">
        /// An optional validator for sender entities.
        /// </param>
        /// <param name="cache">
        /// An optional entity cache.
        /// </param>
        /// <param name="systemTime">
        /// An optional system time provider.
        /// </param>
        /// <param name="services">
        /// An optional service provider.
        /// </param>
        /// <param name="loggerFactory">
        /// An optional logger factory.
        /// </param>
        public SenderManager(IRepository<TSender> repository,
            ISenderValidator<TSender>? validator = null,
            IEntityCache<TSender>? cache = null,
            ISystemTime? systemTime = null,
            IServiceProvider? services = null,
            ILoggerFactory? loggerFactory = null)
        : base(repository, validator, cache, systemTime, null, services, loggerFactory)
        {
        }
        
        protected ISenderRepository<TSender> SenderRepository => Repository as ISenderRepository<TSender> 
            ?? throw new InvalidOperationException($"The underlying repository must implement {nameof(ISenderRepository<TSender>)}.");
        
        /// <summary>
        /// Finds a sender by its logical name.
        /// </summary>
        /// <param name="name">The logical name of the sender.</param>
        /// <returns>
        /// An <see cref="OperationResult{TSender}"/> containing the sender if found,
        /// or a failure result if not found or an error occurred.
        /// </returns>
        public virtual async Task<OperationResult<TSender>> FindByNameAsync(string name)
        {
            try
            {
                var sender = await SenderRepository.FindByNameAsync(name, CancellationToken);
                return sender is null
                    ? OperationResult<TSender>.Fail(SenderErrorCodes.SenderNotFound, SenderErrorCodes.ErrorDomain, $"Sender with name '{name}' not found.")
                    : OperationResult<TSender>.Success(sender);
            }
            catch (Exception ex)
            {
                Logger.LogFailedToFindSenderByName(ex, name);
                return OperationResult<TSender>.Fail(SenderErrorCodes.SenderError, SenderErrorCodes.ErrorDomain, ex.Message);
            }
        }

        /// <summary>
        /// Finds a sender by its endpoint address and type.
        /// </summary>
        /// <param name="address">The endpoint address to match.</param>
        /// <param name="endpointType">The endpoint type.</param>
        /// <returns>
        /// An <see cref="OperationResult{TSender}"/> containing the sender if found,
        /// or a failure result if not found or an error occurred.
        /// </returns>
        public virtual async Task<OperationResult<TSender>> FindByEndpointAsync(string address, EndpointType endpointType)
        {
            try
            {
                var sender = await SenderRepository.FindByEndpointAsync(address, endpointType, CancellationToken);
                return sender is null
                    ? OperationResult<TSender>.Fail(SenderErrorCodes.SenderNotFound, SenderErrorCodes.ErrorDomain, $"Sender with endpoint '{address}' ({endpointType}) not found.")
                    : OperationResult<TSender>.Success(sender);
            }
            catch (Exception ex)
            {
                Logger.LogFailedToFindSenderByEndpoint(ex, address, endpointType);
                return OperationResult<TSender>.Fail(SenderErrorCodes.SenderError, SenderErrorCodes.ErrorDomain, ex.Message);
            }
        }

        /// <summary>
        /// Retrieves all active sender entities.
        /// </summary>
        /// <returns>
        /// An <see cref="OperationResult"/> containing the list of active senders,
        /// or a failure result if an error occurred.
        /// </returns>
        public virtual async Task<OperationResult<IList<TSender>>> GetAllActiveAsync()
        {
            try
            {
                var senders = await SenderRepository.GetAllActiveAsync(CancellationToken);
                return OperationResult<IList<TSender>>.Success(senders);
            }
            catch (Exception ex)
            {
                Logger.LogFailedToRetrieveAllActiveSenders(ex);
                return OperationResult<IList<TSender>>.Fail(SenderErrorCodes.SenderError, SenderErrorCodes.ErrorDomain, ex.Message);
            }
        }

        /// <summary>
        /// Sets the active state of a sender by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the sender.</param>
        /// <param name="isActive">
        /// <c>true</c> to activate the sender; <c>false</c> to deactivate.
        /// </param>
        /// <returns>
        /// An <see cref="OperationResult"/> indicating success or failure.
        /// </returns>
        public virtual async Task<OperationResult> SetActiveAsync(string id, bool isActive)
        {
            try
            {
                var findResult = await FindAsync(id);
                if (!findResult.IsSuccess() || findResult.Value is null)
                    return OperationResult.Fail(SenderErrorCodes.SenderNotFound, SenderErrorCodes.ErrorDomain, "Sender not found.");

                var sender = findResult.Value;
                await SenderRepository.SetActiveAsync(sender, isActive, CancellationToken);
                return await UpdateAsync(sender);
            }
            catch (Exception ex)
            {
                Logger.LogFailedToSetActiveState(ex, id);
                return OperationResult.Fail(SenderErrorCodes.SenderError, SenderErrorCodes.ErrorDomain, ex.Message);
            }
        }

        /// <summary>
        /// Activates a sender by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the sender.</param>
        /// <returns>
        /// An <see cref="OperationResult"/> indicating success or failure.
        /// </returns>
        public virtual async Task<OperationResult> ActivateAsync(string id)
        {
            return await SetActiveAsync(id, true);
        }

        /// <summary>
        /// Deactivates a sender by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the sender.</param>
        /// <returns>
        /// An <see cref="OperationResult"/> indicating success or failure.
        /// </returns>
        public virtual async Task<OperationResult> DeactivateAsync(string id)
        {
            return await SetActiveAsync(id, false);
        }
    }
}
