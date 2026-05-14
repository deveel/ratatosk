//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Xunit;

namespace Deveel.Messaging
{
    /// <summary>
    /// Debugging tests to identify and fix the authentication integration issues.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Application")]
    [Trait("Feature", "AuthenticationDebugging")]
    public class AuthenticationDebuggingTests
    {
        [Fact]
        public void Should_Step1_When_DebugSimpleApiKeyAuthentication()
        {
            // Step 1: Test if AuthenticationConfigurations works
            var config = AuthenticationConfigurations.ApiKeyAuthentication();

            Assert.NotNull(config);
            Assert.Equal(AuthenticationType.ApiKey, config.AuthenticationType);
            Assert.Single(config.RequiredFields);
        }

        [Fact]
        public void Should_Step2_When_DebugSimpleApiKeyAuthentication()
        {
            // Step 2: Test if schema creation works
			var schema = new ChannelSchemaBuilder("TestEmail", "Email", "1.0.0")
				.WithCapability(ChannelCapability.SendMessages)
				.AddContentType(MessageContentType.PlainText)
				.HandlesMessageEndpoint(EndpointType.EmailAddress, e =>
				{
					e.CanSend = true;
					e.CanReceive = false;
				})
				.AddAuthenticationConfiguration(AuthenticationConfigurations.ApiKeyAuthentication())
				.Build();

			Assert.NotNull(schema);
            Assert.Single(schema.AuthenticationConfigurations);
            Assert.Equal(AuthenticationType.ApiKey, schema.AuthenticationConfigurations.First().AuthenticationType);
        }

        [Fact]
        public void Should_Step3_When_DebugSimpleApiKeyAuthentication()
        {
            // Step 3: Test if connection settings work
            var connectionSettings = new ConnectionSettings()
                .SetParameter("ApiKey", "test-api-key");

            Assert.NotNull(connectionSettings);
            Assert.NotNull(connectionSettings.GetParameter("ApiKey"));
            Assert.Equal("test-api-key", connectionSettings.GetParameter("ApiKey"));
        }

        [Fact]
        public async Task Should_Step4_When_DebugSimpleApiKeyAuthentication()
        {
            // Step 4: Test authentication manager directly
            var authManager = new AuthenticationManager();
            var connectionSettings = new ConnectionSettings()
                .SetParameter("ApiKey", "test-api-key");
            var config = AuthenticationConfigurations.ApiKeyAuthentication();

            var result = await authManager.AuthenticateAsync(connectionSettings, config, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccessful, $"Authentication failed: {result.ErrorCode} - {result.ErrorMessage}");
            Assert.NotNull(result.Credential);
            Assert.Equal(AuthenticationType.ApiKey, result.Credential.AuthenticationType);
            Assert.Equal("test-api-key", result.Credential.CredentialValue);
        }

        [Fact]
        public async Task Should_Step5_When_DebugSimpleApiKeyAuthentication()
        {
            // Step 5: Test basic connector authentication without sending messages
			var schema = new ChannelSchemaBuilder("TestEmail", "Email", "1.0.0")
				.AddAuthenticationConfiguration(AuthenticationConfigurations.ApiKeyAuthentication())
				.Build();

			var connectionSettings = new ConnectionSettings()
                .SetParameter("ApiKey", "test-api-key");

            var connector = new DebugTestConnector(schema, connectionSettings);

            // Just test initialization which includes authentication
            var initResult = await connector.InitializeAsync(TestContext.Current.CancellationToken);

            Assert.True(initResult.IsSuccess(), $"Initialization failed: {initResult.Error?.Code} - {initResult.Error?.Message}");
            Assert.Equal(ConnectorState.Ready, connector.State);
            Assert.NotNull(connector.TestAuthenticationCredential);
            Assert.Equal(AuthenticationType.ApiKey, connector.TestAuthenticationCredential.AuthenticationType);
        }

        [Fact]
        public async Task Should_Step1_When_DebugBasicAuthentication()
        {
            // Test basic authentication
            var authManager = new AuthenticationManager();
            var connectionSettings = new ConnectionSettings()
                .SetParameter("AccountSid", "AC123456789")
                .SetParameter("AuthToken", "test-token");
            var config = AuthenticationConfigurations.TwilioBasicAuthentication();

            var result = await authManager.AuthenticateAsync(connectionSettings, config, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccessful, $"Authentication failed: {result.ErrorCode} - {result.ErrorMessage}");
            Assert.NotNull(result.Credential);
            Assert.Equal(AuthenticationType.Basic, result.Credential.AuthenticationType);
        }


    }

    /// <summary>
    /// Simple test connector for debugging authentication issues.
    /// </summary>
    public class DebugTestConnector : ChannelConnectorBase
    {

        public DebugTestConnector(IChannelSchema schema, ConnectionSettings connectionSettings)
            : base(schema, connectionSettings)
        {
        }

        public AuthenticationCredential? TestAuthenticationCredential => AuthenticationCredential;

        protected override async ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
        {
            // Only authenticate - don't do any other initialization
            var authResult = await AuthenticateAsync(cancellationToken);
            if (!authResult.IsSuccess())
                throw new Exception($"Authentication failed during initialization: {authResult.Error?.Code} - {authResult.Error?.Message}");
        }

        protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }

        protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Debugging connector doesn't support sending messages");
        }

        protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new StatusInfo("Debug Connector Ready"));
        }
    }
}
