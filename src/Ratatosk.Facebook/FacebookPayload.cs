//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
	/// <summary>
	/// Represents a Facebook attachment payload.
	/// </summary>
	public class FacebookPayload
    {
		/// <summary>
		/// Gets or sets the publicly reachable URL of the media attachment.
		/// </summary>
        public string Url { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets a value indicating whether the uploaded asset can be reused by Facebook.
		/// </summary>
        public bool IsReusable { get; set; } = true;
    }
}
