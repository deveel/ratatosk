namespace SenderManagerSample.Dtos;

/// <summary>
/// Represents a request to update an existing sender identity.
/// All fields are optional; only non-null values are applied.
/// </summary>
/// <param name="DisplayName">
/// The new display name, or <c>null</c> to keep the current value.
/// </param>
/// <param name="Address">
/// The new endpoint address, or <c>null</c> to keep the current value.
/// </param>
/// <param name="EndpointType">
/// The new endpoint type, or <c>null</c> to keep the current value.
/// </param>
/// <param name="IsActive">
/// Whether the sender is active, or <c>null</c> to keep the current value.
/// </param>
public record UpdateSenderRequest(
    string? DisplayName,
    string? Address,
    string? EndpointType,
    bool? IsActive
);
