//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;

namespace Deveel.Messaging
{
	/// <summary>
	/// Provides logging extensions for the Telegram Bot connector.
	/// </summary>
	internal static partial class LoggerExtensions
	{
		[LoggerMessage(
			EventId = TelegramLoggerEventId.BotInitialized,
			Level = LogLevel.Information,
			Message = "Bot initialized successfully: @{BotUsername} ({BotId})")]
		public static partial void LogBotInitialized(this ILogger logger, string? botUsername, long botId);

        [LoggerMessage(
            EventId = TelegramLoggerEventId.BotConnectionTestSuccessfull,
            Level = LogLevel.Information,
            Message = "Bot connection test successful: @{BotUsername} ({BotId})")]
        public static partial void LogBotConnectionTestSuccessful(this ILogger logger, string? botUsername, long botId);

		[LoggerMessage(
			EventId = 3002,
			Level = LogLevel.Information,
			Message = "Telegram message sent successfully. MessageId: {TelegramMessageId}, ChatId: {ChatId}")]
		public static partial void LogMessageSent(this ILogger logger, int telegramMessageId, long chatId);

		[LoggerMessage(
			EventId = 4001,
			Level = LogLevel.Debug,
			Message = "Receiving Telegram message from webhook")]
		public static partial void LogReceivingMessage(this ILogger logger);

		[LoggerMessage(
			EventId = 4002,
			Level = LogLevel.Error,
			Message = "Failed to receive Telegram message from webhook")]
		public static partial void LogReceiveMessageFailed(this ILogger logger, Exception exception);

		[LoggerMessage(
			EventId = 5001,
			Level = LogLevel.Debug,
			Message = "Setting up Telegram webhook: {WebhookUrl}")]
		public static partial void LogSettingUpWebhook(this ILogger logger, string webhookUrl);

		[LoggerMessage(
			EventId = 5002,
			Level = LogLevel.Information,
			Message = "Webhook set up successfully: {WebhookUrl}")]
		public static partial void LogWebhookSetUp(this ILogger logger, string webhookUrl);

		[LoggerMessage(
			EventId = 5003,
			Level = LogLevel.Error,
			Message = "Failed to set up webhook: {WebhookUrl}")]
		public static partial void LogWebhookSetupFailed(this ILogger logger, string webhookUrl, Exception exception);

		[LoggerMessage(
			EventId = 5004,
			Level = LogLevel.Information,
			Message = "Removing Telegram webhook...")]
		public static partial void LogRemovingWebhook(this ILogger logger);

		[LoggerMessage(
			EventId = 5005,
			Level = LogLevel.Information,
			Message = "Webhook removed successfully")]
		public static partial void LogWebhookRemoved(this ILogger logger);

		[LoggerMessage(
			EventId = 5006,
			Level = LogLevel.Warning,
			Message = "Failed to remove webhook during shutdown")]
		public static partial void LogWebhookRemovalFailed(this ILogger logger, Exception exception);
        
		[LoggerMessage(
			EventId = 6002,
			Level = LogLevel.Warning,
			Message = "Failed to get webhook info for status")]
		public static partial void LogGetWebhookInfoFailed(this ILogger logger, Exception exception);
	}
}
