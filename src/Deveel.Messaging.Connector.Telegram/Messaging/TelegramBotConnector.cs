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
	[ChannelSchema(typeof(TelegramBotSchemaFactory))]
	public class TelegramBotConnector : ChannelConnectorBase
	{
		private readonly ConnectionSettings _connectionSettings;
		private readonly ILogger<TelegramBotConnector>? _logger;
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
		/// <param name="schema">The channel schema that defines the connector's capabilities and configuration.</param>
		/// <param name="connectionSettings">The connection settings containing bot token and configuration.</param>
		/// <param name="telegramService">The Telegram service for API operations.</param>
		/// <param name="logger">Optional logger for diagnostic and operational logging.</param>
		/// <exception cref="ArgumentNullException">Thrown when schema or connectionSettings is null.</exception>
		public TelegramBotConnector(IChannelSchema schema, ConnectionSettings connectionSettings, ITelegramService? telegramService = null, ILogger<TelegramBotConnector>? logger = null)
			: base(schema, logger)
		{
			_connectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));
			_telegramService = telegramService ?? new TelegramService();
			_logger = logger;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TelegramBotConnector"/> class using one of the predefined schemas.
		/// </summary>
		/// <param name="connectionSettings">The connection settings containing bot token and configuration.</param>
		/// <param name="telegramService">The Telegram service for API operations.</param>
		/// <param name="logger">Optional logger for diagnostic and operational logging.</param>
		/// <exception cref="ArgumentNullException">Thrown when connectionSettings is null.</exception>
		public TelegramBotConnector(ConnectionSettings connectionSettings, ITelegramService? telegramService = null, ILogger<TelegramBotConnector>? logger = null)
			: this(TelegramChannelSchemas.TelegramBot, connectionSettings, telegramService, logger)
		{
		}

		/// <inheritdoc/>
		protected override async Task<ConnectorResult<bool>> InitializeConnectorAsync(CancellationToken cancellationToken)
		{
			try
			{
				_logger?.LogInformation("Initializing Telegram Bot connector...");

				// Extract required parameters
				_botToken = _connectionSettings.GetParameter("BotToken") as string;

				// Extract optional parameters
				_webhookUrl = _connectionSettings.GetParameter("WebhookUrl") as string;
				_secretToken = _connectionSettings.GetParameter("SecretToken") as string;
				_disableWebPagePreview = _connectionSettings.GetParameter("DisableWebPagePreview") as bool? ?? false;
				_disableNotification = _connectionSettings.GetParameter("DisableNotification") as bool? ?? false;
				_parseMode = _connectionSettings.GetParameter("ParseMode") as string ?? "Markdown";
				_maxRetries = _connectionSettings.GetParameter("MaxRetries") as int? ?? 3;
				_timeoutSeconds = _connectionSettings.GetParameter("TimeoutSeconds") as int? ?? 30;

				// Validate required parameters
				if (string.IsNullOrWhiteSpace(_botToken))
				{
					return ConnectorResult<bool>.Fail(TelegramErrorCodes.MissingBotToken, 
						"Bot token is required for Telegram Bot API");
				}

				// Validate connection settings against schema
				if (Schema is ChannelSchema channelSchema)
				{
					var validationResults = channelSchema.ValidateConnectionSettings(_connectionSettings);
					var validationErrors = validationResults.ToList();
					if (validationErrors.Count > 0)
					{
						_logger?.LogError("Connection settings validation failed: {Errors}", 
							string.Join(", ", validationErrors.Select(e => e.ErrorMessage)));
						return ConnectorResult<bool>.ValidationFailed(TelegramErrorCodes.InvalidBotToken, 
							"Connection settings validation failed", validationErrors);
					}
				}

				// Initialize Telegram service
				_telegramService.Initialize(_botToken);

				// Get bot information to verify the token
				_botInfo = await _telegramService.GetMeAsync(cancellationToken);
				_logger?.LogInformation("Bot initialized successfully: @{BotUsername} ({BotId})", 
					_botInfo.Username, _botInfo.Id);

				// Set up webhook if URL is provided
				if (!string.IsNullOrWhiteSpace(_webhookUrl))
				{
					await SetupWebhookAsync(cancellationToken);
				}

				_logger?.LogInformation("Telegram Bot connector initialized successfully");
				return ConnectorResult<bool>.Success(true);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Failed to initialize Telegram Bot connector");
				return ConnectorResult<bool>.Fail(ConnectorErrorCodes.InitializationError, ex.Message);
			}
		}

		/// <inheritdoc/>
		protected override async Task<ConnectorResult<bool>> TestConnectorConnectionAsync(CancellationToken cancellationToken)
		{
			try
			{
				_logger?.LogDebug("Testing Telegram connection...");

				// Test connection by calling getMe API
				var botInfo = await _telegramService.GetMeAsync(cancellationToken);
				
				if (botInfo == null)
				{
					return ConnectorResult<bool>.Fail(TelegramErrorCodes.ConnectionFailed, 
						"Unable to retrieve bot information");
				}

				_logger?.LogDebug("Connection test successful. Bot: @{BotUsername} ({BotId})", 
					botInfo.Username, botInfo.Id);
				return ConnectorResult<bool>.Success(true);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Connection test failed");
				return ConnectorResult<bool>.Fail(TelegramErrorCodes.ConnectionTestFailed, ex.Message);
			}
		}

		/// <inheritdoc/>
		protected override async Task<ConnectorResult<SendResult>> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
		{
			try
			{
				_logger?.LogDebug("Sending Telegram message {MessageId}", message.Id);

				// Extract chat ID from receiver
				var chatId = ExtractChatId(message.Receiver);
				if (chatId == null)
				{
					return ConnectorResult<SendResult>.Fail(TelegramErrorCodes.InvalidChatId, 
						"Receiver must contain a valid Telegram chat ID");
				}

				Telegram.Bot.Types.Message sentMessage = await SendMessageByContentType(message, chatId, cancellationToken);

				_logger?.LogInformation("Telegram message sent successfully. MessageId: {TelegramMessageId}, ChatId: {ChatId}", 
					sentMessage.MessageId, sentMessage.Chat.Id);

				var result = new SendResult(message.Id, sentMessage.MessageId.ToString())
				{
					Status = MessageStatus.Sent,
					Timestamp = sentMessage.Date
				};

				// Add Telegram-specific properties
				result.AdditionalData["TelegramMessageId"] = sentMessage.MessageId;
				result.AdditionalData["ChatId"] = sentMessage.Chat.Id;
				result.AdditionalData["ChatType"] = sentMessage.Chat.Type.ToString();
				result.AdditionalData["BotId"] = _botInfo?.Id ?? 0;
				result.AdditionalData["Date"] = sentMessage.Date;

				if (sentMessage.Chat.Username != null)
				{
					result.AdditionalData["ChatUsername"] = sentMessage.Chat.Username;
				}

				return ConnectorResult<SendResult>.Success(result);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Failed to send Telegram message {MessageId}", message.Id);
				return ConnectorResult<SendResult>.Fail(TelegramErrorCodes.SendMessageFailed, ex.Message);
			}
		}

		/// <inheritdoc/>
		protected override async Task<ConnectorResult<StatusInfo>> GetConnectorStatusAsync(CancellationToken cancellationToken)
		{
			try
			{
				var statusInfo = new StatusInfo($"Telegram Bot Connector (@{_botInfo?.Username ?? "unknown"})");

				statusInfo.AdditionalData["BotId"] = _botInfo?.Id ?? 0;
				statusInfo.AdditionalData["BotUsername"] = _botInfo?.Username ?? "";
				statusInfo.AdditionalData["BotFirstName"] = _botInfo?.FirstName ?? "";
				statusInfo.AdditionalData["State"] = State.ToString();
				statusInfo.AdditionalData["Uptime"] = DateTime.UtcNow - _startTime;
				statusInfo.AdditionalData["WebhookUrl"] = _webhookUrl ?? "";
				statusInfo.AdditionalData["HasWebhook"] = !string.IsNullOrWhiteSpace(_webhookUrl);

				// Get webhook info if webhook is configured
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
						_logger?.LogWarning(ex, "Failed to get webhook info for status");
						statusInfo.AdditionalData["WebhookError"] = ex.Message;
					}
				}

				return ConnectorResult<StatusInfo>.Success(statusInfo);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Failed to get connector status");
				return ConnectorResult<StatusInfo>.Fail(TelegramErrorCodes.StatusError, ex.Message);
			}
		}

		/// <inheritdoc/>
		protected override async Task<ConnectorResult<ConnectorHealth>> GetConnectorHealthAsync(CancellationToken cancellationToken)
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
					// Test connectivity by calling getMe API
					var testResult = await TestConnectorConnectionAsync(cancellationToken);
					if (!testResult.Successful)
					{
						health.IsHealthy = false;
						health.Issues.Add($"Connection test failed: {testResult.Error?.ErrorMessage}");
					}

					// Check webhook health if configured
					if (!string.IsNullOrWhiteSpace(_webhookUrl))
					{
						try
						{
							var webhookInfo = await _telegramService.GetWebhookInfoAsync(cancellationToken);
							if (!string.IsNullOrEmpty(webhookInfo.LastErrorMessage))
							{
								health.Issues.Add($"Webhook error: {webhookInfo.LastErrorMessage}");
							}
							if (webhookInfo.PendingUpdateCount > 100)
							{
								health.Issues.Add($"High pending update count: {webhookInfo.PendingUpdateCount}");
							}
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

			return ConnectorResult<ConnectorHealth>.Success(health);
		}

		/// <inheritdoc/>
		protected override async Task<ConnectorResult<ReceiveResult>> ReceiveMessagesCoreAsync(MessageSource source, CancellationToken cancellationToken)
		{
			try
			{
				_logger?.LogDebug("Receiving Telegram message from webhook");

				if (source.ContentType == MessageSource.JsonContentType)
				{
					var messages = ParseTelegramWebhookJson(source);
					
					if (messages.Count == 0)
					{
						return ConnectorResult<ReceiveResult>.Fail(TelegramErrorCodes.InvalidWebhookData, 
							"No valid messages found in webhook data");
					}

					var result = new ReceiveResult(Guid.NewGuid().ToString(), messages);
					return ConnectorResult<ReceiveResult>.Success(result);
				}

				return ConnectorResult<ReceiveResult>.Fail(TelegramErrorCodes.UnsupportedContentType, 
					"Only JSON content type is supported for Telegram message receiving");
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Failed to receive Telegram message from webhook");
				return ConnectorResult<ReceiveResult>.Fail(TelegramErrorCodes.ReceiveMessageFailed, ex.Message);
			}
		}

		/// <inheritdoc/>
		protected override async IAsyncEnumerable<ValidationResult> ValidateMessageCoreAsync(IMessage message, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			// Validate message content based on type
			if (message.Content is ITextContent textContent)
			{
				if (textContent.Text.Length > TelegramConnectorConstants.MaxMessageLength)
				{
					yield return new ValidationResult(
						$"Message text cannot exceed {TelegramConnectorConstants.MaxMessageLength} characters",
						new[] { "Content" });
				}
			}

			// Validate media content
			if (message.Content is IMediaContent mediaContent)
			{
				if (!string.IsNullOrWhiteSpace(mediaContent.FileUrl))
				{
					if (!Uri.TryCreate(mediaContent.FileUrl, UriKind.Absolute, out _))
					{
						yield return new ValidationResult("Invalid media URL format", new[] { "Content" });
					}
				}

				// Validate caption length
				var caption = GetMessageProperty(message, "Caption");
				if (!string.IsNullOrEmpty(caption) && caption.Length > TelegramConnectorConstants.MaxCaptionLength)
				{
					yield return new ValidationResult(
						$"Media caption cannot exceed {TelegramConnectorConstants.MaxCaptionLength} characters",
						new[] { "Properties.Caption" });
				}
			}

			// Validate location content
			if (message.Content is ILocationContent locationContent)
			{
				if (locationContent.Latitude < -90 || locationContent.Latitude > 90)
				{
					yield return new ValidationResult(
						"Latitude must be between -90 and 90 degrees",
						new[] { "Content.Latitude" });
				}

				if (locationContent.Longitude < -180 || locationContent.Longitude > 180)
				{
					yield return new ValidationResult(
						"Longitude must be between -180 and 180 degrees",
						new[] { "Content.Longitude" });
				}

				if (locationContent.LivePeriod.HasValue && 
					(locationContent.LivePeriod.Value < 60 || locationContent.LivePeriod.Value > 86400))
				{
					yield return new ValidationResult(
						"Live period must be between 60 and 86400 seconds",
						new[] { "Content.LivePeriod" });
				}

				if (locationContent.Heading.HasValue && 
					(locationContent.Heading.Value < 1 || locationContent.Heading.Value > 360))
				{
					yield return new ValidationResult(
						"Heading must be between 1 and 360 degrees",
						new[] { "Content.Heading" });
				}

				if (locationContent.ProximityAlertRadius.HasValue && 
					(locationContent.ProximityAlertRadius.Value < 1 || locationContent.ProximityAlertRadius.Value > 100000))
				{
					yield return new ValidationResult(
						"Proximity alert radius must be between 1 and 100000 meters",
						new[] { "Content.ProximityAlertRadius" });
				}
			}

			// Validate inline keyboard if present
			var inlineKeyboardJson = GetMessageProperty(message, "InlineKeyboard");
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
				{
					yield return result;
				}

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

			// Call base validation last
			await foreach (var result in base.ValidateMessageCoreAsync(message, cancellationToken))
			{
				yield return result;
			}
		}

		/// <summary>
		/// Sets up webhook for receiving bot updates.
		/// </summary>
		private async Task SetupWebhookAsync(CancellationToken cancellationToken)
		{
			try
			{
				_logger?.LogDebug("Setting up Telegram webhook: {WebhookUrl}", _webhookUrl);

				var maxConnections = _connectionSettings.GetParameter("MaxConnections") as int?;
				var dropPendingUpdates = _connectionSettings.GetParameter("DropPendingUpdates") as bool? ?? false;

				await _telegramService.SetWebhookAsync(
					_webhookUrl!,
					maxConnections: maxConnections,
					dropPendingUpdates: dropPendingUpdates,
					secretToken: _secretToken,
					cancellationToken: cancellationToken);

				_logger?.LogInformation("Webhook set up successfully: {WebhookUrl}", _webhookUrl);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Failed to set up webhook: {WebhookUrl}", _webhookUrl);
				throw;
			}
		}

		/// <summary>
		/// Extracts chat ID from endpoint.
		/// </summary>
		private static ChatId? ExtractChatId(IEndpoint? endpoint)
		{
			if (endpoint?.Type != EndpointType.Id)
				return null;

			var address = endpoint.Address;
			if (string.IsNullOrWhiteSpace(address))
				return null;

			// Try to parse as long (numeric chat ID)
			if (long.TryParse(address, out var chatIdLong))
			{
				return new ChatId(chatIdLong);
			}

			// Use as string (username)
			return new ChatId(address);
		}

		/// <summary>
		/// Sends message based on content type.
		/// </summary>
		private async Task<Telegram.Bot.Types.Message> SendMessageByContentType(IMessage message, ChatId chatId, CancellationToken cancellationToken)
		{
			var parseMode = GetMessageParseMode(message);
			var disableWebPagePreview = GetMessageBoolProperty(message, "DisableWebPagePreview", _disableWebPagePreview);
			var disableNotification = GetMessageBoolProperty(message, "DisableNotification", _disableNotification);
			var replyToMessageId = GetMessageIntProperty(message, "ReplyToMessageId");
			var replyMarkup = CreateReplyMarkup(message);

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

				// Handle location data as JSON content with latitude/longitude (for backward compatibility)
				IJsonContent jsonContent when IsLocationMessage(jsonContent) => await SendLocationFromJson(
					chatId, jsonContent, disableNotification, replyToMessageId, replyMarkup, cancellationToken),

				_ => throw new NotSupportedException($"Content type {message.Content?.GetType().Name} is not supported")
			};
		}

		/// <summary>
		/// Checks if JSON content represents a location message.
		/// </summary>
		private static bool IsLocationMessage(IJsonContent jsonContent)
		{
			try
			{
				var json = JsonSerializer.Deserialize<JsonElement>(jsonContent.Json);
				return json.TryGetProperty("latitude", out _) && json.TryGetProperty("longitude", out _);
			}
			catch
			{
				return false;
			}
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
			var caption = GetMessageProperty(message, "Caption");
			var inputFile = CreateInputFile(mediaContent);

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
					duration: GetMessageIntProperty(message, "Duration"),
					width: GetMessageIntProperty(message, "Width"),
					height: GetMessageIntProperty(message, "Height"),
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
					duration: GetMessageIntProperty(message, "Duration"),
					performer: GetMessageProperty(message, "Performer"),
					title: GetMessageProperty(message, "Title"),
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

		/// <summary>
		/// Creates InputFile from media content.
		/// </summary>
		private static InputFile CreateInputFile(IMediaContent mediaContent)
		{
			if (!string.IsNullOrWhiteSpace(mediaContent.FileUrl))
			{
				return InputFile.FromUri(mediaContent.FileUrl);
			}

			if (mediaContent.Data != null && mediaContent.Data.Length > 0)
			{
				var fileName = mediaContent.FileName ?? "file";
				return InputFile.FromStream(new MemoryStream(mediaContent.Data), fileName);
			}

			throw new ArgumentException("Media content must have either URL or data");
		}

		/// <summary>
		/// Gets parse mode for message.
		/// </summary>
		private ParseMode? GetMessageParseMode(IMessage message)
		{
			var parseModeString = GetMessageProperty(message, "ParseMode") ?? _parseMode;
			
			return parseModeString?.ToLowerInvariant() switch
			{
				"markdown" => ParseMode.Markdown,
				"markdownv2" => ParseMode.MarkdownV2,
				"html" => ParseMode.Html,
				"none" => null,
				_ => ParseMode.Markdown
			};
		}

		/// <summary>
		/// Creates reply markup from message properties.
		/// </summary>
		private static IReplyMarkup? CreateReplyMarkup(IMessage message)
		{
			var inlineKeyboardJson = GetMessageProperty(message, "InlineKeyboard");
			if (!string.IsNullOrEmpty(inlineKeyboardJson))
			{
				try
				{
					var keyboard = JsonSerializer.Deserialize<InlineKeyboardButton[][]>(inlineKeyboardJson);
					if (keyboard != null)
					{
						return new InlineKeyboardMarkup(keyboard);
					}
				}
				catch (JsonException)
				{
					// Invalid JSON, ignore
				}
			}

			var replyKeyboardJson = GetMessageProperty(message, "ReplyKeyboard");
			if (!string.IsNullOrEmpty(replyKeyboardJson))
			{
				try
				{
					var keyboard = JsonSerializer.Deserialize<KeyboardButton[][]>(replyKeyboardJson);
					if (keyboard != null)
					{
						return new ReplyKeyboardMarkup(keyboard);
					}
				}
				catch (JsonException)
				{
					// Invalid JSON, ignore
				}
			}

			return null;
		}

		/// <summary>
		/// Gets message property value as string.
		/// </summary>
		private static string? GetMessageProperty(IMessage message, string propertyName)
		{
			if (message.Properties?.TryGetValue(propertyName, out var property) == true)
			{
				return property.Value?.ToString();
			}
			return null;
		}

		/// <summary>
		/// Gets boolean property from message.
		/// </summary>
		private static bool? GetMessageBoolProperty(IMessage message, string propertyName, bool defaultValue = false)
		{
			var value = GetMessageProperty(message, propertyName);
			if (bool.TryParse(value, out var parsedValue))
				return parsedValue;
			return defaultValue;
		}

		/// <summary>
		/// Gets integer property from message.
		/// </summary>
		private static int? GetMessageIntProperty(IMessage message, string propertyName)
		{
			var value = GetMessageProperty(message, propertyName);
			if (int.TryParse(value, out var parsedValue))
				return parsedValue;
			return null;
		}

		/// <summary>
		/// Parses Telegram webhook JSON data.
		/// </summary>
		private List<IMessage> ParseTelegramWebhookJson(MessageSource source)
		{
			var messages = new List<IMessage>();
			var jsonData = source.AsJson<JsonElement>();

			if (jsonData.TryGetProperty("message", out var messageElement))
			{
				var message = ParseTelegramMessage(messageElement);
				if (message != null)
					messages.Add(message);
			}
			else if (jsonData.TryGetProperty("edited_message", out var editedMessageElement))
			{
				var message = ParseTelegramMessage(editedMessageElement);
				if (message != null)
					messages.Add(message);
			}

			return messages;
		}

		/// <summary>
		/// Parses Telegram message from JSON element.
		/// </summary>
		private IMessage? ParseTelegramMessage(JsonElement messageElement)
		{
			if (!messageElement.TryGetProperty("message_id", out var messageIdElement))
				return null;

			var messageId = messageIdElement.GetInt32().ToString();

			if (!messageElement.TryGetProperty("from", out var fromElement) ||
				!messageElement.TryGetProperty("chat", out var chatElement))
				return null;

			var fromId = fromElement.TryGetProperty("id", out var fromIdElement) ? fromIdElement.GetInt64().ToString() : "";
			var chatId = chatElement.TryGetProperty("id", out var chatIdElement) ? chatIdElement.GetInt64().ToString() : "";

			if (string.IsNullOrEmpty(fromId) || string.IsNullOrEmpty(chatId))
				return null;

			var sender = new Endpoint(EndpointType.Id, fromId);
			var receiver = new Endpoint(EndpointType.Id, chatId);

			// Parse content based on message type
			IMessageContent? content = null;

			if (messageElement.TryGetProperty("text", out var textElement))
			{
				content = new TextContent(textElement.GetString() ?? "");
			}
			else if (messageElement.TryGetProperty("photo", out var photoElement))
			{
				var photoArray = photoElement.EnumerateArray().ToArray();
				if (photoArray.Length > 0)
				{
					var largestPhoto = photoArray.OrderByDescending(p => 
						p.TryGetProperty("file_size", out var sizeElement) ? sizeElement.GetInt32() : 0).First();
					
					if (largestPhoto.TryGetProperty("file_id", out var fileIdElement))
					{
						content = new MediaContent(MediaType.Image, fileIdElement.GetString() ?? "", "");
					}
				}
			}
			else if (messageElement.TryGetProperty("video", out var videoElement))
			{
				if (videoElement.TryGetProperty("file_id", out var fileIdElement))
				{
					content = new MediaContent(MediaType.Video, fileIdElement.GetString() ?? "", "");
				}
			}
			else if (messageElement.TryGetProperty("audio", out var audioElement))
			{
				if (audioElement.TryGetProperty("file_id", out var fileIdElement))
				{
					content = new MediaContent(MediaType.Audio, fileIdElement.GetString() ?? "", "");
				}
			}
			else if (messageElement.TryGetProperty("document", out var documentElement))
			{
				if (documentElement.TryGetProperty("file_id", out var fileIdElement))
				{
					content = new MediaContent(MediaType.Document, fileIdElement.GetString() ?? "", "");
				}
			}
			else if (messageElement.TryGetProperty("location", out var locationElement))
			{
				if (locationElement.TryGetProperty("latitude", out var latElement) &&
					locationElement.TryGetProperty("longitude", out var lonElement))
				{
					var locationContent = new LocationContent(latElement.GetDouble(), lonElement.GetDouble());

					// Parse optional location properties
					if (locationElement.TryGetProperty("horizontal_accuracy", out var accuracyElement))
					{
						locationContent.WithHorizontalAccuracy(accuracyElement.GetDouble());
					}

					if (locationElement.TryGetProperty("live_period", out var livePeriodElement))
					{
						locationContent.WithLivePeriod(livePeriodElement.GetInt32());
					}

					if (locationElement.TryGetProperty("heading", out var headingElement))
					{
						locationContent.WithHeading(headingElement.GetInt32());
					}

					if (locationElement.TryGetProperty("proximity_alert_radius", out var proximityElement))
					{
						locationContent.WithProximityAlertRadius(proximityElement.GetInt32());
					}

					content = locationContent;
				}
			}

			content ??= new TextContent();

			var message = new Message
			{
				Id = messageId,
				Sender = sender,
				Receiver = receiver,
				Content = MessageContent.Create(content),
				Properties = new Dictionary<string, MessageProperty>()
			};

			// Add Telegram-specific properties
			if (messageElement.TryGetProperty("date", out var dateElement))
			{
				var timestamp = DateTimeOffset.FromUnixTimeSeconds(dateElement.GetInt64()).DateTime;
				message.Properties["Date"] = new MessageProperty("Date", timestamp);
			}

			if (messageElement.TryGetProperty("reply_to_message", out var replyElement) &&
				replyElement.TryGetProperty("message_id", out var replyIdElement))
			{
				message.Properties["ReplyToMessageId"] = new MessageProperty("ReplyToMessageId", replyIdElement.GetInt32());
			}

			return message;
		}

		/// <inheritdoc/>
		protected override async Task ShutdownConnectorAsync(CancellationToken cancellationToken)
		{
			try
			{
				// Remove webhook if it was set up
				if (!string.IsNullOrWhiteSpace(_webhookUrl))
				{
					_logger?.LogInformation("Removing Telegram webhook...");
					await _telegramService.DeleteWebhookAsync(cancellationToken: cancellationToken);
					_logger?.LogInformation("Webhook removed successfully");
				}
			}
			catch (Exception ex)
			{
				_logger?.LogWarning(ex, "Failed to remove webhook during shutdown");
			}

			await base.ShutdownConnectorAsync(cancellationToken);
		}
	}
}