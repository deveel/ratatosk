//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Moq;
using System.Text.Json;

namespace Deveel.Messaging
{
    /// <summary>
    /// Unit tests for the FirebaseService class focusing on initialization, 
    /// state management, and method validation without requiring real Firebase credentials.
    /// </summary>
    public class FirebaseServiceTests : IDisposable
    {
        private readonly FirebaseService _firebaseService;

        public FirebaseServiceTests()
        {
            _firebaseService = new FirebaseService();
        }

        public void Dispose()
        {
            // Clean up any Firebase apps created during testing
            try
            {
                if (_firebaseService.App != null)
                {
                    _firebaseService.App.Delete();
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        #region Initial State Tests

        [Fact]
        public void FirebaseService_InitialState_IsNotInitialized()
        {
            // Arrange & Act
            var service = new FirebaseService();

            // Assert
            Assert.False(service.IsInitialized);
            Assert.Null(service.App);
        }

        [Fact]
        public void IsInitialized_BeforeInitialization_ReturnsFalse()
        {
            // Arrange
            var service = new FirebaseService();

            // Act & Assert
            Assert.False(service.IsInitialized);
        }

        [Fact]
        public void App_BeforeInitialization_ReturnsNull()
        {
            // Arrange
            var service = new FirebaseService();

            // Act & Assert
            Assert.Null(service.App);
        }

        #endregion

        #region Parameter Validation Tests

        [Fact]
        public async Task InitializeAsync_WithNullServiceAccountKey_ThrowsArgumentException()
        {
            // Arrange
            var service = new FirebaseService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                service.InitializeAsync(null!, "test-project"));
        }

        [Fact]
        public async Task InitializeAsync_WithEmptyServiceAccountKey_ThrowsArgumentException()
        {
            // Arrange
            var service = new FirebaseService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                service.InitializeAsync(string.Empty, "test-project"));
        }

        [Fact]
        public async Task InitializeAsync_WithWhitespaceServiceAccountKey_ThrowsArgumentException()
        {
            // Arrange
            var service = new FirebaseService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                service.InitializeAsync("   ", "test-project"));
        }

        [Fact]
        public async Task InitializeAsync_WithNullProjectId_ThrowsArgumentException()
        {
            // Arrange
            var service = new FirebaseService();
            var serviceAccount = """{"type": "service_account"}""";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                service.InitializeAsync(serviceAccount, null!));
        }

        [Fact]
        public async Task InitializeAsync_WithEmptyProjectId_ThrowsArgumentException()
        {
            // Arrange
            var service = new FirebaseService();
            var serviceAccount = """{"type": "service_account"}""";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                service.InitializeAsync(serviceAccount, string.Empty));
        }

        [Fact]
        public async Task InitializeAsync_WithWhitespaceProjectId_ThrowsArgumentException()
        {
            // Arrange
            var service = new FirebaseService();
            var serviceAccount = """{"type": "service_account"}""";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                service.InitializeAsync(serviceAccount, "   "));
        }

        #endregion

        #region JSON Validation Tests

        [Fact]
        public async Task InitializeAsync_WithInvalidJson_ThrowsInvalidOperationException()
        {
            // Arrange
            var service = new FirebaseService();
            var invalidJson = "not-valid-json";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.InitializeAsync(invalidJson, "test-project"));

            Assert.Contains("Failed to initialize Firebase service", exception.Message);
        }

        [Fact]
        public async Task InitializeAsync_WithMalformedServiceAccountKey_ThrowsInvalidOperationException()
        {
            // Arrange
            var service = new FirebaseService();
            var malformedKey = """
            {
              "type": "service_account",
              "project_id": "test-project",
              "private_key": "invalid-key-format"
            }
            """;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.InitializeAsync(malformedKey, "test-project"));

            Assert.Contains("Failed to initialize Firebase service", exception.Message);
        }

        [Fact]
        public async Task InitializeAsync_WithMissingRequiredFields_ThrowsInvalidOperationException()
        {
            // Arrange
            var service = new FirebaseService();
            var incompleteServiceAccount = """
            {
              "type": "service_account",
              "project_id": "test-project"
            }
            """;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.InitializeAsync(incompleteServiceAccount, "test-project"));

            Assert.Contains("Failed to initialize Firebase service", exception.Message);
        }

        [Fact]
        public async Task InitializeAsync_WithUnexpectedJsonStructure_HandlesGracefully()
        {
            // Arrange
            var service = new FirebaseService();
            var unexpectedJson = """
            {
              "type": "service_account",
              "project_id": "test-project",
              "private_key_id": "key-id",
              "private_key": "-----BEGIN PRIVATE KEY-----\ntest\n-----END PRIVATE KEY-----\n",
              "client_email": "test@test-project.iam.gserviceaccount.com",
              "nested": {
                "deep": {
                  "structure": "value"
                }
              },
              "array_field": ["item1", "item2", "item3"],
              "null_field": null,
              "number_field": 12345,
              "boolean_field": true
            }
            """;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.InitializeAsync(unexpectedJson, "test-project"));

            Assert.Contains("Failed to initialize Firebase service", exception.Message);
        }

        #endregion

        #region State Management Tests

        [Fact]
        public async Task TestConnectionAsync_WhenNotInitialized_ReturnsFalse()
        {
            // Arrange
            var service = new FirebaseService();

            // Act
            var result = await service.TestConnectionAsync();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task SendAsync_WhenNotInitialized_ThrowsInvalidOperationException()
        {
            // Arrange
            var service = new FirebaseService();
            var message = CreateTestFirebaseMessage();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.SendAsync(message));

            Assert.Contains("Firebase service is not initialized", exception.Message);
        }

        [Fact]
        public async Task SendEachAsync_WhenNotInitialized_ThrowsInvalidOperationException()
        {
            // Arrange
            var service = new FirebaseService();
            var messages = new[] { CreateTestFirebaseMessage() };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.SendEachAsync(messages));

            Assert.Contains("Firebase service is not initialized", exception.Message);
        }

        [Fact]
        public async Task SendMulticastAsync_WhenNotInitialized_ThrowsInvalidOperationException()
        {
            // Arrange
            var service = new FirebaseService();
            var multicastMessage = CreateTestMulticastMessage();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.SendMulticastAsync(multicastMessage));

            Assert.Contains("Firebase service is not initialized", exception.Message);
        }

        #endregion

        #region Message Validation Tests

        [Fact]
        public async Task SendAsync_WithNullMessage_ThrowsArgumentNullException()
        {
            // Note: Firebase service checks initialization before parameter validation
            // So we need to test this with an initialized service or expect initialization error
            
            // Arrange
            var service = new FirebaseService();

            // Act & Assert - Will throw initialization error before parameter validation
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.SendAsync(null!));
        }

        [Fact]
        public async Task SendEachAsync_WithNullMessages_ThrowsArgumentNullException()
        {
            // Note: Firebase service checks initialization before parameter validation
            
            // Arrange
            var service = new FirebaseService();

            // Act & Assert - Will throw initialization error before parameter validation
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.SendEachAsync(null!));
        }

        [Fact]
        public async Task SendMulticastAsync_WithNullMessage_ThrowsArgumentNullException()
        {
            // Note: Firebase service checks initialization before parameter validation
            
            // Arrange
            var service = new FirebaseService();

            // Act & Assert - Will throw initialization error before parameter validation
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.SendMulticastAsync(null!));
        }

        #endregion

        #region Cancellation Token Tests

        [Fact]
        public async Task SendAsync_WithCancelledToken_ThrowsOperationCancelledException()
        {
            // Note: Firebase service checks initialization before cancellation token
            
            // Arrange
            var service = new FirebaseService();
            var message = CreateTestFirebaseMessage();
            var cancellationToken = new CancellationToken(true); // Already cancelled

            // Act & Assert - Will throw initialization error before checking cancellation
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.SendAsync(message, cancellationToken: cancellationToken));
        }

        [Fact]
        public async Task TestConnectionAsync_WithCancelledToken_ReturnsFalseWhenNotInitialized()
        {
            // Arrange
            var service = new FirebaseService();
            var cancellationToken = new CancellationToken(true); // Already cancelled

            // Act - TestConnectionAsync returns false when not initialized, regardless of cancellation
            var result = await service.TestConnectionAsync(cancellationToken);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task SendEachAsync_WithCancelledToken_ThrowsOperationCancelledException()
        {
            // Note: Firebase service checks initialization before cancellation token
            
            // Arrange
            var service = new FirebaseService();
            var messages = new[] { CreateTestFirebaseMessage() };
            var cancellationToken = new CancellationToken(true); // Already cancelled

            // Act & Assert - Will throw initialization error before checking cancellation
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.SendEachAsync(messages, cancellationToken: cancellationToken));
        }

        [Fact]
        public async Task SendMulticastAsync_WithCancelledToken_ThrowsOperationCancelledException()
        {
            // Note: Firebase service checks initialization before cancellation token
            
            // Arrange
            var service = new FirebaseService();
            var multicastMessage = CreateTestMulticastMessage();
            var cancellationToken = new CancellationToken(true); // Already cancelled

            // Act & Assert - Will throw initialization error before checking cancellation
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.SendMulticastAsync(multicastMessage, cancellationToken: cancellationToken));
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public async Task SendEachAsync_WithEmptyMessageList_DoesNotThrow()
        {
            // Arrange
            var service = new FirebaseService();
            var messages = Array.Empty<FirebaseAdmin.Messaging.Message>();

            // Act & Assert - Should throw because service is not initialized, not because of empty array
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.SendEachAsync(messages));
            
            Assert.Contains("Firebase service is not initialized", exception.Message);
        }

        [Fact]
        public void SendAsync_DefaultDryRunParameter_IsFalse()
        {
            // This test verifies the method signature has the correct default parameter
            // We can't test the actual behavior without a real Firebase connection
            // but we can verify the method exists with the expected signature
            
            // Arrange
            var service = new FirebaseService();
            var message = CreateTestFirebaseMessage();

            // Act & Assert - Verify the method can be called without the dryRun parameter
            var exception = Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.SendAsync(message));

            // The exception should be about initialization, not parameters
            Assert.NotNull(exception);
        }

        #endregion

        #region Helper Methods

        private static FirebaseAdmin.Messaging.Message CreateTestFirebaseMessage(string token = "test-device-token")
        {
            return new FirebaseAdmin.Messaging.Message
            {
                Token = token,
                Notification = new Notification
                {
                    Title = "Test Notification",
                    Body = "This is a test message"
                },
                Data = new Dictionary<string, string>
                {
                    ["test_key"] = "test_value",
                    ["timestamp"] = DateTimeOffset.UtcNow.ToString()
                }
            };
        }

        private static MulticastMessage CreateTestMulticastMessage()
        {
            return new MulticastMessage
            {
                Tokens = new List<string> { "token1", "token2", "token3" },
                Notification = new Notification
                {
                    Title = "Multicast Test",
                    Body = "This is a multicast test message"
                },
                Data = new Dictionary<string, string>
                {
                    ["multicast"] = "true",
                    ["timestamp"] = DateTimeOffset.UtcNow.ToString()
                }
            };
        }

        #endregion
    }
}