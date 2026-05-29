//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
	/// <summary>
	/// Factory class for creating Telegram Bot channel schemas.
	/// </summary>
	/// <remarks>
	/// This factory provides methods to create various Telegram channel schema configurations
	/// suitable for different use cases and requirements.
	/// </remarks>
	public static class TelegramBotSchemaFactory
	{
		/// <summary>
		/// Creates a comprehensive Telegram Bot channel schema with full capabilities.
		/// </summary>
		/// <returns>A channel schema configured for complete Telegram Bot functionality.</returns>
		public static IChannelSchema CreateSchema()
		{
			return new ChannelSchemaBuilder(TelegramConnectorConstants.Provider, TelegramConnectorConstants.BotChannel, "1.0.0")
				.WithDisplayName("Telegram Bot API")
				.WithCapabilities(
					ChannelCapability.SendMessages |
					ChannelCapability.ReceiveMessages |
					ChannelCapability.MessageStatusQuery |
					ChannelCapability.HandleMessageState |
					ChannelCapability.HealthCheck |
					ChannelCapability.InteractiveContent)
            .AddParameter(new ChannelParameter(TelegramConnectionParameters.BotToken, DataType.String)
            {
                IsRequired = true,
                IsSensitive = true,
                Description = "Telegram Bot Token obtained from @BotFather"
            })
            .AddParameter(new ChannelParameter(TelegramConnectionParameters.WebhookUrl, DataType.String)
            {
                IsRequired = false,
                Description = "URL to receive webhook notifications for incoming messages"
            })
            .AddParameter(new ChannelParameter(TelegramConnectionParameters.SecretToken, DataType.String)
            {
                IsRequired = false,
                IsSensitive = true,
                Description = "Secret token for webhook validation (optional but recommended)"
            })
            .AddParameter(new ChannelParameter(TelegramConnectionParameters.DisableWebPagePreview, DataType.Boolean)
            {
                IsRequired = false,
                DefaultValue = TelegramConnectionSettingsDefaults.DisableWebPagePreview,
                Description = "Disable web page previews in messages"
            })
            .AddParameter(new ChannelParameter(TelegramConnectionParameters.DisableNotification, DataType.Boolean)
            {
                IsRequired = false,
                DefaultValue = TelegramConnectionSettingsDefaults.DisableNotification,
                Description = "Send messages silently (users will receive notification with no sound)"
            })
            .AddParameter(new ChannelParameter(TelegramConnectionParameters.ParseMode, DataType.String)
            {
                IsRequired = false,
                DefaultValue = TelegramConnectionSettingsDefaults.ParseMode,
                Description = "Message parsing mode (Markdown, MarkdownV2, HTML, or None)"
            })
            .AddParameter(new ChannelParameter(TelegramConnectionParameters.MaxRetries, DataType.Integer)
            {
                IsRequired = false,
                DefaultValue = TelegramConnectionSettingsDefaults.MaxRetries,
                Description = "Maximum number of retry attempts for failed operations"
            })
            .AddParameter(new ChannelParameter(TelegramConnectionParameters.TimeoutSeconds, DataType.Integer)
            {
                IsRequired = false,
                DefaultValue = TelegramConnectionSettingsDefaults.TimeoutSeconds,
                Description = "Request timeout in seconds"
            })
				.AddContentType(MessageContentType.PlainText)
				.AddContentType(MessageContentType.Media)
				.AddContentType(MessageContentType.Location)
				.AddContentType(MessageContentType.Json) // For custom data
				.AddContentType(MessageContentType.Button)
				.AddContentType(MessageContentType.QuickReply)
				.HandlesMessageEndpoint(EndpointType.Id, e =>
				{
					e.CanSend = true;
					e.CanReceive = true;
					e.IsRequired = true;
					e.Description = "Telegram Chat ID (can be user ID, group ID, or channel username)";
				})
				.AddAuthenticationConfiguration(new AuthenticationConfiguration(AuthenticationScheme.Bearer, "Bot Token")
					.WithField(TelegramConnectionParameters.BotToken, DataType.String, f =>
					{
						f.DisplayName = "Bot Token";
						f.Description = "Telegram Bot Token obtained from @BotFather";
						f.AuthenticationRole = "principal";
						f.IsSensitive = true;
					}))
				.AddMessageProperty(new MessagePropertyConfiguration("ParseMode", DataType.String)
				{
					IsRequired = false,
					Description = "Override default parse mode for this message (Markdown, MarkdownV2, HTML, or None)"
				})
				.AddMessageProperty(new MessagePropertyConfiguration("DisableWebPagePreview", DataType.Boolean)
				{
					IsRequired = false,
					Description = "Disable web page preview for this message"
				})
				.AddMessageProperty(new MessagePropertyConfiguration("DisableNotification", DataType.Boolean)
				{
					IsRequired = false,
					Description = "Send this message silently"
				})
				.AddMessageProperty(new MessagePropertyConfiguration("ReplyToMessageId", DataType.Integer)
				{
					IsRequired = false,
					Description = "ID of the message to reply to"
				})
				.AddMessageProperty(new MessagePropertyConfiguration("InlineKeyboard", DataType.String)
				{
					IsRequired = false,
					Description = "JSON representation of inline keyboard markup"
				})
				.AddMessageProperty(new MessagePropertyConfiguration("ReplyKeyboard", DataType.String)
				{
					IsRequired = false,
					Description = "JSON representation of reply keyboard markup"
				})
				.AddMessageProperty(new MessagePropertyConfiguration("Caption", DataType.String)
				{
					IsRequired = false,
					Description = "Caption for media messages"
				})
				.AddMessageProperty(new MessagePropertyConfiguration("FileName", DataType.String)
				{
					IsRequired = false,
					Description = "File name for document messages"
				})
				.AddMessageProperty(new MessagePropertyConfiguration("Duration", DataType.Integer)
				{
					IsRequired = false,
					Description = "Duration in seconds for audio/video messages"
				})
				.AddMessageProperty(new MessagePropertyConfiguration("Width", DataType.Integer)
				{
					IsRequired = false,
					Description = "Width in pixels for video messages"
				})
				.AddMessageProperty(new MessagePropertyConfiguration("Height", DataType.Integer)
				{
					IsRequired = false,
					Description = "Height in pixels for video messages"
				})
				.AddMessageProperty(new MessagePropertyConfiguration("HorizontalAccuracy", DataType.Number)
				{
					IsRequired = false,
					Description = "Horizontal accuracy of location in meters"
				})
				.AddMessageProperty(new MessagePropertyConfiguration("LivePeriod", DataType.Integer)
				{
					IsRequired = false,
					Description = "Period in seconds for which the location can be updated (60-86400)"
				})
				.AddMessageProperty(new MessagePropertyConfiguration("Heading", DataType.Integer)
				{
					IsRequired = false,
					Description = "Direction in which the user is moving, in degrees (1-360)"
				})
				.AddMessageProperty(new MessagePropertyConfiguration("ProximityAlertRadius", DataType.Integer)
				{
					IsRequired = false,
					Description = "Maximum distance for proximity alerts about approaching the location, in meters (1-100000)"
				})
				.Build();
		}

		/// <summary>
		/// Creates a simple Telegram Bot channel schema for basic text messaging.
		/// </summary>
		/// <returns>A channel schema configured for basic text messaging only.</returns>
		public static IChannelSchema CreateSimpleSchema()
		{
			return new ChannelSchemaBuilder(TelegramConnectorConstants.Provider, "simple-bot", "1.0.0")
				.WithDisplayName("Simple Telegram Bot")
				.WithCapabilities(
					ChannelCapability.SendMessages |
					ChannelCapability.HealthCheck)
            .AddParameter(new ChannelParameter(TelegramConnectionParameters.BotToken, DataType.String)
            {
                IsRequired = true,
                IsSensitive = true,
                Description = "Telegram Bot Token obtained from @BotFather"
            })
            .AddParameter(new ChannelParameter(TelegramConnectionParameters.ParseMode, DataType.String)
            {
                IsRequired = false,
                DefaultValue = TelegramConnectionSettingsDefaults.ParseMode,
                Description = "Message parsing mode (Markdown, MarkdownV2, HTML, or None)"
            })
            .AddContentType(MessageContentType.PlainText)
            .AddAuthenticationConfiguration(new AuthenticationConfiguration(AuthenticationScheme.Bearer, "Bot Token")
                .WithField(TelegramConnectionParameters.BotToken, DataType.String, f =>
                {
                    f.DisplayName = "Bot Token";
                    f.Description = "Telegram Bot Token obtained from @BotFather";
                    f.AuthenticationRole = "principal";
                    f.IsSensitive = true;
                }))
            .HandlesMessageEndpoint(EndpointType.Id, e =>
            {
                e.CanSend = true;
                e.CanReceive = false;
                e.IsRequired = true;
                e.Description = "Telegram Chat ID";
            })
            .Build();
		}

		/// <summary>
		/// Creates a notification-focused Telegram Bot channel schema.
		/// </summary>
		/// <returns>A channel schema configured for sending notifications and alerts.</returns>
		public static IChannelSchema CreateNotificationSchema()
		{
			return new ChannelSchemaBuilder(TelegramConnectorConstants.Provider, "notification-bot", "1.0.0")
				.WithDisplayName("Telegram Notification Bot")
				.WithCapabilities(
					ChannelCapability.SendMessages |
					ChannelCapability.HealthCheck)
            .AddParameter(new ChannelParameter(TelegramConnectionParameters.BotToken, DataType.String)
            {
                IsRequired = true,
                IsSensitive = true,
                Description = "Telegram Bot Token obtained from @BotFather"
            })
            .AddParameter(new ChannelParameter(TelegramConnectionParameters.DefaultChatId, DataType.String)
            {
                IsRequired = false,
                Description = "Default chat ID for notifications (optional)"
            })
            .AddParameter(new ChannelParameter(TelegramConnectionParameters.DisableNotification, DataType.Boolean)
            {
                IsRequired = false,
                DefaultValue = TelegramConnectionSettingsDefaults.DisableNotification,
                Description = "Send notifications silently by default"
            })
            .AddParameter(new ChannelParameter(TelegramConnectionParameters.ParseMode, DataType.String)
            {
                IsRequired = false,
                DefaultValue = "HTML",
                Description = "Message parsing mode for notifications"
            })
			.AddContentType(MessageContentType.PlainText)
			.AddContentType(MessageContentType.Media)
			.AddAuthenticationConfiguration(new AuthenticationConfiguration(AuthenticationScheme.Bearer, "Bot Token")
				.WithField(TelegramConnectionParameters.BotToken, DataType.String, f =>
				{
					f.DisplayName = "Bot Token";
					f.Description = "Telegram Bot Token obtained from @BotFather";
					f.AuthenticationRole = "principal";
					f.IsSensitive = true;
				}))
			.HandlesMessageEndpoint(EndpointType.Id, e =>
			{
				e.CanSend = true;
				e.CanReceive = true;
				e.IsRequired = true;
				e.Description = "Telegram Chat ID";
			})
			.Build();
		}

		/// <summary>
		/// Creates a webhook-enabled Telegram Bot channel schema.
		/// </summary>
		/// <returns>A channel schema configured for webhook-based real-time messaging.</returns>
		public static IChannelSchema CreateWebhookSchema()
		{
			return new ChannelSchemaBuilder(TelegramConnectorConstants.Provider, "webhook-bot", "1.0.0")
				.WithDisplayName("Telegram Webhook Bot")
				.WithCapabilities(
					ChannelCapability.SendMessages |
					ChannelCapability.ReceiveMessages |
					ChannelCapability.HandleMessageState |
					ChannelCapability.HealthCheck)
            .AddParameter(new ChannelParameter(TelegramConnectionParameters.BotToken, DataType.String)
            {
                IsRequired = true,
                IsSensitive = true,
                Description = "Telegram Bot Token obtained from @BotFather"
            })
            .AddParameter(new ChannelParameter(TelegramConnectionParameters.WebhookUrl, DataType.String)
            {
                IsRequired = true,
                Description = "HTTPS webhook URL for receiving bot updates"
            })
            .AddParameter(new ChannelParameter(TelegramConnectionParameters.SecretToken, DataType.String)
            {
                IsRequired = true,
                IsSensitive = true,
                Description = "Secret token for webhook validation"
            })
            .AddParameter(new ChannelParameter(TelegramConnectionParameters.MaxConnections, DataType.Integer)
            {
                IsRequired = false,
                DefaultValue = TelegramConnectionSettingsDefaults.MaxConnections,
                Description = "Maximum allowed number of simultaneous HTTPS connections to the webhook"
            })
            .AddParameter(new ChannelParameter(TelegramConnectionParameters.DropPendingUpdates, DataType.Boolean)
            {
                IsRequired = false,
                DefaultValue = TelegramConnectionSettingsDefaults.DropPendingUpdates,
                Description = "Drop all pending updates when setting webhook"
            })
				.AddContentType(MessageContentType.PlainText)
				.AddContentType(MessageContentType.Media)
				.AddContentType(MessageContentType.Location)
				.AddContentType(MessageContentType.Json) // For custom data
				.AddAuthenticationConfiguration(new AuthenticationConfiguration(AuthenticationScheme.Bearer, "Bot Token")
					.WithField(TelegramConnectionParameters.BotToken, DataType.String, f =>
					{
						f.DisplayName = "Bot Token";
						f.Description = "Telegram Bot Token obtained from @BotFather";
						f.AuthenticationRole = "principal";
						f.IsSensitive = true;
					}))
				.HandlesMessageEndpoint(EndpointType.Id, e =>
				{
					e.CanSend = true;
					e.CanReceive = true;
					e.IsRequired = true;
					e.Description = "Telegram Chat ID";
				})
				.Build();
		}
	}
}