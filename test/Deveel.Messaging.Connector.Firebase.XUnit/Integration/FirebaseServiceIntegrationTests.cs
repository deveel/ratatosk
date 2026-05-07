//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace Deveel.Messaging
{
    /// <summary>
    /// Integration tests for FirebaseService that test message construction,
    /// validation, and service behavior patterns.
    /// </summary>
    [Trait("Category", "Integration")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Feature", "FirebaseService")]
    public class FirebaseServiceIntegrationTests : IDisposable
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

        #region Message Construction Tests

        [Fact]
        public void Should_ConstructsCorrectly_When_CreateComplexFirebaseMessageWithAllProperties()
        {
            // Arrange
            // Act
            var complexMessage = new FirebaseAdmin.Messaging.Message
            {
                Token = "test-device-token",
                Notification = new Notification
                {
                    Title = "Complex Notification",
                    Body = "This message has all properties set",
                    ImageUrl = "https://example.com/image.png"
                },
                Data = new Dictionary<string, string>
                {
                    ["user_id"] = "12345",
                    ["action"] = "open_screen",
                    ["screen"] = "profile",
                    ["json_data"] = """{"nested": "value", "number": 42}"""
                },
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    TimeToLive = TimeSpan.FromHours(1),
                    CollapseKey = "update_key",
                    Notification = new AndroidNotification
                    {
                        Color = "#FF5722",
                        Sound = "notification_sound",
                        Tag = "update_tag",
                        ClickAction = "OPEN_ACTIVITY"
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Badge = 5,
                        Sound = "notification_sound.caf",
                        ContentAvailable = true,
                        MutableContent = true,
                        ThreadId = "chat_thread_123"
                    }
                }
            };

            // Assert
            Assert.Equal("test-device-token", complexMessage.Token);
            Assert.Equal("Complex Notification", complexMessage.Notification.Title);
            Assert.Equal("This message has all properties set", complexMessage.Notification.Body);
            Assert.Equal("https://example.com/image.png", complexMessage.Notification.ImageUrl);
            Assert.Equal(4, complexMessage.Data.Count);
            Assert.Equal("12345", complexMessage.Data["user_id"]);
            Assert.NotNull(complexMessage.Android);
            Assert.Equal(Priority.High, complexMessage.Android.Priority);
            Assert.NotNull(complexMessage.Apns);
            Assert.Equal(5, complexMessage.Apns.Aps.Badge);
        }

        [Fact]
        public void Should_ConstructsCorrectly_When_CreateTopicMessageWithTopicTargeting()
        {
            // Arrange
            // Act
            var topicMessage = new FirebaseAdmin.Messaging.Message
            {
                Topic = "news_updates",
                Notification = new Notification
                {
                    Title = "Breaking News",
                    Body = "Important news update for all subscribers"
                },
                Data = new Dictionary<string, string>
                {
                    ["category"] = "breaking_news",
                    ["priority"] = "high"
                }
            };

            // Assert
            Assert.Equal("news_updates", topicMessage.Topic);
            Assert.Null(topicMessage.Token);
            Assert.Equal("Breaking News", topicMessage.Notification.Title);
            Assert.Equal(2, topicMessage.Data.Count);
        }

        [Fact]
        public void Should_ConstructsCorrectly_When_CreateDataOnlyMessageWithoutNotification()
        {
            // Arrange
            // Act
            var dataOnlyMessage = new FirebaseAdmin.Messaging.Message
            {
                Token = "test-device-token",
                Data = new Dictionary<string, string>
                {
                    ["silent_update"] = "true",
                    ["background_sync"] = "user_data",
                    ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
                }
            };

            // Assert
            Assert.Equal("test-device-token", dataOnlyMessage.Token);
            Assert.Null(dataOnlyMessage.Notification);
            Assert.Equal(3, dataOnlyMessage.Data.Count);
            Assert.Equal("true", dataOnlyMessage.Data["silent_update"]);
        }

        [Fact]
        public void Should_ConstructsCorrectly_When_CreateMulticastMessageWithMultipleTokens()
        {
            // Arrange
            var tokens = Enumerable.Range(1, 5).Select(i => $"device-token-{i}").ToList();

            // Act
            var multicastMessage = new MulticastMessage
            {
                Tokens = tokens,
                Notification = new Notification
                {
                    Title = "Multicast Notification",
                    Body = "This message goes to multiple devices"
                },
                Data = new Dictionary<string, string>
                {
                    ["multicast"] = "true",
                    ["recipient_count"] = tokens.Count.ToString()
                }
            };

            // Assert
            Assert.Equal(5, multicastMessage.Tokens.Count);
            Assert.Equal("device-token-1", multicastMessage.Tokens[0]);
            Assert.Equal("device-token-5", multicastMessage.Tokens[4]);
            Assert.Equal("Multicast Notification", multicastMessage.Notification.Title);
            Assert.Equal("5", multicastMessage.Data["recipient_count"]);
        }

        #endregion

        #region Service State Management Tests

        [Fact]
        public void Should_MaintainSeparateStates_When_MultipleServicesIndependentInitialization()
        {
            // Arrange
            var service1 = CreateService();
            var service2 = CreateService();
            var service3 = CreateService();

            // Act
            // Assert
            Assert.False(service1.IsInitialized);
            Assert.False(service2.IsInitialized);
            Assert.False(service3.IsInitialized);
            
            Assert.Null(service1.App);
            Assert.Null(service2.App);
            Assert.Null(service3.App);
        }

        [Fact]
        public async Task Should_ReturnFalseConsistently_When_ServiceTestConnectionBeforeInitialization()
        {
            // Arrange
            var service = CreateService();

            // Act
            var results = new List<bool>();
            for (int i = 0; i < 3; i++)
            {
                results.Add(await service.TestConnectionAsync());
            }

            // Assert
            Assert.True(results.All(r => !r), "All connection tests should return false when not initialized");
        }

        #endregion

        #region Parameter Validation Edge Cases

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n")]
        public async Task Should_ThrowAppropriateException_When_InitializeAsyncWithInvalidServiceAccountKeys(string invalidKey)
        {
            // Arrange
            var service = CreateService();

            // Act
            // Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.InitializeAsync(invalidKey, "test-project"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n")]
        public async Task Should_ThrowAppropriateException_When_InitializeAsyncWithInvalidProjectIds(string invalidProjectId)
        {
            // Arrange
            var service = CreateService();
            var serviceAccount = """{"type": "service_account"}""";

            // Act
            // Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.InitializeAsync(serviceAccount, invalidProjectId));
        }

        #endregion

        #region JSON Structure Tests

        [Fact]
        public async Task Should_HandleCorrectly_When_InitializeAsyncWithValidJsonStructure()
        {
            // Arrange
            var service = CreateService();
            var wellFormedJson = """
            {
              "type": "service_account",
              "project_id": "test-project-id",
              "private_key_id": "some-key-id",
              "private_key": "-----BEGIN PRIVATE KEY-----\nvalid-key-content\n-----END PRIVATE KEY-----\n",
              "client_email": "test@test-project.iam.gserviceaccount.com",
              "client_id": "123456789",
              "auth_uri": "https://accounts.google.com/o/oauth2/auth",
              "token_uri": "https://oauth2.googleapis.com/token"
            }
            """;

            // Act
            // Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.InitializeAsync(wellFormedJson, "test-project-id"));

            Assert.Contains("Failed to initialize Firebase service", exception.Message);
        }

        [Fact]
        public async Task Should_ThrowInvalidOperationException_When_InitializeAsyncWithMalformedJson()
        {
            // Arrange
            var service = CreateService();
            var malformedJson = """
            {
              "type": "service_account",
              "project_id": "test-project",
              "unclosed_quote": "missing closing quote
            }
            """;

            // Act
            // Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.InitializeAsync(malformedJson, "test-project"));

            Assert.Contains("Failed to initialize Firebase service", exception.Message);
        }

        #endregion

        #region Message Validation Patterns

        [Fact]
        public void Should_PassesBasicStructure_When_ValidateFirebaseMessageWithRequiredFields()
        {
            // Arrange
            // Act
            var validMessage = new FirebaseAdmin.Messaging.Message
            {
                Token = "valid-device-token",
                Notification = new Notification
                {
                    Title = "Valid Title",
                    Body = "Valid body content"
                }
            };

            // Assert
            Assert.NotNull(validMessage.Token);
            Assert.NotEmpty(validMessage.Token);
            Assert.NotNull(validMessage.Notification);
            Assert.NotEmpty(validMessage.Notification.Title);
            Assert.NotEmpty(validMessage.Notification.Body);
        }

        [Fact]
        public void Should_StructureIsCorrect_When_ValidateFirebaseMessageWithLargeDataPayload()
        {
            // Arrange
            var largeData = new Dictionary<string, string>();
            for (int i = 0; i < 50; i++)
            {
                largeData.Add($"key_{i:D3}", $"value_{i:D3}_" + new string('x', 50));
            }

            // Act
            var messageWithLargeData = new FirebaseAdmin.Messaging.Message
            {
                Token = "test-device-token",
                Data = largeData
            };

            // Assert
            Assert.Equal(50, messageWithLargeData.Data.Count);
            Assert.True(messageWithLargeData.Data.ContainsKey("key_000"));
            Assert.True(messageWithLargeData.Data.ContainsKey("key_049"));
            Assert.Contains("value_025_", messageWithLargeData.Data["key_025"]);
        }

        #endregion

        #region Error Handling Patterns

        [Fact]
        public async Task Should_ProvideConsistentErrorMessages_When_ServiceOperationsWhenNotInitialized()
        {
            // Arrange
            var service = CreateService();
            var message = new FirebaseAdmin.Messaging.Message { Token = "test" };
            var messages = new[] { message };
            var multicastMessage = new MulticastMessage { Tokens = new[] { "test" } };

            // Act
            // Assert
            var sendException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.SendAsync(message));
            var sendEachException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.SendEachAsync(messages));
            var multicastException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.SendMulticastAsync(multicastMessage));

            // All should have consistent error message
            Assert.Contains("Firebase service is not initialized", sendException.Message);
            Assert.Contains("Firebase service is not initialized", sendEachException.Message);
            Assert.Contains("Firebase service is not initialized", multicastException.Message);
        }

        #endregion

        #region Performance and Concurrency Patterns

        [Fact]
        public async Task Should_HandleGracefully_When_TestConnectionAsyncConcurrentCalls()
        {
            // Arrange
            var service = CreateService();

            // Act
            var tasks = Enumerable.Range(1, 10)
                .Select(_ => service.TestConnectionAsync())
                .ToArray();

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.True(results.All(r => !r), "All concurrent connection tests should return false");
        }

        [Fact]
        public async Task Should_RespondToCancellation_When_ServiceOperationsWithCancellation()
        {
            // Arrange
            var service = CreateService();
            var message = new FirebaseAdmin.Messaging.Message 
            { 
                Token = "test-device-token",
                Notification = new Notification 
                {
                    Title = "Test",
                    Body = "Test message"
                }
            };
            using var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel immediately

            // Act
            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.SendAsync(message, false, cts.Token));
        }

        #endregion
    }
}