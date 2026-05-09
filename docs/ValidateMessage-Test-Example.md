# ValidateMessage Test Example

Use this as a quick pattern for unit tests around schema validation.

## Example test fixture

```csharp
public class ValidateMessageTests
{
    private static IChannelSchema CreateEmailSchema() =>
        new ChannelSchema("SMTP", "Email", "1.0.0")
            .AddContentType(MessageContentType.PlainText)
            .AllowsMessageEndpoint(EndpointType.EmailAddress)
            .AddMessageProperty(new MessagePropertyConfiguration("Subject", DataType.String) { IsRequired = true });

    [Fact]
    public void ValidateMessage_ValidEmail_ReturnsNoErrors()
    {
        var schema = CreateEmailSchema();
        var message = new MessageBuilder()
            .WithId("msg-001")
            .WithEmailSender("sender@example.com")
            .WithEmailReceiver("receiver@example.com")
            .WithTextContent("Hello")
            .WithProperty("Subject", "Welcome")
            .Message;

        var issues = schema.ValidateMessage(message);
        Assert.Empty(issues);
    }

    [Fact]
    public void ValidateMessage_MissingRequiredProperty_ReturnsError()
    {
        var schema = CreateEmailSchema();
        var message = new MessageBuilder()
            .WithId("msg-002")
            .WithEmailSender("sender@example.com")
            .WithEmailReceiver("receiver@example.com")
            .WithTextContent("Hello")
            .Message;

        var issues = schema.ValidateMessage(message).ToList();
        Assert.NotEmpty(issues);
        Assert.Contains(issues, x => x.ErrorMessage?.Contains("Subject", StringComparison.OrdinalIgnoreCase) == true);
    }
}
```

## What to assert in practice

- No validation issues for happy-path messages
- Required property errors
- Endpoint direction/type errors
- Unsupported content type errors
- Strict-mode unknown property errors

This keeps connector tests focused and catches contract regressions early.
