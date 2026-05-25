using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Deveel.Messaging
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "FirebaseServiceAccountAuthentication")]
    public class FirebaseServiceAccountAuthenticationProviderTests
    {
        [Fact]
        public void Should_HandleCertificateScheme_When_CanHandle()
        {
            var provider = new FirebaseServiceAccountAuthenticationProvider(NullLogger<FirebaseServiceAccountAuthenticationProvider>.Instance);
            var config = new AuthenticationConfiguration(AuthenticationScheme.Certificate, "Cert");

            var result = provider.CanHandle(config);

            Assert.True(result);
        }

        [Fact]
        public void Should_HandleCustomScheme_When_FieldContainsServiceAccount()
        {
            var provider = new FirebaseServiceAccountAuthenticationProvider(NullLogger<FirebaseServiceAccountAuthenticationProvider>.Instance);
            var config = new AuthenticationConfiguration(AuthenticationScheme.Custom, "Custom")
                .WithField("ServiceAccountKey", DataType.String, f => f.AuthenticationRole = "principal");

            var result = provider.CanHandle(config);

            Assert.True(result);
        }

        [Fact]
        public void Should_NotHandleBearerScheme_When_CanHandle()
        {
            var provider = new FirebaseServiceAccountAuthenticationProvider(NullLogger<FirebaseServiceAccountAuthenticationProvider>.Instance);
            var config = new AuthenticationConfiguration(AuthenticationScheme.Bearer, "Bearer");

            var result = provider.CanHandle(config);

            Assert.False(result);
        }

        [Fact]
        public async Task Should_ReturnSuccess_When_ServiceAccountKeyParameterProvided()
        {
            var provider = new FirebaseServiceAccountAuthenticationProvider(NullLogger<FirebaseServiceAccountAuthenticationProvider>.Instance);
            var settings = new ConnectionSettings()
                .SetParameter("ServiceAccountKey", "{\"type\": \"service_account\"}");
            var config = new AuthenticationConfiguration(AuthenticationScheme.Certificate, "Firebase SA");

            var result = await provider.ObtainCredentialAsync(settings, config, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccessful);
            Assert.NotNull(result.Credential);
            Assert.Equal(AuthenticationScheme.Certificate, result.Credential.Scheme);
            Assert.Equal("ServiceAccount", result.Credential.Properties["CredentialType"]);
            Assert.Equal("Firebase", result.Credential.Properties["Provider"]);
        }

        [Fact]
        public async Task Should_ReturnSuccess_When_ServiceAccountJsonParameterProvided()
        {
            var provider = new FirebaseServiceAccountAuthenticationProvider(NullLogger<FirebaseServiceAccountAuthenticationProvider>.Instance);
            var settings = new ConnectionSettings()
                .SetParameter("ServiceAccountJson", "{\"type\": \"service_account\"}");
            var config = new AuthenticationConfiguration(AuthenticationScheme.Certificate, "Firebase SA");

            var result = await provider.ObtainCredentialAsync(settings, config, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public async Task Should_ReturnSuccess_When_CertificateParameterProvided()
        {
            var provider = new FirebaseServiceAccountAuthenticationProvider(NullLogger<FirebaseServiceAccountAuthenticationProvider>.Instance);
            var settings = new ConnectionSettings()
                .SetParameter("Certificate", "{\"type\": \"service_account\"}");
            var config = new AuthenticationConfiguration(AuthenticationScheme.Certificate, "Firebase SA");

            var result = await provider.ObtainCredentialAsync(settings, config, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_MissingServiceAccountKey()
        {
            var provider = new FirebaseServiceAccountAuthenticationProvider(NullLogger<FirebaseServiceAccountAuthenticationProvider>.Instance);
            var settings = new ConnectionSettings();
            var config = new AuthenticationConfiguration(AuthenticationScheme.Certificate, "Firebase SA");

            var result = await provider.ObtainCredentialAsync(settings, config, TestContext.Current.CancellationToken);

            Assert.False(result.IsSuccessful);
            Assert.Equal("MISSING_SERVICE_ACCOUNT_KEY", result.ErrorCode);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_ServiceAccountKeyIsNotValidJson()
        {
            var provider = new FirebaseServiceAccountAuthenticationProvider(NullLogger<FirebaseServiceAccountAuthenticationProvider>.Instance);
            var settings = new ConnectionSettings()
                .SetParameter("ServiceAccountKey", "not-json-at-all");
            var config = new AuthenticationConfiguration(AuthenticationScheme.Certificate, "Firebase SA");

            var result = await provider.ObtainCredentialAsync(settings, config, TestContext.Current.CancellationToken);

            Assert.False(result.IsSuccessful);
            Assert.Equal("INVALID_SERVICE_ACCOUNT_JSON", result.ErrorCode);
        }

        [Fact]
        public async Task Should_IncludeProjectId_When_Provided()
        {
            var provider = new FirebaseServiceAccountAuthenticationProvider(NullLogger<FirebaseServiceAccountAuthenticationProvider>.Instance);
            var settings = new ConnectionSettings()
                .SetParameter("ServiceAccountKey", "{\"type\": \"service_account\"}")
                .SetParameter("ProjectId", "my-project-123");
            var config = new AuthenticationConfiguration(AuthenticationScheme.Certificate, "Firebase SA");

            var result = await provider.ObtainCredentialAsync(settings, config, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccessful);
            Assert.Equal("my-project-123", result.Credential!.Properties["ProjectId"]);
        }

        [Fact]
        public async Task Should_ReturnExisting_When_RefreshCredentialAsyncValid()
        {
            var provider = new FirebaseServiceAccountAuthenticationProvider(NullLogger<FirebaseServiceAccountAuthenticationProvider>.Instance);
            var existing = AuthenticationCredential.ForCertificate("{\"type\": \"service_account\"}");

            var result = await provider.RefreshCredentialAsync(existing, new ConnectionSettings(), new AuthenticationConfiguration(AuthenticationScheme.Certificate, "Firebase SA"), TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccessful);
            Assert.Same(existing, result.Credential);
        }

        [Fact]
        public async Task Should_Reobtain_When_RefreshCredentialAsyncInvalidScheme()
        {
            var provider = new FirebaseServiceAccountAuthenticationProvider(NullLogger<FirebaseServiceAccountAuthenticationProvider>.Instance);
            var settings = new ConnectionSettings()
                .SetParameter("ServiceAccountKey", "{\"type\": \"service_account\"}");
            var config = new AuthenticationConfiguration(AuthenticationScheme.Certificate, "Firebase SA");
            var existing = new AuthenticationCredential(AuthenticationScheme.Bearer, "some-token");

            var result = await provider.RefreshCredentialAsync(existing, settings, config, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccessful);
            Assert.NotSame(existing, result.Credential);
        }

        [Fact]
        public async Task Should_Reobtain_When_RefreshCredentialAsyncDifferentScheme()
        {
            var provider = new FirebaseServiceAccountAuthenticationProvider(NullLogger<FirebaseServiceAccountAuthenticationProvider>.Instance);
            var settings = new ConnectionSettings()
                .SetParameter("ServiceAccountKey", "{\"type\": \"service_account\"}");
            var config = new AuthenticationConfiguration(AuthenticationScheme.Certificate, "Firebase SA");
            var existing = AuthenticationCredential.ForBearerToken("some-token");

            var result = await provider.RefreshCredentialAsync(existing, settings, config, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccessful);
            Assert.NotSame(existing, result.Credential);
        }
    }
}
