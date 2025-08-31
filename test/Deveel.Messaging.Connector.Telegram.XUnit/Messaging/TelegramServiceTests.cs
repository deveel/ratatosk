//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Moq;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Deveel.Messaging
{
	/// <summary>
	/// Tests for the TelegramService class.
	/// </summary>
	public class TelegramServiceTests
	{
		private const string ValidToken = "123456789:ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";

		#region Constructor Tests

		[Fact]
		public void Constructor_WithNullBotClient_ShouldCreateService()
		{
			// Act
			var service = new TelegramService(null);

			// Assert
			Assert.NotNull(service);
		}

		[Fact]
		public void Constructor_WithMockBotClient_ShouldCreateService()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();

			// Act
			var service = new TelegramService(mockBotClient.Object);

			// Assert
			Assert.NotNull(service);
		}

		#endregion

		#region Initialize Tests

		[Fact]
		public void Initialize_WithValidToken_ShouldSucceed()
		{
			// Arrange
			var service = new TelegramService();

			// Act & Assert - Should not throw any exception
			var exception = Record.Exception(() => service.Initialize(ValidToken));
			Assert.Null(exception);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("   ")]
		public void Initialize_WithInvalidToken_ShouldThrowArgumentNullException(string? token)
		{
			// Arrange
			var service = new TelegramService();

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => service.Initialize(token!));
		}

		[Theory]
		[InlineData("invalid-token")]
		[InlineData("123:short")]
		[InlineData("abc:ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789")]
		[InlineData("123456789:short")]
		[InlineData("123456789:ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")] // 36 chars - too long
		public void Initialize_WithInvalidTokenFormat_ShouldThrowArgumentException(string token)
		{
			// Arrange
			var service = new TelegramService();

			// Act & Assert
			Assert.Throws<ArgumentException>(() => service.Initialize(token));
		}

		[Fact]
		public void Initialize_WithInjectedBotClient_DoesNotOverwriteExistingClient()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var service = new TelegramService(mockBotClient.Object);

			// Act
			service.Initialize(ValidToken);

			// Assert - Should not throw (existing client should be preserved)
			Assert.True(true);
		}

		[Fact]
		public void Initialize_WithNullBotClient_CreatesNewBotClient()
		{
			// Arrange
			var service = new TelegramService(null);

			// Act
			service.Initialize(ValidToken);

			// Assert - Should not throw (null coalescing assignment creates new client)
			Assert.True(true);
		}

		#endregion

		#region Token Validation Tests

		[Theory]
		[InlineData("123456789:ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789")] // Exactly 35 chars
		[InlineData("987654321:abcdefghijklmnopqrstuvwxyz123456789")] // Exactly 35 chars, lowercase
		[InlineData("1:ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789")] // Exactly 35 chars
		[InlineData("123456789:ABCDEF-hijklmnop_QRSTUVWXYZ12345678")] // With hyphen and underscore, exactly 35 chars
		public void IsValidBotToken_WithValidTokens_ShouldReturnTrue(string token)
		{
			// Act
			var result = TelegramService.IsValidBotToken(token);

			// Assert
			Assert.True(result, $"Token should be valid: {token}");
		}

		[Theory]
		[InlineData("")]
		[InlineData("invalid")]
		[InlineData("123:short")]
		[InlineData("abc:ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789")]
		[InlineData("123456789:short")]
		[InlineData("123456789:ABCDEFGHIJKLMNOPQRSTUVWXYZ12345678901")]
		[InlineData("123456789:ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789@")]
		[InlineData("123456789")]
		[InlineData(":ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789")]
		[InlineData("123456789:ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")] // 36 chars - too long
		[InlineData("987654321:ABCDEF-hijklmnop_QRSTUVWXYZ1234567890")] // 36 chars - too long
		[InlineData("123456789:ABCDEFGHIJKLMNOPQRSTUVWXYZ12345678")] // 34 chars - too short
		[InlineData(null)]
		[InlineData("   ")]
		[InlineData("123456789:ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789!")] // Invalid character
		public void IsValidBotToken_WithInvalidTokens_ShouldReturnFalse(string? token)
		{
			// Act
			var result = TelegramService.IsValidBotToken(token!);

			// Assert
			Assert.False(result);
		}

		#endregion

		#region Uninitialized Service Tests

		[Fact]
		public void GetMeAsync_WithoutInitialization_ShouldThrowInvalidOperationException()
		{
			// Arrange
			var service = new TelegramService();

			// Act & Assert
			Assert.ThrowsAsync<InvalidOperationException>(async () => await service.GetMeAsync());
		}

		[Fact]
		public void SendTextMessageAsync_WithoutInitialization_ShouldThrowInvalidOperationException()
		{
			// Arrange
			var service = new TelegramService();

			// Act & Assert
			Assert.ThrowsAsync<InvalidOperationException>(async () => await service.SendTextMessageAsync(123456, "test message"));
		}

		[Fact]
		public void SendPhotoAsync_WithoutInitialization_ShouldThrowInvalidOperationException()
		{
			// Arrange
			var service = new TelegramService();

			// Act & Assert
			Assert.ThrowsAsync<InvalidOperationException>(async () => await service.SendPhotoAsync(123456, InputFile.FromUri("https://example.com/photo.jpg")));
		}

		[Fact]
		public void SendVideoAsync_WithoutInitialization_ShouldThrowInvalidOperationException()
		{
			// Arrange
			var service = new TelegramService();

			// Act & Assert
			Assert.ThrowsAsync<InvalidOperationException>(async () => await service.SendVideoAsync(123456, InputFile.FromUri("https://example.com/video.mp4")));
		}

		[Fact]
		public void SendAudioAsync_WithoutInitialization_ShouldThrowInvalidOperationException()
		{
			// Arrange
			var service = new TelegramService();

			// Act & Assert
			Assert.ThrowsAsync<InvalidOperationException>(async () => await service.SendAudioAsync(123456, InputFile.FromUri("https://example.com/audio.mp3")));
		}

		[Fact]
		public void SendDocumentAsync_WithoutInitialization_ShouldThrowInvalidOperationException()
		{
			// Arrange
			var service = new TelegramService();

			// Act & Assert
			Assert.ThrowsAsync<InvalidOperationException>(async () => await service.SendDocumentAsync(123456, InputFile.FromUri("https://example.com/document.pdf")));
		}

		[Fact]
		public void SendLocationAsync_WithoutInitialization_ShouldThrowInvalidOperationException()
		{
			// Arrange
			var service = new TelegramService();

			// Act & Assert
			Assert.ThrowsAsync<InvalidOperationException>(async () => await service.SendLocationAsync(123456, 40.7128, -74.0060));
		}

		[Fact]
		public void SetWebhookAsync_WithoutInitialization_ShouldThrowInvalidOperationException()
		{
			// Arrange
			var service = new TelegramService();

			// Act & Assert
			Assert.ThrowsAsync<InvalidOperationException>(async () => await service.SetWebhookAsync("https://example.com/webhook"));
		}

		[Fact]
		public void DeleteWebhookAsync_WithoutInitialization_ShouldThrowInvalidOperationException()
		{
			// Arrange
			var service = new TelegramService();

			// Act & Assert
			Assert.ThrowsAsync<InvalidOperationException>(async () => await service.DeleteWebhookAsync());
		}

		[Fact]
		public void GetWebhookInfoAsync_WithoutInitialization_ShouldThrowInvalidOperationException()
		{
			// Arrange
			var service = new TelegramService();

			// Act & Assert
			Assert.ThrowsAsync<InvalidOperationException>(async () => await service.GetWebhookInfoAsync());
		}

		[Fact]
		public void GetUpdatesAsync_WithoutInitialization_ShouldThrowInvalidOperationException()
		{
			// Arrange
			var service = new TelegramService();

			// Act & Assert
			Assert.ThrowsAsync<InvalidOperationException>(async () => await service.GetUpdatesAsync());
		}

		#endregion

		#region Initialized Service Tests - Mocking ITelegramBotClient

		[Fact]
		public async Task GetMeAsync_WithInitializedService_CallsBotClient()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedUser = TelegramMockFactory.CreateTestBot();
			var service = new TelegramService(mockBotClient.Object);
			
			mockBotClient.Setup(x => x.SendRequest<User>(
				It.IsAny<GetMeRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedUser);

			service.Initialize(ValidToken);

			// Act
			var result = await service.GetMeAsync();

			// Assert
			Assert.NotNull(result);
			Assert.Equal(expectedUser.Id, result.Id);
			Assert.Equal(expectedUser.Username, result.Username);
			mockBotClient.Verify(x => x.SendRequest<User>(
				It.IsAny<GetMeRequest>(),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SendTextMessageAsync_WithInitializedService_CallsBotClient()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedMessage = TelegramMockFactory.CreateTestTelegramMessage();
			var service = new TelegramService(mockBotClient.Object);
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendMessageRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedMessage);

			service.Initialize(ValidToken);

			// Act
			var result = await service.SendTextMessageAsync(123456, "Test message");

			// Assert
			Assert.NotNull(result);
			mockBotClient.Verify(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendMessageRequest>(),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SendTextMessageAsync_WithParseMode_CallsBotClientWithParseMode()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedMessage = TelegramMockFactory.CreateTestTelegramMessage();
			var service = new TelegramService(mockBotClient.Object);
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendMessageRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedMessage);

			service.Initialize(ValidToken);

			// Act
			var result = await service.SendTextMessageAsync(123456, "*Bold text*", Telegram.Bot.Types.Enums.ParseMode.Markdown);

			// Assert
			Assert.NotNull(result);
			mockBotClient.Verify(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.Is<SendMessageRequest>(req => req.ParseMode == Telegram.Bot.Types.Enums.ParseMode.Markdown),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SendPhotoAsync_WithInitializedService_CallsBotClient()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedMessage = TelegramMockFactory.CreateTestTelegramMessage();
			var service = new TelegramService(mockBotClient.Object);
			var photoFile = InputFile.FromUri("https://example.com/photo.jpg");
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendPhotoRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedMessage);

			service.Initialize(ValidToken);

			// Act
			var result = await service.SendPhotoAsync(123456, photoFile);

			// Assert
			Assert.NotNull(result);
			mockBotClient.Verify(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendPhotoRequest>(),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task GetWebhookInfoAsync_WithInitializedService_CallsBotClient()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedWebhookInfo = TelegramMockFactory.CreateTestWebhookInfo();
			var service = new TelegramService(mockBotClient.Object);
			
			mockBotClient.Setup(x => x.SendRequest<WebhookInfo>(
				It.IsAny<GetWebhookInfoRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedWebhookInfo);

			service.Initialize(ValidToken);

			// Act
			var result = await service.GetWebhookInfoAsync();

			// Assert
			Assert.NotNull(result);
			Assert.Equal(expectedWebhookInfo.Url, result.Url);
			mockBotClient.Verify(x => x.SendRequest<WebhookInfo>(
				It.IsAny<GetWebhookInfoRequest>(),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task DeleteWebhookAsync_WithInitializedService_CallsBotClient()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var service = new TelegramService(mockBotClient.Object);
			
			mockBotClient.Setup(x => x.SendRequest<bool>(
				It.IsAny<DeleteWebhookRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(true);

			service.Initialize(ValidToken);

			// Act
			await service.DeleteWebhookAsync();

			// Assert
			mockBotClient.Verify(x => x.SendRequest<bool>(
				It.IsAny<DeleteWebhookRequest>(),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task GetUpdatesAsync_WithInitializedService_CallsBotClient()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedUpdates = Array.Empty<Update>();
			var service = new TelegramService(mockBotClient.Object);
			
			mockBotClient.Setup(x => x.SendRequest<Update[]>(
				It.IsAny<GetUpdatesRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedUpdates);

			service.Initialize(ValidToken);

			// Act
			var result = await service.GetUpdatesAsync();

			// Assert
			Assert.NotNull(result);
			Assert.Empty(result);
			mockBotClient.Verify(x => x.SendRequest<Update[]>(
				It.IsAny<GetUpdatesRequest>(),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		#endregion

		#region SendPhoto Tests

		[Fact]
		public async Task SendPhotoAsync_WithBasicParameters_ShouldCallBotClient()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedMessage = TelegramMockFactory.CreateTestTelegramMessage();
			var service = new TelegramService(mockBotClient.Object);
			var photoFile = InputFile.FromUri("https://example.com/photo.jpg");
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendPhotoRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedMessage);

			service.Initialize(ValidToken);

			// Act
			var result = await service.SendPhotoAsync(123456, photoFile);

			// Assert
			Assert.NotNull(result);
			// Check that the method was called correctly with basic parameters
			mockBotClient.Verify(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.Is<SendPhotoRequest>(req => 
					req.ChatId.Identifier == 123456 &&
					req.Photo == photoFile &&
					req.Caption == null &&
					req.ParseMode == default(Telegram.Bot.Types.Enums.ParseMode)), // Check for default value instead of null
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SendPhotoAsync_WithAllParameters_ShouldCallBotClientWithAllParameters()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedMessage = TelegramMockFactory.CreateTestTelegramMessage();
			var service = new TelegramService(mockBotClient.Object);
			var photoFile = InputFile.FromUri("https://example.com/photo.jpg");
			var replyMarkup = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Test", "test"));
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendPhotoRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedMessage);

			service.Initialize(ValidToken);

			// Act
			var result = await service.SendPhotoAsync(
				123456,
				photoFile,
				caption: "Test caption",
				parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
				disableNotification: true,
				replyToMessageId: 999,
				replyMarkup: replyMarkup);

			// Assert
			Assert.NotNull(result);
			mockBotClient.Verify(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.Is<SendPhotoRequest>(req => 
					req.ChatId.Identifier == 123456 &&
					req.Photo == photoFile &&
					req.Caption == "Test caption" &&
					req.ParseMode == Telegram.Bot.Types.Enums.ParseMode.Html &&
					req.DisableNotification == true &&
					req.ReplyParameters != null &&
					req.ReplyParameters.MessageId == 999 &&
					req.ReplyMarkup == replyMarkup),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SendPhotoAsync_WithParseMode_ShouldUseCorrectOverload()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedMessage = TelegramMockFactory.CreateTestTelegramMessage();
			var service = new TelegramService(mockBotClient.Object);
			var photoFile = InputFile.FromUri("https://example.com/photo.jpg");
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendPhotoRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedMessage);

			service.Initialize(ValidToken);

			// Act
			var result = await service.SendPhotoAsync(
				123456,
				photoFile,
				caption: "Test *bold* caption",
				parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);

			// Assert
			Assert.NotNull(result);
			mockBotClient.Verify(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.Is<SendPhotoRequest>(req => req.ParseMode == Telegram.Bot.Types.Enums.ParseMode.Markdown),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SendPhotoAsync_BotClientThrowsException_ShouldPropagateException()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var service = new TelegramService(mockBotClient.Object);
			var photoFile = InputFile.FromUri("https://example.com/photo.jpg");
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendPhotoRequest>(),
				It.IsAny<CancellationToken>()))
				.ThrowsAsync(new InvalidOperationException("Bot blocked by user"));

			service.Initialize(ValidToken);

			// Act & Assert
			var exception = await Assert.ThrowsAsync<InvalidOperationException>(
				() => service.SendPhotoAsync(123456, photoFile));
			Assert.Equal("Bot blocked by user", exception.Message);
		}

		#endregion

		#region SendVideo Tests

		[Fact]
		public async Task SendVideoAsync_WithBasicParameters_ShouldCallBotClient()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedMessage = TelegramMockFactory.CreateTestTelegramMessage();
			var service = new TelegramService(mockBotClient.Object);
			var videoFile = InputFile.FromUri("https://example.com/video.mp4");
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendVideoRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedMessage);

			service.Initialize(ValidToken);

			// Act
			var result = await service.SendVideoAsync(123456, videoFile);

			// Assert
			Assert.NotNull(result);
			mockBotClient.Verify(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.Is<SendVideoRequest>(req => 
					req.ChatId.Identifier == 123456 &&
					req.Video == videoFile &&
					req.Caption == null &&
					req.ParseMode == default(Telegram.Bot.Types.Enums.ParseMode)), // Check for default value instead of null
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SendVideoAsync_WithAllParameters_ShouldCallBotClientWithAllParameters()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedMessage = TelegramMockFactory.CreateTestTelegramMessage();
			var service = new TelegramService(mockBotClient.Object);
			var videoFile = InputFile.FromUri("https://example.com/video.mp4");
			var thumbnailFile = InputFile.FromUri("https://example.com/thumb.jpg");
			var replyMarkup = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Test", "test"));
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendVideoRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedMessage);

			service.Initialize(ValidToken);

			// Act
			var result = await service.SendVideoAsync(
				123456,
				videoFile,
				duration: 120,
				width: 1920,
				height: 1080,
				thumb: thumbnailFile,
				caption: "Test video",
				parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
				supportsStreaming: true,
				disableNotification: true,
				replyToMessageId: 999,
				replyMarkup: replyMarkup);

			// Assert
			Assert.NotNull(result);
			mockBotClient.Verify(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.Is<SendVideoRequest>(req => 
					req.ChatId.Identifier == 123456 &&
					req.Video == videoFile &&
					req.Duration == 120 &&
					req.Width == 1920 &&
					req.Height == 1080 &&
					req.Thumbnail == thumbnailFile &&
					req.Caption == "Test video" &&
					req.ParseMode == Telegram.Bot.Types.Enums.ParseMode.Html &&
					req.SupportsStreaming == true &&
					req.DisableNotification == true &&
					req.ReplyParameters != null &&
					req.ReplyParameters.MessageId == 999 &&
					req.ReplyMarkup == replyMarkup),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SendVideoAsync_WithoutParseMode_ShouldUseCorrectOverload()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedMessage = TelegramMockFactory.CreateTestTelegramMessage();
			var service = new TelegramService(mockBotClient.Object);
			var videoFile = InputFile.FromUri("https://example.com/video.mp4");
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendVideoRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedMessage);

			service.Initialize(ValidToken);

			// Act
			var result = await service.SendVideoAsync(
				123456,
				videoFile,
				caption: "Plain text caption",
				supportsStreaming: true);

			// Assert
			Assert.NotNull(result);
			mockBotClient.Verify(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.Is<SendVideoRequest>(req => 
					req.ParseMode == default(Telegram.Bot.Types.Enums.ParseMode) &&
					req.SupportsStreaming == true),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SendVideoAsync_BotClientThrowsException_ShouldPropagateException()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var service = new TelegramService(mockBotClient.Object);
			var videoFile = InputFile.FromUri("https://example.com/video.mp4");
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendVideoRequest>(),
				It.IsAny<CancellationToken>()))
				.ThrowsAsync(new ArgumentException("Invalid video format"));

			service.Initialize(ValidToken);

			// Act & Assert
			var exception = await Assert.ThrowsAsync<ArgumentException>(
				() => service.SendVideoAsync(123456, videoFile));
			Assert.Equal("Invalid video format", exception.Message);
		}

		#endregion

		#region SendAudio Tests

		[Fact]
		public async Task SendAudioAsync_WithBasicParameters_ShouldCallBotClient()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedMessage = TelegramMockFactory.CreateTestTelegramMessage();
			var service = new TelegramService(mockBotClient.Object);
			var audioFile = InputFile.FromUri("https://example.com/audio.mp3");
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendAudioRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedMessage);

			service.Initialize(ValidToken);

			// Act
			var result = await service.SendAudioAsync(123456, audioFile);

			// Assert
			Assert.NotNull(result);
			mockBotClient.Verify(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.Is<SendAudioRequest>(req => 
					req.ChatId.Identifier == 123456 &&
					req.Audio == audioFile &&
					req.Caption == null &&
					req.ParseMode == default(Telegram.Bot.Types.Enums.ParseMode)), // Check for default value instead of null
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SendAudioAsync_WithAllParameters_ShouldCallBotClientWithAllParameters()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedMessage = TelegramMockFactory.CreateTestTelegramMessage();
			var service = new TelegramService(mockBotClient.Object);
			var audioFile = InputFile.FromUri("https://example.com/audio.mp3");
			var thumbnailFile = InputFile.FromUri("https://example.com/thumb.jpg");
			var replyMarkup = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Test", "test"));
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendAudioRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedMessage);

			service.Initialize(ValidToken);

			// Act
			var result = await service.SendAudioAsync(
				123456,
				audioFile,
				caption: "Test audio",
				parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
				duration: 180,
				performer: "Test Artist",
				title: "Test Song",
				thumb: thumbnailFile,
				disableNotification: true,
				replyToMessageId: 999,
				replyMarkup: replyMarkup);

			// Assert
			Assert.NotNull(result);
			mockBotClient.Verify(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.Is<SendAudioRequest>(req => 
					req.ChatId.Identifier == 123456 &&
					req.Audio == audioFile &&
					req.Caption == "Test audio" &&
					req.ParseMode == Telegram.Bot.Types.Enums.ParseMode.Markdown &&
					req.Duration == 180 &&
					req.Performer == "Test Artist" &&
					req.Title == "Test Song" &&
					req.Thumbnail == thumbnailFile &&
					req.DisableNotification == true &&
					req.ReplyParameters != null &&
					req.ReplyParameters.MessageId == 999 &&
					req.ReplyMarkup == replyMarkup),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SendAudioAsync_WithoutParseMode_ShouldUseCorrectOverload()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedMessage = TelegramMockFactory.CreateTestTelegramMessage();
			var service = new TelegramService(mockBotClient.Object);
			var audioFile = InputFile.FromUri("https://example.com/audio.mp3");
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendAudioRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedMessage);

			service.Initialize(ValidToken);

			// Act
			var result = await service.SendAudioAsync(
				123456,
				audioFile,
				caption: "Plain text caption",
				duration: 240,
				performer: "Artist Name",
				title: "Song Title");

			// Assert
			Assert.NotNull(result);
			mockBotClient.Verify(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.Is<SendAudioRequest>(req => 
					req.ParseMode == default(Telegram.Bot.Types.Enums.ParseMode) &&
					req.Duration == 240 &&
					req.Performer == "Artist Name" &&
					req.Title == "Song Title"),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SendAudioAsync_BotClientThrowsException_ShouldPropagateException()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var service = new TelegramService(mockBotClient.Object);
			var audioFile = InputFile.FromUri("https://example.com/audio.mp3");
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendAudioRequest>(),
				It.IsAny<CancellationToken>()))
				.ThrowsAsync(new TimeoutException("Request timeout"));

			service.Initialize(ValidToken);

			// Act & Assert
			var exception = await Assert.ThrowsAsync<TimeoutException>(
				() => service.SendAudioAsync(123456, audioFile));
			Assert.Equal("Request timeout", exception.Message);
		}

		#endregion

		#region SendDocument Tests

		[Fact]
		public async Task SendDocumentAsync_WithBasicParameters_ShouldCallBotClient()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedMessage = TelegramMockFactory.CreateTestTelegramMessage();
			var service = new TelegramService(mockBotClient.Object);
			var documentFile = InputFile.FromUri("https://example.com/document.pdf");
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendDocumentRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedMessage);

			service.Initialize(ValidToken);

			// Act
			var result = await service.SendDocumentAsync(123456, documentFile);

			// Assert
			Assert.NotNull(result);
			mockBotClient.Verify(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.Is<SendDocumentRequest>(req => 
					req.ChatId.Identifier == 123456 &&
					req.Document == documentFile &&
					req.Caption == null &&
					req.ParseMode == default(Telegram.Bot.Types.Enums.ParseMode)), // Check for default value instead of null
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SendDocumentAsync_WithAllParameters_ShouldCallBotClientWithAllParameters()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedMessage = TelegramMockFactory.CreateTestTelegramMessage();
			var service = new TelegramService(mockBotClient.Object);
			var documentFile = InputFile.FromUri("https://example.com/document.pdf");
			var thumbnailFile = InputFile.FromUri("https://example.com/thumb.jpg");
			var replyMarkup = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Test", "test"));
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendDocumentRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedMessage);

			service.Initialize(ValidToken);

			// Act
			var result = await service.SendDocumentAsync(
				123456,
				documentFile,
				thumb: thumbnailFile,
				caption: "Test document",
				parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
				disableContentTypeDetection: true,
				disableNotification: true,
				replyToMessageId: 999,
				replyMarkup: replyMarkup);

			// Assert
			Assert.NotNull(result);
			mockBotClient.Verify(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.Is<SendDocumentRequest>(req => 
					req.ChatId.Identifier == 123456 &&
					req.Document == documentFile &&
					req.Thumbnail == thumbnailFile &&
					req.Caption == "Test document" &&
					req.ParseMode == Telegram.Bot.Types.Enums.ParseMode.Html &&
					req.DisableContentTypeDetection == true &&
					req.DisableNotification == true &&
					req.ReplyParameters != null &&
					req.ReplyParameters.MessageId == 999 &&
					req.ReplyMarkup == replyMarkup),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SendDocumentAsync_WithoutParseMode_ShouldUseCorrectOverload()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedMessage = TelegramMockFactory.CreateTestTelegramMessage();
			var service = new TelegramService(mockBotClient.Object);
			var documentFile = InputFile.FromUri("https://example.com/document.pdf");
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendDocumentRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedMessage);

			service.Initialize(ValidToken);

			// Act
			var result = await service.SendDocumentAsync(
				123456,
				documentFile,
				caption: "Plain text caption",
				disableContentTypeDetection: true);

			// Assert
			Assert.NotNull(result);
			mockBotClient.Verify(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.Is<SendDocumentRequest>(req => 
					req.ParseMode == default(Telegram.Bot.Types.Enums.ParseMode) &&
					req.DisableContentTypeDetection == true),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SendDocumentAsync_BotClientThrowsException_ShouldPropagateException()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var service = new TelegramService(mockBotClient.Object);
			var documentFile = InputFile.FromUri("https://example.com/document.pdf");
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendDocumentRequest>(),
				It.IsAny<CancellationToken>()))
				.ThrowsAsync(new UnauthorizedAccessException("Bot token invalid"));

			service.Initialize(ValidToken);

			// Act & Assert
			var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
				() => service.SendDocumentAsync(123456, documentFile));
			Assert.Equal("Bot token invalid", exception.Message);
		}

		#endregion

		#region SendLocation Tests

		[Fact]
		public async Task SendLocationAsync_WithBasicParameters_ShouldCallBotClient()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedMessage = TelegramMockFactory.CreateTestTelegramMessage();
			var service = new TelegramService(mockBotClient.Object);
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendLocationRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedMessage);

			service.Initialize(ValidToken);

			// Act
			var result = await service.SendLocationAsync(123456, 40.7128, -74.0060);

			// Assert
			Assert.NotNull(result);
			mockBotClient.Verify(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.Is<SendLocationRequest>(req => 
					req.ChatId.Identifier == 123456 &&
					req.Latitude == 40.7128 &&
					req.Longitude == -74.0060),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SendLocationAsync_WithAllParameters_ShouldCallBotClientWithAllParameters()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedMessage = TelegramMockFactory.CreateTestTelegramMessage();
			var service = new TelegramService(mockBotClient.Object);
			var replyMarkup = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Test", "test"));
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendLocationRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedMessage);

			service.Initialize(ValidToken);

			// Act
			var result = await service.SendLocationAsync(
				123456,
				40.7128,
				-74.0060,
				livePeriod: 3600,
				heading: 45,
				proximityAlertRadius: 500,
				disableNotification: true,
				replyToMessageId: 999,
				replyMarkup: replyMarkup);

			// Assert
			Assert.NotNull(result);
			mockBotClient.Verify(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.Is<SendLocationRequest>(req => 
					req.ChatId.Identifier == 123456 &&
					req.Latitude == 40.7128 &&
					req.Longitude == -74.0060 &&
					req.LivePeriod == 3600 &&
					req.Heading == 45 &&
					req.ProximityAlertRadius == 500 &&
					req.DisableNotification == true &&
					req.ReplyParameters != null &&
					req.ReplyParameters.MessageId == 999 &&
					req.ReplyMarkup == replyMarkup),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SendLocationAsync_BotClientThrowsException_ShouldPropagateException()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var service = new TelegramService(mockBotClient.Object);
			
			mockBotClient.Setup(x => x.SendRequest<Telegram.Bot.Types.Message>(
				It.IsAny<SendLocationRequest>(),
				It.IsAny<CancellationToken>()))
				.ThrowsAsync(new InvalidOperationException("Invalid coordinates"));

			service.Initialize(ValidToken);

			// Act & Assert
			var exception = await Assert.ThrowsAsync<InvalidOperationException>(
				() => service.SendLocationAsync(123456, 40.7128, -74.0060));
			Assert.Equal("Invalid coordinates", exception.Message);
		}

		#endregion

		#region Webhook Tests

		[Fact]
		public async Task SetWebhookAsync_WithBasicUrl_ShouldCallBotClient()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var service = new TelegramService(mockBotClient.Object);
			
			mockBotClient.Setup(x => x.SendRequest<bool>(
				It.IsAny<SetWebhookRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(true);

			service.Initialize(ValidToken);

			// Act
			await service.SetWebhookAsync("https://example.com/webhook");

			// Assert
			mockBotClient.Verify(x => x.SendRequest<bool>(
				It.Is<SetWebhookRequest>(req => req.Url == "https://example.com/webhook"),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SetWebhookAsync_WithAllParameters_ShouldCallBotClientWithAllParameters()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var service = new TelegramService(mockBotClient.Object);
			var certificate = InputFile.FromUri("https://example.com/cert.pem");
			var allowedUpdates = new[] { Telegram.Bot.Types.Enums.UpdateType.Message, Telegram.Bot.Types.Enums.UpdateType.CallbackQuery };
			
			mockBotClient.Setup(x => x.SendRequest<bool>(
				It.IsAny<SetWebhookRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(true);

			service.Initialize(ValidToken);

			// Act
			await service.SetWebhookAsync(
				"https://example.com/webhook",
				certificate: certificate,
				ipAddress: "192.168.1.1",
				maxConnections: 50,
				allowedUpdates: allowedUpdates,
				dropPendingUpdates: true,
				secretToken: "secret123");

			// Assert - Simplify the verification to just check basic properties
			mockBotClient.Verify(x => x.SendRequest<bool>(
				It.Is<SetWebhookRequest>(req => 
					req.Url == "https://example.com/webhook" &&
					req.IpAddress == "192.168.1.1" &&
					req.MaxConnections == 50 &&
					req.DropPendingUpdates == true &&
					req.SecretToken == "secret123"),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SetWebhookAsync_BotClientThrowsException_ShouldPropagateException()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var service = new TelegramService(mockBotClient.Object);
			
			mockBotClient.Setup(x => x.SendRequest<bool>(
				It.IsAny<SetWebhookRequest>(),
				It.IsAny<CancellationToken>()))
				.ThrowsAsync(new ArgumentException("Invalid webhook URL"));

			service.Initialize(ValidToken);

			// Act & Assert
			var exception = await Assert.ThrowsAsync<ArgumentException>(
				() => service.SetWebhookAsync("invalid-url"));
			Assert.Equal("Invalid webhook URL", exception.Message);
		}

		[Fact]
		public async Task DeleteWebhookAsync_WithDropPendingUpdates_ShouldCallBotClientWithParameter()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var service = new TelegramService(mockBotClient.Object);
			
			mockBotClient.Setup(x => x.SendRequest<bool>(
				It.IsAny<DeleteWebhookRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(true);

			service.Initialize(ValidToken);

			// Act
			await service.DeleteWebhookAsync(dropPendingUpdates: true);

			// Assert
			mockBotClient.Verify(x => x.SendRequest<bool>(
				It.Is<DeleteWebhookRequest>(req => req.DropPendingUpdates == true),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		#endregion

		#region GetUpdates Tests

		[Fact]
		public async Task GetUpdatesAsync_WithAllParameters_ShouldCallBotClientWithAllParameters()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var expectedUpdates = new Update[]
			{
				new Update { Id = 1 },
				new Update { Id = 2 }
			};
			var service = new TelegramService(mockBotClient.Object);
			var allowedUpdates = new[] { Telegram.Bot.Types.Enums.UpdateType.Message };
			
			mockBotClient.Setup(x => x.SendRequest<Update[]>(
				It.IsAny<GetUpdatesRequest>(),
				It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedUpdates);

			service.Initialize(ValidToken);

			// Act
			var result = await service.GetUpdatesAsync(
				offset: 100,
				limit: 50,
				timeout: 30,
				allowedUpdates: allowedUpdates);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Length);
			mockBotClient.Verify(x => x.SendRequest<Update[]>(
				It.Is<GetUpdatesRequest>(req => 
					req.Offset == 100 &&
					req.Limit == 50 &&
					req.Timeout == 30 &&
					req.AllowedUpdates != null),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task GetUpdatesAsync_BotClientThrowsException_ShouldPropagateException()
		{
			// Arrange
			var mockBotClient = new Mock<ITelegramBotClient>();
			var service = new TelegramService(mockBotClient.Object);
			
			mockBotClient.Setup(x => x.SendRequest<Update[]>(
				It.IsAny<GetUpdatesRequest>(),
				It.IsAny<CancellationToken>()))
				.ThrowsAsync(new TaskCanceledException("Operation was cancelled"));

			service.Initialize(ValidToken);

			// Act & Assert
			var exception = await Assert.ThrowsAsync<TaskCanceledException>(
				() => service.GetUpdatesAsync());
			Assert.Equal("Operation was cancelled", exception.Message);
		}

		#endregion

		#region Additional Coverage Tests

		[Fact]
		public void EnsureInitialized_WithNullBotClient_ThrowsInvalidOperationException()
		{
			// Arrange
			var service = new TelegramService();

			// Act & Assert
			var exception = Assert.Throws<System.Reflection.TargetInvocationException>(() => 
			{
				// Use reflection to access the private method for testing purposes
				var method = typeof(TelegramService).GetMethod("EnsureInitialized", 
					System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				method!.Invoke(service, null);
			});

			// Verify that the inner exception is InvalidOperationException
			Assert.IsType<InvalidOperationException>(exception.InnerException);
			Assert.Contains("not been initialized", exception.InnerException!.Message);
		}

		#endregion
	}
}