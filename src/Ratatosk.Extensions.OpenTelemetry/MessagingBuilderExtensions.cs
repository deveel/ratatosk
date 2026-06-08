using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;

namespace Ratatosk
{
    /// <summary>
    /// Provides extension methods for <see cref="MessagingBuilder"/> to
    /// enable OpenTelemetry instrumentation.
    /// </summary>
    public static class MessagingBuilderExtensions
    {
        /// <summary>
        /// Configures OpenTelemetry tracing and metrics for all Ratatosk
        /// sources registered in the application.
        /// </summary>
        /// <param name="builder">The <see cref="MessagingBuilder"/> to configure.</param>
        /// <returns>The builder instance for chaining.</returns>
        /// <remarks>
        /// This is a convenience method that internally calls
        /// <c>AddOpenTelemetry()</c> on the service collection and
        /// invokes <c>AddRatatoskInstrumentation()</c> on both the
        /// tracer and meter providers.
        ///
        /// If you need fine-grained control over OpenTelemetry configuration,
        /// use <c>AddRatatoskInstrumentation()</c> on
        /// <c>TracerProviderBuilder</c> and <c>MeterProviderBuilder</c>
        /// directly instead.
        /// </remarks>
        public static MessagingBuilder WithOpenTelemetry(
            this MessagingBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Services.AddOpenTelemetry()
                .WithTracing(tracing => tracing.AddRatatoskInstrumentation())
                .WithMetrics(metrics => metrics.AddRatatoskInstrumentation());

            return builder;
        }
    }
}
