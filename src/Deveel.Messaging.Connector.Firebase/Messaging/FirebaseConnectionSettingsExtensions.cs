namespace Deveel.Messaging
{
    public static class FirebaseConnectionSettingsExtensions
    {
        public static string? GetProjectId(this ConnectionSettings settings)
            => settings.GetParameter<string>(FirebaseConnectionParameters.ProjectId);

        public static string? GetServiceAccountKey(this ConnectionSettings settings)
            => settings.GetParameter<string>(FirebaseConnectionParameters.ServiceAccountKey);

        public static bool? GetDryRun(this ConnectionSettings settings)
            => settings.GetParameter<bool?>(FirebaseConnectionParameters.DryRun);
    }
}
