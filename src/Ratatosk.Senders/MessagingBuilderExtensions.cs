using Ratatosk.Senders;

namespace Ratatosk;

/// <summary>
/// Provides extension methods for <see cref="MessagingBuilder"/> to register sender identity services.
/// </summary>
public static class MessagingBuilderExtensions
{
    // ── Sender identity services ──────────────────────────────────────────

    /// <summary>
    /// Registers the sender identity services,
    /// including cache, manager, validator, and resolver.
    /// </summary>
    /// <returns>
    /// Returns a <see cref="SenderServiceBuilder"/> to further configure
    /// sender services.
    /// </returns>
    public static SenderServiceBuilder AddSenders(this MessagingBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return new SenderServiceBuilder(builder.Services);
    }

    /// <summary>
    /// Registers and configures sender identity services
    /// using a configuration delegate.
    /// </summary>
    /// <param name="builder">The messaging builder.</param>
    /// <param name="configure">
    /// A delegate to configure the sender services.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="MessagingBuilder"/> instance
    /// to allow chaining further messaging configuration.
    /// </returns>
    public static MessagingBuilder AddSenders(this MessagingBuilder builder, Action<SenderServiceBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var sender = new SenderServiceBuilder(builder.Services);
        configure(sender);

        return builder;
    }

}