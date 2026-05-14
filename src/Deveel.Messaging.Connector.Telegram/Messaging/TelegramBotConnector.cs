//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;

using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text.Json;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Deveel.Messaging
{
	/// <summary>
	/// A channel connector that implements Telegram Bot messaging using the Telegram Bot API.
	/// </summary>
	/// <remarks>
	/// This connector provides comprehensive support for Telegram Bot capabilities including
	/// sending messages, receiving messages via webhooks or long polling, media support,
	/// inline keyboards, and health monitoring.
	/// </remarks>
	[ChannelSchema(typeof(TelegramBotConnectorSchemaFactory))]
	public class TelegramBotConnector : ChannelConnectorBase
	{
		private readonly ITelegramService _telegramService;
		private readonly DateTime _startTime = DateTime.UtcNow;

		private string? _botToken;
		private string? _webhookUrl;
		private string? _secretToken;
		private bool _disableWebPagePreview;
		private bool _disableNotification;
		private string? _parseMode;
		private int _maxRetries = 3;
		private int _timeoutSeconds = 30;
		private User? _botInfo;

		/// <summary>
		/// Initializes a new instance of the <see cref="TelegramBotConnector"/> class.
		/// </summary>
		public TelegramBotConnector(IChannelSchema schema, ConnectionSettings connectionSettings, ITelegramService? telegramService = null, ILogger<TelegramBotConnector>? logger = null)
			: base(schema, connectionSettings, logger)
		{
            ArgumentNullException.ThrowIfNull(connectionSettings);
			_telegramService = telegramService ?? new TelegramService();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TelegramBotConnector"/> class using one of the predefined schemas.
		/// </summary>
		public TelegramBotConnector(ConnectionSettings connectionSettings, ITelegramService? telegramService = null, ILogger<TelegramBotConnector>? logger = null)
			: this(TelegramChannelSchemas.TelegramBot, connectionSettings, telegramService, logger)
		{
		}

        /// <inheritdoc/>
        protected override async ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
        {
            _botToken = AuthenticationCredential?.Value ?? ConnectionSettings.GetBotToken();
            _webhookUrl = ConnectionSettings.GetWebhookUrl();
            _secretToken = ConnectionSettings.GetSecretToken();
            _disableWebPagePreview = ConnectionSettings.GetDisableWebPagePreview() ?? TelegramConnectionSettingsDefaults.DisableWebPagePreview;
            _disableNotification = ConnectionSettings.GetDisableNotification() ?? TelegramConnectionSettingsDefaults.DisableNotification;
            _parseMode = ConnectionSettings.GetParseMode() ?? TelegramConnectionSettingsDefaults.ParseMode;
            _maxRetries = ConnectionSettings.GetMaxRetries() ?? TelegramConnectionSettingsDefaults.MaxRetries;
            _timeoutSeconds = ConnectionSettings.GetTimeoutSeconds() ?? 60;

            if (string.IsNullOrWhiteSpace(_botToken))
                throw new MessagingException(MessagingErrorCodes.MissingCredentials, TelegramErrorCodes.ErrorDomain, "Bot token is required for Telegram Bot API");

            _telegramService.Initialize(_botToken);
            _botInfo = await _telegramService.GetMeAsync(cancellationToken);
            Logger.LogBotInitialized(_botInfo.Username, _botInfo.Id);

            if (!string.IsNullOrWhiteSpace(_webhookUrl))
                await SetupWebhookAsync(cancellationToken);
        }

        /// <inheritdoc/>
        protected override async ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
        {
            var botInfo = await _telegramService.GetMeAsync(cancellationToken);

            if (botInfo == null)
                throw new ConnectorException(ConnectorErrorCodes.ConnectionTestError, TelegramErrorCodes.ErrorDomain, "Unable to retrieve bot information");

            Logger.LogBotConnectionTestSuccessful(botInfo.Username, botInfo.Id);
        }

        /// <inheritdoc/>
        protected override async Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
        {
            var chatId = TelegramMessageBuilder.ExtractChatId(message.Receiver);
            if (chatId == null)
                throw new ConnectorException(TelegramErrorCodes.InvalidChatId, TelegramErrorCodes.ErrorDomain, "Receiver must contain a valid Telegram chat ID");

            Telegram.Bot.Types.Message sentMessage = await SendMessageByContentType(message, chatId, cancellationToken);

            Logger.LogMessageSent(sentMessage.MessageId, sentMessage.Chat.Id);

            var result = new SendResult(message.Id, sentMessage.MessageId.ToString())
            {
                Status = MessageStatus.Sent,
                Timestamp = sentMessage.Date
            };

            result.AdditionalData["TelegramMessageId"] = sentMessage.MessageId;
            result.AdditionalData["ChatId"] = sentMessage.Chat.Id;
            result.AdditionalData["ChatType"] = sentMessage.Chat.Type.ToString();
            result.AdditionalData["BotId"] = _botInfo?.Id ?? 0;
            result.AdditionalData["Date"] = sentMessage.Date;

            if (sentMessage.Chat.Username != null)
                result.AdditionalData["ChatUsername"] = sentMessage.Chat.Username;

            return result;
        }

        /// <inheritdoc/>
        protected override async Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
        {
            var statusInfo = new StatusInfo($"Telegram Bot Connector (@{_botInfo?.Username ?? "unknown"})");

            statusInfo.AdditionalData["BotId"] = _botInfo?.Id ?? 0;
            statusInfo.AdditionalData["BotUsername"] = _botInfo?.Username ?? "";
            statusInfo.AdditionalData["BotFirstName"] = _botInfo?.FirstName ?? "";
            statusInfo.AdditionalData["State"] = State.ToString();
            statusInfo.AdditionalData["Uptime"] = DateTime.UtcNow - _startTime;
            statusInfo.AdditionalData["WebhookUrl"] = _webhookUrl ?? "";
            statusInfo.AdditionalData["HasWebhook"] = !string.IsNullOrWhiteSpace(_webhookUrl);

            if (!string.IsNullOrWhiteSpace(_webhookUrl))
            {
                try
                {
                    var webhookInfo = await _telegramService.GetWebhookInfoAsync(cancellationToken);
                    statusInfo.AdditionalData["WebhookUrl"] = webhookInfo.Url ?? "";
                    statusInfo.AdditionalData["WebhookPendingUpdateCount"] = webhookInfo.PendingUpdateCount;
                    statusInfo.AdditionalData["WebhookLastErrorDate"] = webhookInfo.LastErrorDate?.ToString() ?? "";
                    statusInfo.AdditionalData["WebhookLastErrorMessage"] = webhookInfo.LastErrorMessage ?? "";
                }
                catch (Exception ex)
                {
                    Logger.LogGetWebhookInfoFailed(ex);
                    statusInfo.AdditionalData["WebhookError"] = ex.Message;
                }
            }

            return statusInfo;
        }

        /// <inheritdoc/>
		protected override async Task<ConnectorHealth> GetConnectorHealthAsync(CancellationToken cancellationToken)
		{
			var health = new ConnectorHealth
			{
				State = State,
				IsHealthy = State == ConnectorState.Ready,
				LastHealthCheck = DateTime.UtcNow,
				Uptime = DateTime.UtcNow - _startTime
			};

			if (State == ConnectorState.Ready)
			{
				try
				{
					await TestConnectorConnectionAsync(cancellationToken);

					if (!string.IsNullOrWhiteSpace(_webhookUrl))
					{
						try
						{
							var webhookInfo = await _telegramService.GetWebhookInfoAsync(cancellationToken);
							if (!string.IsNullOrEmpty(webhookInfo.LastErrorMessage))
								health.Issues.Add($"Webhook error: {webhookInfo.LastErrorMessage}");
							if (webhookInfo.PendingUpdateCount > 100)
								health.Issues.Add($"High pending update count: {webhookInfo.PendingUpdateCount}");
						}
						catch (Exception ex)
						{
							health.Issues.Add($"Webhook check failed: {ex.Message}");
						}
					}
				}
				catch (Exception ex)
				{
					health.IsHealthy = false;
					health.Issues.Add($"Health check failed: {ex.Message}");
				}
			}
			else
			{
				health.Issues.Add($"Connector is in {State} state");
			}

			return health;
		}

        /// <inheritdoc/>
        protected override Task<ReceiveResult> ReceiveMessagesCoreAsync(MessageSource source, CancellationToken cancellationToken)
        {
            if (source.ContentType == MessageSource.JsonContentType)
            {
                var messages = TelegramMessageParser.ParseWebhookJson(source);

                if (messages.Count == 0)
                    throw new ConnectorException(MessagingErrorCodes.InvalidWebhookData, TelegramErrorCodes.ErrorDomain, "No valid messages found in webhook data");

                return Task.FromResult(new ReceiveResult(Guid.NewGuid().ToString(), messages));
            }

            throw new ConnectorException(MessagingErrorCodes.UnsupportedContentType,
                 TelegramErrorCodes.ErrorDomain,
                 $"Unsupported content type: {source.ContentType}. Only JSON content type is supported for Telegram message receiving");
        }

        /// <inheritdoc/>
		protected override async IAsyncEnumerable<ValidationResult> ValidateMessageCoreAsync(IMessage message, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			if (message.Content is ITextContent textContent)
			{
				if (!string.IsNullOrEmpty(textContent.Text) && textContent.Text.Length > TelegramConnectorConstants.MaxMessageLength)
				{
					yield return new ValidationResult(
						$"Message text cannot exceed {TelegramConnectorConstants.MaxMessageLength} characters",
						new[] { "Content" });
				}
			}

			if (message.Content is IMediaContent mediaContent)
			{
				if (!string.IsNullOrWhiteSpace(mediaContent.FileUrl))
				{
					if (!Uri.TryCreate(mediaContent.FileUrl, UriKind.Absolute, out _))
						yield return new ValidationResult("Invalid media URL format", new[] { "Content" });
				}

				var caption = TelegramMessageBuilder.GetMessageProperty(message, "Caption");
				if (!string.IsNullOrEmpty(caption) && caption.Length > TelegramConnectorConstants.MaxCaptionLength)
				{
					yield return new ValidationResult(
						$"Media caption cannot exceed {TelegramConnectorConstants.MaxCaptionLength} characters",
						new[] { "Properties.Caption" });
				}
			}

			if (message.Content is ILocationContent locationContent)
			{
				if (locationContent.Latitude < -90 || locationContent.Latitude > 90)
					yield return new ValidationResult("Latitude must be between -90 and 90 degrees", new[] { "Content.Latitude" });

				if (locationContent.Longitude < -180 || locationContent.Longitude > 180)
					yield return new ValidationResult("Longitude must be between -180 and 180 degrees", new[] { "Content.Longitude" });

				if (locationContent.LivePeriod.HasValue && (locationContent.LivePeriod.Value < 60 || locationContent.LivePeriod.Value > 86400))
					yield return new ValidationResult("Live period must be between 60 and 86400 seconds", new[] { "Content.LivePeriod" });

				if (locationContent.Heading.HasValue && (locationContent.Heading.Value < 1 || locationContent.Heading.Value > 360))
					yield return new ValidationResult("Heading must be between 1 and 360 degrees", new[] { "Content.Heading" });

				if (locationContent.ProximityAlertRadius.HasValue && (locationContent.ProximityAlertRadius.Value < 1 || locationContent.ProximityAlertRadius.Value > 100000))
					yield return new ValidationResult("Proximity alert radius must be between 1 and 100000 meters", new[] { "Content.ProximityAlertRadius" });
			}

			var inlineKeyboardJson = TelegramMessageBuilder.GetMessageProperty(message, "InlineKeyboard");
			if (!string.IsNullOrEmpty(inlineKeyboardJson))
			{
				InlineKeyboardButton[][]? keyboard = null;
				var validationResults = new List<ValidationResult>();

				try
				{
					keyboard = JsonSerializer.Deserialize<InlineKeyboardButton[][]>(inlineKeyboardJson);
				}
				catch (JsonException)
				{
					validationResults.Add(new ValidationResult("Invalid inline keyboard JSON format", new[] { "Properties.InlineKeyboard" }));
				}

				foreach (var result in validationResults)
					yield return result;

				if (keyboard != null)
				{
					if (keyboard.Length > TelegramConnectorConstants.MaxInlineKeyboardRows)
					{
						yield return new ValidationResult(
							$"Inline keyboard cannot have more than {TelegramConnectorConstants.MaxInlineKeyboardRows} rows",
							new[] { "Properties.InlineKeyboard" });
					}

					foreach (var row in keyboard)
					{
						if (row.Length > TelegramConnectorConstants.MaxInlineKeyboardButtonsPerRow)
						{
							yield return new ValidationResult(
								$"Inline keyboard row cannot have more than {TelegramConnectorConstants.MaxInlineKeyboardButtonsPerRow} buttons",
								new[] { "Properties.InlineKeyboard" });
						}
					}
				}
			}

			await foreach (var result in base.ValidateMessageCoreAsync(message, cancellationToken))
				yield return result;
		}

		/// <summary>
		/// Sets up webhook for receiving bot updates.
		/// </summary>
		private async Task SetupWebhookAsync(CancellationToken cancellationToken)
		{
			try
			{
				Logger.LogSettingUpWebhook(_webhookUrl ?? string.Empty);

				var maxConnections = ConnectionSettings.GetMaxConnections();
				var dropPendingUpdates = ConnectionSettings.GetDropPendingUpdates() ?? TelegramConnectionSettingsDefaults.DropPendingUpdates;

				await _telegramService.SetWebhookAsync(
					_webhookUrl!,
					maxConnections: maxConnections,
					dropPendingUpdates: dropPendingUpdates,
					secretToken: _secretToken,
					cancellationToken: cancellationToken);

				Logger.LogWebhookSetUp(_webhookUrl ?? string.Empty);
			}
			catch (Exception ex)
			{
				Logger.LogWebhookSetupFailed(_webhookUrl ?? string.Empty, ex);
				throw;
			}
		}

		/// <summary>
		/// Sends message based on content type.
		/// </summary>
		private async Task<Telegram.Bot.Types.Message> SendMessageByContentType(IMessage message, ChatId chatId, CancellationToken cancellationToken)
		{
			var parseMode = TelegramMessageBuilder.GetMessageParseMode(message, _parseMode);
			var disableWebPagePreview = TelegramMessageBuilder.GetMessageBoolProperty(message, "DisableWebPagePreview", _disableWebPagePreview);
			var disableNotification = TelegramMessageBuilder.GetMessageBoolProperty(message, "DisableNotification", _disableNotification);
			var replyToMessageId = TelegramMessageBuilder.GetMessageIntProperty(message, "ReplyToMessageId");
			var replyMarkup = TelegramMessageBuilder.CreateReplyMarkup(message);

			return message.Content switch
			{
				ITextContent textContent => await _telegramService.SendTextMessageAsync(
					chatId,
					textContent.Text ?? "",
					parseMode: parseMode,
					disableWebPagePreview: disableWebPagePreview,
					disableNotification: disableNotification,
					replyToMessageId: replyToMessageId,
					replyMarkup: replyMarkup,
					cancellationToken: cancellationToken),

				IMediaContent mediaContent => await SendMediaMessage(
					chatId, mediaContent, message,
					parseMode, disableNotification, replyToMessageId, replyMarkup, cancellationToken),

				ILocationContent locationContent => await _telegramService.SendLocationAsync(
					chatId,
					locationContent.Latitude,
					locationContent.Longitude,
					livePeriod: locationContent.LivePeriod,
					heading: locationContent.Heading,
					proximityAlertRadius: locationContent.ProximityAlertRadius,
					disableNotification: disableNotification,
					replyToMessageId: replyToMessageId,
					replyMarkup: replyMarkup,
					cancellationToken: cancellationToken),

				IJsonContent jsonContent when TelegramMessageBuilder.IsLocationMessage(jsonContent) => await SendLocationFromJson(
					chatId, jsonContent, disableNotification, replyToMessageId, replyMarkup, cancellationToken),

				_ => throw new NotSupportedException($"Content type {message.Content?.GetType().Name} is not supported")
			};
		}

		/// <summary>
		/// Sends location message from JSON content.
		/// </summary>
		private async Task<Telegram.Bot.Types.Message> SendLocationFromJson(ChatId chatId, IJsonContent jsonContent,
			bool? disableNotification, int? replyToMessageId, IReplyMarkup? replyMarkup, CancellationToken cancellationToken)
		{
			var json = JsonSerializer.Deserialize<JsonElement>(jsonContent.Json);
			var latitude = json.GetProperty("latitude").GetDouble();
			var longitude = json.GetProperty("longitude").GetDouble();

			var livePeriod = json.TryGetProperty("livePeriod", out var liveProp) ? liveProp.GetInt32() : (int?)null;
			var heading = json.TryGetProperty("heading", out var headingProp) ? headingProp.GetInt32() : (int?)null;
			var proximityAlertRadius = json.TryGetProperty("proximityAlertRadius", out var proxProp) ? proxProp.GetInt32() : (int?)null;

			return await _telegramService.SendLocationAsync(
				chatId, latitude, longitude,
				livePeriod: livePeriod,
				heading: heading,
				proximityAlertRadius: proximityAlertRadius,
				disableNotification: disableNotification,
				replyToMessageId: replyToMessageId,
				replyMarkup: replyMarkup,
				cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Sends media message based on media type.
		/// </summary>
		private async Task<Telegram.Bot.Types.Message> SendMediaMessage(ChatId chatId, IMediaContent mediaContent, IMessage message,
			ParseMode? parseMode, bool? disableNotification, int? replyToMessageId, IReplyMarkup? replyMarkup, CancellationToken cancellationToken)
		{
			var caption = TelegramMessageBuilder.GetMessageProperty(message, "Caption");
			var inputFile = TelegramMessageBuilder.CreateInputFile(mediaContent);

			return mediaContent.MediaType switch
			{
				MediaType.Image => await _telegramService.SendPhotoAsync(
					chatId, inputFile,
					caption: caption,
					parseMode: parseMode,
					disableNotification: disableNotification,
					replyToMessageId: replyToMessageId,
					replyMarkup: replyMarkup,
					cancellationToken: cancellationToken),

				MediaType.Video => await _telegramService.SendVideoAsync(
					chatId, inputFile,
					duration: TelegramMessageBuilder.GetMessageIntProperty(message, "Duration"),
					width: TelegramMessageBuilder.GetMessageIntProperty(message, "Width"),
					height: TelegramMessageBuilder.GetMessageIntProperty(message, "Height"),
					caption: caption,
					parseMode: parseMode,
					disableNotification: disableNotification,
					replyToMessageId: replyToMessageId,
					replyMarkup: replyMarkup,
					cancellationToken: cancellationToken),

				MediaType.Audio => await _telegramService.SendAudioAsync(
					chatId, inputFile,
					caption: caption,
					parseMode: parseMode,
					duration: TelegramMessageBuilder.GetMessageIntProperty(message, "Duration"),
					performer: TelegramMessageBuilder.GetMessageProperty(message, "Performer"),
					title: TelegramMessageBuilder.GetMessageProperty(message, "Title"),
					disableNotification: disableNotification,
					replyToMessageId: replyToMessageId,
					replyMarkup: replyMarkup,
					cancellationToken: cancellationToken),

				MediaType.Document => await _telegramService.SendDocumentAsync(
					chatId, inputFile,
					caption: caption,
					parseMode: parseMode,
					disableNotification: disableNotification,
					replyToMessageId: replyToMessageId,
					replyMarkup: replyMarkup,
					cancellationToken: cancellationToken),

				_ => throw new NotSupportedException($"Media type {mediaContent.MediaType} is not supported")
			};
		}

		/// <inheritdoc/>
		protected override async Task ShutdownConnectorAsync(CancellationToken cancellationToken)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(_webhookUrl))
				{
                    Logger.LogRemovingWebhook();
					await _telegramService.DeleteWebhookAsync(cancellationToken: cancellationToken);
                    Logger.LogWebhookRemoved();
				}
			}
			catch (Exception ex)
			{
                Logger.LogWebhookRemovalFailed(ex);
			}

			await base.ShutdownConnectorAsync(cancellationToken);
		}
	}
}
