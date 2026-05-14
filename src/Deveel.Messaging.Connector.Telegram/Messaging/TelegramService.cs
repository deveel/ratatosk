//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Requests;

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
			try
			{
				return await _botClient!.SendRequest(new GetMeRequest(), cancellationToken);
			}
			catch (ApiRequestException ex)
			{
				throw new ConnectorException(
					MapTelegramErrorCode(ex.ErrorCode),
					TelegramErrorCodes.ErrorDomain,
					$"Telegram API error: {ex.Message}",
					ex);
			}
		}

		private static string MapTelegramErrorCode(int errorCode)
		{
			return errorCode switch
			{
				400 => MessagingErrorCodes.InvalidCredentials,
				401 => TelegramErrorCodes.Unauthorized,
				403 => TelegramErrorCodes.BotBlocked,
				404 => TelegramErrorCodes.ChatNotFound,
				429 => MessagingErrorCodes.UnsupportedContentType,
				_ => TelegramErrorCodes.Unauthorized
			};
		}

		private static string MapTelegramSendErrorCode(int errorCode)
		{
			return errorCode switch
			{
				400 => TelegramErrorCodes.InvalidChatId,
				403 => TelegramErrorCodes.BotBlocked,
				404 => TelegramErrorCodes.ChatNotFound,
				429 => MessagingErrorCodes.UnsupportedContentType,
				_ => MessagingErrorCodes.UnsupportedContentType
			};
		}

		private async Task<T> SendWithErrorHandlingAsync<T>(Func<Task<T>> apiCall, string errorCode, string operationDescription)
		{
			try
			{
				return await apiCall();
			}
			catch (ApiRequestException ex)
			{
				throw new ConnectorException(
					errorCode,
					TelegramErrorCodes.ErrorDomain,
					$"Telegram API error {operationDescription}: {ex.Message}",
					ex);
			}
		}

		private async Task SendWithErrorHandlingAsync(Func<Task> apiCall, string errorCode, string operationDescription)
		{
			try
			{
				await apiCall();
			}
			catch (ApiRequestException ex)
			{
				throw new ConnectorException(
					errorCode,
					TelegramErrorCodes.ErrorDomain,
					$"Telegram API error {operationDescription}: {ex.Message}",
					ex);
			}
		}

		private async Task<T> SendWithErrorHandlingAsync<T>(Func<Task<T>> apiCall, Func<int, string> mapErrorCode, string operationDescription)
		{
			try
			{
				return await apiCall();
			}
			catch (ApiRequestException ex)
			{
				throw new ConnectorException(
					mapErrorCode(ex.ErrorCode),
					TelegramErrorCodes.ErrorDomain,
					$"Telegram API error {operationDescription}: {ex.Message}",
					ex);
			}
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

			var request = new SendMessageRequest
			{
				ChatId = chatId,
				Text = text,
				LinkPreviewOptions = disableWebPagePreview == true ? new LinkPreviewOptions { IsDisabled = true } : null,
				DisableNotification = disableNotification ?? false,
				ReplyParameters = replyToMessageId.HasValue ? new ReplyParameters { MessageId = replyToMessageId.Value } : null,
				ReplyMarkup = replyMarkup
			};

			if (parseMode.HasValue)
				request.ParseMode = parseMode.Value;

			try
			{
				return await _botClient!.SendRequest(request, cancellationToken);
			}
			catch (ApiRequestException ex)
			{
				throw new ConnectorException(
					MapTelegramSendErrorCode(ex.ErrorCode),
					TelegramErrorCodes.ErrorDomain,
					$"Telegram API error sending text message: {ex.Message}",
					ex);
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

			var request = new SendPhotoRequest
			{
				ChatId = chatId,
				Photo = photo,
				Caption = caption,
				DisableNotification = disableNotification ?? false,
				ReplyParameters = replyToMessageId.HasValue ? new ReplyParameters { MessageId = replyToMessageId.Value } : null,
				ReplyMarkup = replyMarkup
			};

			if (parseMode.HasValue)
				request.ParseMode = parseMode.Value;

			return await SendWithErrorHandlingAsync(
				() => _botClient!.SendRequest(request, cancellationToken),
				MapTelegramSendErrorCode,
				"sending photo");
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

			var request = new SendVideoRequest
			{
				ChatId = chatId,
				Video = video,
				Duration = duration,
				Width = width,
				Height = height,
				Thumbnail = thumb,
				Caption = caption,
				SupportsStreaming = supportsStreaming ?? false,
				DisableNotification = disableNotification ?? false,
				ReplyParameters = replyToMessageId.HasValue ? new ReplyParameters { MessageId = replyToMessageId.Value } : null,
				ReplyMarkup = replyMarkup
			};

			if (parseMode.HasValue)
				request.ParseMode = parseMode.Value;

			return await SendWithErrorHandlingAsync(
				() => _botClient!.SendRequest(request, cancellationToken),
				MapTelegramSendErrorCode,
				"sending video");
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

			var request = new SendAudioRequest
			{
				ChatId = chatId,
				Audio = audio,
				Caption = caption,
				Duration = duration,
				Performer = performer,
				Title = title,
				Thumbnail = thumb,
				DisableNotification = disableNotification ?? false,
				ReplyParameters = replyToMessageId.HasValue ? new ReplyParameters { MessageId = replyToMessageId.Value } : null,
				ReplyMarkup = replyMarkup
			};

			if (parseMode.HasValue)
				request.ParseMode = parseMode.Value;

			return await SendWithErrorHandlingAsync(
				() => _botClient!.SendRequest(request, cancellationToken),
				MapTelegramSendErrorCode,
				"sending audio");
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

			var request = new SendDocumentRequest
			{
				ChatId = chatId,
				Document = document,
				Thumbnail = thumb,
				Caption = caption,
				DisableContentTypeDetection = disableContentTypeDetection ?? false,
				DisableNotification = disableNotification ?? false,
				ReplyParameters = replyToMessageId.HasValue ? new ReplyParameters { MessageId = replyToMessageId.Value } : null,
				ReplyMarkup = replyMarkup
			};

			if (parseMode.HasValue)
				request.ParseMode = parseMode.Value;

			return await SendWithErrorHandlingAsync(
				() => _botClient!.SendRequest(request, cancellationToken),
				MapTelegramSendErrorCode,
				"sending document");
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
			
			var request = new SendLocationRequest
			{
				ChatId = chatId,
				Latitude = latitude,
				Longitude = longitude,
				LivePeriod = livePeriod,
				Heading = heading,
				ProximityAlertRadius = proximityAlertRadius,
				DisableNotification = disableNotification ?? false,
				ReplyParameters = replyToMessageId.HasValue ? new ReplyParameters { MessageId = replyToMessageId.Value } : null,
				ReplyMarkup = replyMarkup
			};

			return await SendWithErrorHandlingAsync(
				() => _botClient!.SendRequest(request, cancellationToken),
				MapTelegramSendErrorCode,
				"sending location");
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
			
			var request = new SetWebhookRequest
			{
				Url = url,
				Certificate = certificate as InputFileStream,
				IpAddress = ipAddress,
				MaxConnections = maxConnections,
				AllowedUpdates = allowedUpdates,
				DropPendingUpdates = dropPendingUpdates ?? false,
				SecretToken = secretToken
			};

			await SendWithErrorHandlingAsync(
				() => _botClient!.SendRequest(request, cancellationToken),
				MessagingErrorCodes.UnsupportedContentType,
				"setting webhook");
		}

		/// <inheritdoc/>
		public async Task DeleteWebhookAsync(bool? dropPendingUpdates = null, CancellationToken cancellationToken = default)
		{
			EnsureInitialized();
			var request = new DeleteWebhookRequest
			{
				DropPendingUpdates = dropPendingUpdates ?? false
			};
			await SendWithErrorHandlingAsync(
				() => _botClient!.SendRequest(request, cancellationToken),
				MessagingErrorCodes.UnsupportedContentType,
				"deleting webhook");
		}

		/// <inheritdoc/>
		public async Task<WebhookInfo> GetWebhookInfoAsync(CancellationToken cancellationToken = default)
		{
			EnsureInitialized();
			return await SendWithErrorHandlingAsync(
				() => _botClient!.SendRequest(new GetWebhookInfoRequest(), cancellationToken),
				MessagingErrorCodes.UnsupportedContentType,
				"getting webhook info");
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
			
			var request = new GetUpdatesRequest
			{
				Offset = offset,
				Limit = limit,
				Timeout = timeout,
				AllowedUpdates = allowedUpdates
			};

			return await SendWithErrorHandlingAsync(
				() => _botClient!.SendRequest(request, cancellationToken),
				MessagingErrorCodes.UnsupportedContentType,
				"getting updates");
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