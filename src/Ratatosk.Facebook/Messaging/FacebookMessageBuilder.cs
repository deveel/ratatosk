//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;

using System.Text.Json;

namespace Ratatosk
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

                case MessageContentType.QuickReply when message.Content is IQuickReplyContent qrContent:
                    fbMessage.QuickReplies = new List<FacebookQuickReply>
                    {
                        new FacebookQuickReply
                        {
                            ContentType = "text",
                            Title = qrContent.Title,
                            Payload = qrContent.Payload ?? qrContent.Title,
                            ImageUrl = qrContent.ImageUrl
                        }
                    };
                    break;

                case MessageContentType.Button when message.Content is IButtonContent btnContent:
                    fbMessage.Template = BuildButtonTemplate(btnContent);
                    break;

                case MessageContentType.Carousel when message.Content is ICarouselContent carouselContent:
                    fbMessage.Template = BuildCarouselTemplate(carouselContent);
                    break;

                case MessageContentType.ListPicker when message.Content is IListPickerContent listContent:
                    fbMessage.Template = BuildListTemplate(listContent);
                    break;
            }

            // Add quick replies if specified (backward compat with raw JSON property)
            if (fbMessage.QuickReplies == null &&
                message.Properties?.TryGetValue("QuickReplies", out var quickRepliesProperty) == true)
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

        private static FacebookTemplate BuildButtonTemplate(IButtonContent button)
        {
            return new FacebookTemplate
            {
                TemplateType = "button",
                Payload = new Dictionary<string, object>
                {
                    ["template_type"] = "button",
                    ["text"] = button.Text,
                    ["buttons"] = new[]
                    {
                        MapButton(button)
                    }
                }
            };
        }

        private static FacebookTemplate BuildCarouselTemplate(ICarouselContent carousel)
        {
            var elements = new List<object>();
            foreach (var card in carousel.Cards)
            {
                var element = new Dictionary<string, object>
                {
                    ["title"] = card.Title ?? "",
                };

                if (!string.IsNullOrEmpty(card.Subtitle))
                    element["subtitle"] = card.Subtitle;
                if (!string.IsNullOrEmpty(card.ImageUrl))
                    element["image_url"] = card.ImageUrl;
                if (card.Buttons.Count > 0)
                    element["buttons"] = card.Buttons.Select(MapButton).ToArray();

                elements.Add(element);
            }

            return new FacebookTemplate
            {
                TemplateType = "generic",
                Payload = new Dictionary<string, object>
                {
                    ["template_type"] = "generic",
                    ["elements"] = elements
                }
            };
        }

        private static FacebookTemplate BuildListTemplate(IListPickerContent listPicker)
        {
            var elements = new List<object>();
            foreach (var item in listPicker.Items)
            {
                var element = new Dictionary<string, object>
                {
                    ["title"] = item.Title
                };

                if (!string.IsNullOrEmpty(item.Description))
                    element["subtitle"] = item.Description;
                if (!string.IsNullOrEmpty(item.ImageUrl))
                    element["image_url"] = item.ImageUrl;

                elements.Add(element);
            }

            var payload = new Dictionary<string, object>
            {
                ["template_type"] = "list",
                ["top_element_style"] = listPicker.Style switch
                {
                    ListPickerStyle.Inlined => "compact",
                    ListPickerStyle.Compact => "compact",
                    ListPickerStyle.Large => "large",
                    _ => "large"
                },
                ["elements"] = elements
            };

            return new FacebookTemplate
            {
                TemplateType = "list",
                Payload = payload
            };
        }

        private static object MapButton(IButtonContent button)
        {
            return button.ButtonType switch
            {
                ButtonType.Url => new Dictionary<string, object>
                {
                    ["type"] = "web_url",
                    ["title"] = button.Text,
                    ["url"] = button.Value ?? ""
                },
                ButtonType.Postback => new Dictionary<string, object>
                {
                    ["type"] = "postback",
                    ["title"] = button.Text,
                    ["payload"] = button.Value ?? button.Text
                },
                ButtonType.PhoneNumber => new Dictionary<string, object>
                {
                    ["type"] = "phone_number",
                    ["title"] = button.Text,
                    ["payload"] = button.Value ?? ""
                },
                _ => new Dictionary<string, object>
                {
                    ["type"] = "postback",
                    ["title"] = button.Text,
                    ["payload"] = button.Value ?? button.Text
                }
            };
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

