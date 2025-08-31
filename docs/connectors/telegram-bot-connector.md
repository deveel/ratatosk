# Telegram Bot Connector Documentation

The Telegram Bot Connector provides comprehensive messaging capabilities through the Telegram Bot API, including bidirectional messaging, media support, inline keyboards, location sharing, and webhook integration.

## Table of Contents

1. [Overview](#overview)
2. [Installation](#installation)
3. [Schema Specifications](#schema-specifications)
4. [Connection Parameters](#connection-parameters)
5. [Message Properties](#message-properties)
6. [Usage Examples](#usage-examples)
7. [Webhook Integration](#webhook-integration)
8. [Error Handling](#error-handling)
9. [Best Practices](#best-practices)

## Overview

The `TelegramBotConnector` implements messaging using the Telegram Bot API. It supports:

- **Send Messages**: Text, media, location, and document messages
- **Receive Messages**: Webhook-based and long polling message receiving
- **Interactive Elements**: Inline keyboards and reply markup
- **Media Support**: Photos, videos, audio, documents
- **Location Sharing**: Live location and proximity alerts
- **Health Monitoring**: Connection testing and diagnostics

### Capabilities

| Capability | Supported | Description |
|------------|-----------|-------------|
| SendMessages | ? | Send messages to Telegram chats |
| ReceiveMessages | ? | Receive messages via webhooks |
| MessageStatusQuery | ? | Query message delivery status |
| HandleMessageState | ? | Process message state callbacks |
| HealthCheck | ? | Monitor connector health |

## Installation

```bash
dotnet add package Deveel.Messaging.Connector.Telegram
```

## Schema Specifications

### Base Schema: TelegramBot

```csharp
var schema = TelegramChannelSchemas.TelegramBot;
// Provider: "telegram"
// Type: "bot" 
// Version: "1.0.0"
// Capabilities: SendMessages | ReceiveMessages | MessageStatusQuery | HandleMessageState | HealthCheck
```

### Available Schema Variants

| Schema | Description | Use Case |
|--------|-------------|----------|
| `TelegramBot` | Full-featured bot with all capabilities | Complete Telegram integration |
| `SimpleTelegramBot` | Basic send-only text messaging | Simple notifications |
| `NotificationBot` | Send notifications with media support | Alert systems |
| `WebhookBot` | Webhook-optimized real-time messaging | Interactive applications |

### Schema Comparison

```csharp
// Full featured schema
var fullSchema = TelegramChannelSchemas.TelegramBot;

// Simple text-only schema  
var simpleSchema = TelegramChannelSchemas.SimpleTelegramBot;
// Removes: ReceiveMessages, MessageStatusQuery, HandleMessageState capabilities
// Removes: WebhookUrl, SecretToken parameters
// Supports: Text content only

// Notification schema with media support
var notificationSchema = TelegramChannelSchemas.NotificationBot;
// Removes: ReceiveMessages, MessageStatusQuery, HandleMessageState capabilities
// Supports: Text and media content
// Adds: Priority and Silent message properties

// Webhook-optimized schema
var webhookSchema = TelegramChannelSchemas.WebhookBot;
// Requires: WebhookUrl, SecretToken parameters
// Optimized for real-time bidirectional messaging
```

## Connection Parameters

### Required Parameters

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `BotToken` | String | Telegram Bot Token from @BotFather | `"1234567890:ABCdefGHIjklMNOpqrsTUVwxyz"` |

### Optional Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `WebhookUrl` | String | null | HTTPS URL for receiving messages and updates |
| `SecretToken` | String | null | Secret token for webhook validation (recommended) |
| `DisableWebPagePreview` | Boolean | false | Disable link previews in messages |
| `DisableNotification` | Boolean | false | Send messages silently by default |
| `ParseMode` | String | "Markdown" | Default message parsing mode |
| `MaxRetries` | Integer | 3 | Maximum retry attempts for failed operations |
| `TimeoutSeconds` | Integer | 30 | Request timeout in seconds |
| `MaxConnections` | Integer | 40 | Maximum webhook connections |
| `DropPendingUpdates` | Boolean | false | Drop pending updates when setting webhook |

### Configuration Example

```csharp
var connectionSettings = new ConnectionSettings()
    .AddParameter("BotToken", "1234567890:ABCdefGHIjklMNOpqrsTUVwxyz")
    .AddParameter("WebhookUrl", "https://yourapp.com/webhooks/telegram")
    .AddParameter("SecretToken", "your-secret-token-here")
    .AddParameter("ParseMode", "HTML")
    .AddParameter("DisableNotification", false)
    .AddParameter("MaxRetries", 5)
    .AddParameter("TimeoutSeconds", 60);
```

## Message Properties

### Basic Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `ParseMode` | String | No | Override default parse mode (Markdown, MarkdownV2, HTML, None) |
| `DisableWebPagePreview` | Boolean | No | Disable web page preview for this message |
| `DisableNotification` | Boolean | No | Send this message silently |
| `ReplyToMessageId` | Integer | No | ID of the message to reply to |
| `InlineKeyboard` | String | No | JSON representation of inline keyboard markup |
| `ReplyKeyboard` | String | No | JSON representation of reply keyboard markup |

### Media Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Caption` | String | No | Caption for media messages (max 1024 characters) |
| `FileName` | String | No | File name for document messages |
| `Duration` | Integer | No | Duration in seconds for audio/video messages |
| `Width` | Integer | No | Width in pixels for video messages |
| `Height` | Integer | No | Height in pixels for video messages |

### Location Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `HorizontalAccuracy` | Number | No | Horizontal accuracy of location in meters |
| `LivePeriod` | Integer | No | Period in seconds for live location (60-86400) |
| `Heading` | Integer | No | Direction in degrees (1-360) |
| `ProximityAlertRadius` | Integer | No | Proximity alert radius in meters (1-100000) |

### Property Usage Examples

```csharp
var message = new MessageBuilder()
    .WithId("telegram-001")
    .WithIdSender("123456789") // Your bot's user ID
    .WithIdReceiver("-1001234567890") // Chat ID
    .WithTextContent("Your order *#12345* has been shipped!")
    .WithProperty("ParseMode", "Markdown")
    .WithProperty("DisableNotification", false)
    .WithProperty("ReplyToMessageId", 42)
    .Message;
```

## Usage Examples

### Basic Text Messaging

```csharp
using Deveel.Messaging;

// Create connector with simple schema
var schema = TelegramChannelSchemas.SimpleTelegramBot;
var connectionSettings = new ConnectionSettings()
    .AddParameter("BotToken", "your_bot_token_here");

var connector = new TelegramBotConnector(schema, connectionSettings);
await connector.InitializeAsync(cancellationToken);

// Create and send message
var message = new MessageBuilder()
    .WithId("welcome-msg")
    .WithIdReceiver("123456789") // User chat ID
    .WithTextContent("Welcome to our service! ??")
    .Message;

var result = await connector.SendMessageAsync(message, cancellationToken);
if (result.IsSuccess)
{
    Console.WriteLine($"Message sent! Telegram MessageId: {result.Value?.MessageId}");
}
```

### Rich Text with Markdown

```csharp
var richMessage = new MessageBuilder()
    .WithId("rich-text")
    .WithIdReceiver("123456789")
    .WithTextContent(@"
*Order Confirmation* #12345

?? *Items:*
- Product A (x2) - $20.00
- Product B (x1) - $15.00

?? *Total:* $35.00
?? *Estimated Delivery:* Tomorrow

[Track your order](https://example.com/track/12345)
")
    .WithProperty("ParseMode", "Markdown")
    .WithProperty("DisableWebPagePreview", false)
    .Message;

await connector.SendMessageAsync(richMessage, cancellationToken);
```

### Sending Photos with Captions

```csharp
var photoMessage = new MessageBuilder()
    .WithId("product-photo")
    .WithIdReceiver("123456789")
    .WithMediaContent("https://example.com/products/laptop.jpg", "image/jpeg")
    .WithProperty("Caption", "??? *MacBook Pro 16\"* - Only $1,999!\n\nFree shipping included ?")
    .WithProperty("ParseMode", "Markdown")
    .Message;

await connector.SendMessageAsync(photoMessage, cancellationToken);
```

### Interactive Messages with Inline Keyboards

```csharp
// Create inline keyboard JSON
var inlineKeyboard = new[]
{
    new[] // First row
    {
        new { text = "? Confirm Order", callback_data = "confirm_order_12345" },
        new { text = "? Cancel", callback_data = "cancel_order_12345" }
    },
    new[] // Second row
    {
        new { text = "?? Contact Support", url = "https://example.com/support" }
    }
};

var keyboardJson = JsonSerializer.Serialize(inlineKeyboard);

var interactiveMessage = new MessageBuilder()
    .WithId("order-confirmation")
    .WithIdReceiver("123456789")
    .WithTextContent("Please confirm your order #12345")
    .WithProperty("InlineKeyboard", keyboardJson)
    .Message;

await connector.SendMessageAsync(interactiveMessage, cancellationToken);
```

### Location Sharing

```csharp
var locationMessage = new MessageBuilder()
    .WithId("store-location")
    .WithIdReceiver("123456789")
    .WithLocationContent(40.7128, -74.0060) // New York coordinates
    .WithProperty("HorizontalAccuracy", 10.0)
    .WithProperty("LivePeriod", 3600) // Live location for 1 hour
    .Message;

await connector.SendMessageAsync(locationMessage, cancellationToken);
```

### Document Sharing

```csharp
var documentMessage = new MessageBuilder()
    .WithId("invoice-doc")
    .WithIdReceiver("123456789")
    .WithMediaContent("https://example.com/invoices/12345.pdf", "application/pdf")
    .WithProperty("Caption", "?? Your invoice for order #12345")
    .WithProperty("FileName", "Invoice_12345.pdf")
    .Message;

await connector.SendMessageAsync(documentMessage, cancellationToken);
```

### Group/Channel Messaging

```csharp
// Send to a group (negative chat ID)
var groupMessage = new MessageBuilder()
    .WithId("announcement")
    .WithIdReceiver("-1001234567890") // Group chat ID
    .WithTextContent("?? *Important Announcement*\n\nOur services will be under maintenance tonight from 11 PM to 2 AM.")
    .WithProperty("ParseMode", "Markdown")
    .WithProperty("DisableNotification", true) // Silent notification
    .Message;

await connector.SendMessageAsync(groupMessage, cancellationToken);

// Send to a channel by username
var channelMessage = new MessageBuilder()
    .WithId("channel-post")
    .WithIdReceiver("@yourchannel") // Channel username
    .WithTextContent("?? New feature released! Check it out in the app.")
    .Message;

await connector.SendMessageAsync(channelMessage, cancellationToken);
```

## Webhook Integration

### Setting Up Webhooks

```csharp
// Configure connector with webhook
var webhookSchema = TelegramChannelSchemas.WebhookBot;
var webhookSettings = new ConnectionSettings()
    .AddParameter("BotToken", "your_bot_token")
    .AddParameter("WebhookUrl", "https://yourapp.com/api/webhooks/telegram")
    .AddParameter("SecretToken", "your-secret-token")
    .AddParameter("MaxConnections", 40)
    .AddParameter("DropPendingUpdates", false);

var webhookConnector = new TelegramBotConnector(webhookSchema, webhookSettings);
await webhookConnector.InitializeAsync(cancellationToken);
```

### Webhook Controller

```csharp
[ApiController]
[Route("api/webhooks/telegram")]
public class TelegramWebhookController : ControllerBase
{
    private readonly TelegramBotConnector _connector;
    private readonly ILogger<TelegramWebhookController> _logger;

    public TelegramWebhookController(TelegramBotConnector connector, ILogger<TelegramWebhookController> logger)
    {
        _connector = connector;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook([FromBody] JsonElement webhookData)
    {
        try
        {
            // Validate secret token (recommended)
            var secretToken = Request.Headers["X-Telegram-Bot-Api-Secret-Token"].FirstOrDefault();
            if (secretToken != "your-secret-token")
            {
                return Unauthorized();
            }

            var messageSource = MessageSource.FromJson(webhookData);
            var result = await _connector.ReceiveMessagesAsync(messageSource, CancellationToken.None);
            
            if (result.IsSuccess)
            {
                foreach (var message in result.Value.Messages)
                {
                    await ProcessIncomingMessage(message);
                }
            }
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Telegram webhook");
            return StatusCode(500);
        }
    }

    private async Task ProcessIncomingMessage(IMessage message)
    {
        _logger.LogInformation("Received message from {SenderId}: {Content}", 
            message.Sender?.Address, message.Content?.Value);
        
        // Process different message types
        switch (message.Content)
        {
            case ITextContent textContent:
                await HandleTextMessage(message, textContent);
                break;
                
            case IMediaContent mediaContent:
                await HandleMediaMessage(message, mediaContent);
                break;
                
            case ILocationContent locationContent:
                await HandleLocationMessage(message, locationContent);
                break;
        }
    }

    private async Task HandleTextMessage(IMessage message, ITextContent textContent)
    {
        // Simple echo bot example
        if (textContent.Text?.StartsWith("/echo ") == true)
        {
            var responseText = textContent.Text[6..]; // Remove "/echo " prefix
            
            var response = new MessageBuilder()
                .WithId($"echo-{Guid.NewGuid()}")
                .WithIdSender(message.Receiver?.Address) // Bot's ID
                .WithIdReceiver(message.Sender?.Address) // User's ID
                .WithTextContent($"You said: {responseText}")
                .WithProperty("ReplyToMessageId", int.Parse(message.Id))
                .Message;
                
            await _connector.SendMessageAsync(response, CancellationToken.None);
        }
        // Command handling
        else if (textContent.Text == "/start")
        {
            await SendWelcomeMessage(message.Sender?.Address);
        }
        else if (textContent.Text == "/help")
        {
            await SendHelpMessage(message.Sender?.Address);
        }
    }

    private async Task HandleMediaMessage(IMessage message, IMediaContent mediaContent)
    {
        _logger.LogInformation("Received {MediaType} from {SenderId}", 
            mediaContent.MediaType, message.Sender?.Address);
        
        // Acknowledge media received
        var response = new MessageBuilder()
            .WithId($"media-ack-{Guid.NewGuid()}")
            .WithIdReceiver(message.Sender?.Address)
            .WithTextContent($"Thanks for the {mediaContent.MediaType.ToString().ToLower()}! ??")
            .Message;
            
        await _connector.SendMessageAsync(response, CancellationToken.None);
    }

    private async Task HandleLocationMessage(IMessage message, ILocationContent locationContent)
    {
        _logger.LogInformation("Received location from {SenderId}: {Lat}, {Lon}", 
            message.Sender?.Address, locationContent.Latitude, locationContent.Longitude);
        
        var response = new MessageBuilder()
            .WithId($"location-ack-{Guid.NewGuid()}")
            .WithIdReceiver(message.Sender?.Address)
            .WithTextContent($"?? Thanks for sharing your location!\nLat: {locationContent.Latitude:F6}\nLon: {locationContent.Longitude:F6}")
            .Message;
            
        await _connector.SendMessageAsync(response, CancellationToken.None);
    }

    private async Task SendWelcomeMessage(string? userId)
    {
        if (string.IsNullOrEmpty(userId)) return;
        
        var welcomeKeyboard = new[]
        {
            new[] 
            { 
                new { text = "?? Get Started", callback_data = "get_started" },
                new { text = "? Help", callback_data = "help" }
            },
            new[] 
            { 
                new { text = "?? Visit Website", url = "https://example.com" }
            }
        };

        var message = new MessageBuilder()
            .WithId($"welcome-{Guid.NewGuid()}")
            .WithIdReceiver(userId)
            .WithTextContent("?? *Welcome to our bot!*\n\nI can help you with various tasks. Use the buttons below or type /help for more information.")
            .WithProperty("ParseMode", "Markdown")
            .WithProperty("InlineKeyboard", JsonSerializer.Serialize(welcomeKeyboard))
            .Message;

        await _connector.SendMessageAsync(message, CancellationToken.None);
    }

    private async Task SendHelpMessage(string? userId)
    {
        if (string.IsNullOrEmpty(userId)) return;
        
        var helpText = @"
?? *Bot Commands:*

/start - Start the bot
/help - Show this help message
/echo [text] - Echo your message

?? *Features:*
• Send text messages
• Share photos and documents
• Share location
• Interactive buttons

?? Just send me a message and I'll respond!
";

        var message = new MessageBuilder()
            .WithId($"help-{Guid.NewGuid()}")
            .WithIdReceiver(userId)
            .WithTextContent(helpText)
            .WithProperty("ParseMode", "Markdown")
            .Message;

        await _connector.SendMessageAsync(message, CancellationToken.None);
    }
}
```

### Webhook Payload Examples

**Text Message Webhook:**
```json
{
  "update_id": 123456789,
  "message": {
    "message_id": 42,
    "from": {
      "id": 123456789,
      "is_bot": false,
      "first_name": "John",
      "username": "johndoe"
    },
    "chat": {
      "id": 123456789,
      "first_name": "John",
      "username": "johndoe",
      "type": "private"
    },
    "date": 1640995200,
    "text": "Hello bot!"
  }
}
```

**Callback Query Webhook:**
```json
{
  "update_id": 123456790,
  "callback_query": {
    "id": "callback_query_id",
    "from": {
      "id": 123456789,
      "is_bot": false,
      "first_name": "John"
    },
    "message": {
      "message_id": 42,
      "date": 1640995200
    },
    "data": "confirm_order_12345"
  }
}
```

### Webhook Security

```csharp
// Validate secret token
private bool ValidateSecretToken(HttpRequest request)
{
    var receivedToken = request.Headers["X-Telegram-Bot-Api-Secret-Token"].FirstOrDefault();
    var expectedToken = "your-configured-secret-token";
    
    return receivedToken == expectedToken;
}

// Additional IP validation (optional)
private bool ValidateSourceIP(HttpRequest request)
{
    var allowedIPRanges = new[]
    {
        "149.154.160.0/20",
        "91.108.4.0/22"
    };
    
    var remoteIP = request.HttpContext.Connection.RemoteIpAddress;
    // Implement IP range validation logic
    return true; // Placeholder
}
```

## Error Handling

### Common Error Codes

| Error Code | Description | Solution |
|------------|-------------|----------|
| `MISSING_BOT_TOKEN` | Bot token is missing or empty | Provide valid bot token from @BotFather |
| `INVALID_BOT_TOKEN` | Bot token format is invalid | Verify token format and validity |
| `INVALID_CHAT_ID` | Chat ID is invalid or missing | Use valid numeric chat ID or @username |
| `SEND_MESSAGE_FAILED` | Message sending failed | Check bot permissions and chat access |
| `BOT_BLOCKED` | Bot was blocked by user | Handle gracefully, remove from active users |
| `CHAT_NOT_FOUND` | Chat does not exist | Verify chat ID exists and bot has access |
| `MESSAGE_TOO_LONG` | Message exceeds 4096 characters | Split message or reduce content |
| `FILE_TOO_LARGE` | Media file exceeds size limits | Reduce file size or use external hosting |

### Error Handling Example

```csharp
var result = await connector.SendMessageAsync(message, cancellationToken);

if (!result.IsSuccess)
{
    switch (result.ErrorCode)
    {
        case TelegramErrorCodes.MissingBotToken:
            _logger.LogError("Bot token is missing. Check configuration.");
            break;
            
        case TelegramErrorCodes.InvalidChatId:
            _logger.LogWarning("Invalid chat ID: {ChatId}", message.Receiver?.Address);
            break;
            
        case TelegramErrorCodes.BotBlocked:
            _logger.LogInformation("Bot blocked by user: {UserId}", message.Receiver?.Address);
            await HandleBotBlocked(message.Receiver?.Address);
            break;
            
        case TelegramErrorCodes.MessageTooLong:
            _logger.LogWarning("Message too long, splitting...");
            await SplitAndSendMessage(message);
            break;
            
        case TelegramErrorCodes.SendMessageFailed:
            _logger.LogError("Send failed: {Error}", result.ErrorMessage);
            await ScheduleRetry(message);
            break;
            
        default:
            _logger.LogError("Unexpected error: {ErrorCode} - {ErrorMessage}", 
                result.ErrorCode, result.ErrorMessage);
            break;
    }
}
```

### Retry Logic with Exponential Backoff

```csharp
public async Task<ConnectorResult<SendResult>> SendWithRetryAsync(IMessage message, int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        var result = await connector.SendMessageAsync(message, CancellationToken.None);
        
        if (result.IsSuccess)
        {
            return result;
        }
        
        // Only retry on transient errors
        if (IsRetriableError(result.ErrorCode) && attempt < maxRetries)
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // Exponential backoff
            _logger.LogWarning("Attempt {Attempt} failed, retrying in {Delay}s: {Error}", 
                attempt, delay.TotalSeconds, result.ErrorMessage);
            
            await Task.Delay(delay);
            continue;
        }
        
        return result; // Return the last failed result
    }
    
    return ConnectorResult<SendResult>.Fail("MAX_RETRIES_EXCEEDED", "Maximum retry attempts reached");
}

private bool IsRetriableError(string? errorCode)
{
    return errorCode switch
    {
        "NETWORK_ERROR" => true,
        "RATE_LIMITED" => true,
        "TEMPORARY_FAILURE" => true,
        "SEND_MESSAGE_FAILED" => true, // May be transient
        _ => false
    };
}
```

## Best Practices

### 1. Bot Token Security

```csharp
// ? Good - Store token securely
var connectionSettings = new ConnectionSettings()
    .AddParameter("BotToken", Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN"));

// ? Avoid - Hardcoded tokens
var connectionSettings = new ConnectionSettings()
    .AddParameter("BotToken", "1234567890:ABCdefGHIjklMNOpqrsTUVwxyz");
```

### 2. Message Length Management

```csharp
// ? Good - Check and split long messages
public async Task SendLongMessageAsync(string text, string chatId)
{
    const int maxLength = 4096;
    
    if (text.Length <= maxLength)
    {
        await SendSimpleMessage(text, chatId);
        return;
    }
    
    // Split into chunks
    var chunks = SplitTextIntoChunks(text, maxLength);
    for (int i = 0; i < chunks.Count; i++)
    {
        var chunk = chunks[i];
        if (i == 0)
        {
            chunk = $"?? *Message ({i + 1}/{chunks.Count})*\n\n{chunk}";
        }
        else
        {
            chunk = $"?? *Continued ({i + 1}/{chunks.Count})*\n\n{chunk}";
        }
        
        await SendSimpleMessage(chunk, chatId);
        await Task.Delay(100); // Small delay between messages
    }
}

private List<string> SplitTextIntoChunks(string text, int maxLength)
{
    var chunks = new List<string>();
    var remaining = text;
    
    while (remaining.Length > maxLength)
    {
        var splitPoint = remaining.LastIndexOf(' ', maxLength);
        if (splitPoint == -1) splitPoint = maxLength;
        
        chunks.Add(remaining[..splitPoint]);
        remaining = remaining[splitPoint..].TrimStart();
    }
    
    if (remaining.Length > 0)
    {
        chunks.Add(remaining);
    }
    
    return chunks;
}
```

### 3. Media Handling

```csharp
// ? Good - Validate media before sending
public async Task<ConnectorResult<SendResult>> SendMediaWithValidationAsync(
    string mediaUrl, MediaType mediaType, string chatId, string? caption = null)
{
    // Validate URL accessibility
    using var httpClient = new HttpClient();
    try
    {
        var response = await httpClient.HeadAsync(mediaUrl);
        if (!response.IsSuccessStatusCode)
        {
            return ConnectorResult<SendResult>.Fail("MEDIA_INACCESSIBLE", 
                $"Media URL is not accessible: {response.StatusCode}");
        }
        
        // Check file size
        if (response.Content.Headers.ContentLength > GetMaxSizeForMediaType(mediaType))
        {
            return ConnectorResult<SendResult>.Fail("FILE_TOO_LARGE", 
                $"File size exceeds limit for {mediaType}");
        }
    }
    catch (Exception ex)
    {
        return ConnectorResult<SendResult>.Fail("MEDIA_VALIDATION_FAILED", ex.Message);
    }
    
    // Create and send message
    var message = new MessageBuilder()
        .WithId($"media-{Guid.NewGuid()}")
        .WithIdReceiver(chatId)
        .WithMediaContent(mediaUrl, GetMimeTypeForMediaType(mediaType))
        .WithProperty("Caption", caption)
        .Message;
    
    return await connector.SendMessageAsync(message, CancellationToken.None);
}

private long GetMaxSizeForMediaType(MediaType mediaType)
{
    return mediaType switch
    {
        MediaType.Image => TelegramConnectorConstants.MaxPhotoSize,
        MediaType.Video => TelegramConnectorConstants.MaxVideoSize,
        MediaType.Audio => TelegramConnectorConstants.MaxAudioSize,
        MediaType.Document => TelegramConnectorConstants.MaxDocumentSize,
        _ => TelegramConnectorConstants.MaxDocumentSize
    };
}
```

### 4. Inline Keyboard Design

```csharp
// ? Good - Create reusable keyboard components
public static class TelegramKeyboards
{
    public static string CreateConfirmationKeyboard(string actionId)
    {
        var keyboard = new[]
        {
            new[]
            {
                new { text = "? Confirm", callback_data = $"confirm_{actionId}" },
                new { text = "? Cancel", callback_data = $"cancel_{actionId}" }
            }
        };
        
        return JsonSerializer.Serialize(keyboard);
    }
    
    public static string CreatePaginationKeyboard(int currentPage, int totalPages, string prefix)
    {
        var buttons = new List<object[]>();
        
        // Page info row
        buttons.Add(new[]
        {
            new { text = $"?? Page {currentPage}/{totalPages}", callback_data = "noop" }
        });
        
        // Navigation row
        var navRow = new List<object>();
        
        if (currentPage > 1)
        {
            navRow.Add(new { text = "?? Previous", callback_data = $"{prefix}_page_{currentPage - 1}" });
        }
        
        if (currentPage < totalPages)
        {
            navRow.Add(new { text = "?? Next", callback_data = $"{prefix}_page_{currentPage + 1}" });
        }
        
        if (navRow.Count > 0)
        {
            buttons.Add(navRow.ToArray());
        }
        
        return JsonSerializer.Serialize(buttons);
    }
    
    public static string CreateMenuKeyboard(Dictionary<string, string> items)
    {
        var buttons = items.Select(item => new[]
        {
            new { text = item.Key, callback_data = item.Value }
        }).ToArray();
        
        return JsonSerializer.Serialize(buttons);
    }
}

// Usage
var confirmKeyboard = TelegramKeyboards.CreateConfirmationKeyboard("order_12345");
var paginationKeyboard = TelegramKeyboards.CreatePaginationKeyboard(2, 5, "products");
var menuKeyboard = TelegramKeyboards.CreateMenuKeyboard(new Dictionary<string, string>
{
    ["??? Browse Products"] = "browse_products",
    ["?? My Orders"] = "my_orders",
    ["?? Support"] = "contact_support",
    ["?? Settings"] = "user_settings"
});
```

### 5. User Context Management

```csharp
// ? Good - Maintain user conversation state
public class TelegramUserContext
{
    public string UserId { get; set; } = "";
    public string CurrentState { get; set; } = "idle";
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
}

public class TelegramConversationManager
{
    private readonly Dictionary<string, TelegramUserContext> _userContexts = new();
    private readonly TelegramBotConnector _connector;
    
    public async Task HandleUserMessage(IMessage message)
    {
        var userId = message.Sender?.Address;
        if (string.IsNullOrEmpty(userId)) return;
        
        var context = GetOrCreateUserContext(userId);
        context.LastActivity = DateTime.UtcNow;
        
        // Route message based on current state
        switch (context.CurrentState)
        {
            case "idle":
                await HandleIdleState(message, context);
                break;
                
            case "awaiting_name":
                await HandleNameInput(message, context);
                break;
                
            case "awaiting_email":
                await HandleEmailInput(message, context);
                break;
                
            default:
                await HandleUnknownState(message, context);
                break;
        }
    }
    
    private TelegramUserContext GetOrCreateUserContext(string userId)
    {
        if (!_userContexts.ContainsKey(userId))
        {
            _userContexts[userId] = new TelegramUserContext { UserId = userId };
        }
        
        return _userContexts[userId];
    }
    
    private async Task HandleIdleState(IMessage message, TelegramUserContext context)
    {
        if (message.Content is ITextContent textContent)
        {
            switch (textContent.Text?.ToLower())
            {
                case "/register":
                    context.CurrentState = "awaiting_name";
                    await SendMessage(context.UserId, "Please enter your full name:");
                    break;
                    
                case "/help":
                    await SendHelpMessage(context.UserId);
                    break;
                    
                default:
                    await SendMessage(context.UserId, "I didn't understand that. Type /help for available commands.");
                    break;
            }
        }
    }
    
    private async Task HandleNameInput(IMessage message, TelegramUserContext context)
    {
        if (message.Content is ITextContent textContent && !string.IsNullOrWhiteSpace(textContent.Text))
        {
            context.Data["name"] = textContent.Text;
            context.CurrentState = "awaiting_email";
            await SendMessage(context.UserId, $"Thanks, {textContent.Text}! Now please enter your email address:");
        }
        else
        {
            await SendMessage(context.UserId, "Please enter a valid name:");
        }
    }
    
    private async Task HandleEmailInput(IMessage message, TelegramUserContext context)
    {
        if (message.Content is ITextContent textContent && IsValidEmail(textContent.Text))
        {
            context.Data["email"] = textContent.Text;
            context.CurrentState = "idle";
            
            var name = context.Data["name"];
            await SendMessage(context.UserId, 
                $"? Registration complete!\n\nName: {name}\nEmail: {textContent.Text}\n\nWelcome aboard! ??");
        }
        else
        {
            await SendMessage(context.UserId, "Please enter a valid email address:");
        }
    }
    
    private async Task SendMessage(string userId, string text)
    {
        var message = new MessageBuilder()
            .WithId($"msg-{Guid.NewGuid()}")
            .WithIdReceiver(userId)
            .WithTextContent(text)
            .Message;
        
        await _connector.SendMessageAsync(message, CancellationToken.None);
    }
    
    private bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
```

### 6. Health Monitoring

```csharp
// ? Good - Implement comprehensive health monitoring
public class TelegramHealthMonitor
{
    private readonly TelegramBotConnector _connector;
    private readonly ILogger<TelegramHealthMonitor> _logger;
    private readonly Timer _healthCheckTimer;
    
    public TelegramHealthMonitor(TelegramBotConnector connector, ILogger<TelegramHealthMonitor> logger)
    {
        _connector = connector;
        _logger = logger;
        
        // Check health every 5 minutes
        _healthCheckTimer = new Timer(CheckHealth, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
    }
    
    private async void CheckHealth(object? state)
    {
        try
        {
            var healthResult = await _connector.GetHealthAsync(CancellationToken.None);
            
            if (healthResult.IsSuccess)
            {
                var health = healthResult.Value;
                
                if (!health.IsHealthy)
                {
                    _logger.LogWarning("Telegram connector is unhealthy: {Issues}", 
                        string.Join(", ", health.Issues));
                    
                    // Attempt to reconnect or notify administrators
                    await HandleUnhealthyConnector(health);
                }
                else
                {
                    _logger.LogDebug("Telegram connector health check passed. Uptime: {Uptime}", 
                        health.Uptime);
                }
            }
            else
            {
                _logger.LogError("Health check failed: {Error}", healthResult.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check encountered an exception");
        }
    }
    
    private async Task HandleUnhealthyConnector(ConnectorHealth health)
    {
        // Notify administrators
        var adminChatId = Environment.GetEnvironmentVariable("ADMIN_CHAT_ID");
        if (!string.IsNullOrEmpty(adminChatId))
        {
            try
            {
                var alertMessage = new MessageBuilder()
                    .WithId($"health-alert-{Guid.NewGuid()}")
                    .WithIdReceiver(adminChatId)
                    .WithTextContent($"?? *Telegram Bot Health Alert*\n\n" +
                                   $"State: {health.State}\n" +
                                   $"Issues: {string.Join(", ", health.Issues)}\n" +
                                   $"Last Check: {health.LastHealthCheck:yyyy-MM-dd HH:mm:ss} UTC")
                    .WithProperty("ParseMode", "Markdown")
                    .Message;
                
                await _connector.SendMessageAsync(alertMessage, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send health alert to administrators");
            }
        }
        
        // Attempt recovery
        if (health.Issues.Any(issue => issue.Contains("Connection")))
        {
            _logger.LogInformation("Attempting to reinitialize connector due to connection issues");
            
            try
            {
                await _connector.InitializeAsync(CancellationToken.None);
                _logger.LogInformation("Connector reinitialization successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reinitialize connector");
            }
        }
    }
    
    public void Dispose()
    {
        _healthCheckTimer?.Dispose();
    }
}
```

## Related Documentation

- [Connector Implementation Guide](../ChannelConnector-Usage.md)
- [Channel Schema Usage Guide](../ChannelSchema-Usage.md)
- [Message Content Types](../message-content-types.md)
- [Error Handling Best Practices](../error-handling.md)

## External Resources

- [Telegram Bot API Documentation](https://core.telegram.org/bots/api)
- [BotFather Commands](https://core.telegram.org/bots#6-botfather)
- [Telegram Bot Examples](https://core.telegram.org/bots/samples)
- [Webhook Setup Guide](https://core.telegram.org/bots/webhooks)