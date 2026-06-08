using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Ratatosk
{
    /// <summary>
    /// Provides extension methods for registering Ratatosk telemetry sources
    /// with OpenTelemetry.
    /// </summary>
    public static class OpenTelemetryBuilderExtensions
    {
        /// <summary>
        /// Adds Ratatosk ActivitySources to the tracer provider, enabling
        /// distributed tracing for messaging operations.
        /// </summary>
        /// <param name="builder">The <see cref="TracerProviderBuilder"/> to configure.</param>
        /// <returns>The builder instance for chaining.</returns>
        /// <remarks>
        /// This registers the following sources:
        /// <list type="bullet">
        ///   <item><c>Ratatosk.Client</c> — spans from the <c>IMessagingClient</c> facade</item>
        ///   <item><c>Ratatosk.Connector.*</c> — spans from all channel connectors (Twilio, SendGrid, etc.)</item>
        /// </list>
        /// </remarks>
        public static TracerProviderBuilder AddRatatoskInstrumentation(
            this TracerProviderBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.AddSource("Ratatosk.Client");
            builder.AddSource("Ratatosk.Connector.*");

            return builder;
        }

        /// <summary>
        /// Adds Ratatosk Meters to the meter provider, enabling metrics
        /// collection for messaging operations.
        /// </summary>
        /// <param name="builder">The <see cref="MeterProviderBuilder"/> to configure.</param>
        /// <returns>The builder instance for chaining.</returns>
        /// <remarks>
        /// This registers the following meters:
        /// <list type="bullet">
        ///   <item><c>Ratatosk.Client</c> — client-level metrics (sent count, send duration)</item>
        ///   <item><c>Ratatosk.Connector.*</c> — per-connector metrics (sent/received/failed counts, latency histograms)</item>
        /// </list>
        /// </remarks>
        public static MeterProviderBuilder AddRatatoskInstrumentation(
            this MeterProviderBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.AddMeter("Ratatosk.Client");
            builder.AddMeter("Ratatosk.Connector.*");

            return builder;
        }
    }
}
