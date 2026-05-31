using Microsoft.Extensions.DependencyInjection;

namespace Ratatosk.Senders
{
    /// <summary>
    /// Provides extension methods for configuring the in-memory sender store.
    /// </summary>
    public static class SenderServiceBuilderExtensions
    {
        /// <summary>
        /// Configures the sender services to use an in-memory store.
        /// </summary>
        /// <param name="builder">The sender service builder.</param>
        /// <param name="seedData">Optional seed data to populate the store on startup.</param>
        /// <returns>The current builder instance for chaining.</returns>
        public static SenderServiceBuilder UseInMemoryStore(
            this SenderServiceBuilder builder,
            IEnumerable<SenderEntity>? seedData = null)
        {
            ArgumentNullException.ThrowIfNull(builder);

            if (seedData != null)
                builder.Services.AddSingleton(seedData);

            builder.Services.AddRepositoryContext()
                .AddRepository<InMemorySenderRepository>();

            return builder;
        }
    }
}
