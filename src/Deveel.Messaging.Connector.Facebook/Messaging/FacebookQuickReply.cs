//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
	/// <summary>
	/// Represents a Facebook quick reply button.
	/// </summary>
	public class FacebookQuickReply
    {
    /// <summary>
    /// Gets or sets the quick reply content type.
    /// </summary>
        public string ContentType { get; set; } = "text";

    /// <summary>
    /// Gets or sets the title shown on the quick reply button.
    /// </summary>
        public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payload returned when the quick reply is selected.
    /// </summary>
        public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional URL of an icon displayed for the quick reply.
    /// </summary>
        public string? ImageUrl { get; set; }
    }
}
