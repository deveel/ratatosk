//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
	/// <summary>
	/// Represents a Facebook message.
	/// </summary>
	public class FacebookMessage
    {
    /// <summary>
    /// Gets or sets the plain text content of the message.
    /// </summary>
        public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the attachment included in the message.
    /// </summary>
        public FacebookAttachment? Attachment { get; set; }

    /// <summary>
    /// Gets or sets the quick reply options available to the recipient.
    /// </summary>
        public List<FacebookQuickReply>? QuickReplies { get; set; }
    }
}
