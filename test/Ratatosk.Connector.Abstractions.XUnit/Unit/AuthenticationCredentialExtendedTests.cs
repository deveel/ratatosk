namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "AuthenticationCredential")]
public class AuthenticationCredentialExtendedTests
{
    [Fact]
    public void Should_ReturnTrue_When_WillExpireSoon()
    {
        var credential = AuthenticationCredential.ForBearerToken("token", expiresAt: DateTime.UtcNow.AddMinutes(1));
        Assert.True(credential.WillExpireSoon(TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void Should_ReturnFalse_When_WillNotExpireSoon()
    {
        var credential = AuthenticationCredential.ForBearerToken("token", expiresAt: DateTime.UtcNow.AddDays(1));
        Assert.False(credential.WillExpireSoon(TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void Should_ReturnTrue_When_NoExpiration()
    {
        var credential = AuthenticationCredential.ForBearerToken("token");
        Assert.False(credential.WillExpireSoon(TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void Should_ReturnTimeUntilExpiration_When_HasExpiration()
    {
        var expiresAt = DateTime.UtcNow.AddHours(2);
        var credential = AuthenticationCredential.ForBearerToken("token", expiresAt: expiresAt);
        var timeLeft = credential.GetTimeUntilExpiration();
        Assert.NotNull(timeLeft);
        Assert.True(timeLeft.Value.TotalHours > 1);
    }

    [Fact]
    public void Should_ReturnNull_When_NoExpiration()
    {
        var credential = AuthenticationCredential.ForBearerToken("token");
        Assert.Null(credential.GetTimeUntilExpiration());
    }

    [Fact]
    public void Should_CreateForClientCredentials()
    {
        var credential = AuthenticationCredential.ForClientCredentials("access-token", DateTime.UtcNow.AddHours(1), refreshToken: "refresh-token");
        Assert.Equal(AuthenticationScheme.Bearer, credential.Scheme);
        Assert.Equal("access-token", credential.Value);
        Assert.NotNull(credential.ExpiresAt);
    }

    [Fact]
    public void Should_CreateForCertificate()
    {
        var credential = AuthenticationCredential.ForCertificate("certificate-json");
        Assert.Equal(AuthenticationScheme.Certificate, credential.Scheme);
        Assert.Equal("certificate-json", credential.Value);
    }

    [Fact]
    public void Should_ThrowArgumentNull_When_NullValue()
    {
        Assert.Throws<ArgumentNullException>(() => new AuthenticationCredential(AuthenticationScheme.ApiKey, null!));
    }

    [Fact]
    public void Should_ThrowArgumentException_When_EmptyValue()
    {
        Assert.Throws<ArgumentException>(() => new AuthenticationCredential(AuthenticationScheme.ApiKey, ""));
    }
}
