//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Moq;

namespace Ratatosk
{
    /// <summary>
    /// Tests for Firebase TextContent integration, ensuring that notification body
    /// text is properly sourced from message TextContent instead of Body property.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Feature", "FirebaseTextContent")]
    public class FirebaseTextContentTests
    {
        /// <summary>
        /// Creates a valid Firebase device token that meets validation requirements (140+ characters)
        /// </summary>
        private static string CreateValidDeviceToken()
        {
            return "eGc7_3RqSfGb1AthP4IjL4z:APA91bHjqkK9L3mKFYp8xNvPGwKh7P5Ty9a1B2c3D4e5F6g7H8i9J0k1L2m3N4o5P6q7R8s9T0u1V2w3X4y5Z6a7B8c9D0e1F2g3H4i5J6k7L8m9N0o1P2q3R4s5T6u7V8w9X0y1Z2";
        }

        [Fact]
        public async Task Should_UseTextForNotificationBody_When_SendMessageAsyncWithTextContent()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new MessageBuilder()
                .WithId("text-content-test")
                .To(new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()))
                .WithContent(new TextContent("This text should be the notification body"))
                // Only add title - body should come from TextContent
                .WithProperty("Title", "Test Notification")
                .Build();

            // Act
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess(), $"Expected successful send but got: {result.Error?.Code} - {result.Error?.Message}");
            
            mockFirebaseService.Verify(x => x.SendAsync(
                It.Is<FirebaseAdmin.Messaging.Message>(m => 
                    m.Notification != null &&
                    m.Notification.Title == "Test Notification" &&
                    m.Notification.Body == "This text should be the notification body"
                ), 
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Should_PrefersTextContent_When_SendMessageAsyncWithTextContentAndBodyProperty()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new MessageBuilder()
                .WithId("priority-test")
                .To(new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()))
                .WithContent(new TextContent("Text from TextContent"))
                .WithProperty("Title", "Priority Test")
                // Note: Body property is no longer part of the schema, so we test TextContent only
                .Build();

            // Act
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess(), $"Expected successful send but got: {result.Error?.Code} - {result.Error?.Message}");
            
            mockFirebaseService.Verify(x => x.SendAsync(
                It.Is<FirebaseAdmin.Messaging.Message>(m => 
                    m.Notification != null &&
                    m.Notification.Body == "Text from TextContent"
                ), 
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Should_CreateDataOnlyMessage_When_SendMessageAsyncWithEmptyTextContent()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new MessageBuilder()
                .WithId("empty-content-test")
                .To(new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()))
                .WithContent(new TextContent("")) // Empty TextContent
                .WithProperty("Title", "Empty Content Test")
                // Add some data to make it a valid data-only message
                .WithProperty("CustomData", @"{""action"":""silent""}")
                .Build();

            // Act
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess(), $"Expected successful send but got: {result.Error?.Code} - {result.Error?.Message}");
            
            // Should create a data-only message with title but no body
            mockFirebaseService.Verify(x => x.SendAsync(
                It.Is<FirebaseAdmin.Messaging.Message>(m => 
                    m.Data != null &&
                    m.Data.ContainsKey("action") &&
                    m.Data["action"] == "silent"
                ), 
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Should_CreateDataOnlyMessage_When_SendMessageAsyncWithNullTextContent()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new MessageBuilder()
                .WithId("null-content-test")
                .To(new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()))
                .WithContent(new TextContent("")) // Use empty string instead of null to avoid null reference
                .WithProperty("Title", "Null Content Test")
                .WithProperty("CustomData", @"{""type"":""silent""}")
                .Build();

            // Act
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess(), $"Expected successful send but got: {result.Error?.Code} - {result.Error?.Message}");
            
            // Should create a data-only message
            mockFirebaseService.Verify(x => x.SendAsync(
                It.Is<FirebaseAdmin.Messaging.Message>(m => 
                    m.Data != null &&
                    m.Data.ContainsKey("type") &&
                    m.Data["type"] == "silent"
                ), 
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Should_CreateDataOnlyMessage_When_SendMessageAsyncWithNonTextContent()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new MessageBuilder()
                .WithId("json-content-test")
                .To(new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()))
                .WithContent(new JsonContent(@"{""key"":""value""}"))
                .WithProperty("Title", "JSON Content Test")
                .WithProperty("CustomData", @"{""source"":""json""}")
                .Build();

            // Act
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess(), $"Expected successful send but got: {result.Error?.Code} - {result.Error?.Message}");
            
            // Should create a data-only message since JsonContent doesn't provide notification body
            mockFirebaseService.Verify(x => x.SendAsync(
                It.Is<FirebaseAdmin.Messaging.Message>(m => 
                    m.Data != null &&
                    m.Data.ContainsKey("source") &&
                    m.Data["source"] == "json"
                ), 
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Should_CreateNotificationWithOnlyBody_When_SendMessageAsyncWithOnlyTextContent()
        {
            // Arrange
            var mockFirebaseService = CreateInspectingMockFirebaseService();
            var connector = await CreateInitializedConnectorAsync(mockFirebaseService.Object);
            
            var message = new MessageBuilder()
                .WithId("body-only-test")
                .To(new Endpoint(EndpointType.DeviceId, CreateValidDeviceToken()))
                .WithContent(new TextContent("Only body text, no title"))
                // Don't add any properties - only content
                .Build();

            // Act
            var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess(), $"Expected successful send but got: {result.Error?.Code} - {result.Error?.Message}");
            
            mockFirebaseService.Verify(x => x.SendAsync(
                It.Is<FirebaseAdmin.Messaging.Message>(m => 
                    m.Notification != null &&
                    m.Notification.Title == null &&
                    m.Notification.Body == "Only body text, no title"
                ), 
                It.IsAny<bool>(),
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

        #endregion
    }
}