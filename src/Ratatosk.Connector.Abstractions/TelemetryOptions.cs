namespace Ratatosk
{
    /// <summary>
    /// Configures which telemetry signals are emitted by a connector or client.
    /// </summary>
    public class TelemetryOptions
    {
        /// <summary>
        /// Gets or sets whether tracing (Activity) is enabled.
        /// </summary>
        public bool EnableTracing { get; set; } = true;

        /// <summary>
        /// Gets or sets whether metrics are enabled.
        /// </summary>
        public bool EnableMetrics { get; set; } = true;

        /// <summary>
        /// Gets or sets whether payload size metrics are collected.
        /// Requires serializing the message to measure its size,
        /// which may impact performance in high-throughput scenarios.
        /// </summary>
        public bool EnablePayloadSizeMetrics { get; set; } = false;
    }
}
