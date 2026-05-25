//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
	/// <summary>
	/// Represents a Facebook message response.
	/// </summary>
	public class FacebookMessageResponse
    {
		/// <summary>
		/// Gets or sets the identifier of the sent message.
		/// </summary>
        public string MessageId { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the recipient identifier associated with the request.
		/// </summary>
		/// <remarks>
		/// In the current implementation this value mirrors the recipient provided in the outbound request.
		/// </remarks>
        public string RecipientId { get; set; } = string.Empty;
    }
}
