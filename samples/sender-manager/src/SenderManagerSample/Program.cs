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
    new SenderEntity
    {
        Id = "seed-support",
        Name = "support",
        DisplayName = "Customer Support",
        Address = "support@example.com",
        EndpointType = "email",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    },
    new SenderEntity
    {
        Id = "seed-notifications",
        Name = "notifications",
        DisplayName = "Notification Service",
        Address = "noreply@example.com",
        EndpointType = "email",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    },
    new SenderEntity
    {
        Id = "seed-sms-alerts",
        Name = "sms-alerts",
        DisplayName = "SMS Alert System",
        Address = "+15551234567",
        EndpointType = "phone",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    }
});

var app = builder.Build();

var senderGroup = app.MapGroup("/api/senders");

senderGroup.MapGet("/", async (ISenderRegistry registry) =>
{
    var senders = await registry.GetAllAsync();
    return Results.Ok(senders.Select(MapToResponse));
});

senderGroup.MapGet("/{id}", async (string id, ISenderRegistry registry) =>
{
    var sender = await registry.FindByIdAsync(id);
    return sender is not null
        ? Results.Ok(MapToResponse(sender))
        : Results.NotFound(new { Error = $"Sender '{id}' not found" });
});

senderGroup.MapPost("/", async (CreateSenderRequest request, ISenderRegistry registry) =>
{
    var entity = new SenderEntity
    {
        Id = Guid.NewGuid().ToString("N"),
        Name = request.Name,
        DisplayName = request.DisplayName,
        Address = request.Address,
        EndpointType = request.EndpointType,
        IsActive = request.IsActive,
        CreatedAt = DateTime.UtcNow
    };

    var result = await registry.CreateAsync(entity);
    if (!result.IsSuccess())
        return Results.Problem(result.Error?.Message, statusCode: 400);

    return Results.Created($"/api/senders/{result.Value!.Id}", MapToResponse(result.Value));
});

senderGroup.MapPut("/{id}", async (string id, UpdateSenderRequest request, ISenderRegistry registry) =>
{
    var existing = await registry.FindByIdAsync(id);
    if (existing is null)
        return Results.NotFound(new { Error = $"Sender '{id}' not found" });

    if (request.DisplayName is not null) existing.DisplayName = request.DisplayName;
    if (request.Address is not null) existing.Address = request.Address;
    if (request.EndpointType is not null) existing.EndpointType = request.EndpointType;
    if (request.IsActive.HasValue) existing.IsActive = request.IsActive.Value;
    existing.UpdatedAt = DateTime.UtcNow;

    var result = await registry.UpdateAsync(existing);
    if (!result.IsSuccess())
        return Results.Problem(result.Error?.Message, statusCode: 400);

    return Results.Ok(MapToResponse(result.Value!));
});

senderGroup.MapDelete("/{id}", async (string id, ISenderRegistry registry) =>
{
    var existing = await registry.FindByIdAsync(id);
    if (existing is null)
        return Results.NotFound(new { Error = $"Sender '{id}' not found" });

    var result = await registry.DeleteAsync(existing);
    if (!result.IsSuccess())
        return Results.Problem(result.Error?.Message, statusCode: 400);

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
/// Maps a <see cref="SenderEntity"/> to a <see cref="SenderResponse"/> DTO.
/// </summary>
/// <param name="entity">The sender entity to map.</param>
/// <returns>A <see cref="SenderResponse"/> representing the entity.</returns>
static SenderResponse MapToResponse(SenderEntity entity) => new(
    entity.Id,
    entity.Name,
    entity.DisplayName,
    entity.Address,
    entity.EndpointType,
    entity.IsActive,
    entity.CreatedAt,
    entity.UpdatedAt
);
