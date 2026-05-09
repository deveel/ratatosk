//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
	/// <summary>
	/// Represents Facebook page information.
	/// </summary>
	public class FacebookPageInfo
    {
    /// <summary>
    /// Gets or sets the unique identifier of the Facebook page.
    /// </summary>
        public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the Facebook page.
    /// </summary>
        public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category assigned to the Facebook page.
    /// </summary>
        public string Category { get; set; } = string.Empty;
    }
}
