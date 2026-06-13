namespace Ratatosk
{
    /// <summary>
    /// Provides constant keys for timeout-related connection settings.
    /// </summary>
    public static class TimeoutSettingsKeys
    {
        /// <summary>
        /// The key for the send timeout setting.
        /// </summary>
        public const string SendTimeout = "Timeout.Send";

        /// <summary>
        /// The key for the receive timeout setting.
        /// </summary>
        public const string ReceiveTimeout = "Timeout.Receive";

        /// <summary>
        /// The key for the status query timeout setting.
        /// </summary>
        public const string StatusQueryTimeout = "Timeout.StatusQuery";

        /// <summary>
        /// The key for the retry-on-timeout setting.
        /// </summary>
        public const string RetryOnTimeout = "Timeout.RetryOnTimeout";
    }
}
