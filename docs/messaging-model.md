# Message Model

At the core of the framework is the `IMessage` interface and its concrete implementation `Message`. Every piece of data that flows through the system — whether it is an SMS text, an email with attachments, a push notification, or a chat message — is represented as an `IMessage`. This unified representation is what makes multi-channel messaging possible: the same message object can be sent through different connectors without transformation.

`Message` is both the data class and the fluent builder. Construct with `new Message()` and chain `With*()` calls — each returns the same instance, so there is no `.Build()` or finalizer step. The design avoids allocating intermediate builder objects and keeps construction straightforward.

The message carries five pieces of information:
- **Identity** — a unique `Id` string you assign
- **Routing** — `Sender` and `Receiver` endpoints, each tagged with their type
- **Content** — the payload, wrapped in a typed content class
- **Metadata** — a dictionary of properties for per-message configuration

## IMessage interface

```csharp
public interface IMessage
{
    string Id { get; }
    IEndpoint? Sender { get; }
    IEndpoint? Receiver { get; }
    IMessageContent? Content { get; }
    IDictionary<string, IMessageProperty>? Properties { get; }
}
```

The concrete `Message` class implements this interface and adds mutable setters and fluent builder methods.

## Basic construction

```csharp
var message = new Message()
    .WithId("msg-1")
    .WithEmailSender("alice@example.com")
    .WithEmailReceiver("bob@example.com")
    .WithTextContent("Hello, Bob!")
    .With("Subject", "Greetings");
```

### Copy constructor

Create a new `Message` from an existing `IMessage`, producing an independent copy:

```csharp
var copy = new Message(original);
// copy.Id == original.Id, but it's a separate instance
```

This deep-copies the endpoint, content, and properties.

## Endpoints

An endpoint identifies who sent the message and who should receive it. Unlike passing raw strings, every endpoint carries a type tag that tells the schema validator what kind of address it is. This prevents mistakes like using an email address where a phone number is expected, and it enables connectors to enforce endpoint-specific rules.

`Endpoint` implements `IEndpoint` and provides typed static factories. Every endpoint carries a type tag and an address string.

```csharp
public enum EndpointType
{
    PhoneNumber, EmailAddress, Url, Topic,
    Id, UserId, ApplicationId, DeviceId, Label, Any
}
```

### Factory methods

```csharp
Endpoint.PhoneNumber("+15550001111")
Endpoint.EmailAddress("user@example.com")
Endpoint.Url("https://hooks.example.com/callback")
Endpoint.Id("chat-123")                    // generic ID
Endpoint.User("user-42")                   // user identifier
Endpoint.Device("fcm-device-token")        // push device token
Endpoint.Application("app-main")           // application identifier
Endpoint.AlphaNumeric("AD12345")           // alphanumeric sender ID
```

### Setting sender and receiver

```csharp
// Generic endpoint methods
new Message()
    .WithSender(Endpoint.PhoneNumber("+15550001111"))
    .WithReceiver(Endpoint.EmailAddress("user@example.com"));

// Convenience shortcuts for common types
new Message()
    .WithPhoneSender("+15550001111")
    .WithPhoneReceiver("+15550002222")
    .WithEmailSender("noreply@example.com")
    .WithEmailReceiver("user@example.com");
```

## Content types

Different channels support different kinds of content. SMS carries plain text, email supports HTML and attachments, chat apps handle media and locations, and push notifications can carry structured JSON payloads. The framework models this with separate content classes, each implementing `IMessageContent`. When you build a message, you choose the content type that matches your channel — and the schema validator confirms the connector supports it.

Eight content classes implement `IMessageContent`. The base class `MessageContent` provides a static factory `MessageContent.Create(IMessageContent?)` that auto-selects the correct subclass.

### TextContent

For plain text messages — the most common content type (SMS, chat, plain-text email):

```csharp
new Message().WithTextContent("Plain text message");
new Message().WithTextContent("Encoded text", "utf-16");
```

Properties: `Text`, `Encoding` (optional, defaults to UTF-8).

### HtmlContent

For rich HTML content, typically used in email:

```csharp
new Message().WithHtmlContent("<h1>Hello</h1><p>World</p>");

// With inline attachments (e.g., embedded images)
new Message().WithHtmlContent("<h1>Hello</h1><img src='cid:logo'/>", html =>
{
    html.Attachments.Add(new MessageAttachment(
        "logo", "logo.png", "image/png", base64ImageData));
});
```

Properties: `Html`, `Attachments` (list of `MessageAttachment`).

### MediaContent

For images, audio, video, and documents:

```csharp
// URL-based media (provider fetches the file)
var image = new MediaContent(MediaType.Image, "photo.jpg",
    "https://example.com/photo.jpg");
new Message().WithContent(image);

// Binary data upload
var pdf = new MediaContent(MediaType.Document, "report.pdf",
    fileUrl: null, fileData: pdfBytes);
new Message().WithContent(pdf);
```

`MediaType` enum: `Image`, `Audio`, `Video`, `Document`, `File`.

Properties: `MediaType`, `FileName`, `FileUrl`, `Data`.

### BinaryContent

For raw binary payloads with a MIME type:

```csharp
var binary = new BinaryContent(rawBytes, "application/octet-stream");
new Message().WithContent(binary);
```

Properties: `RawData`, `MimeType`.

### JsonContent

For structured JSON payloads:

```csharp
var json = new JsonContent("{\"key\": \"value\", \"count\": 42}");
new Message().WithContent(json);
```

Properties: `Json` (string).

### LocationContent

For geographical coordinates, used in chat channels:

```csharp
var location = new LocationContent(41.9028, 12.4964)   // Rome, Italy
    .WithHorizontalAccuracy(10.0)
    .WithLivePeriod(60)       // valid for 60 seconds
    .WithHeading(180)         // heading in degrees
    .WithProximityAlertRadius(50);  // alert within 50m

new Message().WithContent(location);
```

Properties: `Latitude`, `Longitude`, `HorizontalAccuracy`, `LivePeriod`, `Heading`, `ProximityAlertRadius`.

### TemplateContent

For provider-side template rendering. The template ID and parameters are sent to the provider, which merges them server-side:

```csharp
var template = new TemplateContent("welcome-template",
    new Dictionary<string, object?>
    {
        ["name"] = "Alice",
        ["link"] = "https://example.com/verify"
    });
new Message().WithContent(template);
```

Typical use: SendGrid dynamic templates, Twilio WhatsApp templates, Facebook message templates.

Properties: `TemplateId`, `Parameters`.

### MultipartContent

Combine multiple content parts into a single message (e.g., text + image):

```csharp
var multipart = new MultipartContent();
multipart.Parts.Add(new TextContent("Check out this photo"));
multipart.Parts.Add(new MediaContent(MediaType.Image, "photo.jpg",
    "https://example.com/photo.jpg"));
multipart.Parts.Add(new HtmlContent("<p>Check out <b>this</b> photo</p>"));

new Message().WithContent(multipart);
```

Each part can be any `IMessageContentPart` implementation: `TextContentPart`, `HtmlContentPart`.

## Message properties

Not everything about a message fits neatly into sender, receiver, and content. Email needs a subject line, SMS has validity periods and max prices, push notifications carry badges and sounds, and every provider has its own set of per-message knobs. Properties are the escape hatch — a key-value dictionary on the message that connectors interpret for channel-specific configuration.

Properties are arbitrary key-value metadata attached to a message. They carry per-message configuration that the connector interprets.

### Single property

```csharp
new Message()
    .With("priority", "high")
    .With("ttl", 3600);
```

### Bulk properties

```csharp
new Message().With(new Dictionary<string, object>
{
    ["ValidityPeriod"] = 3600,
    ["MaxPrice"] = 0.05,
    ["SmartEncoded"] = true
});
```

### With MessageProperty objects

```csharp
new Message().With(new Dictionary<string, MessageProperty>
{
    ["ApiKeyOverride"] = new MessageProperty("ApiKeyOverride", "custom-key"),
    ["IsTest"] = MessageProperty.Sensitive("IsTest", true)
});
```

Use `MessageProperty.Sensitive(name, value)` to mark a property that should be redacted in logs.

### Known constants

Predefined property keys for common scenarios:

| Constant | Key | Typical use |
|---|---|---|
| `KnownMessageProperties.Subject` | `"subject"` | Email subject line |
| `KnownMessageProperties.RemoteMessageId` | `"remoteMessageId"` | Provider-assigned message ID |
| `KnownMessageProperties.ReplyTo` | `"replyTo"` | Message being replied to |
| `KnownMessageProperties.CorrelationId` | `"correlationId"` | Cross-channel correlation |

```csharp
new Message()
    .WithSubject("Your order confirmation")
    .WithRemoteId("ext-msg-456")
    .WithReplyTo("msg-123");
```

These are convenience wrappers around `With(key, value)` — they produce the same result.

## Message batches

When you need to send many messages at once, individual `SendMessageAsync` calls create overhead. Many providers offer a bulk API that accepts multiple messages in a single HTTP request. The `MessageBatch` type wraps this pattern: collect your messages into a batch, send once, and get per-message results back.

For sending multiple messages in a single batch:

```csharp
var batch = new MessageBatch();
batch.Id = "batch-001";
batch.Messages.Add(msg1);
batch.Messages.Add(msg2);
batch.Messages.Add(msg3);

var result = await connector.SendBatchAsync(batch, ct);

if (result.IsSuccess)
{
    Console.WriteLine($"Batch {result.Data!.BatchId} sent");
    foreach (var (msgId, sendResult) in result.Data.MessageResults)
        Console.WriteLine($"  {msgId}: {sendResult.Status}");
}
```

`IMessageBatch` also supports a `Properties` dictionary for batch-level metadata.

## Message status lifecycle

A message passes through several states between creation and delivery. The framework models these with `MessageStatus`, from initial receipt through queuing, sending, delivery, and optional read receipts. Status updates arrive either by polling (`GetMessageStatusAsync`) or via webhook callbacks (`ReceiveMessageStatusAsync`).

The status lifecycle:

```csharp
public enum MessageStatus
{
    Unknown,          // initial state
    Received,         // received by the messaging system
    Queued,           // queued for delivery
    Routed,           // routed to the provider
    RouteFailed,      // routing to the provider failed
    Sent,             // accepted by the provider
    Delivered,        // delivered to the recipient device
    DeliveryFailed,   // delivery to the recipient failed
    Read,             // recipient read the message
    Deleted           // message was deleted
}
```

`IMessageState` captures a message's status at a point in time, used by `GetMessageStatusAsync` queries and `ReceiveMessageStatusAsync` webhook receivers.

```csharp
var statusResult = await connector.GetMessageStatusAsync("msg-1", ct);
if (statusResult.IsSuccess)
{
    foreach (var update in statusResult.Data!.Updates)
        Console.WriteLine($"[{update.Timestamp}] {update.Status}: {update.Description}");
}
```

## IMessageProperty

Represents a single named property value:

```csharp
public interface IMessageProperty
{
    string Name { get; }
    object? Value { get; }
    bool IsSensitive { get; }
}
```

The `MessageProperty` class implements this and provides `.IsSensitive` to control log redaction.

## IMessageContentPart

Content parts for multipart messages. The `TextContentPart` and `HtmlContentPart` classes implement both the content interface and the content-type-specific interface:

```csharp
var textPart = new TextContentPart("Hello from part 1");
var htmlPart = new HtmlContentPart("<p>Hello from part 2</p>");
```

## MessageAttachment

Used by `HtmlContent` for embedding files:

```csharp
var attachment = new MessageAttachment(
    id: "logo",
    fileName: "logo.png",
    mimeType: "image/png",
    content: base64Data
);
```

Properties: `Id`, `FileName`, `MimeType`, `Content` (base64-encoded string).
