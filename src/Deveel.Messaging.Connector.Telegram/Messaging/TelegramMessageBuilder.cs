//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.Text.Json;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Deveel.Messaging
{
    /// <summary>
    /// Provides static methods for building Telegram Bot API request objects and
    /// extracting typed values from <see cref="IMessage"/> instances.
    /// </summary>
    internal static class TelegramMessageBuilder
    {
        /// <summary>
        /// Extracts a Telegram <see cref="ChatId"/> from an endpoint.
        /// Returns <c>null</c> if the endpoint is not of type <see cref="EndpointType.Id"/>.
        /// </summary>
        public static ChatId? ExtractChatId(IEndpoint? endpoint)
        {
            if (endpoint?.Type != EndpointType.Id)
                return null;

            var address = endpoint.Address;
            if (string.IsNullOrWhiteSpace(address))
                return null;

            // Try to parse as long (numeric chat ID)
            if (long.TryParse(address, out var chatIdLong))
                return new ChatId(chatIdLong);

            // Use as string (username)
            return new ChatId(address);
        }

        /// <summary>
        /// Gets a message property value as a string.
        /// </summary>
        public static string? GetMessageProperty(IMessage message, string propertyName)
        {
            if (message.Properties?.TryGetValue(propertyName, out var property) == true)
                return property.Value?.ToString();

            return null;
        }

        /// <summary>
        /// Gets a boolean property from the message, falling back to <paramref name="defaultValue"/>.
        /// </summary>
        public static bool? GetMessageBoolProperty(IMessage message, string propertyName, bool defaultValue = false)
        {
            var value = GetMessageProperty(message, propertyName);
            if (bool.TryParse(value, out var parsedValue))
                return parsedValue;

            return defaultValue;
        }

        /// <summary>
        /// Gets an integer property from the message, or <c>null</c> if not present or unparseable.
        /// </summary>
        public static int? GetMessageIntProperty(IMessage message, string propertyName)
        {
            var value = GetMessageProperty(message, propertyName);
            if (int.TryParse(value, out var parsedValue))
                return parsedValue;

            return null;
        }

        /// <summary>
        /// Determines the Telegram <see cref="ParseMode"/> for the message, using the
        /// connector-level default when the message does not specify one.
        /// </summary>
        public static ParseMode? GetMessageParseMode(IMessage message, string? defaultParseMode)
        {
            var parseModeString = GetMessageProperty(message, "ParseMode") ?? defaultParseMode;

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
        /// Creates a Telegram <see cref="IReplyMarkup"/> from message properties, or
        /// <c>null</c> when no keyboard is defined.
        /// </summary>
        public static IReplyMarkup? CreateReplyMarkup(IMessage message)
        {
            var inlineKeyboardJson = GetMessageProperty(message, "InlineKeyboard");
            if (!string.IsNullOrEmpty(inlineKeyboardJson))
            {
                try
                {
                    var keyboard = JsonSerializer.Deserialize<InlineKeyboardButton[][]>(inlineKeyboardJson);
                    if (keyboard != null)
                        return new InlineKeyboardMarkup(keyboard);
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
                        return new ReplyKeyboardMarkup(keyboard);
                }
                catch (JsonException)
                {
                    // Invalid JSON, ignore
                }
            }

            return null;
        }

        /// <summary>
        /// Creates an <see cref="InlineKeyboardMarkup"/> from a collection of buttons.
        /// </summary>
        public static InlineKeyboardMarkup CreateInlineKeyboardMarkup(IReadOnlyList<IButtonContent> buttons)
        {
            var rows = new List<InlineKeyboardButton[]>();
            foreach (var button in buttons)
            {
                var tgButton = button.ButtonType switch
                {
                    ButtonType.Url => InlineKeyboardButton.WithUrl(button.Text, button.Value ?? ""),
                    ButtonType.Postback => InlineKeyboardButton.WithCallbackData(button.Text, button.Value ?? button.Text),
                    _ => InlineKeyboardButton.WithCallbackData(button.Text, button.Value ?? button.Text)
                };
                rows.Add(new[] { tgButton });
            }

            return new InlineKeyboardMarkup(rows.ToArray());
        }

        /// <summary>
        /// Creates a <see cref="ReplyKeyboardMarkup"/> from a collection of quick replies.
        /// </summary>
        public static ReplyKeyboardMarkup CreateReplyKeyboardMarkup(IReadOnlyList<IQuickReplyContent> quickReplies)
        {
            var rows = new List<KeyboardButton[]>();
            foreach (var qr in quickReplies)
            {
                rows.Add(new[] { new KeyboardButton(qr.Title) });
            }

            return new ReplyKeyboardMarkup(rows.ToArray())
            {
                OneTimeKeyboard = true,
                ResizeKeyboard = true
            };
        }

        /// <summary>
        /// Creates an <see cref="InputFile"/> from a media content, preferring a URL over
        /// raw byte data.
        /// </summary>
        public static InputFile CreateInputFile(IMediaContent mediaContent)
        {
            if (!string.IsNullOrWhiteSpace(mediaContent.FileUrl))
                return InputFile.FromUri(mediaContent.FileUrl);

            if (mediaContent.Data != null && mediaContent.Data.Length > 0)
            {
                var fileName = mediaContent.FileName ?? "file";
                return InputFile.FromStream(new MemoryStream(mediaContent.Data), fileName);
            }

            throw new ArgumentException("Media content must have either a URL or raw data");
        }

        /// <summary>
        /// Checks whether a JSON content object contains latitude/longitude and therefore
        /// represents a location message.
        /// </summary>
        public static bool IsLocationMessage(IJsonContent jsonContent)
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
    }
}

