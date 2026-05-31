using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ratatosk.Senders
{
    /// <summary>
    /// Provides extension methods for configuring Entity Framework as the sender store.
    /// </summary>
    public static class SenderServiceBuilderExtensions
    {
        /// <summary>
        /// Configures the sender services to use Entity Framework for persistence.
        /// </summary>
        /// <param name="builder">The sender service builder.</param>
        /// <param name="configureDbContext">A delegate to configure the DbContext options.</param>
        /// <returns>The current builder instance for chaining.</returns>
        public static SenderServiceBuilder UseEntityFramework(
            this SenderServiceBuilder builder,
            Action<DbContextOptionsBuilder> configureDbContext)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configureDbContext);

            builder.Services.AddDbContext<SenderDbContext>(configureDbContext);
            builder.Services.AddLogging();
            builder.Services.AddRepositoryContext()
                .AddRepository<EntitySenderRepository>();
            builder.Services.TryAddScoped<ISenderValidator<DbSender>, SenderValidator<DbSender>>();
            builder.Services.TryAddScoped<SenderManager<DbSender>>();
            builder.Services.AddScoped<ISenderRepository<ISender>>(sp =>
                new SenderRepositoryAdapter<DbSender>(sp.GetRequiredService<ISenderRepository<DbSender>>()));

            return builder;
        }
    }
}
