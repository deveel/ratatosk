# ValidateMessage Usage

`ValidateMessage` is the safest pre-flight check before calling a connector.

It validates structure, endpoints, content type, and message properties in one pass.

## Basic pattern

```csharp
var issues = schema.ValidateMessage(message);

if (issues.Any())
{
    foreach (var issue in issues)
        logger.LogWarning("Validation: {Error}", issue.ErrorMessage);

    return; // stop before provider API call
}
```

## What it checks

- Message id presence
- Sender endpoint type and direction compatibility
- Receiver endpoint type and direction compatibility
- Content type support
- Required and typed message properties

## Typical failure examples

- Receiver is `EndpointType.Url` but schema only allows phone numbers
- Content is `Html` but schema only allows `PlainText`
- Required property like `Subject` is missing
- Unknown properties appear in strict mode

## Small schema + message example

```csharp
var schema = new ChannelSchema("SMTP", "Email", "1.0.0")
    .AddContentType(MessageContentType.PlainText)
    .AllowsMessageEndpoint(EndpointType.EmailAddress)
    .AddMessageProperty(new MessagePropertyConfiguration("Subject", DataType.String) { IsRequired = true });

var message = new MessageBuilder()
    .WithId("msg-001")
    .WithEmailSender("sender@example.com")
    .WithEmailReceiver("receiver@example.com")
    .WithTextContent("Hello")
    .WithProperty("Subject", "Welcome")
    .Message;
```

## Related docs

- [Validation test example](validatemessage-test-example.md)
- [Channel schema usage](channelschema-usage.md)
