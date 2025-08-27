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
	/// Defines the contract for Telegram Bot API operations.
	/// </summary>
	/// <remarks>
	/// This interface abstracts the Telegram Bot API client to enable dependency injection,
	/// unit testing, and provide a consistent interface for Telegram operations.
	/// </remarks>
	public interface ITelegramService
	{
		/// <summary>
		/// Initializes the Telegram service with the provided bot token.
		/// </summary>
		/// <param name="botToken">The Telegram bot token obtained from BotFather.</param>
		/// <exception cref="ArgumentNullException">Thrown when the bot token is null or empty.</exception>
		/// <exception cref="ArgumentException">Thrown when the bot token format is invalid.</exception>
		void Initialize(string botToken);

		/// <summary>
		/// Gets information about the bot.
		/// </summary>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous operation and contains the bot information.</returns>
		/// <exception cref="InvalidOperationException">Thrown when the service has not been initialized.</exception>
		Task<User> GetMeAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends a text message to the specified chat.
		/// </summary>
		/// <param name="chatId">The chat identifier.</param>
		/// <param name="text">The message text.</param>
		/// <param name="parseMode">The parse mode for the message (optional).</param>
		/// <param name="disableWebPagePreview">Whether to disable web page previews (optional).</param>
		/// <param name="disableNotification">Whether to send the message silently (optional).</param>
		/// <param name="replyToMessageId">The message ID to reply to (optional).</param>
		/// <param name="replyMarkup">The reply markup for inline keyboards (optional).</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous operation and contains the sent message.</returns>
		Task<Telegram.Bot.Types.Message> SendTextMessageAsync(
			ChatId chatId,
			string text,
			Telegram.Bot.Types.Enums.ParseMode? parseMode = null,
			bool? disableWebPagePreview = null,
			bool? disableNotification = null,
			int? replyToMessageId = null,
			IReplyMarkup? replyMarkup = null,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends a photo to the specified chat.
		/// </summary>
		/// <param name="chatId">The chat identifier.</param>
		/// <param name="photo">The photo to send (file ID, URL, or InputFile).</param>
		/// <param name="caption">The photo caption (optional).</param>
		/// <param name="parseMode">The parse mode for the caption (optional).</param>
		/// <param name="disableNotification">Whether to send the message silently (optional).</param>
		/// <param name="replyToMessageId">The message ID to reply to (optional).</param>
		/// <param name="replyMarkup">The reply markup for inline keyboards (optional).</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous operation and contains the sent message.</returns>
		Task<Telegram.Bot.Types.Message> SendPhotoAsync(
			ChatId chatId,
			InputFile photo,
			string? caption = null,
			Telegram.Bot.Types.Enums.ParseMode? parseMode = null,
			bool? disableNotification = null,
			int? replyToMessageId = null,
			IReplyMarkup? replyMarkup = null,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends a video to the specified chat.
		/// </summary>
		/// <param name="chatId">The chat identifier.</param>
		/// <param name="video">The video to send (file ID, URL, or InputFile).</param>
		/// <param name="duration">The video duration in seconds (optional).</param>
		/// <param name="width">The video width (optional).</param>
		/// <param name="height">The video height (optional).</param>
		/// <param name="thumb">The video thumbnail (optional).</param>
		/// <param name="caption">The video caption (optional).</param>
		/// <param name="parseMode">The parse mode for the caption (optional).</param>
		/// <param name="supportsStreaming">Whether the video supports streaming (optional).</param>
		/// <param name="disableNotification">Whether to send the message silently (optional).</param>
		/// <param name="replyToMessageId">The message ID to reply to (optional).</param>
		/// <param name="replyMarkup">The reply markup for inline keyboards (optional).</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous operation and contains the sent message.</returns>
		Task<Telegram.Bot.Types.Message> SendVideoAsync(
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
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends an audio file to the specified chat.
		/// </summary>
		/// <param name="chatId">The chat identifier.</param>
		/// <param name="audio">The audio file to send (file ID, URL, or InputFile).</param>
		/// <param name="caption">The audio caption (optional).</param>
		/// <param name="parseMode">The parse mode for the caption (optional).</param>
		/// <param name="duration">The audio duration in seconds (optional).</param>
		/// <param name="performer">The audio performer (optional).</param>
		/// <param name="title">The audio title (optional).</param>
		/// <param name="thumb">The audio thumbnail (optional).</param>
		/// <param name="disableNotification">Whether to send the message silently (optional).</param>
		/// <param name="replyToMessageId">The message ID to reply to (optional).</param>
		/// <param name="replyMarkup">The reply markup for inline keyboards (optional).</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous operation and contains the sent message.</returns>
		Task<Telegram.Bot.Types.Message> SendAudioAsync(
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
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends a document to the specified chat.
		/// </summary>
		/// <param name="chatId">The chat identifier.</param>
		/// <param name="document">The document to send (file ID, URL, or InputFile).</param>
		/// <param name="thumb">The document thumbnail (optional).</param>
		/// <param name="caption">The document caption (optional).</param>
		/// <param name="parseMode">The parse mode for the caption (optional).</param>
		/// <param name="disableContentTypeDetection">Whether to disable content type detection (optional).</param>
		/// <param name="disableNotification">Whether to send the message silently (optional).</param>
		/// <param name="replyToMessageId">The message ID to reply to (optional).</param>
		/// <param name="replyMarkup">The reply markup for inline keyboards (optional).</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous operation and contains the sent message.</returns>
		Task<Telegram.Bot.Types.Message> SendDocumentAsync(
			ChatId chatId,
			InputFile document,
			InputFile? thumb = null,
			string? caption = null,
			Telegram.Bot.Types.Enums.ParseMode? parseMode = null,
			bool? disableContentTypeDetection = null,
			bool? disableNotification = null,
			int? replyToMessageId = null,
			IReplyMarkup? replyMarkup = null,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends a location to the specified chat.
		/// </summary>
		/// <param name="chatId">The chat identifier.</param>
		/// <param name="latitude">The latitude coordinate.</param>
		/// <param name="longitude">The longitude coordinate.</param>
		/// <param name="livePeriod">The live period for live locations (optional).</param>
		/// <param name="heading">The direction in degrees (optional).</param>
		/// <param name="proximityAlertRadius">The proximity alert radius (optional).</param>
		/// <param name="disableNotification">Whether to send the message silently (optional).</param>
		/// <param name="replyToMessageId">The message ID to reply to (optional).</param>
		/// <param name="replyMarkup">The reply markup for inline keyboards (optional).</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous operation and contains the sent message.</returns>
		Task<Telegram.Bot.Types.Message> SendLocationAsync(
			ChatId chatId,
			double latitude,
			double longitude,
			int? livePeriod = null,
			int? heading = null,
			int? proximityAlertRadius = null,
			bool? disableNotification = null,
			int? replyToMessageId = null,
			IReplyMarkup? replyMarkup = null,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Sets the webhook URL for receiving bot updates.
		/// </summary>
		/// <param name="url">The webhook URL.</param>
		/// <param name="certificate">The certificate for self-signed certificates (optional).</param>
		/// <param name="ipAddress">The IP address to bind the webhook to (optional).</param>
		/// <param name="maxConnections">The maximum number of connections (optional).</param>
		/// <param name="allowedUpdates">The list of allowed update types (optional).</param>
		/// <param name="dropPendingUpdates">Whether to drop pending updates (optional).</param>
		/// <param name="secretToken">The secret token for webhook validation (optional).</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous operation.</returns>
		Task SetWebhookAsync(
			string url,
			InputFile? certificate = null,
			string? ipAddress = null,
			int? maxConnections = null,
			IEnumerable<Telegram.Bot.Types.Enums.UpdateType>? allowedUpdates = null,
			bool? dropPendingUpdates = null,
			string? secretToken = null,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Removes the webhook and returns to getUpdates mode.
		/// </summary>
		/// <param name="dropPendingUpdates">Whether to drop pending updates (optional).</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous operation.</returns>
		Task DeleteWebhookAsync(bool? dropPendingUpdates = null, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gets webhook information.
		/// </summary>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous operation and contains webhook information.</returns>
		Task<WebhookInfo> GetWebhookInfoAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Gets updates using long polling.
		/// </summary>
		/// <param name="offset">The identifier of the first update to be returned (optional).</param>
		/// <param name="limit">The maximum number of updates to retrieve (optional).</param>
		/// <param name="timeout">The timeout for long polling in seconds (optional).</param>
		/// <param name="allowedUpdates">The list of allowed update types (optional).</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous operation and contains the list of updates.</returns>
		Task<Update[]> GetUpdatesAsync(
			int? offset = null,
			int? limit = null,
			int? timeout = null,
			IEnumerable<Telegram.Bot.Types.Enums.UpdateType>? allowedUpdates = null,
			CancellationToken cancellationToken = default);
	}
}