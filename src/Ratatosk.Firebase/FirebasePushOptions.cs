namespace Ratatosk
{
    /// <summary>
    /// Provides options for configuring the Firebase Cloud Messaging connector.
    /// </summary>
    public class FirebasePushOptions : IConnectorOptions
    {
        /// <summary>
        /// Gets or sets the Firebase project identifier.
        /// </summary>
        public string? ProjectId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the message should
        /// be sent in dry run mode (not actually delivered).
        /// </summary>
        public bool DryRun { get; set; }

        /// <summary>
        /// Gets or sets the timeout for send operations.
        /// </summary>
        public TimeSpan? SendTimeout { get; set; }

        /// <summary>
        /// Gets or sets the timeout for receive operations.
        /// </summary>
        public TimeSpan? ReceiveTimeout { get; set; }

        /// <summary>
        /// Gets or sets the timeout for status query operations.
        /// </summary>
        public TimeSpan? StatusQueryTimeout { get; set; }

        /// <summary>
        /// Gets or sets whether timeout errors should be retried.
        /// </summary>
        public bool? RetryOnTimeout { get; set; }

        /// <inheritdoc/>
        public ConnectionSettings ToConnectionSettings()
        {
            var settings = new ConnectionSettings();

            if (!string.IsNullOrWhiteSpace(ProjectId))
                settings.SetParameter(FirebaseConnectionParameters.ProjectId, ProjectId);
            if (DryRun)
                settings.SetParameter(FirebaseConnectionParameters.DryRun, true);

            if (SendTimeout.HasValue)
                settings.SetParameter(TimeoutSettingsKeys.SendTimeout, SendTimeout.Value);
            if (ReceiveTimeout.HasValue)
                settings.SetParameter(TimeoutSettingsKeys.ReceiveTimeout, ReceiveTimeout.Value);
            if (StatusQueryTimeout.HasValue)
                settings.SetParameter(TimeoutSettingsKeys.StatusQueryTimeout, StatusQueryTimeout.Value);
            if (RetryOnTimeout.HasValue)
                settings.SetParameter(TimeoutSettingsKeys.RetryOnTimeout, RetryOnTimeout.Value);

            return settings;
        }
    }
}
