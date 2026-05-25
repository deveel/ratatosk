//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
	/// <summary>
	/// Represents a Facebook message request.
	/// </summary>
	public class FacebookMessageRequest
    {
    /// <summary>
    /// Gets or sets the target recipient identifier.
    /// </summary>
        public string Recipient { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message payload sent to the recipient.
    /// </summary>
        public FacebookMessage Message { get; set; } = new();

    /// <summary>
    /// Gets or sets the messaging type (for example RESPONSE, UPDATE, or MESSAGE_TAG).
    /// </summary>
        public string MessagingType { get; set; } = "RESPONSE";

    /// <summary>
    /// Gets or sets the notification behavior for push delivery.
    /// </summary>
        public string NotificationType { get; set; } = "REGULAR";

    /// <summary>
    /// Gets or sets the Facebook message tag used for specific message categories.
    /// </summary>
        public string? Tag { get; set; }

            /// <summary>
            /// Gets or sets optional quick replies at request level.
            /// </summary>
            /// <remarks>
            /// The current serializer uses <see cref="FacebookMessage.QuickReplies"/> when building
                    /// the outbound payload. This property is retained for backward compatibility and is ignored.
            /// </remarks>
                    [Obsolete("Use Message.QuickReplies instead. Request-level QuickReplies is deprecated and ignored.")]
        public List<FacebookQuickReply>? QuickReplies { get; set; }
    }
}
