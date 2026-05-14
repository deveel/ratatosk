namespace Deveel.Messaging
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

        /// <inheritdoc/>
        public ConnectionSettings ToConnectionSettings()
        {
            var settings = new ConnectionSettings();

            if (!string.IsNullOrWhiteSpace(ProjectId))
                settings.SetParameter(FirebaseConnectionParameters.ProjectId, ProjectId);
            if (DryRun)
                settings.SetParameter(FirebaseConnectionParameters.DryRun, true);

            return settings;
        }
    }
}
