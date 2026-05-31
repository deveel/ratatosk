using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ratatosk.Senders
{
    /// <summary>
    /// Provides extension methods for configuring the in-memory
    /// sender store on <see cref="SenderRegistrationBuilder{TConnector}"/>.
    /// </summary>
    public static class SenderRegistrationBuilderExtensions
    {
        /// <summary>
        /// Configures the sender storage to use an in-memory store,
        /// optionally seeded with sender entities.
        /// </summary>
        public static SenderRegistrationBuilder<TConnector> UseInMemoryStore<TConnector>(
            this SenderRegistrationBuilder<TConnector> builder,
            IEnumerable<SenderEntity>? seedSenders = null)
            where TConnector : class, IChannelConnector
        {
            AddInMemoryStore(builder.Services, seedSenders);
            return builder;
        }

        private static void AddInMemoryStore(IServiceCollection services, IEnumerable<SenderEntity>? seedSenders)
        {
            var repoBuilder = services.AddRepositoryContext()
                .AddRepository<InMemorySenderRepository>();

            if (seedSenders != null)
            {
                repoBuilder.WithSeedData(seedSenders);

                foreach (var sender in seedSenders)
                    services.AddSingleton(sender);
            }

            services.TryAddScoped<SenderManager<SenderEntity>>();
            services.TryAddScoped<ISenderResolver, SenderResolver<SenderEntity>>();
        }
    }
}
