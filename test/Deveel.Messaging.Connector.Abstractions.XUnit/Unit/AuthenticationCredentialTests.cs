namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Feature", "AuthenticationCredential")]
public class AuthenticationCredentialTests
{
    [Fact]
    public void Should_CreateWithTypeAndValue()
    {
        var cred = new AuthenticationCredential(AuthenticationType.ApiKey, "my-key");
        Assert.Equal(AuthenticationType.ApiKey, cred.AuthenticationType);
        Assert.Equal("my-key", cred.CredentialValue);
        Assert.Null(cred.ExpiresAt);
        Assert.False(cred.IsExpired);
    }

    [Fact]
    public void Should_Throw_When_ValueIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new AuthenticationCredential(AuthenticationType.ApiKey, null!));
    }

    [Fact]
    public void Should_Throw_When_ValueIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new AuthenticationCredential(AuthenticationType.ApiKey, ""));
    }

    [Fact]
    public void Should_BeExpired_When_ExpirationPassed()
    {
        var cred = new AuthenticationCredential(AuthenticationType.Token, "tok", DateTime.UtcNow.AddMinutes(-5));
        Assert.True(cred.IsExpired);
    }

    [Fact]
    public void Should_NotBeExpired_When_ExpirationInFuture()
    {
        var cred = new AuthenticationCredential(AuthenticationType.Token, "tok", DateTime.UtcNow.AddHours(1));
        Assert.False(cred.IsExpired);
    }

    [Fact]
    public void Should_WillExpireSoon()
    {
        var cred = new AuthenticationCredential(AuthenticationType.Token, "tok", DateTime.UtcNow.AddSeconds(30));
        Assert.True(cred.WillExpireSoon(TimeSpan.FromMinutes(1)));
    }

    [Fact]
    public void Should_NotWillExpireSoon()
    {
        var cred = new AuthenticationCredential(AuthenticationType.Token, "tok", DateTime.UtcNow.AddHours(2));
        Assert.False(cred.WillExpireSoon(TimeSpan.FromMinutes(1)));
    }

    [Fact]
    public void Should_WillExpireSoon_ReturnFalse_WhenNoExpiration()
    {
        var cred = new AuthenticationCredential(AuthenticationType.ApiKey, "key");
        Assert.False(cred.WillExpireSoon(TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void Should_GetTimeUntilExpiration()
    {
        var cred = new AuthenticationCredential(AuthenticationType.Token, "tok", DateTime.UtcNow.AddMinutes(30));
        var remaining = cred.GetTimeUntilExpiration();
        Assert.NotNull(remaining);
        Assert.True(remaining.Value.TotalMinutes > 29);
    }

    [Fact]
    public void Should_GetTimeUntilExpiration_ReturnNull_WhenNoExpiration()
    {
        var cred = new AuthenticationCredential(AuthenticationType.ApiKey, "key");
        Assert.Null(cred.GetTimeUntilExpiration());
    }

    [Fact]
    public void Should_GetTimeUntilExpiration_ReturnZero_WhenExpired()
    {
        var cred = new AuthenticationCredential(AuthenticationType.Token, "tok", DateTime.UtcNow.AddMinutes(-5));
        var remaining = cred.GetTimeUntilExpiration();
        Assert.NotNull(remaining);
        Assert.Equal(TimeSpan.Zero, remaining.Value);
    }

    [Fact]
    public void Should_CreateTokenCredential()
    {
        var cred = AuthenticationCredential.CreateToken("my-token", null, "Bearer");
        Assert.Equal(AuthenticationType.Token, cred.AuthenticationType);
        Assert.Equal("my-token", cred.CredentialValue);
        Assert.Equal("Bearer", cred.Properties["TokenType"]);
    }

    [Fact]
    public void Should_CreateTokenWithExpiration()
    {
        var expires = DateTime.UtcNow.AddHours(1);
        var cred = AuthenticationCredential.CreateToken("tok", expires);
        Assert.Equal(expires, cred.ExpiresAt);
    }

    [Fact]
    public void Should_CreateApiKeyCredential()
    {
        var cred = AuthenticationCredential.CreateApiKey("api-key-123");
        Assert.Equal(AuthenticationType.ApiKey, cred.AuthenticationType);
        Assert.Equal("api-key-123", cred.CredentialValue);
    }

    [Fact]
    public void Should_Throw_When_CreateApiKeyWithNull()
    {
        Assert.Throws<ArgumentNullException>(() => AuthenticationCredential.CreateApiKey(null!));
    }

    [Fact]
    public void Should_CreateBasicCredential()
    {
        var cred = AuthenticationCredential.CreateBasic("user", "pass");
        Assert.Equal(AuthenticationType.Basic, cred.AuthenticationType);
        Assert.Equal("user", cred.Properties["Username"]);
        Assert.Equal("pass", cred.Properties["Password"]);
    }

    [Fact]
    public void Should_Throw_When_CreateBasicWithNull()
    {
        Assert.Throws<ArgumentNullException>(() => AuthenticationCredential.CreateBasic(null!, "pass"));
        Assert.Throws<ArgumentNullException>(() => AuthenticationCredential.CreateBasic("user", null!));
    }

    [Fact]
    public void Should_HaveObtainedAtTimestamp()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var cred = new AuthenticationCredential(AuthenticationType.ApiKey, "key");
        var after = DateTime.UtcNow.AddSeconds(1);
        Assert.InRange(cred.ObtainedAt, before, after);
    }
}
