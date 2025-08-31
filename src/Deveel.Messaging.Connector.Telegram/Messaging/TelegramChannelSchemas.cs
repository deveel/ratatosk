//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
	/// <summary>
	/// Provides predefined channel schemas for Telegram messaging connectors.
	/// </summary>
	/// <remarks>
	/// This static class contains commonly used Telegram channel schema configurations
	/// that can be used directly or as templates for custom implementations.
	/// </remarks>
	public static class TelegramChannelSchemas
	{
		/// <summary>
		/// Gets a comprehensive Telegram Bot schema that supports all messaging capabilities.
		/// </summary>
		/// <value>
		/// A channel schema configured for full-featured Telegram Bot messaging including:
		/// - Send and receive messages
		/// - Support for text, media, location, and document content
		/// - Webhook and long polling support
		/// - Inline keyboards and reply markup
		/// - Health monitoring and status tracking
		/// </value>
		public static IChannelSchema TelegramBot => TelegramBotSchemaFactory.CreateSchema();

		/// <summary>
		/// Gets a simple Telegram Bot schema for basic text messaging only.
		/// </summary>
		/// <value>
		/// A channel schema configured for basic text messaging including:
		/// - Send text messages only
		/// - No media support
		/// - No webhook configuration
		/// - Basic error handling
		/// </value>
		public static IChannelSchema SimpleTelegramBot => TelegramBotSchemaFactory.CreateSimpleSchema();

		/// <summary>
		/// Gets a notification-focused Telegram Bot schema for sending alerts and updates.
		/// </summary>
		/// <value>
		/// A channel schema configured for notification messaging including:
		/// - Send messages only (no receiving)
		/// - Support for text and media content
		/// - Silent notification options
		/// - Channel/group messaging support
		/// </value>
		public static IChannelSchema NotificationBot => TelegramBotSchemaFactory.CreateNotificationSchema();

		/// <summary>
		/// Gets a webhook-enabled Telegram Bot schema for real-time message processing.
		/// </summary>
		/// <value>
		/// A channel schema configured for webhook-based messaging including:
		/// - Bidirectional messaging
		/// - Webhook configuration required
		/// - Real-time message processing
		/// - Status update handling
		/// </value>
		public static IChannelSchema WebhookBot => TelegramBotSchemaFactory.CreateWebhookSchema();
	}
}