//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Telegram.Bot.Types;

namespace Deveel.Messaging
{
	/// <summary>
	/// Tests for the TelegramService class.
	/// </summary>
	public class TelegramServiceTests
	{
		[Fact]
		public void Initialize_WithValidToken_ShouldSucceed()
		{
			// Arrange
			var service = new TelegramService();
			// This is exactly 35 characters: ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789
			var validToken = "123456789:ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";

			// Act & Assert - Should not throw any exception
			var exception = Record.Exception(() => service.Initialize(validToken));
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

		[Theory]
		[InlineData("123456789:ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789")] // Exactly 35 chars
		[InlineData("987654321:abcdefghijklmnopqrstuvwxyz123456789")] // Exactly 35 chars, lowercase
		[InlineData("1:ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789")] // Exactly 35 chars
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
		public void IsValidBotToken_WithInvalidTokens_ShouldReturnFalse(string token)
		{
			// Act
			var result = TelegramService.IsValidBotToken(token);

			// Assert
			Assert.False(result);
		}

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
	}
}