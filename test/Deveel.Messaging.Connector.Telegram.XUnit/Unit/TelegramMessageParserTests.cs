using System.Text.Json;

namespace Deveel.Messaging
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "TelegramMessageParser")]
    public class TelegramMessageParserTests
    {
        [Fact]
        public void Should_ReturnEmpty_When_ParseWebhookJsonWithNoMessageOrEditedMessage()
        {
            var source = MessageSource.Json("""{"update_id": 123}""");
            var messages = TelegramMessageParser.ParseWebhookJson(source);
            Assert.Empty(messages);
        }

        [Fact]
        public void Should_ParseTextMessage_When_ParseWebhookJsonWithTextMessage()
        {
            var source = MessageSource.Json("""
            {
                "update_id": 1,
                "message": {
                    "message_id": 100,
                    "from": {"id": 111, "is_bot": false, "first_name": "User"},
                    "chat": {"id": 222, "type": "private"},
                    "date": 1640995200,
                    "text": "Hello"
                }
            }
            """);
            var messages = TelegramMessageParser.ParseWebhookJson(source);
            Assert.Single(messages);
            var msg = messages[0];
            Assert.Equal("100", msg.Id);
            Assert.IsType<TextContent>(msg.Content);
            Assert.Equal("Hello", ((TextContent)msg.Content).Text);
        }

        [Fact]
        public void Should_ParseEditedMessage_When_ParseWebhookJsonWithEditedMessage()
        {
            var source = MessageSource.Json("""
            {
                "update_id": 2,
                "edited_message": {
                    "message_id": 200,
                    "from": {"id": 111, "is_bot": false, "first_name": "User"},
                    "chat": {"id": 222, "type": "private"},
                    "date": 1640995200,
                    "text": "Edited"
                }
            }
            """);
            var messages = TelegramMessageParser.ParseWebhookJson(source);
            Assert.Single(messages);
        }

        [Fact]
        public void Should_ReturnEmpty_When_ParseWebhookJsonWithMalformedData()
        {
            var source = MessageSource.Json("{ invalid json }");
            Assert.Throws<JsonException>(() => TelegramMessageParser.ParseWebhookJson(source));
        }

        [Fact]
        public void Should_ReturnNull_When_ParseMessageWithoutMessageId()
        {
            var json = """{"from": {"id": 1}, "chat": {"id": 2}}""";
            var element = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
            Assert.Null(TelegramMessageParser.ParseMessage(element));
        }

        [Fact]
        public void Should_ReturnNull_When_ParseMessageWithoutFromOrChat()
        {
            var json = """{"message_id": 1, "from": {"id": 1}}""";
            var element = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
            Assert.Null(TelegramMessageParser.ParseMessage(element));
        }

        [Fact]
        public void Should_ParseVideoMessage_When_ParseWebhookJsonWithVideo()
        {
            var source = MessageSource.Json("""
            {
                "update_id": 3,
                "message": {
                    "message_id": 300,
                    "from": {"id": 111, "is_bot": false, "first_name": "User"},
                    "chat": {"id": 222, "type": "private"},
                    "date": 1640995200,
                    "video": {"file_id": "video123", "file_size": 1024, "width": 640, "height": 480}
                }
            }
            """);
            var messages = TelegramMessageParser.ParseWebhookJson(source);
            Assert.Single(messages);
            Assert.IsType<MediaContent>(messages[0].Content);
            Assert.Equal(MediaType.Video, ((MediaContent)messages[0].Content).MediaType);
        }

        [Fact]
        public void Should_ParseAudioMessage_When_ParseWebhookJsonWithAudio()
        {
            var source = MessageSource.Json("""
            {
                "update_id": 4,
                "message": {
                    "message_id": 400,
                    "from": {"id": 111, "is_bot": false, "first_name": "User"},
                    "chat": {"id": 222, "type": "private"},
                    "date": 1640995200,
                    "audio": {"file_id": "audio123", "file_size": 2048, "duration": 120}
                }
            }
            """);
            var messages = TelegramMessageParser.ParseWebhookJson(source);
            Assert.Single(messages);
            Assert.IsType<MediaContent>(messages[0].Content);
            Assert.Equal(MediaType.Audio, ((MediaContent)messages[0].Content).MediaType);
        }

        [Fact]
        public void Should_ParseDocumentMessage_When_ParseWebhookJsonWithDocument()
        {
            var source = MessageSource.Json("""
            {
                "update_id": 5,
                "message": {
                    "message_id": 500,
                    "from": {"id": 111, "is_bot": false, "first_name": "User"},
                    "chat": {"id": 222, "type": "private"},
                    "date": 1640995200,
                    "document": {"file_id": "doc123", "file_size": 4096}
                }
            }
            """);
            var messages = TelegramMessageParser.ParseWebhookJson(source);
            Assert.Single(messages);
            Assert.IsType<MediaContent>(messages[0].Content);
            Assert.Equal(MediaType.Document, ((MediaContent)messages[0].Content).MediaType);
        }

        [Fact]
        public void Should_ParseReplyMessage_When_ParseWebhookJsonWithReplyToMessage()
        {
            var source = MessageSource.Json("""
            {
                "update_id": 6,
                "message": {
                    "message_id": 600,
                    "from": {"id": 111, "is_bot": false, "first_name": "User"},
                    "chat": {"id": 222, "type": "private"},
                    "date": 1640995200,
                    "text": "A reply",
                    "reply_to_message": {
                        "message_id": 1,
                        "from": {"id": 333, "is_bot": true, "first_name": "Bot"},
                        "chat": {"id": 222, "type": "private"},
                        "date": 1640995100,
                        "text": "Original"
                    }
                }
            }
            """);
            var messages = TelegramMessageParser.ParseWebhookJson(source);
            Assert.Single(messages);
            Assert.True(messages[0].Properties?.ContainsKey("ReplyToMessageId"));
        }

        [Fact]
        public void Should_ReturnTextContent_When_ParseMessageWithUnknownContent()
        {
            var json = """{"message_id": 1, "from": {"id": 1}, "chat": {"id": 2}, "date": 1640995200}""";
            var element = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
            var message = TelegramMessageParser.ParseMessage(element);
            Assert.NotNull(message);
            Assert.IsType<TextContent>(message.Content);
        }
    }
}
