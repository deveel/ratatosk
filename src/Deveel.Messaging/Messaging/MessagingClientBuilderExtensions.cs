using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Deveel.Messaging
{
    /// <summary>
    /// Provides extension methods for registering the messaging client
    /// (<see cref="IMessagingClient"/>) into the application's service
    /// collection via the <see cref="MessagingBuilder"/>.
    /// </summary>
    public static class MessagingClientBuilderExtensions
    {
        /// <summary>
        /// Registers an <see cref="IMessagingClient"/> singleton into the
        /// service collection with default options.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="MessagingBuilder"/> instance used to configure
        /// the messaging services.
        /// </param>
        /// <returns>
        /// Returns the same <see cref="MessagingBuilder"/> instance to
        /// allow chaining further registrations.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="builder"/> is <c>null</c>.
        /// </exception>
        public static MessagingBuilder AddClient(this MessagingBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Services.TryAddSingleton<MessagingClientOptions>(new MessagingClientOptions());
            builder.Services.TryAddSingleton<IMessagingClient, MessagingClient>();
            return builder;
        }

        /// <summary>
        /// Registers an <see cref="IMessagingClient"/> singleton into the
        /// service collection with the specified options.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="MessagingBuilder"/> instance used to configure
        /// the messaging services.
        /// </param>
        /// <param name="configure">
        /// A delegate that configures the <see cref="MessagingClientOptions"/>
        /// instance used by the client.
        /// </param>
        /// <returns>
        /// Returns the same <see cref="MessagingBuilder"/> instance to
        /// allow chaining further registrations.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="builder"/> or <paramref name="configure"/>
        /// is <c>null</c>.
        /// </exception>
        public static MessagingBuilder AddClient(this MessagingBuilder builder, Action<MessagingClientOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);

            var options = new MessagingClientOptions();
            configure(options);
            builder.Services.TryAddSingleton<MessagingClientOptions>(options);
            builder.Services.TryAddSingleton<IMessagingClient>(sp =>
                ActivatorUtilities.CreateInstance<MessagingClient>(sp, options));
            return builder;
        }
    }
}
