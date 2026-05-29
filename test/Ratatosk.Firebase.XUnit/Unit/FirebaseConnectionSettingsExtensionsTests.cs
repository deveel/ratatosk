namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "FirebaseConnector")]
public class FirebaseConnectionSettingsExtensionsTests
{
    [Fact]
    public void Should_GetProjectId_When_Set()
    {
        var settings = new ConnectionSettings();
        settings.SetParameter("ProjectId", "my-project");
        var result = settings.GetProjectId();
        Assert.Equal("my-project", result);
    }

    [Fact]
    public void Should_ReturnNull_When_ProjectIdNotSet()
    {
        var settings = new ConnectionSettings();
        var result = settings.GetProjectId();
        Assert.Null(result);
    }

    [Fact]
    public void Should_GetServiceAccountKey_When_Set()
    {
        var settings = new ConnectionSettings();
        settings.SetParameter("ServiceAccountKey", "{\"key\":\"value\"}");
        var result = settings.GetServiceAccountKey();
        Assert.Equal("{\"key\":\"value\"}", result);
    }

    [Fact]
    public void Should_ReturnNull_When_ServiceAccountKeyNotSet()
    {
        var settings = new ConnectionSettings();
        var result = settings.GetServiceAccountKey();
        Assert.Null(result);
    }

    [Fact]
    public void Should_GetDryRun_When_Set()
    {
        var settings = new ConnectionSettings();
        settings.SetParameter("DryRun", true);
        var result = settings.GetDryRun();
        Assert.True(result);
    }

    [Fact]
    public void Should_ReturnNull_When_DryRunNotSet()
    {
        var settings = new ConnectionSettings();
        var result = settings.GetDryRun();
        Assert.Null(result);
    }
}
