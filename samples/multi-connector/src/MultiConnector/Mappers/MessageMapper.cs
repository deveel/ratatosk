using Deveel;
using Deveel.Messaging;
using MultiConnector.Dtos;

namespace MultiConnector.Mappers;

public static class MessageMapper
{
    public static Message MapToMessage(MessageDto dto, string channel)
    {
        var messageId = string.IsNullOrWhiteSpace(dto.Id)
            ? Guid.NewGuid().ToString()
            : dto.Id;

        var message = new Message
        {
            Id = messageId,
            Sender = dto.Sender != null ? MapEndpoint(dto.Sender) : null,
            Receiver = dto.Receiver != null ? MapChannelEndpoint(dto.Receiver, channel) : null,
            Content = MapContent(dto.Content),
            Properties = MapProperties(dto.Properties)
        };

        return message;
    }

    public static Deveel.Messaging.Endpoint MapEndpoint(EndpointDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Type))
            return new Deveel.Messaging.Endpoint(dto.Type, dto.Address);

        return new Deveel.Messaging.Endpoint(EndpointType.Id, dto.Address);
    }

    public static Deveel.Messaging.Endpoint MapChannelEndpoint(EndpointDto dto, string channel)
    {
        if (!string.IsNullOrWhiteSpace(dto.Type))
            return new Deveel.Messaging.Endpoint(dto.Type, dto.Address);

        var endpointType = channel.ToLowerInvariant() switch
        {
            "facebook" => EndpointType.UserId,
            "firebase" => EndpointType.DeviceId,
            "sendgrid" => EndpointType.EmailAddress,
            "telegram" => EndpointType.Id,
            "sms" => EndpointType.PhoneNumber,
            "whatsapp" => EndpointType.PhoneNumber,
            _ => EndpointType.Id
        };

        return new Deveel.Messaging.Endpoint(endpointType, dto.Address);
    }

    public static MessageContent? MapContent(MessageContentDto? dto)
    {
        if (dto == null)
            return null;

        switch ((dto.ContentType ?? "text").ToLowerInvariant())
        {
            case "html":
                return new HtmlContent(dto.Html ?? string.Empty);

            case "image":
            case "audio":
            case "video":
            case "document":
            case "file":
            case "media":
                var mediaType = dto.MediaType?.ToLowerInvariant() switch
                {
                    "image" => MediaType.Image,
                    "audio" => MediaType.Audio,
                    "video" => MediaType.Video,
                    "document" => MediaType.Document,
                    "file" => MediaType.File,
                    _ => MediaType.Image
                };

                return new MediaContent(mediaType, dto.FileName, dto.FileUrl);

            case "template":
                if (string.IsNullOrWhiteSpace(dto.TemplateId))
                    return null;

                return new TemplateContent(dto.TemplateId,
                    dto.TemplateData?.ToDictionary(kv => kv.Key, kv => (object?)kv.Value)
                    ?? new Dictionary<string, object?>());

            case "location":
                if (dto.Latitude == null || dto.Longitude == null)
                    return null;

                var location = new LocationContent(dto.Latitude.Value, dto.Longitude.Value);
                if (dto.LivePeriod.HasValue)
                    location = location.WithLivePeriod(dto.LivePeriod.Value);

                return location;

            case "text":
            default:
                return new TextContent(dto.Text ?? string.Empty, dto.Encoding);
        }
    }

    public static IDictionary<string, MessageProperty>? MapProperties(Dictionary<string, string>? properties)
    {
        if (properties == null || properties.Count == 0)
            return null;

        return properties.ToDictionary(
            kv => kv.Key,
            kv => new MessageProperty(kv.Key, kv.Value));
    }
}
