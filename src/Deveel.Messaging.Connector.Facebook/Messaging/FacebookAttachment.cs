//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
	/// <summary>
	/// Represents a Facebook message attachment.
	/// </summary>
	public class FacebookAttachment
    {
		/// <summary>
		/// Gets or sets the attachment type (for example, image, audio, video, or file).
		/// </summary>
        public string Type { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the payload metadata associated with the attachment.
		/// </summary>
        public FacebookPayload Payload { get; set; } = new();
    }
}
