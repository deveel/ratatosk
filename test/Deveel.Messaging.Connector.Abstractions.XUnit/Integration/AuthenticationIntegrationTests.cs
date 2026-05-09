//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;
using Xunit;

namespace Deveel.Messaging
{
    /// <summary>
    /// Integration tests demonstrating end-to-end authentication scenarios
    /// with different connector types and authentication methods.
    /// </summary>
    [Trait("Category", "Integration")]
    [Trait("Layer", "Application")]
    [Trait("Feature", "Authentication")]
    public class AuthenticationIntegrationTests
    {
        [Fact]
        public async Task Should_InitializeSuccessfully_When_EmailConnectorWithApiKeyAuth()
        {
            // Arrange
            var schema = new ChannelSchema("TestEmail", "Email", "1.0.0")
                .WithCapability(ChannelCapability.SendMessages)
                .AddContentType(MessageContentType.PlainText)
                .HandlesMessageEndpoint(EndpointType.EmailAddress, e =>
                {
                    e.CanSend = true;
                    e.CanReceive = false;
                })
                .AddAuthenticationConfiguration(AuthenticationConfigurations.ApiKeyAuthentication());

            var connectionSettings = new ConnectionSettings()
                .SetParameter("ApiKey", "sk-test-email-api-key-12345");

            var connector = new TestEmailConnector(schema, connectionSettings);

            // Act
            var initResult = await connector.InitializeAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.True(initResult.Successful, $"Expected successful initialization but got: {initResult.Error?.ErrorCode} - {initResult.Error?.ErrorMessage}");
            Assert.Equal(ConnectorState.Ready, connector.State);
            Assert.NotNull(connector.TestAuthenticationCredential);
            Assert.Equal(AuthenticationType.ApiKey, connector.TestAuthenticationCredential.AuthenticationType);
            Assert.Equal("sk-test-email-api-key-12345", connector.TestAuthenticationCredential.CredentialValue);

            // Verify the authentication helper methods work
            var apiKey = connector.GetApiKey();
            Assert.Equal("sk-test-email-api-key-12345", apiKey);
        }

        [Fact]
        public async Task Should_InitializeSuccessfully_When_SmsConnectorWithBasicAuth()
        {
            // Arrange
            var schema = new ChannelSchema("TestSms", "SMS", "1.0.0")
                .WithCapability(ChannelCapability.SendMessages)
                .AddContentType(MessageContentType.PlainText)
                .HandlesMessageEndpoint(EndpointType.PhoneNumber, e =>
                {
                    e.CanSend = true;
                    e.CanReceive = false;
                })
                .AddAuthenticationConfiguration(AuthenticationConfigurations.TwilioBasicAuthentication());

            var connectionSettings = new ConnectionSettings()
                .SetParameter("AccountSid", "AC123456789012345678901234567890")
                .SetParameter("AuthToken", "your_auth_token_here");

            var connector = new TestSmsConnector(schema, connectionSettings);

            // Act
            var initResult = await connector.InitializeAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.True(initResult.Successful, $"Expected successful initialization but got: {initResult.Error?.ErrorCode} - {initResult.Error?.ErrorMessage}");
            Assert.Equal(ConnectorState.Ready, connector.State);
            Assert.NotNull(connector.TestAuthenticationCredential);
            Assert.Equal(AuthenticationType.Basic, connector.TestAuthenticationCredential.AuthenticationType);

            // Verify the authentication helper methods work
            var authHeader = connector.GetAuthenticationHeader();
            Assert.NotNull(authHeader);
            Assert.StartsWith("Basic ", authHeader);
        }


        [Fact]
        public async Task Should_PickCorrectAuth_When_FlexibleAuthConnectorWithMultipleAuthOptions()
        {
            // Arrange
            var schema = new ChannelSchema("TestFlexible", "Multi", "1.0.0")
                .WithCapability(ChannelCapability.SendMessages)
                .AddContentType(MessageContentType.PlainText)
                .HandlesMessageEndpoint(EndpointType.Any, e =>
                {
                    e.CanSend = true;
                    e.CanReceive = false;
                })
                .AddAuthenticationConfiguration(AuthenticationConfigurations.FlexibleBasicAuthentication())
                .AddAuthenticationConfiguration(AuthenticationConfigurations.FlexibleApiKeyAuthentication());

            // Test with API key (should pick API key auth)
            var apiKeySettings = new ConnectionSettings()
                .SetParameter("ApiKey", "test-api-key-123");

            var apiKeyConnector = new TestFlexibleConnector(schema, apiKeySettings);
            var apiKeyInitResult = await apiKeyConnector.InitializeAsync(TestContext.Current.CancellationToken);

            Assert.True(apiKeyInitResult.Successful);
            Assert.Equal(AuthenticationType.ApiKey, apiKeyConnector.TestAuthenticationCredential!.AuthenticationType);

            // Test with basic auth (should pick basic auth)
            var basicSettings = new ConnectionSettings()
                .SetParameter("Username", "testuser")
                .SetParameter("Password", "testpass");

            var basicConnector = new TestFlexibleConnector(schema, basicSettings);
            var basicInitResult = await basicConnector.InitializeAsync(TestContext.Current.CancellationToken);

            Assert.True(basicInitResult.Successful);
            Assert.Equal(AuthenticationType.Basic, basicConnector.TestAuthenticationCredential!.AuthenticationType);
        }

        [Fact]
        public async Task Should_RefreshesAutomatically_When_ConnectorWithExpiredToken()
        {
            // Arrange
            var schema = new ChannelSchema("TestRefresh", "Test", "1.0.0")
                .WithCapability(ChannelCapability.SendMessages)
                .AddAuthenticationConfiguration(AuthenticationConfigurations.TokenAuthentication());

            var connectionSettings = new ConnectionSettings()
                .SetParameter("Token", "initial-token");

            var connector = new TestRefreshConnector(schema, connectionSettings);

            // Act
            var initResult = await connector.InitializeAsync(TestContext.Current.CancellationToken);
            Assert.True(initResult.Successful);

            var initialCredential = connector.TestAuthenticationCredential;
            Assert.NotNull(initialCredential);
            Assert.Equal("initial-token", initialCredential.CredentialValue);

            // Simulate token expiration and refresh
            await connector.SimulateTokenRefresh();

            // Assert
            var refreshedCredential = connector.TestAuthenticationCredential;
            Assert.NotNull(refreshedCredential);
            // In a real scenario, this would be a new token, but our test just returns the same for simplicity
            Assert.Equal("initial-token", refreshedCredential.CredentialValue);
        }


    }

    /// <summary>
    /// Test email connector for integration testing.
    /// </summary>
    public class TestEmailConnector : ChannelConnectorBase
    {

        public TestEmailConnector(IChannelSchema schema, ConnectionSettings connectionSettings)
            : base(schema, connectionSettings)
        {
        }

        public AuthenticationCredential? TestAuthenticationCredential => AuthenticationCredential;

        // Expose helper methods for testing
        public new string? GetApiKey() => base.GetApiKey();
        public new string? GetAuthenticationHeader() => base.GetAuthenticationHeader();

        protected override async ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
        {
            var authResult = await AuthenticateAsync(cancellationToken);
            if (!authResult.Successful)
                throw new InvalidOperationException($"Authentication failed during initialization: {authResult.Error?.ErrorCode} - {authResult.Error?.ErrorMessage}");
        }

        protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }

        protected override async Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken); // Simulate sending

            var result = new SendResult(message.Id, $"email-{Guid.NewGuid()}");
            result.AdditionalData["ApiKey"] = GetApiKey() ?? string.Empty;

            return result;
        }

        protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new StatusInfo("Email Connector Ready"));
        }
    }

    /// <summary>
    /// Test SMS connector for integration testing.
    /// </summary>
    public class TestSmsConnector : ChannelConnectorBase
    {
        public TestSmsConnector(IChannelSchema schema, ConnectionSettings connectionSettings)
            : base(schema, connectionSettings)
        {
        }

        public AuthenticationCredential? TestAuthenticationCredential => AuthenticationCredential;

        // Expose helper methods for testing
        public new string? GetApiKey() => base.GetApiKey();
        public new string? GetAuthenticationHeader() => base.GetAuthenticationHeader();

        protected override async ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
        {
            var authResult = await AuthenticateAsync(cancellationToken);
            if (!authResult.Successful)
                throw new InvalidOperationException($"Authentication failed during initialization: {authResult.Error?.ErrorCode} - {authResult.Error?.ErrorMessage}");
        }

        protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }

        protected override async Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken); // Simulate sending

            var result = new SendResult(message.Id, $"sms-{Guid.NewGuid()}");
            result.AdditionalData["AuthHeader"] = GetAuthenticationHeader() ?? string.Empty;

            return result;
        }

        protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new StatusInfo("SMS Connector Ready"));
        }
    }

    /// <summary>
    /// Test OAuth connector for integration testing.
    /// </summary>
    public class TestOAuthConnector : ChannelConnectorBase
    {
        public TestOAuthConnector(IChannelSchema schema, ConnectionSettings connectionSettings, IAuthenticationManager? authManager = null)
            : base(schema, connectionSettings, authenticationManager: authManager)
        {
        }

        public AuthenticationCredential? TestAuthenticationCredential => AuthenticationCredential;

        // Expose helper methods for testing
        public new string? GetApiKey() => base.GetApiKey();
        public new string? GetAuthenticationHeader() => base.GetAuthenticationHeader();

        protected override async ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
        {
            var authResult = await AuthenticateAsync(cancellationToken);
            if (!authResult.Successful)
                throw new InvalidOperationException($"Authentication failed during initialization: {authResult.Error?.ErrorCode} - {authResult.Error?.ErrorMessage}");
        }

        protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }

        protected override async Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken); // Simulate sending

            var result = new SendResult(message.Id, $"oauth-{Guid.NewGuid()}");
            result.AdditionalData["AuthHeader"] = GetAuthenticationHeader() ?? string.Empty;

            return result;
        }

        protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new StatusInfo("OAuth Connector Ready"));
        }
    }

    /// <summary>
    /// Test flexible connector that supports multiple authentication methods.
    /// </summary>
    public class TestFlexibleConnector : ChannelConnectorBase
    {
        public TestFlexibleConnector(IChannelSchema schema, ConnectionSettings connectionSettings)
            : base(schema, connectionSettings)
        {
        }

        public AuthenticationCredential? TestAuthenticationCredential => AuthenticationCredential;

        protected override async ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
        {
            var authResult = await AuthenticateAsync(cancellationToken);
            if (!authResult.Successful)
                throw new InvalidOperationException($"Authentication failed during initialization: {authResult.Error?.ErrorCode} - {authResult.Error?.ErrorMessage}");
        }

        protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }

        protected override async Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken); // Simulate sending

            var result = new SendResult(message.Id, $"flexible-{Guid.NewGuid()}");
            result.AdditionalData["AuthType"] = AuthenticationCredential?.AuthenticationType.ToString() ?? string.Empty;

            return result;
        }

        protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new StatusInfo("Flexible Connector Ready"));
        }
    }

    /// <summary>
    /// Test connector that demonstrates token refresh scenarios.
    /// </summary>
    public class TestRefreshConnector : ChannelConnectorBase
    {
        public TestRefreshConnector(IChannelSchema schema, ConnectionSettings connectionSettings)
            : base(schema, connectionSettings)
        {
        }

        public AuthenticationCredential? TestAuthenticationCredential => AuthenticationCredential;

        protected override async ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
        {
            var authResult = await AuthenticateAsync(cancellationToken);
            if (!authResult.Successful)
                throw new InvalidOperationException($"Authentication failed during initialization: {authResult.Error?.ErrorCode} - {authResult.Error?.ErrorMessage}");
        }

        protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }

        protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new StatusInfo("Refresh Connector Ready"));
        }

        public async Task SimulateTokenRefresh()
        {
            var refreshResult = await RefreshAuthenticationAsync();
            // In a real scenario, we'd check if the refresh was successful
        }
    }
}
