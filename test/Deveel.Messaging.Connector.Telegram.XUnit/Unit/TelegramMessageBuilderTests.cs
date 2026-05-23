using System.Text.Json;
using Telegram.Bot.Types.ReplyMarkups;

namespace Deveel.Messaging
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "TelegramMessageBuilder")]
    public class TelegramMessageBuilderTests
    {
        [Fact]
        public void Should_ReturnNull_When_ExtractChatIdWithNonIdEndpoint()
        {
            var endpoint = new Endpoint(EndpointType.EmailAddress, "test@example.com");
            Assert.Null(TelegramMessageBuilder.ExtractChatId(endpoint));
        }

        [Fact]
        public void Should_ReturnNull_When_ExtractChatIdWithNullEndpoint()
        {
            Assert.Null(TelegramMessageBuilder.ExtractChatId(null));
        }

        [Fact]
        public void Should_ReturnNull_When_ExtractChatIdWithEmptyAddress()
        {
            var endpoint = new Endpoint(EndpointType.Id, "");
            Assert.Null(TelegramMessageBuilder.ExtractChatId(endpoint));
        }

        [Fact]
        public void Should_ReturnNumericChatId_When_ExtractChatIdWithNumericAddress()
        {
            var endpoint = new Endpoint(EndpointType.Id, "123456789");
            var chatId = TelegramMessageBuilder.ExtractChatId(endpoint);
            Assert.NotNull(chatId);
            Assert.Equal(123456789, chatId.Identifier);
        }

        [Fact]
        public void Should_ReturnStringChatId_When_ExtractChatIdWithUsername()
        {
            var endpoint = new Endpoint(EndpointType.Id, "@testuser");
            var chatId = TelegramMessageBuilder.ExtractChatId(endpoint);
            Assert.NotNull(chatId);
            Assert.Equal("@testuser", chatId.Username);
        }

        [Fact]
        public void Should_ReturnNull_When_GetMessageIntPropertyWithNonParsableValue()
        {
            var message = new Message
            {
                Id = "test",
                Properties = new Dictionary<string, MessageProperty>
                {
                    { "Count", new MessageProperty("Count", "not-a-number") }
                }
            };
            Assert.Null(TelegramMessageBuilder.GetMessageIntProperty(message, "Count"));
        }

        [Fact]
        public void Should_ReturnNull_When_GetMessagePropertyWithMissingProperty()
        {
            var message = new Message { Id = "test" };
            Assert.Null(TelegramMessageBuilder.GetMessageProperty(message, "Missing"));
        }

        [Fact]
        public void Should_ReturnParseModeMarkdown_When_GetMessageParseModeWithUnrecognizedMode()
        {
            var message = new Message { Id = "test" };
            var result = TelegramMessageBuilder.GetMessageParseMode(message, "unknown");
            Assert.Equal(Telegram.Bot.Types.Enums.ParseMode.Markdown, result);
        }

        [Fact]
        public void Should_ReturnNull_When_GetMessageParseModeWithNone()
        {
            var message = new Message
            {
                Id = "test",
                Properties = new Dictionary<string, MessageProperty>
                {
                    { "ParseMode", new MessageProperty("ParseMode", "None") }
                }
            };
            Assert.Null(TelegramMessageBuilder.GetMessageParseMode(message, null));
        }

        [Fact]
        public void Should_ReturnNull_When_CreateReplyMarkupWithInvalidInlineKeyboardJson()
        {
            var message = new Message
            {
                Id = "test",
                Properties = new Dictionary<string, MessageProperty>
                {
                    { "InlineKeyboard", new MessageProperty("InlineKeyboard", "not valid json") }
                }
            };
            Assert.Null(TelegramMessageBuilder.CreateReplyMarkup(message));
        }

        [Fact]
        public void Should_ReturnNull_When_CreateReplyMarkupWithNoKeyboard()
        {
            var message = new Message { Id = "test" };
            Assert.Null(TelegramMessageBuilder.CreateReplyMarkup(message));
        }

        [Fact]
        public void Should_Throw_When_CreateInlineKeyboardMarkupWithPhoneNumberButton()
        {
            var button = new ButtonContent("Call", ButtonType.PhoneNumber, "+1234567890");
            Assert.Throws<NotSupportedException>(() =>
                TelegramMessageBuilder.CreateInlineKeyboardMarkup(new[] { button }));
        }

        [Fact]
        public void Should_Throw_When_CreateInputFileWithNoUrlOrData()
        {
            var media = new MediaContent(MediaType.Image, "test.jpg", (byte[])null!);
            Assert.Throws<ArgumentException>(() =>
                TelegramMessageBuilder.CreateInputFile(media));
        }

        [Fact]
        public void Should_CreateInputFileFromData_When_MediaHasData()
        {
            var media = new MediaContent(MediaType.Image, "test.jpg", new byte[] { 0x01, 0x02 });
            var inputFile = TelegramMessageBuilder.CreateInputFile(media);
            Assert.NotNull(inputFile);
        }

        [Fact]
        public void Should_ReturnFalse_When_IsLocationMessageWithInvalidJson()
        {
            var jsonContent = new JsonContent("not valid json");
            Assert.False(TelegramMessageBuilder.IsLocationMessage(jsonContent));
        }

        [Fact]
        public void Should_ReturnFalse_When_IsLocationMessageWithoutCoordinates()
        {
            var jsonContent = new JsonContent("""{"name": "test"}""");
            Assert.False(TelegramMessageBuilder.IsLocationMessage(jsonContent));
        }

        [Fact]
        public void Should_ReturnTrue_When_IsLocationMessageWithCoordinates()
        {
            var jsonContent = new JsonContent("""{"latitude": 40.71, "longitude": -74.00}""");
            Assert.True(TelegramMessageBuilder.IsLocationMessage(jsonContent));
        }

        [Fact]
        public void Should_ReturnDefault_When_GetMessageBoolPropertyWithNonParsableValue()
        {
            var message = new Message
            {
                Id = "test",
                Properties = new Dictionary<string, MessageProperty>
                {
                    { "Flag", new MessageProperty("Flag", "not-a-bool") }
                }
            };
            Assert.False(TelegramMessageBuilder.GetMessageBoolProperty(message, "Flag"));
        }

        [Fact]
        public void Should_ReturnTrue_When_GetMessageBoolPropertyWithTrueValue()
        {
            var message = new Message
            {
                Id = "test",
                Properties = new Dictionary<string, MessageProperty>
                {
                    { "Flag", new MessageProperty("Flag", "true") }
                }
            };
            Assert.True(TelegramMessageBuilder.GetMessageBoolProperty(message, "Flag"));
        }

        [Fact]
        public void Should_CreateReplyKeyboardFromQuickReplies_When_CreateReplyKeyboardMarkup()
        {
            var quickReplies = new List<IQuickReplyContent>
            {
                new QuickReplyContent("Yes"),
                new QuickReplyContent("No")
            };
            var markup = TelegramMessageBuilder.CreateReplyKeyboardMarkup(quickReplies);
            Assert.NotNull(markup);
            Assert.True(markup.OneTimeKeyboard);
            Assert.True(markup.ResizeKeyboard);
        }

        [Fact]
        public void Should_CreateInlineKeyboardFromButtons_When_CreateInlineKeyboardMarkup()
        {
            var buttons = new List<IButtonContent>
            {
                new ButtonContent("Click", ButtonType.Postback, "payload"),
                new ButtonContent("Visit", ButtonType.Url, "https://example.com")
            };
            var markup = TelegramMessageBuilder.CreateInlineKeyboardMarkup(buttons);
            Assert.NotNull(markup);
            Assert.Equal(2, markup.InlineKeyboard.Count());
        }
    }
}
