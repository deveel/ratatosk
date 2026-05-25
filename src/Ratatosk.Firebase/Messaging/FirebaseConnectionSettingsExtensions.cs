namespace Ratatosk
{
    /// <summary>
    /// Provides extension methods for <see cref="ConnectionSettings"/> to access Firebase-specific parameters.
    /// </summary>
    public static class FirebaseConnectionSettingsExtensions
    {
        /// <summary>
        /// Gets the Firebase project identifier from the connection settings.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns>The project ID, or <c>null</c> if not configured.</returns>
        public static string? GetProjectId(this ConnectionSettings settings)
            => settings.GetParameter<string>(FirebaseConnectionParameters.ProjectId);

        /// <summary>
        /// Gets the Firebase service account key (JSON) from the connection settings.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns>The service account key, or <c>null</c> if not configured.</returns>
        public static string? GetServiceAccountKey(this ConnectionSettings settings)
            => settings.GetParameter<string>(FirebaseConnectionParameters.ServiceAccountKey);

        /// <summary>
        /// Gets whether the dry-run mode is enabled for Firebase message sending.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns><c>true</c> if dry-run mode is enabled, or <c>null</c> if not configured.</returns>
        public static bool? GetDryRun(this ConnectionSettings settings)
            => settings.GetParameter<bool?>(FirebaseConnectionParameters.DryRun);
    }
}
