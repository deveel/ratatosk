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
}
