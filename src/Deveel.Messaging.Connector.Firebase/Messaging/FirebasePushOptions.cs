namespace Deveel.Messaging
{
    public class FirebasePushOptions : IConnectorOptions
    {
        public string? ProjectId { get; set; }

        public bool DryRun { get; set; }

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
