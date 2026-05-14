namespace MultiConnector.Dtos;

public class MessagePropertyDto
{
    public string Name { get; set; } = string.Empty;
    public string? Value { get; set; }
    public bool IsSensitive { get; set; }
}
