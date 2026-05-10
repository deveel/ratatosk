//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;

using System.Text.Json;

namespace Deveel.Messaging
{
    /// <summary>
    /// Provides static methods for building Facebook Messenger API request objects
    /// from <see cref="IMessage"/> instances.
    /// </summary>
    internal static class FacebookMessageBuilder
    {
        /// <summary>
        /// Extracts the Facebook PSID (User ID) from an endpoint.
        /// Returns <c>null</c> if the endpoint is not of type <see cref="EndpointType.UserId"/>.
        /// </summary>
        public static string? ExtractUserId(IEndpoint? endpoint)
        {
            if (endpoint?.Type == EndpointType.UserId)
                return endpoint.Address;

            return null;
        }

        /// <summary>
        /// Builds a <see cref="FacebookMessageRequest"/> for the given message and recipient.
        /// </summary>
        public static FacebookMessageRequest BuildMessageRequest(IMessage message, string recipientId, ILogger? logger = null)
        {
            var request = new FacebookMessageRequest
            {
                Recipient = recipientId
            };

            // Apply message properties
            if (message.Properties != null)
            {
                foreach (var property in message.Properties)
                {
                    switch (property.Key.ToLowerInvariant())
                    {
                        case "messagingtype":
                            request.MessagingType = property.Value?.Value?.ToString() ?? "RESPONSE";
                            break;
                        case "notificationtype":
                            request.NotificationType = property.Value?.Value?.ToString() ?? "REGULAR";
                            break;
                        case "tag":
                            request.Tag = property.Value?.Value?.ToString();
                            break;
                    }
                }
            }

            request.Message = BuildFacebookMessage(message, logger);
            return request;
        }

        /// <summary>
        /// Builds a <see cref="FacebookMessage"/> from the given <see cref="IMessage"/>.
        /// </summary>
        public static FacebookMessage BuildFacebookMessage(IMessage message, ILogger? logger = null)
        {
            var fbMessage = new FacebookMessage();

            switch (message.Content?.ContentType)
            {
                case MessageContentType.PlainText when message.Content is ITextContent textContent:
                    fbMessage.Text = textContent.Text;
                    break;

                case MessageContentType.Media when message.Content is IMediaContent mediaContent:
                    fbMessage.Attachment = new FacebookAttachment
                    {
                        Type = GetAttachmentType(mediaContent.MediaType.ToString() ?? "file"),
                        Payload = new FacebookPayload
                        {
                            Url = mediaContent.FileUrl ?? string.Empty,
                            IsReusable = true
                        }
                    };
                    break;
            }

            // Add quick replies if specified
            if (message.Properties?.TryGetValue("QuickReplies", out var quickRepliesProperty) == true)
            {
                var quickRepliesJson = quickRepliesProperty.Value?.ToString();
                if (!string.IsNullOrEmpty(quickRepliesJson))
                {
                    try
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                        };
                        fbMessage.QuickReplies = JsonSerializer.Deserialize<List<FacebookQuickReply>>(quickRepliesJson, options);
                    }
                    catch (JsonException ex)
                    {
                        logger?.LogQuickRepliesParsingFailed(quickRepliesJson, ex);
                    }
                }
            }

            return fbMessage;
        }

        private static string GetAttachmentType(string mediaType)
        {
            return mediaType.ToLowerInvariant() switch
            {
                "image" => "image",
                "audio" => "audio",
                "video" => "video",
                _ => "file"
            };
        }
    }
}

