using Ratatosk.Senders;

namespace Ratatosk;

/// <summary>
/// Provides extension methods for <see cref="MessagingBuilder"/> to register sender identity services.
/// </summary>
public static class MessagingBuilderExtensions
{
    // ── Sender identity services ──────────────────────────────────────────

    /// <summary>
    /// Registers the sender identity services with a specific sender type,
    /// including cache, manager, validator, and resolver.
    /// </summary>
    /// <typeparam name="TSender">
    /// The type of sender entity, which must implement <see cref="ISender"/>.
    /// </typeparam>
    /// <returns>
    /// Returns a <see cref="SenderServiceBuilder"/> to further configure
    /// sender services.
    /// </returns>
    public static SenderServiceBuilder AddSenders<TSender>(this MessagingBuilder builder)
        where TSender : class, ISender
    {
        return new SenderServiceBuilder(builder.Services, typeof(TSender));
    }

    /// <summary>
    /// Registers and configures sender identity services with a specific
    /// sender type, using a configuration delegate.
    /// </summary>
    /// <typeparam name="TSender">
    /// The type of sender entity, which must implement <see cref="ISender"/>.
    /// </typeparam>
    /// <param name="configure">
    /// A delegate to configure the sender services.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="MessagingBuilder"/> instance
    /// to allow chaining further messaging configuration.
    /// </returns>
    public static MessagingBuilder AddSenders<TSender>(this MessagingBuilder builder, Action<SenderServiceBuilder> configure)
        where TSender : class, ISender
    {
        ArgumentNullException.ThrowIfNull(configure);

        var sender = new SenderServiceBuilder(builder.Services, typeof(TSender));
        configure(sender);

        return builder;
    }

}