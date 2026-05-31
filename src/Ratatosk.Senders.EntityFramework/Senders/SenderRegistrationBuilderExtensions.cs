using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Ratatosk.Senders
{
    /// <summary>
    /// Provides extension methods for configuring the Entity Framework
    /// sender store on a <see cref="SenderRegistrationBuilder{TConnector}"/>.
    /// </summary>
    public static class SenderRegistrationBuilderExtensions
    {
        /// <summary>
        /// Configures the sender storage to use Entity Framework Core.
        /// </summary>
        public static SenderRegistrationBuilder<TConnector> UseEntityFramework<TConnector>(
            this SenderRegistrationBuilder<TConnector> builder,
            Action<DbContextOptionsBuilder> optionsAction)
            where TConnector : class, IChannelConnector
        {
            builder.Services.AddDbContext<SenderDbContext>(optionsAction);
            builder.Services.AddLogging();

            builder.Services.AddRepositoryContext()
                .AddRepository<EntitySenderRepository>();

            builder.Services.TryAddScoped<SenderManager<DbSender>>();
            builder.Services.TryAddScoped<ISenderResolver, SenderResolver<DbSender>>();

            return builder;
        }
    }
}
