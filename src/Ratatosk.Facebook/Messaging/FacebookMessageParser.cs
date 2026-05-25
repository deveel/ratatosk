//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.Text.Json;

namespace Ratatosk
{
    /// <summary>
    /// Provides static methods for parsing Facebook Messenger webhook payloads
    /// into <see cref="IMessage"/> instances.
    /// </summary>
    internal static class FacebookMessageParser
    {
        /// <summary>
        /// Parses a Facebook webhook <see cref="MessageSource"/> and returns the list
        /// of messages found in the payload.
        /// </summary>
        public static List<IMessage> ParseWebhook(MessageSource source, string? pageId)
        {
            var messages = new List<IMessage>();
            var jsonData = source.AsJson<JsonElement>();

            if (!jsonData.TryGetProperty("object", out var objectProperty) ||
                objectProperty.GetString() != "page")
            {
                return messages;
            }

            if (!jsonData.TryGetProperty("entry", out var entryArray))
                return messages;

            foreach (var entry in entryArray.EnumerateArray())
            {
                if (entry.TryGetProperty("messaging", out var messagingArray))
                {
                    foreach (var messagingEvent in messagingArray.EnumerateArray())
                    {
                        var message = ParseMessagingEvent(messagingEvent, pageId);
                        if (message != null)
                            messages.Add(message);
                    }
                }
            }

            return messages;
        }

        /// <summary>
        /// Parses a single messaging event JSON element into an <see cref="IMessage"/>.
        /// Returns <c>null</c> if the event cannot be converted to a message.
        /// </summary>
        private static IMessage? ParseMessagingEvent(JsonElement messagingEvent, string? pageId)
        {
            // Check if this is a message event (not postback, delivery, etc.)
            if (!messagingEvent.TryGetProperty("message", out var messageProperty))
                return null;

            // Extract sender and recipient
            if (!messagingEvent.TryGetProperty("sender", out var senderProperty) ||
                !messagingEvent.TryGetProperty("recipient", out var recipientProperty))
                return null;

            var senderId = senderProperty.GetProperty("id").GetString();
            var recipientId = recipientProperty.GetProperty("id").GetString();

            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(recipientId))
                return null;

            // Extract message ID and timestamp
            var messageId = messageProperty.TryGetProperty("mid", out var midProperty)
                ? midProperty.GetString() ?? Guid.NewGuid().ToString()
                : Guid.NewGuid().ToString();

            var timestamp = messageProperty.TryGetProperty("timestamp", out var timestampProperty)
                ? DateTimeOffset.FromUnixTimeMilliseconds(timestampProperty.GetInt64()).DateTime
                : DateTime.UtcNow;

            // Extract message content
            MessageContent? content = null;

            if (messageProperty.TryGetProperty("text", out var textProperty))
            {
                content = new TextContent(textProperty.GetString() ?? "");
            }
            else if (messageProperty.TryGetProperty("attachments", out var attachmentsProperty))
            {
                // Handle attachments — take the first one
                var firstAttachment = attachmentsProperty.EnumerateArray().FirstOrDefault();
                if (firstAttachment.ValueKind != JsonValueKind.Undefined)
                {
                    var type = GetMediaType(firstAttachment.GetProperty("type").GetString() ?? "file");
                    var payload = firstAttachment.GetProperty("payload");
                    var url = payload.GetProperty("url").GetString() ?? "";
                    content = new MediaContent(type, "", url);
                }
            }

            if (content == null)
                return null;

            return new Message
            {
                Id = messageId,
                Sender = new Endpoint(EndpointType.UserId, senderId),
                Receiver = new Endpoint(EndpointType.UserId, recipientId),
                Content = content,
                Properties = new Dictionary<string, MessageProperty>
                {
                    { "Timestamp", new MessageProperty("Timestamp", timestamp.ToString()) },
                    { "PageId", new MessageProperty("PageId", pageId ?? "") }
                }
            };
        }

        private static MediaType GetMediaType(string type)
        {
            return type.ToLowerInvariant() switch
            {
                "image" => MediaType.Image,
                "video" => MediaType.Video,
                "audio" => MediaType.Audio,
                _ => MediaType.File
            };
        }
    }
}

