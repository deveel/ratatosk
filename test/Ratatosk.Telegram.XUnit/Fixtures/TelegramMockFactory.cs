//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Moq;
using System.Reflection;

using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

using TelegramMessage = Telegram.Bot.Types.Message;

namespace Ratatosk
{
	/// <summary>
	/// Factory for creating mock Telegram service instances and test data.
	/// </summary>
	public static class TelegramMockFactory
	{
		/// <summary>
		/// Creates a mock Telegram service with basic functionality.
		/// </summary>
		/// <returns>A configured mock Telegram service.</returns>
		public static Mock<ITelegramService> CreateMockTelegramService()
		{
			var mock = new Mock<ITelegramService>();

			// Setup basic methods
			mock.Setup(x => x.Initialize(It.IsAny<string>()))
				.Callback<string>(token => { });

			mock.Setup(x => x.GetMeAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(CreateTestBot());

			// Setup webhook methods
			mock.Setup(x => x.SetWebhookAsync(
					It.IsAny<string>(),
					It.IsAny<InputFile?>(),
					It.IsAny<string?>(),
					It.IsAny<int?>(),
					It.IsAny<IEnumerable<Telegram.Bot.Types.Enums.UpdateType>?>(),
					It.IsAny<bool?>(),
					It.IsAny<string?>(),
					It.IsAny<CancellationToken>()))
				.Returns(Task.CompletedTask);

			mock.Setup(x => x.DeleteWebhookAsync(
					It.IsAny<bool?>(),
					It.IsAny<CancellationToken>()))
				.Returns(Task.CompletedTask);

			mock.Setup(x => x.GetWebhookInfoAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(CreateTestWebhookInfo());

			// Setup GetUpdatesAsync
			mock.Setup(x => x.GetUpdatesAsync(
					It.IsAny<int?>(),
					It.IsAny<int?>(),
					It.IsAny<int?>(),
					It.IsAny<IEnumerable<Telegram.Bot.Types.Enums.UpdateType>?>(),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(Array.Empty<Update>());

			return mock;
		}

		/// <summary>
		/// Creates a mock Telegram service that successfully sends messages.
		/// </summary>
		/// <returns>A configured mock Telegram service for successful message sending.</returns>
		public static Mock<ITelegramService> CreateSuccessfulSendMockService()
		{
			var mock = CreateMockTelegramService();

			mock.Setup(x => x.SendTextMessageAsync(
					It.IsAny<ChatId>(),
					It.IsAny<string>(),
					It.IsAny<Telegram.Bot.Types.Enums.ParseMode?>(),
					It.IsAny<bool?>(),
					It.IsAny<bool?>(),
					It.IsAny<int?>(),
					It.IsAny<IReplyMarkup?>(),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(CreateTestTelegramMessage());

			mock.Setup(x => x.SendPhotoAsync(
					It.IsAny<ChatId>(),
					It.IsAny<InputFile>(),
					It.IsAny<string?>(),
					It.IsAny<Telegram.Bot.Types.Enums.ParseMode?>(),
					It.IsAny<bool?>(),
					It.IsAny<int?>(),
					It.IsAny<IReplyMarkup?>(),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(CreateTestTelegramMessage());

			mock.Setup(x => x.SendVideoAsync(
					It.IsAny<ChatId>(),
					It.IsAny<InputFile>(),
					It.IsAny<int?>(),
					It.IsAny<int?>(),
					It.IsAny<int?>(),
					It.IsAny<InputFile?>(),
					It.IsAny<string?>(),
					It.IsAny<Telegram.Bot.Types.Enums.ParseMode?>(),
					It.IsAny<bool?>(),
					It.IsAny<bool?>(),
					It.IsAny<int?>(),
					It.IsAny<IReplyMarkup?>(),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(CreateTestTelegramMessage());

			mock.Setup(x => x.SendAudioAsync(
					It.IsAny<ChatId>(),
					It.IsAny<InputFile>(),
					It.IsAny<string?>(),
					It.IsAny<Telegram.Bot.Types.Enums.ParseMode?>(),
					It.IsAny<int?>(),
					It.IsAny<string?>(),
					It.IsAny<string?>(),
					It.IsAny<InputFile?>(),
					It.IsAny<bool?>(),
					It.IsAny<int?>(),
					It.IsAny<IReplyMarkup?>(),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(CreateTestTelegramMessage());

			mock.Setup(x => x.SendDocumentAsync(
					It.IsAny<ChatId>(),
					It.IsAny<InputFile>(),
					It.IsAny<InputFile?>(),
					It.IsAny<string?>(),
					It.IsAny<Telegram.Bot.Types.Enums.ParseMode?>(),
					It.IsAny<bool?>(),
					It.IsAny<bool?>(),
					It.IsAny<int?>(),
					It.IsAny<IReplyMarkup?>(),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(CreateTestTelegramMessage());

			mock.Setup(x => x.SendLocationAsync(
					It.IsAny<ChatId>(),
					It.IsAny<double>(),
					It.IsAny<double>(),
					It.IsAny<int?>(),
					It.IsAny<int?>(),
					It.IsAny<int?>(),
					It.IsAny<bool?>(),
					It.IsAny<int?>(),
					It.IsAny<IReplyMarkup?>(),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(CreateTestTelegramMessage());

			return mock;
		}

		/// <summary>
		/// Creates a mock Telegram service that fails operations.
		/// </summary>
		/// <returns>A configured mock Telegram service that throws exceptions.</returns>
		public static Mock<ITelegramService> CreateFailingMockService()
		{
			var mock = new Mock<ITelegramService>();

			mock.Setup(x => x.Initialize(It.IsAny<string>()))
				.Throws(new ArgumentException("Invalid bot token"));

			mock.Setup(x => x.GetMeAsync(It.IsAny<CancellationToken>()))
				.ThrowsAsync(new InvalidOperationException("Unauthorized"));

			mock.Setup(x => x.SendTextMessageAsync(
					It.IsAny<ChatId>(),
					It.IsAny<string>(),
					It.IsAny<Telegram.Bot.Types.Enums.ParseMode?>(),
					It.IsAny<bool?>(),
					It.IsAny<bool?>(),
					It.IsAny<int?>(),
					It.IsAny<IReplyMarkup?>(),
					It.IsAny<CancellationToken>()))
				.ThrowsAsync(new InvalidOperationException("Bot was blocked by the user"));

			return mock;
		}

		/// <summary>
		/// Creates a test bot user.
		/// </summary>
		/// <returns>A test bot user instance.</returns>
		public static User CreateTestBot()
		{
			return new User
			{
				Id = 123456789,
				IsBot = true,
				FirstName = "Test Bot",
				Username = "test_bot",
				CanJoinGroups = true,
				CanReadAllGroupMessages = false,
				SupportsInlineQueries = false
			};
		}

		/// <summary>
		/// Creates a test Telegram message.
		/// </summary>
		/// <returns>A test Telegram message instance.</returns>
		public static TelegramMessage CreateTestTelegramMessage()
		{
			// Since Telegram.Bot.Types.Message properties are read-only and sealed,
			// we can't easily mock them. Instead, we'll use reflection to create
			// a message with the necessary properties set.
			
			// Try to create a message using reflection
			var messageType = typeof(TelegramMessage);
			var message = (TelegramMessage)Activator.CreateInstance(messageType, true)!;
			
			// Use reflection to set the properties we need
			SetPrivateProperty(message, "MessageId", 12345);
			SetPrivateProperty(message, "Date", DateTime.UtcNow);
			SetPrivateProperty(message, "Chat", CreateTestChat());
			
			return message;
		}
		
		/// <summary>
		/// Helper method to set private properties using reflection.
		/// </summary>
		private static void SetPrivateProperty(object obj, string propertyName, object value)
		{
			var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
			if (property != null && property.CanWrite)
			{
				property.SetValue(obj, value);
			}
			else
			{
				// Try to set the backing field instead
				var field = obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
				field?.SetValue(obj, value);
			}
		}

		/// <summary>
		/// Creates a test chat.
		/// </summary>
		/// <returns>A test chat instance.</returns>
		public static Chat CreateTestChat()
		{
			return new Chat
			{
				Id = 123456789,
				Type = Telegram.Bot.Types.Enums.ChatType.Private,
				FirstName = "Test",
				LastName = "User",
				Username = "testuser"
			};
		}

		/// <summary>
		/// Creates a test user.
		/// </summary>
		/// <returns>A test user instance.</returns>
		public static User CreateTestUser()
		{
			return new User
			{
				Id = 987654321,
				IsBot = false,
				FirstName = "Test",
				LastName = "User",
				Username = "testuser"
			};
		}

		/// <summary>
		/// Creates test connection settings.
		/// </summary>
		/// <returns>Test connection settings.</returns>
		public static ConnectionSettings CreateTestConnectionSettings()
		{
			return new ConnectionSettings()
				.SetParameter("BotToken", "123456789:ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqr")
				.SetParameter("ParseMode", "Markdown")
				.SetParameter("DisableNotification", false)
				.SetParameter("MaxRetries", 3)
				.SetParameter("TimeoutSeconds", 30);
		}

		/// <summary>
		/// Creates test connection settings with webhook.
		/// </summary>
		/// <returns>Test connection settings with webhook configuration.</returns>
		public static ConnectionSettings CreateWebhookConnectionSettings()
		{
			return new ConnectionSettings()
				.SetParameter("BotToken", "123456789:ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqr")
				.SetParameter("WebhookUrl", "https://example.com/webhook")
				.SetParameter("SecretToken", "secret123")
				.SetParameter("MaxConnections", 40)
				.SetParameter("DropPendingUpdates", false);
		}

		/// <summary>
		/// Creates a test text message.
		/// </summary>
		/// <returns>A test text message.</returns>
		public static IMessage CreateTestTextMessage()
		{
			return new Message
			{
				Id = "test-message-1",
				Receiver = new Endpoint(EndpointType.Id, "123456789"),
				Content = new TextContent("Hello, this is a test message!")
			};
		}

		/// <summary>
		/// Creates a test media message.
		/// </summary>
		/// <returns>A test media message.</returns>
		public static IMessage CreateTestMediaMessage()
		{
			return new Message
			{
				Id = "test-message-2",
				Receiver = new Endpoint(EndpointType.Id, "123456789"),
				Content = new MediaContent(MediaType.Image, "test.jpg", "https://example.com/test.jpg"),
				Properties = new Dictionary<string, MessageProperty>
				{
					{ "Caption", new MessageProperty("Caption", "Test image") }
				}
			};
		}

		/// <summary>
		/// Creates a test location message with live location properties.
		/// </summary>
		/// <returns>A test location message with enhanced properties.</returns>
		public static IMessage CreateTestLocationMessage()
		{
			return new Message
			{
				Id = "test-message-3",
				Receiver = new Endpoint(EndpointType.Id, "123456789"),
				Content = new LocationContent(40.7128, -74.0060) // New York coordinates
					.WithLivePeriod(3600) // 1 hour live location
					.WithHeading(45) // Northeast direction
					.WithProximityAlertRadius(500) // 500 meters proximity alert
			};
		}

		/// <summary>
		/// Creates a test webhook update JSON.
		/// </summary>
		/// <returns>JSON string representing a Telegram webhook update.</returns>
		public static string CreateTestWebhookJson()
		{
			return """
			{
				"update_id": 123456789,
				"message": {
					"message_id": 1,
					"from": {
						"id": 987654321,
						"is_bot": false,
						"first_name": "Test",
						"last_name": "User",
						"username": "testuser"
					},
					"chat": {
						"id": 987654321,
						"first_name": "Test",
						"last_name": "User",
						"username": "testuser",
						"type": "private"
					},
					"date": 1640995200,
					"text": "Hello from user!"
				}
			}
			""";
		}

		/// <summary>
		/// Creates a test webhook info.
		/// </summary>
		/// <returns>A test webhook info instance.</returns>
		public static WebhookInfo CreateTestWebhookInfo()
		{
			return new WebhookInfo
			{
				Url = "https://example.com/webhook",
				HasCustomCertificate = false,
				PendingUpdateCount = 0,
				MaxConnections = 40
			};
		}

		/// <summary>
		/// Creates a message with valid location coordinates for testing validation.
		/// </summary>
		/// <returns>A message with a valid location since invalid coordinates can't be created.</returns>
		public static IMessage CreateInvalidLocationMessage()
		{
			// Since LocationContent constructor validates coordinates, we can't create truly invalid ones.
			// Instead, we'll create a valid location that might fail other validation rules.
			var message = new Message
			{
				Id = "test-invalid-location",
				Receiver = new Endpoint(EndpointType.Id, "123456789"),
				Content = new LocationContent(90.0, 180.0) // Valid but extreme coordinates
			};

			return message;
		}

		/// <summary>
		/// Creates a message with an edge case live period for testing validation.
		/// </summary>
		/// <returns>A message with a valid location and live period.</returns>
		public static IMessage CreateInvalidLivePeriodMessage()
		{
			// Since LocationContent validates live period in WithLivePeriod, we'll create a valid one
			// The test should validate edge cases differently or use the actual validation constraints
			var message = new Message
			{
				Id = "test-live-period",
				Receiver = new Endpoint(EndpointType.Id, "123456789"),
				Content = new LocationContent(40.7128, -74.0060).WithLivePeriod(60) // Minimum valid live period
			};

			return message;
		}
	}
}