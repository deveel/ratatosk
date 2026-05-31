using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ratatosk.Senders
{
    /// <summary>
    /// Provides a fluent builder for configuring sender identity services.
    /// </summary>
    public sealed class SenderServiceBuilder
    {
        internal SenderServiceBuilder(IServiceCollection services, Type senderType)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(senderType);

            Services = services;
            SenderType = senderType;

            services.AddOptions<SenderCacheOptions>();
            services.TryAddSingleton<IDistributedCache, MemoryDistributedCache>();
            services.TryAddSingleton<ISenderCache, DistributedSenderCache>();
            services.TryAddScoped(typeof(SenderManager<>).MakeGenericType(senderType));
            services.TryAddScoped(typeof(ISenderValidator<>).MakeGenericType(senderType),
                typeof(SenderValidator<>).MakeGenericType(senderType));
            services.TryAddScoped(typeof(ISenderResolver),
                typeof(SenderResolver<>).MakeGenericType(senderType));
        }
        
        /// <summary>
        /// Gets the service collection for registering sender services.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Gets the type of sender entity being configured.
        /// </summary>
        public Type SenderType { get; }

        /// <summary>
        /// Replaces the default sender cache with the specified implementation.
        /// </summary>
        /// <typeparam name="TCache">The cache implementation type.</typeparam>
        /// <returns>The current builder instance for chaining.</returns>
        public SenderServiceBuilder WithCache<TCache>()
            where TCache : class, ISenderCache
        {
            Services.Replace(ServiceDescriptor.Singleton<ISenderCache, TCache>());
            return this;
        }

        /// <summary>
        /// Replaces the default sender cache with a factory-created instance.
        /// </summary>
        /// <param name="factory">A factory delegate to create the cache instance.</param>
        /// <returns>The current builder instance for chaining.</returns>
        public SenderServiceBuilder WithCache(Func<IServiceProvider, ISenderCache> factory)
        {
            ArgumentNullException.ThrowIfNull(factory);
            Services.Replace(ServiceDescriptor.Singleton(factory));
            return this;
        }

        /// <summary>
        /// Configures the sender cache options.
        /// </summary>
        /// <param name="configure">A delegate to configure the cache options.</param>
        /// <returns>The current builder instance for chaining.</returns>
        public SenderServiceBuilder ConfigureCacheOptions(Action<SenderCacheOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);
            Services.Configure(configure);
            return this;
        }

        /// <summary>
        /// Replaces the default sender resolver with the specified implementation.
        /// </summary>
        /// <typeparam name="TResolver">The resolver implementation type.</typeparam>
        /// <returns>The current builder instance for chaining.</returns>
        public SenderServiceBuilder WithResolver<TResolver>()
            where TResolver : class, ISenderResolver
        {
            Services.Replace(ServiceDescriptor.Scoped<ISenderResolver, TResolver>());
            return this;
        }

        /// <summary>
        /// Replaces the default sender resolver with a factory-created instance.
        /// </summary>
        /// <param name="factory">A factory delegate to create the resolver instance.</param>
        /// <returns>The current builder instance for chaining.</returns>
        public SenderServiceBuilder WithResolver(Func<IServiceProvider, ISenderResolver> factory)
        {
            ArgumentNullException.ThrowIfNull(factory);
            Services.Replace(ServiceDescriptor.Scoped(factory));
            return this;
        }
    }
}
