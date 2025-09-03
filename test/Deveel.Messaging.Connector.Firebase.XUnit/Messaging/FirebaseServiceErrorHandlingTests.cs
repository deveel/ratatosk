//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using System.Text.Json;

namespace Deveel.Messaging
{
    /// <summary>
    /// Tests for FirebaseService error handling, edge cases, and boundary conditions.
    /// These tests ensure the service behaves correctly under abnormal conditions.
    /// </summary>
    public class FirebaseServiceErrorHandlingTests : IDisposable
    {
        private readonly List<FirebaseService> _services = new();

        public void Dispose()
        {
            foreach (var service in _services)
            {
                try
                {
                    service.App?.Delete();
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        private FirebaseService CreateService()
        {
            var service = new FirebaseService();
            _services.Add(service);
            return service;
        }

        #region Invalid Service Account Scenarios

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n")]
        public async Task InitializeAsync_WithWhitespaceServiceAccountKey_ThrowsArgumentException(string whitespaceKey)
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.InitializeAsync(whitespaceKey, "test-project"));
        }

        [Fact]
        public async Task InitializeAsync_WithMissingRequiredFields_ThrowsInvalidOperationException()
        {
            // Arrange
            var service = CreateService();
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
        public async Task InitializeAsync_WithInvalidPrivateKeyFormat_ThrowsInvalidOperationException()
        {
            // Arrange
            var service = CreateService();
            var invalidPrivateKeyServiceAccount = """
            {
              "type": "service_account",
              "project_id": "test-project",
              "private_key_id": "key-id",
              "private_key": "invalid-private-key-format",
              "client_email": "test@test-project.iam.gserviceaccount.com"
            }
            """;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.InitializeAsync(invalidPrivateKeyServiceAccount, "test-project"));

            Assert.Contains("Failed to initialize Firebase service", exception.Message);
        }

        [Fact]
        public async Task InitializeAsync_WithWrongServiceAccountType_ThrowsInvalidOperationException()
        {
            // Arrange
            var service = CreateService();
            var wrongTypeServiceAccount = """
            {
              "type": "user_account",
              "project_id": "test-project",
              "private_key_id": "key-id",
              "private_key": "-----BEGIN PRIVATE KEY-----\ntest\n-----END PRIVATE KEY-----\n",
              "client_email": "test@test-project.iam.gserviceaccount.com"
            }
            """;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.InitializeAsync(wrongTypeServiceAccount, "test-project"));

            Assert.Contains("Failed to initialize Firebase service", exception.Message);
        }

        #endregion

        #region Project ID Validation

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n")]
        public async Task InitializeAsync_WithInvalidProjectId_ThrowsArgumentException(string invalidProjectId)
        {
            // Arrange
            var service = CreateService();
            var serviceAccount = """{"type": "service_account"}""";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.InitializeAsync(serviceAccount, invalidProjectId));
        }

        #endregion

        #region Message Validation Errors

        [Fact]
        public async Task SendAsync_WithMessageMissingTarget_ThrowsWhenNotInitialized()
        {
            // Arrange
            var service = CreateService();
            var messageWithoutTarget = new FirebaseAdmin.Messaging.Message
            {
                // No Token, Topic, or Condition
                Notification = new Notification { Title = "Test", Body = "Test message" }
            };

            // Act & Assert - Should fail because service is not initialized
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.SendAsync(messageWithoutTarget));

            Assert.Contains("Firebase service is not initialized", exception.Message);
        }

        [Fact]
        public async Task SendAsync_WithEmptyNotificationAndData_FailsWhenNotInitialized()
        {
            // Arrange
            var service = CreateService();
            var emptyMessage = new FirebaseAdmin.Messaging.Message
            {
                Token = "test-device-token"
                // No notification or data - Firebase allows this when properly initialized
            };

            // Act & Assert - Should fail because service is not initialized
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.SendAsync(emptyMessage));

            Assert.Contains("Firebase service is not initialized", exception.Message);
        }

        [Fact]
        public void CreateMessage_WithExcessivelyLongData_ConstructsCorrectly()
        {
            // Arrange
            var largeData = new Dictionary<string, string>();

            // Add data that approaches Firebase limits
            for (int i = 0; i < 20; i++)
            {
                largeData.Add($"key_{i}", new string('x', 100)); // 100 chars per field
            }

            // Act
            var largeDataMessage = new FirebaseAdmin.Messaging.Message
            {
                Token = "test-device-token",
                Data = largeData
            };

            // Assert
            Assert.Equal(20, largeDataMessage.Data.Count);
            Assert.Equal(100, largeDataMessage.Data["key_0"].Length);
            Assert.True(largeDataMessage.Data["key_19"].All(c => c == 'x'));
        }

        #endregion

        #region Batch Operations Error Scenarios

        [Fact]
        public async Task SendEachAsync_WithEmptyMessageArray_FailsWhenNotInitialized()
        {
            // Arrange
            var service = CreateService();
            var emptyMessages = Array.Empty<FirebaseAdmin.Messaging.Message>();

            // Act & Assert - Should fail because service is not initialized, not because array is empty
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.SendEachAsync(emptyMessages));

            Assert.Contains("Firebase service is not initialized", exception.Message);
        }

        [Fact]
        public void CreateMulticastMessage_WithDuplicateTokens_ConstructsCorrectly()
        {
            // Arrange & Act
            var multicastWithDuplicates = new MulticastMessage
            {
                Tokens = new List<string> { "token1", "token2", "token1", "token3", "token2" }, // Duplicates
                Notification = new Notification { Title = "Duplicate Test", Body = "Testing duplicate tokens" }
            };

            // Assert - Message should construct correctly; Firebase handles duplicates at send time
            Assert.Equal(5, multicastWithDuplicates.Tokens.Count);
            Assert.Equal("token1", multicastWithDuplicates.Tokens[0]);
            Assert.Equal("token1", multicastWithDuplicates.Tokens[2]); // Duplicate is preserved
        }

        [Fact]
        public void CreateMulticastMessage_WithExcessiveTokenCount_ConstructsCorrectly()
        {
            // Arrange
            var excessiveTokens = Enumerable.Range(1, 1500).Select(i => $"token-{i}").ToList();

            // Act
            var multicastMessage = new MulticastMessage
            {
                Tokens = excessiveTokens,
                Notification = new Notification { Title = "Excessive Tokens", Body = "Testing large token list" }
            };

            // Assert - Message should construct correctly; Firebase will validate at send time
            Assert.Equal(1500, multicastMessage.Tokens.Count);
            Assert.Equal("token-1", multicastMessage.Tokens[0]);
            Assert.Equal("token-1500", multicastMessage.Tokens[1499]);
        }

        #endregion

        #region Connection Test Error Scenarios

        [Fact]
        public async Task TestConnectionAsync_MultipleCallsWhenNotInitialized_ConsistentlyReturnsFalse()
        {
            // Arrange
            var service = CreateService();

            // Act - Test connection multiple times
            var results = new List<bool>();
            for (int i = 0; i < 5; i++)
            {
                results.Add(await service.TestConnectionAsync());
            }

            // Assert
            Assert.True(results.All(r => !r), "All connection tests should return false when not initialized");
        }

        [Fact]
        public async Task TestConnectionAsync_WithNetworkTimeout_ReturnsTimeoutBehavior()
        {
            // Arrange
            var service = CreateService();

            // Use a very short timeout to simulate network issues
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));

            // Act - When not initialized, TestConnectionAsync returns false regardless of cancellation
            var result = await service.TestConnectionAsync(cts.Token);

            // Assert - Should return false when not initialized, not throw cancellation
            Assert.False(result);
        }

        #endregion

        #region Concurrent Access Error Scenarios

        [Fact]
        public async Task ServiceOperations_ConcurrentCallsWhenNotInitialized_HandleConcurrency()
        {
            // Arrange
            var service = CreateService();
            var messages = Enumerable.Range(1, 10).Select(i =>
                new FirebaseAdmin.Messaging.Message
                {
                    Token = $"concurrent-token-{i}",
                    Notification = new Notification 
                    { 
                        Title = $"Concurrent Message {i}", 
                        Body = $"Message {i} sent concurrently" 
                    }
                }).ToArray();

            // Act - Try to send messages concurrently when service is not initialized
            var tasks = messages.Select(message =>
                Assert.ThrowsAsync<InvalidOperationException>(() => service.SendAsync(message))
            ).ToArray();

            var exceptions = await Task.WhenAll(tasks);

            // Assert - All should fail with initialization error
            Assert.Equal(10, exceptions.Length);
            Assert.True(exceptions.All(ex => ex.Message.Contains("Firebase service is not initialized")));
        }

        #endregion

        #region Memory and Resource Management

        [Fact]
        public async Task MultipleServiceInstances_IndependentErrorStates_NoInterference()
        {
            // Arrange
            var services = new[] { CreateService(), CreateService(), CreateService() };
            var testMessage = new FirebaseAdmin.Messaging.Message
            {
                Token = "test-token",
                Notification = new Notification { Title = "Test", Body = "Test message" }
            };

            // Act - All services should fail independently
            var tasks = services.Select(service =>
                Assert.ThrowsAsync<InvalidOperationException>(() => service.SendAsync(testMessage))
            ).ToArray();

            var exceptions = await Task.WhenAll(tasks);

            // Assert - All should fail independently with same error
            Assert.Equal(3, exceptions.Length);
            Assert.True(exceptions.All(ex => ex.Message.Contains("Firebase service is not initialized")));
            
            // All services should still be in uninitialized state
            Assert.True(services.All(s => !s.IsInitialized));
            Assert.True(services.All(s => s.App == null));
        }

        #endregion

        #region JSON Parsing Edge Cases

        [Fact]
        public async Task InitializeAsync_WithUnexpectedJsonStructure_HandlesGracefully()
        {
            // Arrange
            var service = CreateService();
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

        [Fact]
        public async Task InitializeAsync_WithEscapedCharactersInJson_HandlesCorrectly()
        {
            // Arrange
            var service = CreateService();
            var jsonWithEscapes = """
            {
              "type": "service_account",
              "project_id": "test-project-with\tescapes\nand\"quotes",
              "private_key_id": "key-id",
              "private_key": "-----BEGIN PRIVATE KEY-----\ntest\n-----END PRIVATE KEY-----\n",
              "client_email": "test@test-project.iam.gserviceaccount.com"
            }
            """;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.InitializeAsync(jsonWithEscapes, "test-project-with\tescapes\nand\"quotes"));

            Assert.Contains("Failed to initialize Firebase service", exception.Message);
        }

        [Fact]
        public async Task InitializeAsync_WithVeryLargeJson_HandlesGracefully()
        {
            // Arrange
            var service = CreateService();
            
            // Create a large JSON with many extra fields
            var largeJsonParts = new List<string>
            {
                "{\"type\": \"service_account\", \"project_id\": \"test-project\""
            };
            
            for (int i = 0; i < 100; i++)
            {
                largeJsonParts.Add($", \"extra_field_{i}\": \"extra_value_{i}_{"x".PadRight(100, 'x')}\"");
            }
            
            largeJsonParts.Add(", \"private_key_id\": \"key-id\", \"private_key\": \"-----BEGIN PRIVATE KEY-----\\ntest\\n-----END PRIVATE KEY-----\\n\", \"client_email\": \"test@test-project.iam.gserviceaccount.com\"}");

            var largeJson = string.Join("", largeJsonParts);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.InitializeAsync(largeJson, "test-project"));

            Assert.Contains("Failed to initialize Firebase service", exception.Message);
        }

        #endregion
    }
}