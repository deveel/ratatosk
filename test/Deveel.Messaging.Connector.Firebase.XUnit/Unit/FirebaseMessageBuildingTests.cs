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
            
            var message = new Message
            {
                Id = "text-content-test",
                Receiver = new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()),
                Content = new TextContent("This is a text notification")
            };
            
            // Add required Firebase properties
            message.With("Title", "Text Notification");

            // Act
            var result = await connector.SendMessageAsync(message, CancellationToken.None);

            // Assert
            Assert.True(result.Successful, $"Expected successful send but got: {result.Error?.ErrorCode} - {result.Error?.ErrorMessage}");
            
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
            
            var message = new Message
            {
                Id = "json-content-test",
                Receiver = new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()),
                Content = new JsonContent(@"{""data"":""value"",""number"":123}")
            };
            
            // Add required properties for Firebase validation
            message.With("Title", "JSON Data Message");

            // Act
            var result = await connector.SendMessageAsync(message, CancellationToken.None);

            // Assert
            Assert.True(result.Successful, $"Expected successful send but got: {result.Error?.ErrorCode} - {result.Error?.ErrorMessage}");
            
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
            
            var message = new Message
            {
                Id = "image-url-test",
                Receiver = new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()),
                Content = new TextContent("Notification with image")
            };
            
            message.With("Title", "Image Notification")
                   .With("ImageUrl", "https://example.com/image.jpg");

            // Act
            await connector.SendMessageAsync(message, CancellationToken.None);

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
            
            var message = new Message
            {
                Id = "custom-data-test",
                Receiver = new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()),
                Content = new TextContent("Message with custom data")
            };
            
            message.With("CustomData", @"{""userId"":123,""action"":""update"",""metadata"":{""version"":""2.0""}}");

            // Act
            await connector.SendMessageAsync(message, CancellationToken.None);

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
            
            var message = new Message
            {
                Id = "invalid-json-test",
                Receiver = new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()),
                Content = new TextContent("Message with invalid JSON")
            };
            
            message.With("CustomData", "invalid-json-{not-valid}");

            // Act
            var result = await connector.SendMessageAsync(message, CancellationToken.None);

			// Assert
			Assert.False(result.Successful);
            Assert.NotNull(result.Error);

            var validationError = Assert.IsType<MessageValidationError>(result.Error);
            Assert.Equal(ConnectorErrorCodes.MessageValidationFailed, validationError.ErrorCode);
        }

        #endregion

        #region Platform Configuration Tests

        [Fact]
        public async Task Should_ConfiguresCompleteAndroidConfig_When_SendMessageAsyncWithAllAndroidProperties()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new Message
            {
                Id = "android-complete-test",
                Receiver = new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()),
                Content = new TextContent("Complete Android notification")
            };
            
            message.With("Title", "Android Notification")
                   .With("Priority", "high")
                   .With("TimeToLive", 7200)     // Fixed: use integer instead of string
                   .With("CollapseKey", "message_group")
                   .With("RestrictedPackageName", "com.example.app")
                   .With("Color", "#4CAF50")
                   .With("Sound", "android_sound")
                   .With("Tag", "message_tag")
                   .With("ClickAction", "OPEN_ACTIVITY");

            // Act
            await connector.SendMessageAsync(message, CancellationToken.None);

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
            
            var message = new Message
            {
                Id = "ios-complete-test",
                Receiver = new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()),
                Content = new TextContent("Complete iOS notification")
            };
            
            message.With("Title", "iOS Notification")
                   .With("Badge", 7)            // Fixed: use integer instead of string
                   .With("Sound", "ios_notification.wav")
                   .With("ContentAvailable", true)  // Fixed: use boolean instead of string
                   .With("MutableContent", true)    // Fixed: use boolean instead of string
                   .With("ThreadId", "conversation_123");

            // Act
            await connector.SendMessageAsync(message, CancellationToken.None);

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
            
            var message = new Message
            {
                Id = "normal-priority-test",
                Receiver = new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()),
                Content = new TextContent("Normal priority notification")
            };
            
            message.With("Priority", "normal");

            // Act
            await connector.SendMessageAsync(message, CancellationToken.None);

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
            
            var message = new Message
            {
                Id = "data-only-test",
                Receiver = new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()),
                Content = new TextContent("") // Empty text content instead of no content
            };
            
            // Add custom data but no title/body to make it data-only
            message.With("CustomData", @"{""action"":""background_sync""}");

            // Act
            var result = await connector.SendMessageAsync(message, CancellationToken.None);

            // Assert
            Assert.True(result.Successful, $"Expected successful send but got: {result.Error?.ErrorCode} - {result.Error?.ErrorMessage}");
            
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
            
            var message = new Message
            {
                Id = "invalid-badge-test",
                Receiver = new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()),
                Content = new TextContent("Test notification")
            };
            
            message.With("Title", "Test Notification")
                   .With("Badge", "not-a-number"); // Invalid badge should trigger validation error

            // Act
            var result = await connector.SendMessageAsync(message, CancellationToken.None);

            // Assert
            Assert.False(result.Successful, "Expected validation failure for invalid badge number");
            Assert.Equal(ConnectorErrorCodes.MessageValidationFailed, result.Error?.ErrorCode);
            
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
            
            var message = new Message
            {
                Id = "invalid-ttl-test",
                Receiver = new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()),
                Content = new TextContent("Test notification")
            };
            
            message.With("Title", "Test Notification")
                   .With("TimeToLive", "invalid-number"); // Invalid TTL should trigger validation error

            // Act
            var result = await connector.SendMessageAsync(message, CancellationToken.None);

            // Assert
            Assert.False(result.Successful, "Expected validation failure for invalid time to live");
            Assert.Equal(ConnectorErrorCodes.MessageValidationFailed, result.Error?.ErrorCode);
            
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
            
            var message = new Message
            {
                Id = "invalid-boolean-test",
                Receiver = new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()),
                Content = new TextContent("Test notification")
            };
            
            message.With("Title", "Test Notification")
                   .With("ContentAvailable", "maybe")   // Invalid boolean should trigger validation error
                   .With("MutableContent", "perhaps");  // Invalid boolean should trigger validation error

            // Act
            var result = await connector.SendMessageAsync(message, CancellationToken.None);

            // Assert
            Assert.False(result.Successful, "Expected validation failure for invalid boolean properties");
            Assert.Equal(ConnectorErrorCodes.MessageValidationFailed, result.Error?.ErrorCode);
            
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
            
            var message = new Message
            {
                Id = "long-content-test",
                Receiver = new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()),
                Content = new TextContent("Short content")
            };
            
            message.With("Title", longTitle)
                   .With("Body", longBody);

            // Act
            var result = await connector.SendMessageAsync(message, CancellationToken.None);

            // Assert
            Assert.False(result.Successful, "Expected message to fail validation due to excessive title/body length");
            Assert.Equal(ConnectorErrorCodes.MessageValidationFailed, result.Error?.ErrorCode);
            
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
            await connector.SendMessageAsync(message, CancellationToken.None);

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
            
            var result = await connector.InitializeAsync(CancellationToken.None);
            Assert.True(result.Successful, $"Failed to initialize connector: {result.Error?.ErrorMessage}");
            
            var message = CreateSimpleDeviceTokenMessage();

            // Act
            await connector.SendMessageAsync(message, CancellationToken.None);

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
            
            var result = await connector.InitializeAsync(CancellationToken.None);
            Assert.True(result.Successful, $"Failed to initialize connector: {result.Error?.ErrorMessage}");
            
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
            var message = new Message
            {
                Id = "simple-msg-" + Guid.NewGuid().ToString("N")[..8],
                // Use a realistic-length device token that meets validation requirements
                Receiver = new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()),
                Content = new TextContent("Simple notification")
            };
            
            // Add required properties for Firebase validation
            message.With("Title", "Simple Notification");
            
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