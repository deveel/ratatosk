//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.Text.Json;

namespace Ratatosk
{
    /// <summary>
    /// Provides static methods for parsing Telegram Bot webhook payloads
    /// into <see cref="IMessage"/> instances.
    /// </summary>
    internal static class TelegramMessageParser
    {
        /// <summary>
        /// Parses a Telegram webhook <see cref="MessageSource"/> and returns the list
        /// of messages found in the JSON payload.
        /// </summary>
        public static List<IMessage> ParseWebhookJson(MessageSource source)
        {
            var messages = new List<IMessage>();
            var jsonData = source.AsJson<JsonElement>();

            if (jsonData.TryGetProperty("message", out var messageElement))
            {
                var message = ParseMessage(messageElement);
                if (message != null)
                    messages.Add(message);
            }
            else if (jsonData.TryGetProperty("edited_message", out var editedMessageElement))
            {
                var message = ParseMessage(editedMessageElement);
                if (message != null)
                    messages.Add(message);
            }

            return messages;
        }

        /// <summary>
        /// Parses a single Telegram message JSON element into an <see cref="IMessage"/>.
        /// Returns <c>null</c> if the element cannot be converted to a message.
        /// </summary>
        public static IMessage? ParseMessage(JsonElement messageElement)
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
                    var largestPhoto = photoArray
                        .OrderByDescending(p => p.TryGetProperty("file_size", out var sizeElement) ? sizeElement.GetInt32() : 0)
                        .First();

                    if (largestPhoto.TryGetProperty("file_id", out var fileIdElement))
                        content = new MediaContent(MediaType.Image, fileIdElement.GetString() ?? "", "");
                }
            }
            else if (messageElement.TryGetProperty("video", out var videoElement))
            {
                if (videoElement.TryGetProperty("file_id", out var fileIdElement))
                    content = new MediaContent(MediaType.Video, fileIdElement.GetString() ?? "", "");
            }
            else if (messageElement.TryGetProperty("audio", out var audioElement))
            {
                if (audioElement.TryGetProperty("file_id", out var fileIdElement))
                    content = new MediaContent(MediaType.Audio, fileIdElement.GetString() ?? "", "");
            }
            else if (messageElement.TryGetProperty("document", out var documentElement))
            {
                if (documentElement.TryGetProperty("file_id", out var fileIdElement))
                    content = new MediaContent(MediaType.Document, fileIdElement.GetString() ?? "", "");
            }
            else if (messageElement.TryGetProperty("location", out var locationElement))
            {
                if (locationElement.TryGetProperty("latitude", out var latElement) &&
                    locationElement.TryGetProperty("longitude", out var lonElement))
                {
                    var locationContent = new LocationContent(latElement.GetDouble(), lonElement.GetDouble());

                    if (locationElement.TryGetProperty("horizontal_accuracy", out var accuracyElement))
                        locationContent.WithHorizontalAccuracy(accuracyElement.GetDouble());

                    if (locationElement.TryGetProperty("live_period", out var livePeriodElement))
                        locationContent.WithLivePeriod(livePeriodElement.GetInt32());

                    if (locationElement.TryGetProperty("heading", out var headingElement))
                        locationContent.WithHeading(headingElement.GetInt32());

                    if (locationElement.TryGetProperty("proximity_alert_radius", out var proximityElement))
                        locationContent.WithProximityAlertRadius(proximityElement.GetInt32());

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
    }
}

