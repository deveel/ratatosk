//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
	/// <summary>
	/// Tests for Telegram-specific error codes and constants.
	/// </summary>
	[Trait("Category", "Unit")]
	[Trait("Layer", "Infrastructure")]
	[Trait("Feature", "TelegramErrorCodes")]
	public class TelegramErrorCodesTests
	{
		[Fact]
		public void Should_HaveExpectedValues_When_TelegramErrorCodes()
		{
			// Assert that all expected error codes are defined
			Assert.NotNull(TelegramErrorCodes.MissingBotToken);
			Assert.NotNull(TelegramErrorCodes.InvalidBotToken);
			Assert.NotNull(TelegramErrorCodes.InvalidChatId);
			Assert.NotNull(TelegramErrorCodes.InvalidWebhookData);
			Assert.NotNull(TelegramErrorCodes.UnsupportedContentType);
			Assert.NotNull(TelegramErrorCodes.MessageTooLong);
			Assert.NotNull(TelegramErrorCodes.FileTooLarge);
			Assert.NotNull(TelegramErrorCodes.InvalidMediaUrl);
			Assert.NotNull(TelegramErrorCodes.BotBlocked);
			Assert.NotNull(TelegramErrorCodes.ChatNotFound);
			Assert.NotNull(TelegramErrorCodes.Unauthorized);
		}

		[Fact]
		public void Should_HaveUniqueValues_When_TelegramErrorCodes()
		{
			// Get all error code values
			var errorCodes = new[]
			{
				TelegramErrorCodes.MissingBotToken,
				TelegramErrorCodes.InvalidBotToken,
				TelegramErrorCodes.InvalidChatId,
				TelegramErrorCodes.InvalidWebhookData,
				TelegramErrorCodes.UnsupportedContentType,
				TelegramErrorCodes.MessageTooLong,
				TelegramErrorCodes.FileTooLarge,
				TelegramErrorCodes.InvalidMediaUrl,
				TelegramErrorCodes.BotBlocked,
				TelegramErrorCodes.ChatNotFound,
				TelegramErrorCodes.Unauthorized
			};

			// Assert all values are unique
			var uniqueErrorCodes = errorCodes.Distinct().ToArray();
			Assert.Equal(errorCodes.Length, uniqueErrorCodes.Length);
		}

		[Fact]
		public void Should_HaveExpectedValues_When_TelegramConnectorConstants()
		{
			// Assert constants have reasonable values
			Assert.True(TelegramConnectorConstants.MaxMessageLength > 0);
			Assert.True(TelegramConnectorConstants.MaxCaptionLength > 0);
			Assert.True(TelegramConnectorConstants.MaxInlineKeyboardRows > 0);
			Assert.True(TelegramConnectorConstants.MaxInlineKeyboardButtonsPerRow > 0);

			// Assert reasonable limits based on Telegram documentation
			Assert.Equal(4096, TelegramConnectorConstants.MaxMessageLength);
			Assert.Equal(1024, TelegramConnectorConstants.MaxCaptionLength);
		}

		[Theory]
		[InlineData("MISSING_BOT_TOKEN")]
		[InlineData("INVALID_BOT_TOKEN")]
		[InlineData("INVALID_CHAT_ID")]
		[InlineData("INVALID_WEBHOOK_DATA")]
		[InlineData("UNSUPPORTED_CONTENT_TYPE")]
		[InlineData("MESSAGE_TOO_LONG")]
		[InlineData("FILE_TOO_LARGE")]
		[InlineData("INVALID_MEDIA_URL")]
		[InlineData("BOT_BLOCKED")]
		[InlineData("CHAT_NOT_FOUND")]
		[InlineData("UNAUTHORIZED")]
		public void Should_ContainExpectedErrorCode_When_TelegramErrorCodes(string expectedCode)
		{
			// Use reflection to get all error code constants
			var errorCodesType = typeof(TelegramErrorCodes);
			var fields = errorCodesType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
			var errorCodeValues = fields.Where(f => f.IsLiteral && !f.IsInitOnly)
				.Select(f => f.GetValue(null)?.ToString()).ToArray();

			// Assert the expected code exists
			Assert.Contains(expectedCode, errorCodeValues);
		}

		[Fact]
		public void Should_FollowNamingConvention_When_TelegramErrorCodes()
		{
			// All error codes should be UPPER_CASE with underscores
			var errorCodesType = typeof(TelegramErrorCodes);
			var fields = errorCodesType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

			foreach (var field in fields.Where(f => f.IsLiteral && !f.IsInitOnly))
			{
				var value = field.GetValue(null)?.ToString();
				Assert.NotNull(value);
				Assert.Matches(@"^[A-Z_]+$", value);
				Assert.DoesNotMatch(@"__", value); // No double underscores
			}
		}

		[Fact]
		public void Should_NotBeNullOrEmpty_When_TelegramErrorCodes()
		{
			// Get all error code values and ensure none are null or empty
			var errorCodesType = typeof(TelegramErrorCodes);
			var fields = errorCodesType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

			foreach (var field in fields.Where(f => f.IsLiteral && !f.IsInitOnly))
			{
				var value = field.GetValue(null)?.ToString();
				Assert.False(string.IsNullOrEmpty(value), $"Error code '{field.Name}' should not be null or empty");
			}
		}
	}
}
