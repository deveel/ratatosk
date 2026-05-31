using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ratatosk.Senders
{
    /// <summary>
    /// An Entity Framework implementation of <see cref="ISenderRepository{TSender}"/>
    /// that queries <see cref="DbSender"/> entities via <see cref="SenderDbContext"/>.
    /// </summary>
    public class EntitySenderRepository : EntityRepository<DbSender>, ISenderRepository<DbSender>
    {
        /// <summary>
        /// Constructs the repository with the given context and dependencies.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="services">The service provider.</param>
        /// <param name="logger">The logger.</param>
        public EntitySenderRepository(SenderDbContext context, IServiceProvider services, ILogger<EntityRepository<DbSender>> logger)
            : base(context, services, logger)
        {
        }
        
        protected SenderDbContext SenderContext => Context as SenderDbContext 
            ?? throw new InvalidOperationException($"The database context must be of type {nameof(SenderDbContext)}.");

        /// <inheritdoc />
        public async Task<DbSender?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await SenderContext.Senders.FirstOrDefaultAsync(s => s.Name == name, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<DbSender?> FindByEndpointAsync(string address, EndpointType endpointType, CancellationToken cancellationToken = default)
        {
            var typeStr = endpointType.ToString();
            return await SenderContext.Senders.FirstOrDefaultAsync(s => s.Address == address && s.Type == typeStr, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IList<DbSender>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        {
            return await SenderContext.Senders.Where(s => s.IsActive).ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task SetActiveAsync(DbSender sender, bool isActive, CancellationToken cancellationToken = default)
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
