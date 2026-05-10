//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using FirebaseAdmin.Messaging;
using Moq;

namespace Deveel.Messaging
{
    /// <summary>
    /// Updated tests for Firebase message building with corrected validation and test setup.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Feature", "FirebaseMessageBuildingFixed")]
    public class FirebaseMessageBuildingFixedTests
    {
        [Fact]
        public async Task Should_CallsFirebaseService_When_SendMessageAsyncWithValidMessage()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new Message
            {
                Id = "valid-test",
                Receiver = new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()),
                Content = new TextContent("Valid test message")
            };
            
            message.With("Title", "Valid Test");

            // Act
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess(), $"Expected successful send but got: {result.Error?.Code} - {result.Error?.Message}");
            
            // Verify Firebase service was called
            mockFirebaseService.Verify(x => x.SendAsync(
                It.IsAny<FirebaseAdmin.Messaging.Message>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Should_ParseCorrectly_When_SendMessageAsyncWithValidCustomData()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new Message
            {
                Id = "custom-data-test",
                Receiver = new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()),
                Content = new TextContent("Message with valid custom data")
            };
            
            // Use simple, valid JSON
            message.With("CustomData", """{"action":"test","value":123}""");

            // Act
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess(), $"Expected successful send but got: {result.Error?.Code} - {result.Error?.Message}");
            
            mockFirebaseService.Verify(x => x.SendAsync(
                It.Is<FirebaseAdmin.Messaging.Message>(m => 
                    m.Data != null &&
                    m.Data.ContainsKey("action") &&
                    m.Data["action"] == "test"
                ), 
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Should_ConfiguresCorrectly_When_SendMessageAsyncWithAndroidProperties()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new Message
            {
                Id = "android-test",
                Receiver = new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()),
                Content = new TextContent("Android notification")
            };
            
            message.With("Title", "Android Test")
                   .With("Priority", "high")
                   .With("Color", "#FF5722");

            // Act
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess(), $"Expected successful send but got: {result.Error?.Code} - {result.Error?.Message}");
            
            mockFirebaseService.Verify(x => x.SendAsync(
                It.Is<FirebaseAdmin.Messaging.Message>(m => 
                    m.Android != null &&
                    m.Android.Priority == Priority.High
                ), 
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Should_ConfiguresCorrectly_When_SendMessageAsyncWithIOSProperties()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new Message
            {
                Id = "ios-test",
                Receiver = new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()),
                Content = new TextContent("iOS notification")
            };
            
            message.With("Title", "iOS Test")
                   .With("Badge", 5)
                   .With("Sound", "notification.wav");

            // Act
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess(), $"Expected successful send but got: {result.Error?.Code} - {result.Error?.Message}");
            
            mockFirebaseService.Verify(x => x.SendAsync(
                It.Is<FirebaseAdmin.Messaging.Message>(m => 
                    m.Apns != null &&
                    m.Apns.Aps != null &&
                    m.Apns.Aps.Badge == 5
                ), 
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Should_FailValidation_When_SendMessageAsyncWithInvalidCustomData()
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
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            Assert.False(result.IsSuccess());
            Assert.Equal(ConnectorErrorCodes.MessageValidationFailed, result.Error?.Code);
            
            // Verify Firebase service was NOT called
            mockFirebaseService.Verify(x => x.SendAsync(
                It.IsAny<FirebaseAdmin.Messaging.Message>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
        }

        [Fact]
        public async Task Should_PassesCorrectFlag_When_SendMessageAsyncWithDryRun()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            var message = CreateSimpleValidMessage();

            // Act
            await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            mockFirebaseService.Verify(x => x.SendAsync(
                It.IsAny<FirebaseAdmin.Messaging.Message>(),
                true, // DryRun should be true from default settings
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        #region Helper Methods

        private async Task<FirebasePushConnector> CreateInitializedConnectorAsync(IFirebaseService firebaseService)
        {
            var schema = FirebaseChannelSchemas.FirebasePush;
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
            
            mock.Setup(x => x.SendAsync(It.IsAny<FirebaseAdmin.Messaging.Message>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((FirebaseAdmin.Messaging.Message msg, bool dryRun, CancellationToken ct) => $"msg-{Guid.NewGuid()}");
            
            return mock;
        }

        private IMessage CreateSimpleValidMessage()
        {
            var message = new Message
            {
                Id = "simple-valid-" + Guid.NewGuid().ToString("N")[..8],
                Receiver = new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()),
                Content = new TextContent("Simple valid message")
            };
            
            message.With("Title", "Simple Test");
            
            return message;
        }

        private string CreateValidDeviceToken()
        {
            return "eGc7_3RqSfGb1AthP4IjL4z:APA91bHjqkK9L3mKFYp8xNvPGwKh7P5Ty9a1B2c3D4e5F6g7H8i9J0k1L2m3N4o5P6q7R8s9T0u1V2w3X4y5Z6a7B8c9D0e1F2g3H4i5J6k7L8m9N0o1P2q3R4s5T6u7V8w9X0y1Z2";
        }

        #endregion
    }
}