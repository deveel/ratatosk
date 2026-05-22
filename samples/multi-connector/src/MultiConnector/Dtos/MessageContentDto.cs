namespace MultiConnector.Dtos;

public class MessageContentDto
{
    public string ContentType { get; set; } = "text";

    public string? Text { get; set; }
    public string? Encoding { get; set; }

    public string? Html { get; set; }

    public string? MediaType { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }

    public string? TemplateId { get; set; }
    public Dictionary<string, string?>? TemplateData { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? LivePeriod { get; set; }

    public string? ButtonText { get; set; }
    public string? ButtonType { get; set; }
    public string? ButtonValue { get; set; }

    public string? QuickReplyTitle { get; set; }
    public string? QuickReplyPayload { get; set; }
    public string? QuickReplyImageUrl { get; set; }

    public List<CarouselCardDto>? CarouselCards { get; set; }

    public string? ListTitle { get; set; }
    public string? ListSubtitle { get; set; }
    public string? ListStyle { get; set; }
    public List<ListPickerItemDto>? ListItems { get; set; }
}

public class CarouselCardDto
{
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? ImageUrl { get; set; }
}

public class ListPickerItemDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}
