namespace SenderManagerSample.Dtos;

/// <summary>
/// Represents a request to create a new sender identity in the registry.
/// </summary>
/// <param name="Name">
/// The logical name used to reference the sender (e.g. <c>"support"</c>).
/// </param>
/// <param name="DisplayName">
/// The human-readable display name (e.g. <c>"Customer Support"</c>).
/// </param>
/// <param name="Address">
/// The endpoint address of the sender (e.g. <c>"support@example.com"</c>).
/// </param>
/// <param name="EndpointType">
/// The type of the endpoint (e.g. <c>"email"</c>, <c>"phone"</c>, <c>"label"</c>).
/// </param>
/// <param name="IsActive">
/// Whether the sender is active and can be used to send messages.
/// </param>
public record CreateSenderRequest(
    string Name,
    string DisplayName,
    string Address,
    string EndpointType,
    bool IsActive = true
);
