using Ratatosk;
using SenderManagerSample.Dtos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss ";
    });
});

builder.Services.AddMessaging()
    .AddClient()
    .AddSenders()
    .AddSendGridEmail("sendgrid", c => c.WithSettings("SendGrid"));

builder.Services.AddSenderInMemoryStore(new[]
{
    new Sender
    {
        Id = "seed-support",
        Name = "support",
        DisplayName = "Customer Support",
        Address = "support@example.com",
        EndpointType = EndpointType.EmailAddress,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    },
    new Sender
    {
        Id = "seed-notifications",
        Name = "notifications",
        DisplayName = "Notification Service",
        Address = "noreply@example.com",
        EndpointType = EndpointType.EmailAddress,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    },
    new Sender
    {
        Id = "seed-sms-alerts",
        Name = "sms-alerts",
        DisplayName = "SMS Alert System",
        Address = "+15551234567",
        EndpointType = EndpointType.PhoneNumber,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    }
});

var app = builder.Build();

var senderGroup = app.MapGroup("/api/senders");

senderGroup.MapGet("/", async (ISenderRepository<Sender> repository) =>
{
    var senders = await repository.FindAllAsync();
    return Results.Ok(senders.Select(MapToResponse));
});

senderGroup.MapGet("/{id}", async (string id, ISenderRepository<Sender> repository) =>
{
    var sender = await repository.FindAsync(id);
    return sender is not null
        ? Results.Ok(MapToResponse(sender))
        : Results.NotFound(new { Error = $"Sender '{id}' not found" });
});

senderGroup.MapPost("/", async (CreateSenderRequest request, ISenderRepository<Sender> repository) =>
{
    var sender = new Sender
    {
        Id = Guid.NewGuid().ToString("N"),
        Name = request.Name,
        DisplayName = request.DisplayName,
        Address = request.Address,
        EndpointType = Endpoint.ParseEndpointType(request.EndpointType),
        IsActive = request.IsActive,
        CreatedAt = DateTime.UtcNow
    };

    await repository.AddAsync(sender);

    return Results.Created($"/api/senders/{sender.Id}", MapToResponse(sender));
});

senderGroup.MapPut("/{id}", async (string id, UpdateSenderRequest request, ISenderRepository<Sender> repository) =>
{
    var existing = await repository.FindAsync(id);
    if (existing is null)
        return Results.NotFound(new { Error = $"Sender '{id}' not found" });

    if (request.DisplayName is not null) existing.DisplayName = request.DisplayName;
    if (request.Address is not null) existing.Address = request.Address;
    if (request.EndpointType is not null) existing.EndpointType = Endpoint.ParseEndpointType(request.EndpointType);
    if (request.IsActive.HasValue) existing.IsActive = request.IsActive.Value;
    existing.UpdatedAt = DateTime.UtcNow;

    await repository.UpdateAsync(existing);

    return Results.Ok(MapToResponse(existing));
});

senderGroup.MapDelete("/{id}", async (string id, ISenderRepository<Sender> repository) =>
{
    var existing = await repository.FindAsync(id);
    if (existing is null)
        return Results.NotFound(new { Error = $"Sender '{id}' not found" });

    await repository.RemoveAsync(existing);

    return Results.NoContent();
});

app.MapPost("/api/messages", async (SendMessageRequest request, IMessagingClient client) =>
{
    var message = new MessageBuilder()
        .From(request.Sender!)
        .To(new Ratatosk.Endpoint(EndpointType.Id, request.ReceiverAddress))
        .WithContent(new TextContent(request.Text))
        .Build();

    var result = await client.SendAsync(request.Channel, message);
    if (!result.IsSuccess())
        return Results.Problem(result.Error?.Message, statusCode: 502);

    return Results.Ok(new
    {
        MessageId = result.Value!.MessageId,
        RemoteMessageId = result.Value!.RemoteMessageId,
        Status = result.Value!.Status.ToString(),
        Timestamp = result.Value.Timestamp
    });
});

app.MapGet("/", () => Results.Ok(new
{
    Message = "Ratatosk Sender Manager Sample",
    Endpoints = new
    {
        ListSenders = "GET /api/senders",
        GetSender = "GET /api/senders/{id}",
        CreateSender = "POST /api/senders",
        UpdateSender = "PUT /api/senders/{id}",
        DeleteSender = "DELETE /api/senders/{id}",
        SendMessage = "POST /api/messages"
    }
}));

app.Run();

/// <summary>
/// Maps a <see cref="Sender"/> to a <see cref="SenderResponse"/> DTO.
/// </summary>
/// <param name="sender">The sender to map.</param>
/// <returns>A <see cref="SenderResponse"/> representing the sender.</returns>
static SenderResponse MapToResponse(Sender sender) => new(
    sender.Id,
    sender.Name,
    sender.DisplayName,
    sender.Address,
    sender.EndpointType.ToString(),
    sender.IsActive,
    sender.CreatedAt,
    sender.UpdatedAt
);
