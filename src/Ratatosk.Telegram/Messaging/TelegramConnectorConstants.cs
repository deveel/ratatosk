//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
	/// <summary>
	/// Provides constant values for the Telegram connector implementation.
	/// </summary>
	public static class TelegramConnectorConstants
	{
		/// <summary>
		/// The provider name for Telegram connectors.
		/// </summary>
		public const string Provider = "telegram";

		/// <summary>
		/// The channel type for Telegram Bot API messaging.
		/// </summary>
		public const string BotChannel = "bot";

		/// <summary>
		/// The base URL for the Telegram Bot API.
		/// </summary>
		public const string BotApiBaseUrl = "https://api.telegram.org";

		/// <summary>
		/// The maximum length for Telegram message text content.
		/// </summary>
		public const int MaxMessageLength = 4096;

		/// <summary>
		/// The maximum length for photo captions in Telegram.
		/// </summary>
		public const int MaxCaptionLength = 1024;

		/// <summary>
		/// The maximum file size for document uploads (50MB).
		/// </summary>
		public const long MaxDocumentSize = 52428800; // 50MB

		/// <summary>
		/// The maximum file size for photo uploads (10MB).
		/// </summary>
		public const long MaxPhotoSize = 10485760; // 10MB

		/// <summary>
		/// The maximum file size for video uploads (50MB).
		/// </summary>
		public const long MaxVideoSize = 52428800; // 50MB

		/// <summary>
		/// The maximum file size for audio uploads (50MB).
		/// </summary>
		public const long MaxAudioSize = 52428800; // 50MB

		/// <summary>
		/// The maximum number of inline keyboard buttons per row.
		/// </summary>
		public const int MaxInlineKeyboardButtonsPerRow = 8;

		/// <summary>
		/// The maximum number of inline keyboard rows.
		/// </summary>
		public const int MaxInlineKeyboardRows = 100;

		/// <summary>
		/// The maximum number of quick reply buttons.
		/// </summary>
		public const int MaxQuickReplies = 12;
	}
}