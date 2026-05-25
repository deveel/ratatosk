namespace Ratatosk.XUnit
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "FirebasePushConnector")]
    public class FirebasePushOptionsTests
    {
        [Fact]
        public void ConnectionParameters_ShouldHaveExpectedValues()
        {
            Assert.Equal("ProjectId", FirebaseConnectionParameters.ProjectId);
            Assert.Equal("ServiceAccountKey", FirebaseConnectionParameters.ServiceAccountKey);
            Assert.Equal("DryRun", FirebaseConnectionParameters.DryRun);
        }

        [Fact]
        public void ConnectionSettingsDefaults_ShouldHaveExpectedValues()
        {
            Assert.False(FirebaseConnectionSettingsDefaults.DryRun);
        }

        [Fact]
        public void ToConnectionSettings_ShouldSetAllProperties()
        {
            var options = new FirebasePushOptions
            {
                ProjectId = "my-project",
                DryRun = true
            };

            var settings = options.ToConnectionSettings();

            Assert.Equal("my-project", settings.GetParameter(FirebaseConnectionParameters.ProjectId));
            Assert.True(settings.GetParameter<bool>(FirebaseConnectionParameters.DryRun));
        }

        [Fact]
        public void ToConnectionSettings_ShouldSkipEmptyProperties()
        {
            var options = new FirebasePushOptions
            {
                ProjectId = "my-project"
            };

            var settings = options.ToConnectionSettings();

            Assert.Equal("my-project", settings.GetParameter(FirebaseConnectionParameters.ProjectId));
            Assert.False(settings.GetParameter<bool?>(FirebaseConnectionParameters.DryRun) ?? false);
        }

        [Fact]
        public void ToConnectionSettings_ShouldReturnMinimal_WhenProjectIdOnly()
        {
            var options = new FirebasePushOptions
            {
                ProjectId = "my-project"
            };

            var settings = options.ToConnectionSettings();

            Assert.Single(settings.Parameters);
            Assert.Equal("my-project", settings.GetParameter(FirebaseConnectionParameters.ProjectId));
        }

        [Fact]
        public void Implements_IConnectorOptions()
        {
            var options = new FirebasePushOptions();
            Assert.IsAssignableFrom<IConnectorOptions>(options);
        }
    }
}
