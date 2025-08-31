//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
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
			return new ChannelSchema(TelegramConnectorConstants.Provider, TelegramConnectorConstants.BotChannel, "1.0.0")
				.WithDisplayName("Telegram Bot API")
				.WithCapabilities(
					ChannelCapability.SendMessages |
					ChannelCapability.ReceiveMessages |
					ChannelCapability.MessageStatusQuery |
					ChannelCapability.HandleMessageState |
					ChannelCapability.HealthCheck)
				.AddParameter(new ChannelParameter("BotToken", DataType.String)
				{
					IsRequired = true,
					IsSensitive = true,
					Description = "Telegram Bot Token obtained from @BotFather"
				})
				.AddParameter(new ChannelParameter("WebhookUrl", DataType.String)
				{
					IsRequired = false,
					Description = "Webhook URL for receiving messages (optional, uses long polling if not provided)"
				})
				.AddParameter(new ChannelParameter("SecretToken", DataType.String)
				{
					IsRequired = false,
					IsSensitive = true,
					Description = "Secret token for webhook validation (optional but recommended)"
				})
				.AddParameter(new ChannelParameter("DisableWebPagePreview", DataType.Boolean)
				{
					IsRequired = false,
					DefaultValue = false,
					Description = "Disable web page previews in messages"
				})
				.AddParameter(new ChannelParameter("DisableNotification", DataType.Boolean)
				{
					IsRequired = false,
					DefaultValue = false,
					Description = "Send messages silently (users will receive notification with no sound)"
				})
				.AddParameter(new ChannelParameter("ParseMode", DataType.String)
				{
					IsRequired = false,
					DefaultValue = "Markdown",
					Description = "Message parsing mode (Markdown, MarkdownV2, HTML, or None)"
				})
				.AddParameter(new ChannelParameter("MaxRetries", DataType.Integer)
				{
					IsRequired = false,
					DefaultValue = 3,
					Description = "Maximum number of retry attempts for failed operations"
				})
				.AddParameter(new ChannelParameter("TimeoutSeconds", DataType.Integer)
				{
					IsRequired = false,
					DefaultValue = 30,
					Description = "Request timeout in seconds"
				})
				.AddContentType(MessageContentType.PlainText)
				.AddContentType(MessageContentType.Media)
				.AddContentType(MessageContentType.Location)
				.AddContentType(MessageContentType.Json) // For custom data
				.HandlesMessageEndpoint(EndpointType.Id, e =>
				{
					e.CanSend = true;
					e.CanReceive = true;
					e.IsRequired = true;
					e.Description = "Telegram Chat ID (can be user ID, group ID, or channel username)";
				})
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
				});
		}

		/// <summary>
		/// Creates a simple Telegram Bot channel schema for basic text messaging.
		/// </summary>
		/// <returns>A channel schema configured for basic text messaging only.</returns>
		public static IChannelSchema CreateSimpleSchema()
		{
			return new ChannelSchema(TelegramConnectorConstants.Provider, "simple-bot", "1.0.0")
				.WithDisplayName("Simple Telegram Bot")
				.WithCapabilities(
					ChannelCapability.SendMessages |
					ChannelCapability.HealthCheck)
				.AddParameter(new ChannelParameter("BotToken", DataType.String)
				{
					IsRequired = true,
					IsSensitive = true,
					Description = "Telegram Bot Token obtained from @BotFather"
				})
				.AddParameter(new ChannelParameter("ParseMode", DataType.String)
				{
					IsRequired = false,
					DefaultValue = "Markdown",
					Description = "Message parsing mode (Markdown, MarkdownV2, HTML, or None)"
				})
				.AddContentType(MessageContentType.PlainText)
				.HandlesMessageEndpoint(EndpointType.Id, e =>
				{
					e.CanSend = true;
					e.CanReceive = false;
					e.IsRequired = true;
					e.Description = "Telegram Chat ID";
				});
		}

		/// <summary>
		/// Creates a notification-focused Telegram Bot channel schema.
		/// </summary>
		/// <returns>A channel schema configured for sending notifications and alerts.</returns>
		public static IChannelSchema CreateNotificationSchema()
		{
			return new ChannelSchema(TelegramConnectorConstants.Provider, "notification-bot", "1.0.0")
				.WithDisplayName("Telegram Notification Bot")
				.WithCapabilities(
					ChannelCapability.SendMessages |
					ChannelCapability.HealthCheck)
				.AddParameter(new ChannelParameter("BotToken", DataType.String)
				{
					IsRequired = true,
					IsSensitive = true,
					Description = "Telegram Bot Token obtained from @BotFather"
				})
				.AddParameter(new ChannelParameter("DefaultChatId", DataType.String)
				{
					IsRequired = false,
					Description = "Default chat ID for notifications (optional)"
				})
				.AddParameter(new ChannelParameter("DisableNotification", DataType.Boolean)
				{
					IsRequired = false,
					DefaultValue = false,
					Description = "Send notifications silently by default"
				})
				.AddParameter(new ChannelParameter("ParseMode", DataType.String)
				{
					IsRequired = false,
					DefaultValue = "HTML",
					Description = "Message parsing mode for notifications"
				})
				.AddContentType(MessageContentType.PlainText)
				.AddContentType(MessageContentType.Media)
				.HandlesMessageEndpoint(EndpointType.Id, e =>
				{
					e.CanSend = true;
					e.CanReceive = false;
					e.IsRequired = true;
					e.Description = "Telegram Chat ID (user, group, or channel)";
				})
				.AddMessageProperty(new MessagePropertyConfiguration("Priority", DataType.String)
				{
					IsRequired = false,
					Description = "Notification priority (Low, Normal, High, Critical)"
				})
				.AddMessageProperty(new MessagePropertyConfiguration("Silent", DataType.Boolean)
				{
					IsRequired = false,
					Description = "Send this notification silently"
				});
		}

		/// <summary>
		/// Creates a webhook-enabled Telegram Bot channel schema.
		/// </summary>
		/// <returns>A channel schema configured for webhook-based real-time messaging.</returns>
		public static IChannelSchema CreateWebhookSchema()
		{
			return new ChannelSchema(TelegramConnectorConstants.Provider, "webhook-bot", "1.0.0")
				.WithDisplayName("Telegram Webhook Bot")
				.WithCapabilities(
					ChannelCapability.SendMessages |
					ChannelCapability.ReceiveMessages |
					ChannelCapability.HandleMessageState |
					ChannelCapability.HealthCheck)
				.AddParameter(new ChannelParameter("BotToken", DataType.String)
				{
					IsRequired = true,
					IsSensitive = true,
					Description = "Telegram Bot Token obtained from @BotFather"
				})
				.AddParameter(new ChannelParameter("WebhookUrl", DataType.String)
				{
					IsRequired = true,
					Description = "HTTPS webhook URL for receiving bot updates"
				})
				.AddParameter(new ChannelParameter("SecretToken", DataType.String)
				{
					IsRequired = true,
					IsSensitive = true,
					Description = "Secret token for webhook validation"
				})
				.AddParameter(new ChannelParameter("MaxConnections", DataType.Integer)
				{
					IsRequired = false,
					DefaultValue = 40,
					Description = "Maximum allowed number of simultaneous HTTPS connections to the webhook"
				})
				.AddParameter(new ChannelParameter("DropPendingUpdates", DataType.Boolean)
				{
					IsRequired = false,
					DefaultValue = false,
					Description = "Drop all pending updates when setting webhook"
				})
				.AddContentType(MessageContentType.PlainText)
				.AddContentType(MessageContentType.Media)
				.AddContentType(MessageContentType.Location)
				.AddContentType(MessageContentType.Json) // For custom data
				.HandlesMessageEndpoint(EndpointType.Id, e =>
				{
					e.CanSend = true;
					e.CanReceive = true;
					e.IsRequired = true;
					e.Description = "Telegram Chat ID";
				});
		}
	}
}