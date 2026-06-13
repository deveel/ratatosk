namespace Ratatosk
{
    /// <summary>
    /// Configures timeout settings for connector operations.
    /// </summary>
    public class TimeoutOptions
    {
        /// <summary>
        /// Gets or sets the timeout for send operations.
        /// </summary>
        /// <remarks>
        /// Default is 60 seconds.
        /// </remarks>
        public TimeSpan SendTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Gets or sets the timeout for receive operations.
        /// </summary>
        /// <remarks>
        /// Default is 60 seconds.
        /// </remarks>
        public TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Gets or sets the timeout for status query operations.
        /// </summary>
        /// <remarks>
        /// Default is 30 seconds.
        /// </remarks>
        public TimeSpan StatusQueryTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets or sets whether timeout errors should be retried by default.
        /// </summary>
        /// <remarks>
        /// When <c>true</c>, timeout error codes are automatically added to the retry policy's
        /// retryable error codes. Default is <c>true</c>.
        /// </remarks>
        public bool RetryOnTimeout { get; set; } = true;

        /// <summary>
        /// Sets the send timeout.
        /// </summary>
        /// <param name="timeout">The timeout duration for send operations.</param>
        /// <returns>This instance for method chaining.</returns>
        public TimeoutOptions WithSendTimeout(TimeSpan timeout)
        {
            SendTimeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets the receive timeout.
        /// </summary>
        /// <param name="timeout">The timeout duration for receive operations.</param>
        /// <returns>This instance for method chaining.</returns>
        public TimeoutOptions WithReceiveTimeout(TimeSpan timeout)
        {
            ReceiveTimeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets the status query timeout.
        /// </summary>
        /// <param name="timeout">The timeout duration for status query operations.</param>
        /// <returns>This instance for method chaining.</returns>
        public TimeoutOptions WithStatusQueryTimeout(TimeSpan timeout)
        {
            StatusQueryTimeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets whether timeout errors should be retried.
        /// </summary>
        /// <param name="retry">If <c>true</c>, timeout errors will be retried.</param>
        /// <returns>This instance for method chaining.</returns>
        public TimeoutOptions WithRetryOnTimeout(bool retry)
        {
            RetryOnTimeout = retry;
            return this;
        }
    }
}
