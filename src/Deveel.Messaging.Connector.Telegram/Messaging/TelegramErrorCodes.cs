//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
	/// <summary>
	/// Provides specific error codes for Telegram connector operations.
	/// </summary>
	public static class TelegramErrorCodes
	{
		/// <summary>
		/// Error code for missing bot token.
		/// </summary>
		public const string MissingBotToken = "MISSING_BOT_TOKEN";

		/// <summary>
		/// Error code for invalid bot token format.
		/// </summary>
		public const string InvalidBotToken = "INVALID_BOT_TOKEN";

		/// <summary>
		/// Error code for invalid chat ID.
		/// </summary>
		public const string InvalidChatId = "INVALID_CHAT_ID";

		/// <summary>
		/// Error code for failed message sending.
		/// </summary>
		public const string SendMessageFailed = "SEND_MESSAGE_FAILED";

		/// <summary>
		/// Error code for failed connection testing.
		/// </summary>
		public const string ConnectionTestFailed = "CONNECTION_TEST_FAILED";

		/// <summary>
		/// Error code for unsupported content type.
		/// </summary>
		public const string UnsupportedContentType = "UNSUPPORTED_CONTENT_TYPE";

		/// <summary>
		/// Error code for invalid webhook data.
		/// </summary>
		public const string InvalidWebhookData = "INVALID_WEBHOOK_DATA";

		/// <summary>
		/// Error code for failed message receiving.
		/// </summary>
		public const string ReceiveMessageFailed = "RECEIVE_MESSAGE_FAILED";

		/// <summary>
		/// Error code for failed status query.
		/// </summary>
		public const string StatusQueryFailed = "STATUS_QUERY_FAILED";

		/// <summary>
		/// Error code for failed status receiving.
		/// </summary>
		public const string ReceiveStatusFailed = "RECEIVE_STATUS_FAILED";

		/// <summary>
		/// Error code for status error.
		/// </summary>
		public const string StatusError = "STATUS_ERROR";

		/// <summary>
		/// Error code for connection failure.
		/// </summary>
		public const string ConnectionFailed = "CONNECTION_FAILED";

		/// <summary>
		/// Error code for message text too long.
		/// </summary>
		public const string MessageTooLong = "MESSAGE_TOO_LONG";

		/// <summary>
		/// Error code for file too large.
		/// </summary>
		public const string FileTooLarge = "FILE_TOO_LARGE";

		/// <summary>
		/// Error code for invalid media URL.
		/// </summary>
		public const string InvalidMediaUrl = "INVALID_MEDIA_URL";

		/// <summary>
		/// Error code for bot blocked by user.
		/// </summary>
		public const string BotBlocked = "BOT_BLOCKED";

		/// <summary>
		/// Error code for chat not found.
		/// </summary>
		public const string ChatNotFound = "CHAT_NOT_FOUND";

		/// <summary>
		/// Error code for unauthorized access.
		/// </summary>
		public const string Unauthorized = "UNAUTHORIZED";
	}
}