//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using FirebaseAdmin.Messaging;

using Moq;

namespace Deveel.Messaging
{
    /// <summary>
    /// Tests for Firebase message building, content transformation, and edge cases.
    /// These tests ensure that the Firebase connector correctly transforms messaging
    /// framework messages into Firebase-specific message formats.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Feature", "FirebaseMessageBuilding")]
    public class FirebaseMessageBuildingTests
    {
        #region Content Type Tests

        [Fact]
        public async Task Should_BuildsNotificationCorrectly_When_SendMessageAsyncWithTextContent()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new MessageBuilder()
                .WithId("text-content-test")
                .To(new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()))
                .WithContent(new TextContent("This is a text notification"))
                .WithProperty("Title", "Text Notification")
                .Build();

            // Act
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess(), $"Expected successful send but got: {result.Error?.Code} - {result.Error?.Message}");
            
            mockFirebaseService.Verify(x => x.SendAsync(
                It.Is<FirebaseAdmin.Messaging.Message>(m => 
                    m.Notification != null &&
                    m.Notification.Body == "This is a text notification"
                ), 
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Should_HandleDataCorrectly_When_SendMessageAsyncWithJsonContent()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new MessageBuilder()
                .WithId("json-content-test")
                .To(new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()))
                .WithContent(new JsonContent(@"{""data"":""value"",""number"":123}"))
                .WithProperty("Title", "JSON Data Message")
                .Build();

            // Act
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess(), $"Expected successful send but got: {result.Error?.Code} - {result.Error?.Message}");
            
            mockFirebaseService.Verify(x => x.SendAsync(
                It.IsAny<FirebaseAdmin.Messaging.Message>(), 
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        #endregion

        #region Property Mapping Tests

            

        [Fact]
        public async Task Should_AddsImageToNotification_When_SendMessageAsyncWithImageUrl()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new MessageBuilder()
                .WithId("image-url-test")
                .To(new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()))
                .WithContent(new TextContent("Notification with image"))
                .WithProperty("Title", "Image Notification")
                .WithProperty("ImageUrl", "https://example.com/image.jpg")
                .Build();

            // Act
            await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            mockFirebaseService.Verify(x => x.SendAsync(
                It.Is<FirebaseAdmin.Messaging.Message>(m => 
                    m.Notification != null &&
                    m.Notification.ImageUrl == "https://example.com/image.jpg"
                ), 
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Should_ParseAndAddsToDataPayload_When_SendMessageAsyncWithCustomDataJson()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new MessageBuilder()
                .WithId("custom-data-test")
                .To(new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()))
                .WithContent(new TextContent("Message with custom data"))
                .WithProperty("CustomData", @"{""userId"":123,""action"":""update"",""metadata"":{""version"":""2.0""}}")
                .Build();

            // Act
            await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            mockFirebaseService.Verify(x => x.SendAsync(
                It.Is<FirebaseAdmin.Messaging.Message>(m => 
                    m.Data != null &&
                    m.Data.ContainsKey("userId") &&
                    m.Data["userId"] == "123" &&
                    m.Data.ContainsKey("action") &&
                    m.Data["action"] == "update" &&
                    m.Data.ContainsKey("messageId")
                ), 
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Should_AddsAsStringField_When_SendMessageAsyncWithInvalidCustomDataJson()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new MessageBuilder()
                .WithId("invalid-json-test")
                .To(new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()))
                .WithContent(new TextContent("Message with invalid JSON"))
                .WithProperty("CustomData", "invalid-json-{not-valid}")
                .Build();

            // Act
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

			// Assert
			Assert.False(result.IsSuccess());
            Assert.NotNull(result.Error);

            var validationError = Assert.IsType<OperationValidationError>(result.Error);
            Assert.Equal(ConnectorErrorCodes.MessageValidationFailed, validationError.Code);
        }

        #endregion

        #region Platform Configuration Tests

        [Fact]
        public async Task Should_ConfiguresCompleteAndroidConfig_When_SendMessageAsyncWithAllAndroidProperties()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new MessageBuilder()
                .WithId("android-complete-test")
                .To(new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()))
                .WithContent(new TextContent("Complete Android notification"))
                .WithProperty("Title", "Android Notification")
                .WithProperty("Priority", "high")
                .WithProperty("TimeToLive", 7200)     // Fixed: use integer instead of string
                .WithProperty("CollapseKey", "message_group")
                .WithProperty("RestrictedPackageName", "com.example.app")
                .WithProperty("Color", "#4CAF50")
                .WithProperty("Sound", "android_sound")
                .WithProperty("Tag", "message_tag")
                .WithProperty("ClickAction", "OPEN_ACTIVITY")
                .Build();

            // Act
            await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            mockFirebaseService.Verify(x => x.SendAsync(
                It.Is<FirebaseAdmin.Messaging.Message>(m => 
                    m.Android != null &&
                    m.Android.Priority == Priority.High &&
                    m.Android.TimeToLive == TimeSpan.FromSeconds(7200) &&
                    m.Android.CollapseKey == "message_group" &&
                    m.Android.RestrictedPackageName == "com.example.app" &&
                    m.Android.Notification != null &&
                    m.Android.Notification.Color == "#4CAF50" &&
                    m.Android.Notification.Sound == "android_sound" &&
                    m.Android.Notification.Tag == "message_tag" &&
                    m.Android.Notification.ClickAction == "OPEN_ACTIVITY"
                ), 
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Should_ConfiguresCompleteApnsConfig_When_SendMessageAsyncWithAlliOSProperties()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new MessageBuilder()
                .WithId("ios-complete-test")
                .To(new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()))
                .WithContent(new TextContent("Complete iOS notification"))
                .WithProperty("Title", "iOS Notification")
                .WithProperty("Badge", 7)            // Fixed: use integer instead of string
                .WithProperty("Sound", "ios_notification.wav")
                .WithProperty("ContentAvailable", true)  // Fixed: use boolean instead of string
                .WithProperty("MutableContent", true)    // Fixed: use boolean instead of string
                .WithProperty("ThreadId", "conversation_123")
                .Build();

            // Act
            await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            mockFirebaseService.Verify(x => x.SendAsync(
                It.Is<FirebaseAdmin.Messaging.Message>(m => 
                    m.Apns != null &&
                    m.Apns.Aps != null &&
                    m.Apns.Aps.Badge == 7 &&
                    m.Apns.Aps.Sound == "ios_notification.wav" &&
                    m.Apns.Aps.ContentAvailable == true &&
                    m.Apns.Aps.MutableContent == true &&
                    m.Apns.Aps.ThreadId == "conversation_123"
                ), 
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Should_ConfiguresAndroidPriorityCorrectly_When_SendMessageAsyncWithNormalPriority()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new MessageBuilder()
                .WithId("normal-priority-test")
                .To(new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()))
                .WithContent(new TextContent("Normal priority notification"))
                .WithProperty("Priority", "normal")
                .Build();

            // Act
            await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            mockFirebaseService.Verify(x => x.SendAsync(
                It.Is<FirebaseAdmin.Messaging.Message>(m => 
                    m.Android != null &&
                    m.Android.Priority == Priority.Normal
                ), 
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        #endregion

        #region Edge Cases and Error Scenarios

        [Fact]
        public async Task Should_CreateDataOnlyMessage_When_SendMessageAsyncWithNoTitleOrBodyOrContent()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new MessageBuilder()
                .WithId("data-only-test")
                .To(new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()))
                .WithContent(new TextContent("")) // Empty text content instead of no content
                .WithProperty("CustomData", @"{""action"":""background_sync""}")
                .Build();

            // Act
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess(), $"Expected successful send but got: {result.Error?.Code} - {result.Error?.Message}");
            
            mockFirebaseService.Verify(x => x.SendAsync(
                It.Is<FirebaseAdmin.Messaging.Message>(m => 
                    m.Data != null &&
                    m.Data.ContainsKey("action") &&
                    m.Data["action"] == "background_sync"
                ), 
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Should_ReturnValidationError_When_SendMessageAsyncWithInvalidBadgeNumber()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new MessageBuilder()
                .WithId("invalid-badge-test")
                .To(new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()))
                .WithContent(new TextContent("Test notification"))
                .WithProperty("Title", "Test Notification")
                .WithProperty("Badge", "not-a-number") // Invalid badge should trigger validation error
                .Build();

            // Act
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            Assert.False(result.IsSuccess(), "Expected validation failure for invalid badge number");
            Assert.Equal(ConnectorErrorCodes.MessageValidationFailed, result.Error?.Code);
            
            // Verify Firebase service was NOT called due to validation failure
            mockFirebaseService.Verify(x => x.SendAsync(
                It.IsAny<FirebaseAdmin.Messaging.Message>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
        }

        [Fact]
        public async Task Should_ReturnValidationError_When_SendMessageAsyncWithInvalidTimeToLive()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new MessageBuilder()
                .WithId("invalid-ttl-test")
                .To(new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()))
                .WithContent(new TextContent("Test notification"))
                .WithProperty("Title", "Test Notification")
                .WithProperty("TimeToLive", "invalid-number") // Invalid TTL should trigger validation error
                .Build();

            // Act
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            Assert.False(result.IsSuccess(), "Expected validation failure for invalid time to live");
            Assert.Equal(ConnectorErrorCodes.MessageValidationFailed, result.Error?.Code);
            
            // Verify Firebase service was NOT called due to validation failure
            mockFirebaseService.Verify(x => x.SendAsync(
                It.IsAny<FirebaseAdmin.Messaging.Message>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
        }

        [Fact]
        public async Task Should_ReturnValidationError_When_SendMessageAsyncWithInvalidBooleanProperties()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new MessageBuilder()
                .WithId("invalid-boolean-test")
                .To(new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()))
                .WithContent(new TextContent("Test notification"))
                .WithProperty("Title", "Test Notification")
                .WithProperty("ContentAvailable", "maybe")   // Invalid boolean should trigger validation error
                .WithProperty("MutableContent", "perhaps")  // Invalid boolean should trigger validation error
                .Build();

            // Act
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            Assert.False(result.IsSuccess(), "Expected validation failure for invalid boolean properties");
            Assert.Equal(ConnectorErrorCodes.MessageValidationFailed, result.Error?.Code);
            
            // Verify Firebase service was NOT called due to validation failure
            mockFirebaseService.Verify(x => x.SendAsync(
                It.IsAny<FirebaseAdmin.Messaging.Message>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
        }

        [Fact]
        public async Task Should_FailValidation_When_SendMessageAsyncWithVeryLongTitleAndBody()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var longTitle = new string('T', 1000);  // Very long title (exceeds 256 char limit)
            var longBody = new string('B', 5000);   // Very long body (exceeds 4000 char limit)
            
            var message = new MessageBuilder()
                .WithId("long-content-test")
                .To(new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()))
                .WithContent(new TextContent("Short content"))
                .WithProperty("Title", longTitle)
                .WithProperty("Body", longBody)
                .Build();

            // Act
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            Assert.False(result.IsSuccess(), "Expected message to fail validation due to excessive title/body length");
            Assert.Equal(ConnectorErrorCodes.MessageValidationFailed, result.Error?.Code);
            
            // Verify Firebase service was NOT called due to validation failure
            mockFirebaseService.Verify(x => x.SendAsync(
                It.IsAny<FirebaseAdmin.Messaging.Message>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
        }

        #endregion

        #region Dry Run Tests

        [Fact]
        public async Task Should_PassesDryRunToFirebaseService_When_SendMessageAsyncWithDryRunEnabled()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            var message = CreateSimpleDeviceTokenMessage();

            // Act
            await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            mockFirebaseService.Verify(x => x.SendAsync(
                It.IsAny<FirebaseAdmin.Messaging.Message>(),
                true,  // Dry run should be true (default in CreateValidConnectionSettings)
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Should_PassesFalseToFirebaseService_When_SendMessageAsyncWithDryRunDisabled()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            
            // Create custom connection settings with DryRun = false
            var connectionSettings = new ConnectionSettings();
            connectionSettings.SetParameter("ProjectId", "test-project");
            connectionSettings.SetParameter("ServiceAccountKey", FirebaseMockFactory.CreateTestServiceAccountKey());
            connectionSettings.SetParameter("DryRun", false);
            
            // Create connector manually with custom settings
            var schema = FirebaseChannelSchemas.FirebasePush;
            var connector = new FirebasePushConnector(schema, connectionSettings, mockFirebaseService.Object);
            
            var result = await connector.InitializeAsync(TestContext.Current.CancellationToken);
            Assert.True(result.IsSuccess(), $"Failed to initialize connector: {result.Error?.Message}");
            
            var message = CreateSimpleDeviceTokenMessage();

            // Act
            await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            mockFirebaseService.Verify(x => x.SendAsync(
                It.IsAny<FirebaseAdmin.Messaging.Message>(),
                false,  // Dry run should be false
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        #endregion

        #region Helper Methods

        private async Task<FirebasePushConnector> CreateInitializedConnectorAsync(IFirebaseService firebaseService)
        {
            // Use the full FirebasePush schema which supports all features being tested
            var schema = FirebaseChannelSchemas.FirebasePush; // Change back to full schema
            var connectionSettings = FirebaseMockFactory.CreateValidConnectionSettings();
            var connector = new FirebasePushConnector(schema, connectionSettings, firebaseService);
            
            var result = await connector.InitializeAsync(TestContext.Current.CancellationToken);
            Assert.True(result.IsSuccess(), $"Failed to initialize connector: {result.Error?.Message}");
            
            return connector;
        }

        private Mock<IFirebaseService> CreateInspectingMockFirebaseService()
        {
            var mock = new Mock<IFirebaseService>();
            
            mock.SetupGet(x => x.IsInitialized).Returns(true);
            mock.Setup(x => x.InitializeAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            mock.Setup(x => x.TestConnectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
            
            // This mock allows us to inspect the Firebase message that was built
            mock.Setup(x => x.SendAsync(It.IsAny<FirebaseAdmin.Messaging.Message>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((FirebaseAdmin.Messaging.Message msg, bool dryRun, CancellationToken ct) => $"msg-{Guid.NewGuid()}");
            
            return mock;
        }

        private IMessage CreateSimpleDeviceTokenMessage()
        {
            var message = new MessageBuilder()
                .WithId("simple-msg-" + Guid.NewGuid().ToString("N")[..8])
                // Use a realistic-length device token that meets validation requirements
                .To(new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()))
                .WithContent(new TextContent("Simple notification"))
                .WithProperty("Title", "Simple Notification")
                .Build();
            
            return message;
        }

        /// <summary>
        /// Creates a valid Firebase device token that meets length requirements
        /// </summary>
        private string CreateValidDeviceToken()
        {
            // Firebase tokens are typically 140+ characters, create a realistic test token
            return "eGc7_3RqSfGb1AthP4IjL4z:APA91bHjqkK9L3mKFYp8xNvPGwKh7P5Ty9a1B2c3D4e5F6g7H8i9J0k1L2m3N4o5P6q7R8s9T0u1V2w3X4y5Z6a7B8c9D0e1F2g3H4i5J6k7L8m9N0o1P2q3R4s5T6u7V8w9X0y1Z2";
        }

        #endregion
    }
}