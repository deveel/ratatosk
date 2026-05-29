using Ratatosk;

namespace SenderManagerSample.Dtos;

/// <summary>
/// Represents a request to send a message through a registered channel.
/// </summary>
/// <param name="Channel">
/// The name of the channel to send through (e.g. <c>"sendgrid"</c>).
/// </param>
/// <param name="Sender">
/// The sender of the message. Accepts polymorphic endpoint types:
/// <list type="bullet">
///   <item>
///     <term>Sender identity reference</term>
///     <description>
///       <c>{ "$type": "senderref", "senderName": "support" }</c> — resolves
///       the sender from the registry at send time via <c>ISenderResolver</c>.
///     </description>
///   </item>
///   <item>
///     <term>Inline endpoint</term>
///     <description>
///       <c>{ "$type": "endpoint", "address": "...", "type": "email" }</c> —
///       used directly as the message sender.
///     </description>
///   </item>
///   <item>
///     <term>Specialised senders</term>
///     <description>
///       <c>{ "$type": "phone", "phoneNumber": "..." }</c>,
///       <c>{ "$type": "email", "emailAddress": "..." }</c>,
///       <c>{ "$type": "bot", "botId": "..." }</c>,
///       <c>{ "$type": "alphanumeric", "text": "..." }</c>.
///     </description>
///   </item>
/// </list>
/// </param>
/// <param name="ReceiverAddress">
/// The address of the message recipient. Used to build an inline endpoint
/// with <c>EndpointType.Id</c>.
/// </param>
/// <param name="Text">
/// The plain-text body of the message.
/// </param>
public record SendMessageRequest(
    string Channel,
    IEndpoint? Sender,
    string ReceiverAddress,
    string Text
);
