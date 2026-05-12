namespace MultiConnector.Dtos;

public class MessageDto
{
    public string? Id { get; set; }
    public EndpointDto? Sender { get; set; }
    public EndpointDto? Receiver { get; set; }
    public MessageContentDto? Content { get; set; }
    public Dictionary<string, string>? Properties { get; set; }
}
