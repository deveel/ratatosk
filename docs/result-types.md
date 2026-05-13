# Result Types

If every connector used its own error model — one throwing exceptions, another returning tuples, a third using custom error objects — the application code that calls connectors would be a mess of adapters and conditionals. The framework standardizes on a single result type for every operation: `OperationResult<T>`.

The result type carries three possible outcomes:
- **Success** — the operation completed, and `.Value` contains the result
- **Validation failure** — the input did not pass schema validation
- **Failure** — the operation failed, and `.Error` provides a machine-readable code and a human-readable message

This tri-state model avoids the ambiguity of throwing exceptions for validation errors (which are not exceptional — they indicate caller bugs) and distinguishes transient provider failures from preventable input errors.

Every connector operation returns an `OperationResult<T>`. This gives you a consistent pattern for handling success, failure, and validation errors across all channels.

## OperationResult\<T>

`OperationResult<T>` is the standard return type for all connector operations. It comes from the `Deveel.Results` package.

### Properties

| Property | Type | Description |
|---|---|---|
| `IsSuccess()` | `bool` | Operation completed successfully (method) |
| `IsFailure()` | `bool` | Operation failed (method) |
| `Value` | `T?` | Result value (non-null when `IsSuccess`) |
| `Error` | `IMessagingError?` | Error code and message (non-null when `IsFailure`) |

Note: `OperationResult<T>` is provided by the `Deveel.Results` package. Use `.IsSuccess()` and `.IsFailure()` as methods.

### Usage patterns

```csharp
var result = await connector.SendMessageAsync(message, ct);

if (result.IsSuccess())
{
    var sendResult = result.Value!;
    Console.WriteLine($"Sent: {sendResult.RemoteMessageId}");
}
else if (result.IsFailure() && result.Error is IValidationError)
{
    var validationError = (IValidationError)result.Error;
    Console.WriteLine("Validation errors:");
    foreach (var error in validationError.Errors)
        Console.WriteLine($"  - {error.ErrorMessage}");
}
else
{
    Console.WriteLine($"Error [{result.Error!.Code}]: {result.Error.Message}");
}
```

### Factory methods

```csharp
// Success
return OperationResult<SendResult>.Success(new SendResult { ... });

// Failure with error code and message
return OperationResult<SendResult>.Fail("RATE_LIMITED", "SMS", "Too many requests");

// Validation failure
return OperationResult<SendResult>.ValidationFailed(
    "VALIDATION_ERROR", "SMS", validationResults);

// Implicit conversion from T
SendResult sendResult = await SendToProviderAsync(message);
return sendResult;  // auto-wraps in OperationResult<SendResult>
```

The implicit conversion is what makes `ChannelConnectorBase` overrides clean — your core methods return raw values, and the base class wraps them.

## SendResult

Returned by `IChannelConnector.SendMessageAsync`.

```csharp
var result = await connector.SendMessageAsync(message, ct);

if (result.IsSuccess())
{
    Console.WriteLine($"Local ID: {result.Value!.MessageId}");
    Console.WriteLine($"Provider ID: {result.Value.RemoteMessageId}");
    Console.WriteLine($"Initial status: {result.Value.Status}");
    Console.WriteLine($"Timestamp: {result.Value.Timestamp}");
}
```

| Property | Type | Description |
|---|---|---|
| `MessageId` | `string` | The local message ID you assigned |
| `RemoteMessageId` | `string` | Provider-assigned message identifier |
| `Status` | `MessageStatus?` | Initial delivery status from provider |
| `Timestamp` | `DateTimeOffset?` | When the provider accepted the message |
| `AdditionalData` | `IDictionary<string, object>` | Provider-specific metadata |

### In a connector override

```csharp
protected override async Task<SendResult> SendMessageCoreAsync(
    IMessage message, CancellationToken ct)
{
    var apiResult = await _httpClient.PostAsync("/send", ...);

    return new SendResult
    {
        MessageId = message.Id,
        RemoteMessageId = apiResult.Id,
        Status = MessageStatus.Sent,
        Timestamp = DateTimeOffset.UtcNow,
        AdditionalData = new Dictionary<string, object>
        {
            ["provider_fee"] = apiResult.Fee,
            ["remaining_balance"] = apiResult.Balance
        }
    };
}
```

## BatchSendResult

Returned by `IChannelConnector.SendBatchAsync`.

```csharp
var batch = new MessageBatch();
batch.Messages.Add(msg1);
batch.Messages.Add(msg2);

var result = await connector.SendBatchAsync(batch, ct);

if (result.IsSuccess())
{
    var data = result.Value!;
    Console.WriteLine($"Batch {data.BatchId}: {data.MessageResults.Count} messages");

    foreach (var (msgId, sendResult) in data.MessageResults)
    {
        if (sendResult.Status == MessageStatus.Sent)
            Console.WriteLine($"  {msgId}: sent ({sendResult.RemoteMessageId})");
        else
            Console.WriteLine($"  {msgId}: {sendResult.Status}");
    }
}
```

| Property | Type | Description |
|---|---|---|
| `BatchId` | `string` | Local batch identifier |
| `RemoteBatchId` | `string?` | Provider-assigned batch ID (if supported) |
| `MessageResults` | `IDictionary<string, SendResult>` | Per-message results keyed by message ID |

## ReceiveResult

Returned by `IChannelConnector.ReceiveMessagesAsync`.

```csharp
var source = new MessageSource("application/json", webhookBody);
var result = await connector.ReceiveMessagesAsync(source, ct);

if (result.IsSuccess())
{
    var data = result.Value!;
    Console.WriteLine($"Received {data.Messages.Count} message(s)");

    foreach (var message in data.Messages)
    {
        Console.WriteLine($"  From: {message.Sender?.Address}");
        Console.WriteLine($"  Content: {(message.Content as TextContent)?.Text}");
    }
}
```

| Property | Type | Description |
|---|---|---|
| `BatchId` | `string` | Identifier for this receive batch |
| `Messages` | `IReadOnlyList<IMessage>` | Received messages |

### MessageSource

The `MessageSource` struct represents an inbound message payload (from webhooks, callbacks, etc.):

```csharp
public readonly struct MessageSource
{
    public string ContentType { get; }       // e.g., "application/json"
    public string? ContentEncoding { get; }  // optional encoding
    public ReadOnlyMemory<byte> Content { get; } // raw payload

    // Static factories for common formats
    MessageSource.Json(...)
    MessageSource.Xml(...)
    MessageSource.Text(...)
    MessageSource.Binary(...)
    MessageSource.UrlPost(...)

    // Parsing helpers
    ReadOnlySpan<char> AsText();
    T AsJson<T>();
}
```

## StatusUpdateResult / StatusUpdatesResult

Returned by `IChannelConnector.ReceiveMessageStatusAsync` and `GetMessageStatusAsync`.

```csharp
// Get message delivery history
var statusResult = await connector.GetMessageStatusAsync("msg-1", ct);

if (statusResult.IsSuccess())
{
    foreach (var update in statusResult.Value!.Updates)
    {
        Console.WriteLine(
            $"[{update.Timestamp:O}] {update.Status}" +
            $"{(update.Description != null ? $": {update.Description}" : "")}");
    }
}
```

**StatusUpdateResult** properties:

| Property | Type | Description |
|---|---|---|
| `MessageId` | `string` | The message ID |
| `Status` | `MessageStatus` | Status value (`Received`, `Queued`, `Sent`, `Delivered`, `DeliveryFailed`, etc.) |
| `Timestamp` | `DateTimeOffset` | When the status was recorded |
| `Description` | `string?` | Optional human-readable description |
| `AdditionalData` | `IDictionary<string, object>` | Provider-specific metadata |

**StatusUpdatesResult** properties:

| Property | Type | Description |
|---|---|---|
| `MessageId` | `string` | The message ID |
| `Updates` | `IList<StatusUpdateResult>` | Ordered list of status updates |

## StatusInfo

Returned by `IChannelConnector.GetStatusAsync`.

```csharp
var status = await connector.GetStatusAsync(ct);

if (status.IsSuccess())
{
    var info = status.Value!;
    Console.WriteLine($"Connector status: {info.Status}");

    if (info.Description != null)
        Console.WriteLine($"Details: {info.Description}");

    Console.WriteLine($"Last updated: {info.Timestamp:O}");
}
```

| Property | Type | Description |
|---|---|---|
| `Status` | `string` | Status string (provider-specific) |
| `Description` | `string?` | Optional description |
| `Timestamp` | `DateTimeOffset` | When the status was determined |
| `AdditionalData` | `IDictionary<string, object>` | Provider-specific metadata |

## ConnectorHealth

Returned by `IChannelConnector.GetHealthAsync`.

```csharp
var health = await connector.GetHealthAsync(ct);

if (health.IsSuccess())
{
    var h = health.Value!;
    Console.WriteLine($"Healthy: {h.IsHealthy}");
    Console.WriteLine($"State: {h.State}");
    Console.WriteLine($"Uptime: {h.Uptime}");
    Console.WriteLine($"Last check: {h.LastHealthCheck:O}");

    if (h.Issues.Count > 0)
    {
        Console.WriteLine("Issues:");
        foreach (var issue in h.Issues)
            Console.WriteLine($"  - {issue}");
    }

    if (h.Metrics.Count > 0)
    {
        Console.WriteLine("Metrics:");
        foreach (var (key, value) in h.Metrics)
            Console.WriteLine($"  {key}: {value}");
    }
}
```

| Property | Type | Description |
|---|---|---|
| `State` | `ConnectorState` | Current lifecycle state |
| `IsHealthy` | `bool` | `true` if the connector is operating normally |
| `LastHealthCheck` | `DateTime` | When the health check was last run |
| `Uptime` | `TimeSpan` | Time since last successful initialization |
| `Metrics` | `Dictionary<string, object>` | Custom metrics (message count, error rate, latency, etc.) |
| `Issues` | `List<string>` | Human-readable issue descriptions |

## IMessagingError

```csharp
public interface IMessagingError
{
    string Code { get; }
    string? Message { get; }
}
```

Standard error code conventions:

| Code | Meaning |
|---|---|
| `INVALID_CREDENTIALS` | Authentication failed |
| `RATE_LIMITED` | Provider rate limit exceeded |
| `NETWORK_ERROR` | Connection timed out, DNS failure, TLS error |
| `PROVIDER_VALIDATION_FAILED` | Provider rejected the message format |
| `MESSAGE_TOO_LARGE` | Content exceeds provider size limits |
| `INVALID_RECIPIENT` | Recipient address is invalid (e.g., wrong format) |
| `RECIPIENT_UNREACHABLE` | Recipient device/app not available |
| `INVALID_STATE` | Operation called when connector is not Ready |
| `CAPABILITY_NOT_SUPPORTED` | Operation not supported by this connector |
| `INTERNAL_ERROR` | Unexpected error in connector or provider |
