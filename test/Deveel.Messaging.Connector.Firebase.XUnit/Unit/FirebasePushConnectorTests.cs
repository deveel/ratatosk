//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using FirebaseAdmin.Messaging;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

namespace Deveel.Messaging
{
    /// <summary>
    /// Tests for the <see cref="FirebasePushConnector"/> class to verify
    /// its functionality and integration with the Firebase Cloud Messaging API.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Feature", "FirebasePushConnector")]
    public class FirebasePushConnectorTests
    {
        [Fact]
        public void Should_CreateConnector_When_ConstructorWithValidSchemaAndSettings()
        {
            // Arrange
            var schema = FirebaseChannelSchemas.FirebasePush;
            var connectionSettings = FirebaseMockFactory.CreateValidConnectionSettings();

            // Act
            var connector = new FirebasePushConnector(schema, connectionSettings);

            // Assert
            Assert.Same(schema, connector.Schema);
            Assert.Equal(ConnectorState.Uninitialized, connector.State);
        }

        [Fact]
        public void Should_UseDefaultSchema_When_ConstructorWithConnectionSettingsOnly()
        {
            // Arrange
            var connectionSettings = FirebaseMockFactory.CreateValidConnectionSettings();

            // Act
            var connector = new FirebasePushConnector(connectionSettings);

            // Assert
            Assert.Equal(FirebaseConnectorConstants.Provider, connector.Schema.ChannelProvider);
            Assert.Equal(FirebaseConnectorConstants.PushChannel, connector.Schema.ChannelType);
            Assert.Equal(ConnectorState.Uninitialized, connector.State);
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_ConstructorWithNullConnectionSettings()
        {
            // Arrange
            var schema = FirebaseChannelSchemas.FirebasePush;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => new FirebasePushConnector(schema, null!));
            Assert.Throws<ArgumentNullException>(() => new FirebasePushConnector(null!));
        }

        [Fact]
        public void Should_StoreLogger_When_ConstructorWithLogger()
        {
            // Arrange
            var schema = FirebaseChannelSchemas.FirebasePush;
            var connectionSettings = FirebaseMockFactory.CreateValidConnectionSettings();

            // Act
            var connector = new FirebasePushConnector(schema, connectionSettings, null, NullLogger<FirebasePushConnector>.Instance);

            // Assert
            Assert.Same(schema, connector.Schema);
            Assert.Equal(ConnectorState.Uninitialized, connector.State);
        }

        [Fact]
        public async Task Should_ReturnSuccess_When_InitializeAsyncWithValidSettings()
        {
            // Arrange
            var schema = FirebaseChannelSchemas.SimplePush;
            var connectionSettings = FirebaseMockFactory.CreateMinimalConnectionSettings();
            var mockFirebaseService = FirebaseMockFactory.CreateMockFirebaseService();
            var connector = new FirebasePushConnector(schema, connectionSettings, mockFirebaseService.Object);

            // Act
            var result = await connector.InitializeAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.Successful, $"Expected successful initialization but got: {result.Error?.ErrorCode} - {result.Error?.ErrorMessage}");
            Assert.Equal(ConnectorState.Ready, connector.State);
            
            // Verify Firebase service was initialized
            mockFirebaseService.Verify(x => x.InitializeAsync(It.IsAny<string>(), "test-project"), Times.Once);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_InitializeAsyncWithMissingProjectId()
        {
            // Arrange
            var schema = FirebaseChannelSchemas.FirebasePush;
            var connectionSettings = new ConnectionSettings();
            connectionSettings.SetParameter("ServiceAccountKey", FirebaseMockFactory.CreateTestServiceAccountKey());
            var connector = new FirebasePushConnector(schema, connectionSettings);

            // Act
            var result = await connector.InitializeAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.False(result.Successful);
            Assert.Equal(ConnectorErrorCodes.InitializationError, result.Error?.ErrorCode);
            Assert.Contains("ProjectId is required", result.Error?.ErrorMessage);
            Assert.Equal(ConnectorState.Error, connector.State);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_InitializeAsyncWithMissingServiceAccountKey()
        {
            // Arrange
            var schema = FirebaseChannelSchemas.FirebasePush;
            var connectionSettings = new ConnectionSettings();
            connectionSettings.SetParameter("ProjectId", "test-project");
            var connector = new FirebasePushConnector(schema, connectionSettings);

            // Act
            var result = await connector.InitializeAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.False(result.Successful);
            // With the new authentication mechanism, missing ServiceAccountKey returns AUTHENTICATION_FAILED
            // which is more accurate than the generic INITIALIZATION_ERROR
            Assert.Equal("AUTHENTICATION_FAILED", result.Error?.ErrorCode);
            // The error message will be about no suitable authentication configuration found
            Assert.Contains("authentication", result.Error?.ErrorMessage?.ToLower());
            Assert.Equal(ConnectorState.Error, connector.State);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_InitializeAsyncWithFirebaseServiceFailure()
        {
            // Arrange
            var schema = FirebaseChannelSchemas.FirebasePush;
            var connectionSettings = FirebaseMockFactory.CreateValidConnectionSettings();
            var mockFirebaseService = FirebaseMockFactory.CreateFailingFirebaseService();
            var connector = new FirebasePushConnector(schema, connectionSettings, mockFirebaseService.Object);

            // Act
            var result = await connector.InitializeAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.False(result.Successful);
            Assert.Equal(ConnectorErrorCodes.InitializationError, result.Error?.ErrorCode);
            Assert.Equal(ConnectorState.Error, connector.State);
        }

        [Fact]
        public async Task Should_ReturnSuccess_When_TestConnectionAsyncWithValidConnection()
        {
            // Arrange
            var connector = await CreateInitializedConnectorAsync();

            // Act
            var result = await connector.TestConnectionAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.Successful);
            Assert.True(result.Value);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_TestConnectionAsyncWithInvalidConnection()
        {
            // Arrange
            var schema = FirebaseChannelSchemas.FirebasePush;
            var connectionSettings = FirebaseMockFactory.CreateValidConnectionSettings();
            var mockFirebaseService = new Mock<IFirebaseService>();
            mockFirebaseService.SetupGet(x => x.IsInitialized).Returns(true);
            mockFirebaseService.Setup(x => x.InitializeAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            mockFirebaseService.Setup(x => x.TestConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var connector = new FirebasePushConnector(schema, connectionSettings, mockFirebaseService.Object);
            await connector.InitializeAsync(TestContext.Current.CancellationToken);

            // Act
            var result = await connector.TestConnectionAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.False(result.Successful);
            Assert.Equal(ConnectorErrorCodes.ConnectionTestError, result.Error?.ErrorCode);
        }

        [Fact]
        public async Task Should_ReturnSuccess_When_SendMessageAsyncWithDeviceToken()
        {
            // Arrange
            var mockFirebaseService = CreateRealMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            var message = FirebaseMockFactory.CreateDeviceTokenMessage();

            // Act
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.Successful, $"Expected successful send but got: {result.Error?.ErrorCode} - {result.Error?.ErrorMessage}");
            Assert.NotNull(result.Value);
            Assert.Equal(message.Id, result.Value.MessageId);
            Assert.NotNull(result.Value.RemoteMessageId);

            // Verify Firebase service was called
            mockFirebaseService.Verify(x => x.SendAsync(
                It.Is<FirebaseAdmin.Messaging.Message>(m => 
                    !string.IsNullOrEmpty(m.Token) &&
                    m.Notification != null
                ), 
                true,
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Should_ReturnSuccess_When_SendMessageAsyncWithTopic()
        {
            // Arrange
            var mockFirebaseService = CreateRealMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            var message = FirebaseMockFactory.CreateTopicMessage();

            // Act
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.Successful, $"Expected successful send but got: {result.Error?.ErrorCode} - {result.Error?.ErrorMessage}");
            Assert.NotNull(result.Value);
            Assert.Equal(message.Id, result.Value.MessageId);

            // Verify Firebase service was called
            mockFirebaseService.Verify(x => x.SendAsync(
                It.Is<FirebaseAdmin.Messaging.Message>(m => 
                    !string.IsNullOrEmpty(m.Topic) &&
                    m.Notification != null
                ), 
                true,
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }


        [Fact]
        public async Task Should_ReturnStatusWithProjectInfo_When_GetStatusAsyncIsInvoked()
        {
            // Arrange
            var connector = await CreateInitializedConnectorAsync();

            // Act
            var result = await connector.GetStatusAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.Successful);
            Assert.Contains("ProjectId", result.Value.AdditionalData.Keys);
            Assert.Contains("IsInitialized", result.Value.AdditionalData.Keys);
        }

        [Fact]
        public async Task Should_ReturnHealthyStatus_When_GetHealthAsyncWithHealthyConnector()
        {
            // Arrange
            var connector = await CreateInitializedConnectorAsync();

            // Act
            var result = await connector.GetHealthAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.Successful);
            Assert.NotNull(result.Value);
            Assert.True(result.Value.IsHealthy);
            Assert.Equal(ConnectorState.Ready, result.Value.State);
            Assert.Empty(result.Value.Issues);
        }

        [Fact]
        public async Task Should_ReturnUnhealthyStatus_When_GetHealthAsyncWithUnhealthyConnector()
        {
            // Arrange
            var schema = FirebaseChannelSchemas.FirebasePush;
            var connectionSettings = FirebaseMockFactory.CreateValidConnectionSettings();
            var mockFirebaseService = new Mock<IFirebaseService>();
            mockFirebaseService.SetupGet(x => x.IsInitialized).Returns(false);
            
            var connector = new FirebasePushConnector(schema, connectionSettings, mockFirebaseService.Object);

            // Act
            var result = await connector.GetHealthAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.Successful);
            Assert.NotNull(result.Value);
            Assert.False(result.Value.IsHealthy);
            Assert.Contains("Firebase service is not initialized", result.Value.Issues);
        }

        [Fact]
        public void Should_ValidateDeviceIdEndpoint_When_SchemaIsInvoked()
        {
            // Arrange
            var schema = FirebaseChannelSchemas.FirebasePush;

            // Act
            var deviceEndpoint = schema.Endpoints.FirstOrDefault(e => e.Type == EndpointType.DeviceId);

            // Assert
            Assert.NotNull(deviceEndpoint);
            Assert.True(deviceEndpoint.CanReceive);
            Assert.False(deviceEndpoint.CanSend);
            Assert.True(deviceEndpoint.IsRequired);
        }

        [Fact]
        public void Should_ValidateTopicEndpoint_When_SchemaIsInvoked()
        {
            // Arrange
            var schema = FirebaseChannelSchemas.FirebasePush;

            // Act
            var topicEndpoint = schema.Endpoints.FirstOrDefault(e => e.Type == EndpointType.Topic);

            // Assert
            Assert.NotNull(topicEndpoint);
            Assert.True(topicEndpoint.CanReceive);
            Assert.False(topicEndpoint.CanSend);
            Assert.False(topicEndpoint.IsRequired);
        }

        [Fact]
        public void Should_ValidateRequiredParameters_When_SchemaIsInvoked()
        {
            // Arrange
            var schema = FirebaseChannelSchemas.FirebasePush;

            // Act
            var projectIdParam = schema.Parameters.FirstOrDefault(p => p.Name == "ProjectId");
            var serviceAccountParam = schema.Parameters.FirstOrDefault(p => p.Name == "ServiceAccountKey");

            // Assert
            Assert.NotNull(projectIdParam);
            Assert.True(projectIdParam.IsRequired);
            Assert.NotNull(serviceAccountParam);
            Assert.True(serviceAccountParam.IsRequired);
            Assert.True(serviceAccountParam.IsSensitive);
        }

        [Fact]
        public void Should_ValidateCapabilities_When_SchemaIsInvoked()
        {
            // Arrange
            var schema = FirebaseChannelSchemas.FirebasePush;

            // Act
            // Assert
            Assert.True(schema.Capabilities.HasFlag(ChannelCapability.SendMessages));
            Assert.True(schema.Capabilities.HasFlag(ChannelCapability.BulkMessaging));
            Assert.True(schema.Capabilities.HasFlag(ChannelCapability.HealthCheck));
            Assert.False(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
            Assert.False(schema.Capabilities.HasFlag(ChannelCapability.MessageStatusQuery));
        }

        [Fact]
        public void Should_RemovesAdvancedFeatures_When_SimplePushSchemaIsInvoked()
        {
            // Arrange
            var schema = FirebaseChannelSchemas.SimplePush;

            // Act
            // Assert
            Assert.False(schema.Capabilities.HasFlag(ChannelCapability.BulkMessaging));
            Assert.Null(schema.Parameters.FirstOrDefault(p => p.Name == "DryRun"));
            Assert.Null(schema.MessageProperties.FirstOrDefault(p => p.Name == "ImageUrl"));
            Assert.Null(schema.MessageProperties.FirstOrDefault(p => p.Name == "CustomData"));
        }

        [Fact]
        public void Should_IncludeBulkFeatures_When_BulkPushSchemaIsInvoked()
        {
            // Arrange
            var schema = FirebaseChannelSchemas.BulkPush;

            // Act
            // Assert
            Assert.True(schema.Capabilities.HasFlag(ChannelCapability.BulkMessaging));
            Assert.NotNull(schema.MessageProperties.FirstOrDefault(p => p.Name == "ConditionExpression"));
            Assert.NotNull(schema.MessageProperties.FirstOrDefault(p => p.Name == "BatchId"));
        }

        [Fact]
        public void Should_IncludeRichFeatures_When_RichPushSchemaIsInvoked()
        {
            // Arrange
            var schema = FirebaseChannelSchemas.RichPush;

            // Act
            // Assert
            Assert.False(schema.Capabilities.HasFlag(ChannelCapability.BulkMessaging));
            Assert.NotNull(schema.MessageProperties.FirstOrDefault(p => p.Name == "Actions"));
            Assert.NotNull(schema.MessageProperties.FirstOrDefault(p => p.Name == "Category"));
            Assert.NotNull(schema.MessageProperties.FirstOrDefault(p => p.Name == "Subtitle"));
        }

        /// <summary>
        /// Helper method to create an initialized connector for testing.
        /// </summary>
        private async Task<FirebasePushConnector> CreateInitializedConnectorAsync()
        {
            var schema = FirebaseChannelSchemas.FirebasePush;
            var connectionSettings = FirebaseMockFactory.CreateValidConnectionSettings();
            var mockFirebaseService = FirebaseMockFactory.CreateMockFirebaseService();
            
            var connector = new FirebasePushConnector(schema, connectionSettings, mockFirebaseService.Object);
            var result = await connector.InitializeAsync(TestContext.Current.CancellationToken);
            
            Assert.True(result.Successful, $"Failed to initialize connector: {result.Error?.ErrorMessage}");
            return connector;
        }

        /// <summary>
        /// Helper method to create an initialized connector with a specific Firebase service.
        /// </summary>
        private async Task<FirebasePushConnector> CreateInitializedConnectorAsync(IFirebaseService firebaseService)
        {
            var schema = FirebaseChannelSchemas.FirebasePush;
            var connectionSettings = FirebaseMockFactory.CreateValidConnectionSettings();
            
            var connector = new FirebasePushConnector(schema, connectionSettings, firebaseService);
            var result = await connector.InitializeAsync(TestContext.Current.CancellationToken);
            
            Assert.True(result.Successful, $"Failed to initialize connector: {result.Error?.ErrorMessage}");
            return connector;
        }

        /// <summary>
        /// Helper method to create an initialized bulk connector.
        /// </summary>
        private async Task<FirebasePushConnector> CreateInitializedBulkConnectorAsync(IFirebaseService firebaseService)
        {
            var schema = FirebaseChannelSchemas.BulkPush;
            var connectionSettings = FirebaseMockFactory.CreateValidConnectionSettings();
            
            var connector = new FirebasePushConnector(schema, connectionSettings, firebaseService);
            var result = await connector.InitializeAsync(TestContext.Current.CancellationToken);
            
            Assert.True(result.Successful, $"Failed to initialize bulk connector: {result.Error?.ErrorMessage}");
            return connector;
        }

        /// <summary>
        /// Creates a real mock Firebase service that actually works for testing.
        /// </summary>
        private Mock<IFirebaseService> CreateRealMockFirebaseService()
        {
            var mock = new Mock<IFirebaseService>();
            
            mock.SetupGet(x => x.IsInitialized).Returns(true);
            mock.Setup(x => x.InitializeAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            mock.Setup(x => x.TestConnectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
            
            // Setup SendAsync
            mock.Setup(x => x.SendAsync(It.IsAny<FirebaseAdmin.Messaging.Message>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((FirebaseAdmin.Messaging.Message msg, bool dryRun, CancellationToken ct) => $"firebase-msg-{Guid.NewGuid()}");
            
            // Setup SendEachAsync
            mock.Setup(x => x.SendEachAsync(It.IsAny<IEnumerable<FirebaseAdmin.Messaging.Message>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<FirebaseAdmin.Messaging.Message> messages, bool dryRun, CancellationToken ct) =>
                {
                    var responses = messages.Select(m => CreateMockSendResponse($"firebase-{Guid.NewGuid()}", true)).ToList();
                    return CreateMockBatchResponse(responses);
                });
            
            // Setup SendMulticastAsync
            mock.Setup(x => x.SendMulticastAsync(It.IsAny<MulticastMessage>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((MulticastMessage msg, bool dryRun, CancellationToken ct) =>
                {
                    var responses = msg.Tokens.Select(token => CreateMockSendResponse($"firebase-{Guid.NewGuid()}", true)).ToList();
                    return CreateMockBatchResponse(responses);
                });
            
            return mock;
        }

        /// <summary>
        /// Creates a mock SendResponse for testing.
        /// </summary>
        private SendResponse CreateMockSendResponse(string messageId, bool isSuccess)
        {
            var mock = new Mock<SendResponse>();
            mock.SetupGet(x => x.MessageId).Returns(messageId);
            mock.SetupGet(x => x.IsSuccess).Returns(isSuccess);
            return mock.Object;
        }

        /// <summary>
        /// Creates a mock BatchResponse for testing.
        /// </summary>
        private BatchResponse CreateMockBatchResponse(IList<SendResponse> responses)
        {
            var mock = new Mock<BatchResponse>();
            mock.SetupGet(x => x.Responses).Returns((IReadOnlyList<SendResponse>)responses.ToList().AsReadOnly());
            mock.SetupGet(x => x.SuccessCount).Returns(responses.Count(r => r.IsSuccess));
            mock.SetupGet(x => x.FailureCount).Returns(responses.Count(r => !r.IsSuccess));
            return mock.Object;
        }
    }
}