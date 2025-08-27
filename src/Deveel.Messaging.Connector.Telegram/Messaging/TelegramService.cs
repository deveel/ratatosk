//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Deveel.Messaging
{
	/// <summary>
	/// Implements Telegram Bot API operations using the official Telegram.Bot library.
	/// </summary>
	/// <remarks>
	/// This implementation provides a wrapper around the Telegram.Bot client to enable
	/// dependency injection, unit testing, and consistent error handling.
	/// </remarks>
	public class TelegramService : ITelegramService
	{
		private ITelegramBotClient? _botClient;
		private string? _botToken;

		/// <summary>
		/// Initializes a new instance of the <see cref="TelegramService"/> class.
		/// </summary>
		/// <param name="botClient">Optional bot client for dependency injection. If null, a new one will be created.</param>
		public TelegramService(ITelegramBotClient? botClient = null)
		{
			_botClient = botClient;
		}

		/// <inheritdoc/>
		public void Initialize(string botToken)
		{
			if (string.IsNullOrWhiteSpace(botToken))
				throw new ArgumentNullException(nameof(botToken), "Bot token cannot be null or empty");

			// Validate Telegram bot token format
			if (!IsValidBotToken(botToken))
				throw new ArgumentException("Invalid bot token format", nameof(botToken));

			_botToken = botToken;
			_botClient ??= new TelegramBotClient(botToken);
		}

		/// <inheritdoc/>
		public async Task<User> GetMeAsync(CancellationToken cancellationToken = default)
		{
			EnsureInitialized();
			return await _botClient!.GetMe(cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<Telegram.Bot.Types.Message> SendTextMessageAsync(
			ChatId chatId,
			string text,
			Telegram.Bot.Types.Enums.ParseMode? parseMode = null,
			bool? disableWebPagePreview = null,
			bool? disableNotification = null,
			int? replyToMessageId = null,
			IReplyMarkup? replyMarkup = null,
			CancellationToken cancellationToken = default)
		{
			EnsureInitialized();
			
			if (parseMode.HasValue)
			{
				return await _botClient!.SendTextMessageAsync(
					chatId,
					text,
					parseMode: parseMode.Value,
					linkPreviewOptions: disableWebPagePreview == true ? new LinkPreviewOptions { IsDisabled = true } : null,
					disableNotification: disableNotification ?? false,
					replyParameters: replyToMessageId.HasValue ? new ReplyParameters { MessageId = replyToMessageId.Value } : null,
					replyMarkup: replyMarkup,
					cancellationToken: cancellationToken);
			}
			else
			{
				return await _botClient!.SendTextMessageAsync(
					chatId,
					text,
					linkPreviewOptions: disableWebPagePreview == true ? new LinkPreviewOptions { IsDisabled = true } : null,
					disableNotification: disableNotification ?? false,
					replyParameters: replyToMessageId.HasValue ? new ReplyParameters { MessageId = replyToMessageId.Value } : null,
					replyMarkup: replyMarkup,
					cancellationToken: cancellationToken);
			}
		}

		/// <inheritdoc/>
		public async Task<Telegram.Bot.Types.Message> SendPhotoAsync(
			ChatId chatId,
			InputFile photo,
			string? caption = null,
			Telegram.Bot.Types.Enums.ParseMode? parseMode = null,
			bool? disableNotification = null,
			int? replyToMessageId = null,
			IReplyMarkup? replyMarkup = null,
			CancellationToken cancellationToken = default)
		{
			EnsureInitialized();
			
			if (parseMode.HasValue)
			{
				return await _botClient!.SendPhotoAsync(
					chatId,
					photo,
					caption: caption,
					parseMode: parseMode.Value,
					disableNotification: disableNotification ?? false,
					replyParameters: replyToMessageId.HasValue ? new ReplyParameters { MessageId = replyToMessageId.Value } : null,
					replyMarkup: replyMarkup,
					cancellationToken: cancellationToken);
			}
			else
			{
				return await _botClient!.SendPhotoAsync(
					chatId,
					photo,
					caption: caption,
					disableNotification: disableNotification ?? false,
					replyParameters: replyToMessageId.HasValue ? new ReplyParameters { MessageId = replyToMessageId.Value } : null,
					replyMarkup: replyMarkup,
					cancellationToken: cancellationToken);
			}
		}

		/// <inheritdoc/>
		public async Task<Telegram.Bot.Types.Message> SendVideoAsync(
			ChatId chatId,
			InputFile video,
			int? duration = null,
			int? width = null,
			int? height = null,
			InputFile? thumb = null,
			string? caption = null,
			Telegram.Bot.Types.Enums.ParseMode? parseMode = null,
			bool? supportsStreaming = null,
			bool? disableNotification = null,
			int? replyToMessageId = null,
			IReplyMarkup? replyMarkup = null,
			CancellationToken cancellationToken = default)
		{
			EnsureInitialized();
			
			if (parseMode.HasValue)
			{
				return await _botClient!.SendVideoAsync(
					chatId,
					video,
					duration: duration,
					width: width,
					height: height,
					thumbnail: thumb,
					caption: caption,
					parseMode: parseMode.Value,
					supportsStreaming: supportsStreaming ?? false,
					disableNotification: disableNotification ?? false,
					replyParameters: replyToMessageId.HasValue ? new ReplyParameters { MessageId = replyToMessageId.Value } : null,
					replyMarkup: replyMarkup,
					cancellationToken: cancellationToken);
			}
			else
			{
				return await _botClient!.SendVideoAsync(
					chatId,
					video,
					duration: duration,
					width: width,
					height: height,
					thumbnail: thumb,
					caption: caption,
					supportsStreaming: supportsStreaming ?? false,
					disableNotification: disableNotification ?? false,
					replyParameters: replyToMessageId.HasValue ? new ReplyParameters { MessageId = replyToMessageId.Value } : null,
					replyMarkup: replyMarkup,
					cancellationToken: cancellationToken);
			}
		}

		/// <inheritdoc/>
		public async Task<Telegram.Bot.Types.Message> SendAudioAsync(
			ChatId chatId,
			InputFile audio,
			string? caption = null,
			Telegram.Bot.Types.Enums.ParseMode? parseMode = null,
			int? duration = null,
			string? performer = null,
			string? title = null,
			InputFile? thumb = null,
			bool? disableNotification = null,
			int? replyToMessageId = null,
			IReplyMarkup? replyMarkup = null,
			CancellationToken cancellationToken = default)
		{
			EnsureInitialized();
			
			if (parseMode.HasValue)
			{
				return await _botClient!.SendAudioAsync(
					chatId,
					audio,
					caption: caption,
					parseMode: parseMode.Value,
					duration: duration,
					performer: performer,
					title: title,
					thumbnail: thumb,
					disableNotification: disableNotification ?? false,
					replyParameters: replyToMessageId.HasValue ? new ReplyParameters { MessageId = replyToMessageId.Value } : null,
					replyMarkup: replyMarkup,
					cancellationToken: cancellationToken);
			}
			else
			{
				return await _botClient!.SendAudioAsync(
					chatId,
					audio,
					caption: caption,
					duration: duration,
					performer: performer,
					title: title,
					thumbnail: thumb,
					disableNotification: disableNotification ?? false,
					replyParameters: replyToMessageId.HasValue ? new ReplyParameters { MessageId = replyToMessageId.Value } : null,
					replyMarkup: replyMarkup,
					cancellationToken: cancellationToken);
			}
		}

		/// <inheritdoc/>
		public async Task<Telegram.Bot.Types.Message> SendDocumentAsync(
			ChatId chatId,
			InputFile document,
			InputFile? thumb = null,
			string? caption = null,
			Telegram.Bot.Types.Enums.ParseMode? parseMode = null,
			bool? disableContentTypeDetection = null,
			bool? disableNotification = null,
			int? replyToMessageId = null,
			IReplyMarkup? replyMarkup = null,
			CancellationToken cancellationToken = default)
		{
			EnsureInitialized();
			
			if (parseMode.HasValue)
			{
				return await _botClient!.SendDocumentAsync(
					chatId,
					document,
					thumbnail: thumb,
					caption: caption,
					parseMode: parseMode.Value,
					disableContentTypeDetection: disableContentTypeDetection ?? false,
					disableNotification: disableNotification ?? false,
					replyParameters: replyToMessageId.HasValue ? new ReplyParameters { MessageId = replyToMessageId.Value } : null,
					replyMarkup: replyMarkup,
					cancellationToken: cancellationToken);
			}
			else
			{
				return await _botClient!.SendDocumentAsync(
					chatId,
					document,
					thumbnail: thumb,
					caption: caption,
					disableContentTypeDetection: disableContentTypeDetection ?? false,
					disableNotification: disableNotification ?? false,
					replyParameters: replyToMessageId.HasValue ? new ReplyParameters { MessageId = replyToMessageId.Value } : null,
					replyMarkup: replyMarkup,
					cancellationToken: cancellationToken);
			}
		}

		/// <inheritdoc/>
		public async Task<Telegram.Bot.Types.Message> SendLocationAsync(
			ChatId chatId,
			double latitude,
			double longitude,
			int? livePeriod = null,
			int? heading = null,
			int? proximityAlertRadius = null,
			bool? disableNotification = null,
			int? replyToMessageId = null,
			IReplyMarkup? replyMarkup = null,
			CancellationToken cancellationToken = default)
		{
			EnsureInitialized();
			
			return await _botClient!.SendLocation(
				chatId,
				latitude,
				longitude,
				livePeriod: livePeriod,
				heading: heading,
				proximityAlertRadius: proximityAlertRadius,
				disableNotification: disableNotification ?? false,
				replyParameters: replyToMessageId.HasValue ? new ReplyParameters { MessageId = replyToMessageId.Value } : null,
				replyMarkup: replyMarkup,
				cancellationToken: cancellationToken);
		}

		/// <inheritdoc/>
		public async Task SetWebhookAsync(
			string url,
			InputFile? certificate = null,
			string? ipAddress = null,
			int? maxConnections = null,
			IEnumerable<Telegram.Bot.Types.Enums.UpdateType>? allowedUpdates = null,
			bool? dropPendingUpdates = null,
			string? secretToken = null,
			CancellationToken cancellationToken = default)
		{
			EnsureInitialized();
			
			await _botClient!.SetWebhook(
				url,
				certificate: certificate as InputFileStream,
				ipAddress: ipAddress,
				maxConnections: maxConnections,
				allowedUpdates: allowedUpdates,
				dropPendingUpdates: dropPendingUpdates ?? false,
				secretToken: secretToken,
				cancellationToken: cancellationToken);
		}

		/// <inheritdoc/>
		public async Task DeleteWebhookAsync(bool? dropPendingUpdates = null, CancellationToken cancellationToken = default)
		{
			EnsureInitialized();
			await _botClient!.DeleteWebhook(dropPendingUpdates ?? false, cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<WebhookInfo> GetWebhookInfoAsync(CancellationToken cancellationToken = default)
		{
			EnsureInitialized();
			return await _botClient!.GetWebhookInfo(cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<Update[]> GetUpdatesAsync(
			int? offset = null,
			int? limit = null,
			int? timeout = null,
			IEnumerable<Telegram.Bot.Types.Enums.UpdateType>? allowedUpdates = null,
			CancellationToken cancellationToken = default)
		{
			EnsureInitialized();
			
			return await _botClient!.GetUpdates(
				offset: offset,
				limit: limit,
				timeout: timeout,
				allowedUpdates: allowedUpdates,
				cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Validates if the provided token follows the Telegram bot token format.
		/// </summary>
		/// <param name="token">The token to validate.</param>
		/// <returns>True if the token format is valid, false otherwise.</returns>
		internal static bool IsValidBotToken(string token)
		{
			if (string.IsNullOrWhiteSpace(token))
				return false;

			// Telegram bot tokens have the format: <bot_id>:<auth_token>
			// bot_id is a numeric ID, auth_token is a 35-character alphanumeric string
			var parts = token.Split(':');
			if (parts.Length != 2)
				return false;

			// Check if the first part is numeric (bot ID)
			if (!long.TryParse(parts[0], out _))
				return false;

			// Check if the second part has the correct length and contains only valid characters
			var authToken = parts[1];
			if (authToken.Length != 35)
				return false;

			// Telegram auth tokens contain letters, numbers, hyphens, and underscores
			return authToken.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
		}

		/// <summary>
		/// Ensures that the service has been initialized with a bot token.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when the service has not been initialized.</exception>
		private void EnsureInitialized()
		{
			if (_botClient == null || string.IsNullOrEmpty(_botToken))
				throw new InvalidOperationException("Telegram service has not been initialized with a bot token.");
		}
	}
}