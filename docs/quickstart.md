# Quickstart

Every interaction with the framework follows the same lifecycle regardless of which channel provider you use:

1. **Configure** — build a `ConnectionSettings` with the provider credentials
2. **Initialize** — create the connector and call `InitializeAsync()` to authenticate and transition to ready state
3. **Build** — construct an `IMessage` using the fluent `Message` builder
4. **Send** — call `SendMessageAsync()` and handle the `OperationResult<T>`
5. **Handle** — check `.IsSuccess`, read `.Data`, or inspect `.Error`

This guide walks through each step twice: first with direct instantiation (clear, explicit, good for understanding), then with DI registration (the pattern you would use in a real application).

## 1. Create a project

```bash
dotnet new console -o MessagingDemo
cd MessagingDemo
dotnet add package Deveel.Messaging.Connector.Twilio
```

## 2. Send an SMS (direct instantiation)

```csharp
using Deveel.Messaging;

// Connection settings: what the connector needs to talk to the provider
var settings = new ConnectionSettings()
    .SetParameter("AccountSid", "AC...")
    .SetParameter("AuthToken", "...");

// Create the connector with its schema and settings
var connector = new TwilioSmsConnector(TwilioChannelSchemas.SimpleSms, settings);

// Initialize opens the connection, authenticates, and transitions to Ready state
await connector.InitializeAsync(CancellationToken.None);

// Build the message using the fluent builder
var message = new Message()
    .WithId("sms-demo-1")
    .WithPhoneSender("+15550001111")
    .WithPhoneReceiver("+15550002222")
    .WithTextContent("Hello from Deveel Messaging");

// Send returns OperationResult<SendResult>
var result = await connector.SendMessageAsync(message, CancellationToken.None);

if (result.IsSuccess)
{
    Console.WriteLine($"Sent! Local ID: {result.Data?.MessageId}");
    Console.WriteLine($"Remote ID: {result.Data?.RemoteMessageId}");
    Console.WriteLine($"Status: {result.Data?.Status}");
}
else if (result.IsValidationFailure)
{
    Console.WriteLine("Validation failed:");
    foreach (var error in result.ValidationErrors)
        Console.WriteLine($"  - {error.ErrorMessage}");
}
else
{
    Console.WriteLine($"Send failed: [{result.Error?.ErrorCode}] {result.Error?.ErrorMessage}");
}
```

## 3. Same code, different channel

Swap the connector and endpoint types — the `IMessage` construction and send pattern stay the same:

```csharp
// Email via SendGrid
var email = new Message()
    .WithId("email-demo-1")
    .WithEmailSender("noreply@example.com")
    .WithEmailReceiver("user@example.com")
    .WithTextContent("Hello from Deveel Messaging")
    .With("Subject", "Welcome");

var sendGridConnector = new SendGridEmailConnector(
    SendGridChannelSchemas.SendGridEmail,
    new ConnectionSettings().SetParameter("ApiKey", "SG..."));

await sendGridConnector.InitializeAsync(ct);
var result = await sendGridConnector.SendMessageAsync(email, ct);
```

The `Message` class, the `SendMessageAsync` call, and the `OperationResult<T>` handling are identical. Only the connector type, settings, and endpoint types change.

## 4. DI registration path

For real applications, register connectors in the DI container:

```csharp
// Program.cs
using Deveel.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMessaging()
    .AddConnector<TwilioSmsConnector>(cfg => cfg
        .WithSettings("Twilio"));

builder.Services.AddSingleton<NotificationService>();

var app = builder.Build();
app.Run();

// NotificationService.cs
public class NotificationService(IChannelConnector smsConnector)
{
    public async Task SendSmsAsync(string to, string text)
    {
        var message = new Message()
            .WithId(Guid.NewGuid().ToString("n"))
            .WithPhoneSender("+15550001111")
            .WithPhoneReceiver(to)
            .WithTextContent(text);

        var result = await smsConnector.SendMessageAsync(message, default);

        if (result.IsFailure)
            throw new InvalidOperationException(
                $"SMS failed: {result.Error?.ErrorMessage}");
    }
}
```

## 5. Receive inbound messages (webhook)

Channels like Twilio, Telegram, and Facebook can push inbound messages and status updates to your application via webhooks. The connector normalizes these into the same `IMessage` model you use for outbound sends, so receiving follows the same patterns as sending.

For channels that support receiving messages, use `ReceiveMessagesAsync`:

```csharp
// ASP.NET Core controller endpoint
[HttpPost("/webhooks/twilio")]
public async Task<IActionResult> TwilioWebhook(
    [FromBody] MessageSource source,
    CancellationToken ct)
{
    var result = await connector.ReceiveMessagesAsync(source, ct);

    if (result.IsSuccess)
    {
        foreach (var message in result.Data?.Messages ?? [])
        {
            Console.WriteLine($"Received: {message.Id} from {message.Sender}");
            // Process inbound message...
        }
        return Ok();
    }

    return BadRequest(result.Error?.ErrorMessage);
}
```

## Next steps

- [Message model](messaging-model.md) — all content types, advanced builder methods, message properties
- [Channel schema](channel-schema.md) — defining connector contracts and validation rules
- [Connector guides](connectors/README.md) — per-provider connection parameters and features
