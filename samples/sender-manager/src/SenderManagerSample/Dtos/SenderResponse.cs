namespace SenderManagerSample.Dtos;

/// <summary>
/// Represents a sender identity returned by the API.
/// </summary>
/// <param name="Id">The unique identifier of the sender.</param>
/// <param name="Name">The logical name used to reference the sender.</param>
/// <param name="DisplayName">The human-readable display name.</param>
/// <param name="Address">The endpoint address (e.g. email, phone number).</param>
/// <param name="EndpointType">The type of endpoint (e.g. <c>"email"</c>, <c>"phone"</c>).</param>
/// <param name="IsActive">Whether the sender is active.</param>
/// <param name="CreatedAt">The timestamp when the sender was created.</param>
/// <param name="UpdatedAt">The timestamp when the sender was last updated.</param>
public record SenderResponse(
    string Id,
    string Name,
    string DisplayName,
    string Address,
    string EndpointType,
    bool IsActive,
    DateTime? CreatedAt,
    DateTime? UpdatedAt
);
