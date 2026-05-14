using Deveel;
using Deveel.Messaging;
using MultiConnector.Dtos;
using MultiConnector.Mappers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();

    if (Environment.GetEnvironmentVariable("DEVEEL_VERBOSE") == "true")
    {
        logging.SetMinimumLevel(LogLevel.Warning)
               .AddSimpleConsole(options =>
               {
                   options.SingleLine = true;
                   options.TimestampFormat = "HH:mm:ss ";
               });
    }
});

builder.Services.AddMessaging()
    .AddClient()
    .AddFacebookMessenger("facebook", c => c.WithSettings("Facebook"))
    .AddFirebasePush("firebase", c => c.WithSettings("Firebase"))
    .AddSendGridEmail("sendgrid", c => c.WithSettings("SendGrid"))
    .AddTelegramBot("telegram", c => c.WithSettings("Telegram"))
    .AddTwilioSms("sms", c => c.WithSettings("Twilio"))
    .AddTwilioWhatsApp("whatsapp", c => c.WithSettings("Twilio"));

var app = builder.Build();

var supportedChannels = new[] { "facebook", "firebase", "sendgrid", "telegram", "sms", "whatsapp" };

app.MapGet("/", () => Results.Ok(new
{
    Message = "Deveel.Messaging Multi-Connector Sample",
    Channels = supportedChannels,
    Endpoints = new
    {
        Send = "POST /{channel}/message",
        StatusCallback = "POST /{channel}/message/status",
        WebhookReceive = "POST /{channel}/receive",
        ConnectorStatus = "GET /{channel}/status",
        Schemas = "GET /schemas"
    }
}));

app.MapGet("/schemas", (IChannelSchemaRegistry registry) =>
{
    var schemas = registry.GetSchemas()
        .Select(s => new
        {
            s.ChannelType,
            s.ChannelProvider,
            s.DisplayName,
            Capabilities = s.Capabilities.ToString(),
            s.Version,
            Endpoints = s.Endpoints.Select(e => new { e.Type, e.CanSend, e.CanReceive, e.IsRequired }),
            Parameters = s.Parameters.Select(p => new { p.Name, p.DataType, p.IsRequired, p.IsSensitive }),
            ContentTypes = s.ContentTypes.Select(ct => ct.ToString())
        })
        .ToList();

    return Results.Ok(schemas);
});

app.MapPost("/{channel}/message", async (
    string channel,
    MessageDto request,
    IMessagingClient client,
    ILogger<Program> logger) =>
{
    if (!supportedChannels.Contains(channel))
        return Results.NotFound(new { Error = $"Unknown channel '{channel}'. Supported: {string.Join(", ", supportedChannels)}" });

    var message = MessageMapper.MapToMessage(request, channel);

    var sendResult = await client.SendAsync(channel, message, CancellationToken.None);
    if (!sendResult.IsSuccess())
        return Results.Problem(sendResult.Error?.Message, statusCode: 502);

    logger.LogMessageSent(message.Id, channel, sendResult.Value!.MessageId, sendResult.Value!.RemoteMessageId, sendResult.Value!.Status.ToString());

    return Results.Ok(new
    {
        MessageId = sendResult.Value!.MessageId,
        RemoteMessageId = sendResult.Value!.RemoteMessageId,
        Status = sendResult.Value!.Status.ToString(),
        Timestamp = sendResult.Value.Timestamp
    });
});

app.MapPost("/{channel}/message/status", async (
    string channel,
    string payload,
    IMessagingClient client,
    ILogger<Program> logger) =>
{
    if (!supportedChannels.Contains(channel))
        return Results.NotFound(new { Error = $"Unknown channel '{channel}'." });

    var source = MessageSource.Json(payload);
    var result = await client.ReceiveMessageStatusAsync(channel, source, CancellationToken.None);

    if (!result.IsSuccess())
        return Results.Problem(result.Error?.Message, statusCode: 502);

    logger.LogStatusUpdateReceived(channel, result.Value!.MessageId, result.Value!.Status.ToString());

    return Results.Ok(new
    {
        MessageId = result.Value!.MessageId,
        Status = result.Value!.Status.ToString(),
        Timestamp = result.Value.Timestamp
    });
});

app.MapPost("/{channel}/receive", async (
    string channel,
    string payload,
    IMessagingClient client,
    ILogger<Program> logger) =>
{
    if (!supportedChannels.Contains(channel))
        return Results.NotFound(new { Error = $"Unknown channel '{channel}'." });

    var source = MessageSource.Json(payload);
    var result = await client.ReceiveAsync(channel, source, CancellationToken.None);

    if (!result.IsSuccess())
        return Results.Problem(result.Error?.Message, statusCode: 502);

    logger.LogMessagesReceived(result.Value!.Messages.Count, channel);

    return Results.Ok(new
    {
        BatchId = result.Value!.BatchId,
        Messages = result.Value!.Messages.Select(m => new
        {
            m.Id,
            From = m.Sender?.Address,
            To = m.Receiver?.Address,
            ContentType = m.Content?.GetType().Name
        })
    });
});

app.MapGet("/{channel}/status", async (
    string channel,
    IMessagingClient client) =>
{
    if (!supportedChannels.Contains(channel))
        return Results.NotFound(new { Error = $"Unknown channel '{channel}'." });

    var statusResult = await client.GetStatusAsync(channel, CancellationToken.None);
    if (!statusResult.IsSuccess())
        return Results.Problem(statusResult.Error?.Message, statusCode: 502);

    return Results.Ok(new
    {
        Channel = channel,
        Status = statusResult.Value.Status,
        Description = statusResult.Value.Description,
        Timestamp = statusResult.Value.Timestamp
    });
});

app.Run();
