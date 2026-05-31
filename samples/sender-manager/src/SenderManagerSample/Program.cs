using Ratatosk;
using Ratatosk.Senders;
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

var seedSenders = new SenderEntity[]
{
    new()
    {
        Id = "seed-support",
        Name = "support",
        DisplayName = "Customer Support",
        Address = "support@example.com",
        Type = EndpointType.EmailAddress,
        CreatedAt = DateTime.UtcNow
    },
    new()
    {
        Id = "seed-notifications",
        Name = "notifications",
        DisplayName = "Notification Service",
        Address = "noreply@example.com",
        Type = EndpointType.EmailAddress,
        CreatedAt = DateTime.UtcNow
    },
    new()
    {
        Id = "seed-sms-alerts",
        Name = "sms-alerts",
        DisplayName = "SMS Alert System",
        Address = "+15551234567",
        Type = EndpointType.PhoneNumber,
        CreatedAt = DateTime.UtcNow
    }
};

foreach (var s in seedSenders)
    s.Activate();

builder.Services.AddMessaging()
    .AddClient()
    .AddSenders(s => s
        .UseInMemoryStore(seedSenders))
    .AddSendGridEmail("sendgrid", c => c.WithSettings("SendGrid"));

var app = builder.Build();

var senderGroup = app.MapGroup("/api/senders");

senderGroup.MapGet("/", async (SenderManager<SenderEntity> manager) =>
{
    var result = await manager.GetAllActiveAsync();
    if (!result.IsSuccess())
        return Results.Problem(result.Error?.Message, statusCode: 500);

    return Results.Ok(result.Value!.Select(MapToResponse));
});

senderGroup.MapGet("/{id}", async (string id, SenderManager<SenderEntity> manager) =>
{
    var result = await manager.FindAsync(id);
    if (!result.IsSuccess() || result.Value is null)
        return Results.NotFound(new { Error = $"Sender '{id}' not found" });

    return Results.Ok(MapToResponse(result.Value));
});

senderGroup.MapGet("/name/{name}", async (string name, SenderManager<SenderEntity> manager) =>
{
    var result = await manager.FindByNameAsync(name);
    if (!result.IsSuccess() || result.Value is null)
        return Results.NotFound(new { Error = $"Sender with name '{name}' not found" });

    return Results.Ok(MapToResponse(result.Value));
});

senderGroup.MapPost("/", async (CreateSenderRequest request, SenderManager<SenderEntity> manager) =>
{
    var sender = new SenderEntity
    {
        Id = Guid.NewGuid().ToString("N"),
        Name = request.Name,
        DisplayName = request.DisplayName,
        Address = request.Address,
        Type = Ratatosk.Endpoint.ParseEndpointType(request.EndpointType),
        CreatedAt = DateTime.UtcNow
    };
    sender.Activate();

    var result = await manager.AddAsync(sender);
    if (!result.IsSuccess())
        return Results.Problem(result.Error?.Message, statusCode: 400);

    return Results.Created($"/api/senders/{sender.Id}", MapToResponse(sender));
});

senderGroup.MapPut("/{id}", async (string id, UpdateSenderRequest request, SenderManager<SenderEntity> manager) =>
{
    var findResult = await manager.FindAsync(id);
    if (!findResult.IsSuccess() || findResult.Value is null)
        return Results.NotFound(new { Error = $"Sender '{id}' not found" });

    var existing = findResult.Value;

    var endpointType = request.EndpointType is not null
        ? Ratatosk.Endpoint.ParseEndpointType(request.EndpointType)
        : (EndpointType?)null;

    existing.Update(request.DisplayName, request.Address, endpointType);

    if (request.IsActive.HasValue)
    {
        if (request.IsActive.Value)
            existing.Activate();
        else
            existing.Deactivate();
    }

    var updateResult = await manager.UpdateAsync(existing);
    if (!updateResult.IsSuccess())
        return Results.Problem(updateResult.Error?.Message, statusCode: 400);

    return Results.Ok(MapToResponse(existing));
});

senderGroup.MapDelete("/{id}", async (string id, SenderManager<SenderEntity> manager) =>
{
    var findResult = await manager.FindAsync(id);
    if (!findResult.IsSuccess() || findResult.Value is null)
        return Results.NotFound(new { Error = $"Sender '{id}' not found" });

    var removeResult = await manager.RemoveAsync(findResult.Value);
    if (!removeResult.IsSuccess())
        return Results.Problem(removeResult.Error?.Message, statusCode: 500);

    return Results.NoContent();
});

senderGroup.MapPost("/{id}/activate", async (string id, SenderManager<SenderEntity> manager) =>
{
    var result = await manager.ActivateAsync(id);
    if (!result.IsSuccess())
        return Results.NotFound(new { Error = result.Error?.Message });

    var senderResult = await manager.FindAsync(id);
    return Results.Ok(MapToResponse(senderResult.Value!));
});

senderGroup.MapPost("/{id}/deactivate", async (string id, SenderManager<SenderEntity> manager) =>
{
    var result = await manager.DeactivateAsync(id);
    if (!result.IsSuccess())
        return Results.NotFound(new { Error = result.Error?.Message });

    var senderResult = await manager.FindAsync(id);
    return Results.Ok(MapToResponse(senderResult.Value!));
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
        GetSenderByName = "GET /api/senders/name/{name}",
        CreateSender = "POST /api/senders",
        UpdateSender = "PUT /api/senders/{id}",
        DeleteSender = "DELETE /api/senders/{id}",
        ActivateSender = "POST /api/senders/{id}/activate",
        DeactivateSender = "POST /api/senders/{id}/deactivate",
        SendMessage = "POST /api/messages"
    }
}));

app.Run();

static SenderResponse MapToResponse(SenderEntity sender) => new(
    sender.Id,
    sender.Name,
    sender.DisplayName,
    sender.Address,
    sender.Type.ToString(),
    sender.IsActive,
    sender.CreatedAt,
    sender.UpdatedAt
);
