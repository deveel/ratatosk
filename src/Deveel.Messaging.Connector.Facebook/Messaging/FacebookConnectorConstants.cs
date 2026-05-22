//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
	/// <summary>
	/// A set of constants used in the Facebook Messenger Connector scope
	/// </summary>
	public static class FacebookConnectorConstants
	{
		/// <summary>
		/// The name of the provider (facebook)
		/// </summary>
		public const string Provider = "facebook";

		/// <summary>
		/// The Messenger channel type
		/// </summary>
		public const string MessengerChannel = "messenger";

		/// <summary>
		/// The Facebook Graph API version to use
		/// </summary>
		public const string GraphApiVersion = "v21.0";

		/// <summary>
		/// The previous Facebook Graph API version supported by the connector schemas.
		/// </summary>
		public const string GraphApiVersion20 = "v20.0";

		/// <summary>
		/// An older Facebook Graph API version supported by the connector schemas.
		/// </summary>
		public const string GraphApiVersion19 = "v19.0";

		/// <summary>
		/// The oldest Facebook Graph API version explicitly supported by the connector schemas.
		/// </summary>
		public const string GraphApiVersion18 = "v18.0";

		/// <summary>
		/// The version of the connector schema, aligned with the Facebook Graph API version.
		/// </summary>
		public const string ConnectorSchemaVersion = GraphApiVersion;

		/// <summary>
		/// Gets the Facebook Graph API versions that have explicit schema variants.
		/// </summary>
		public static IReadOnlyList<string> SupportedSchemaVersions { get; } = new[]
		{
			ConnectorSchemaVersion,
			GraphApiVersion20,
			GraphApiVersion19,
			GraphApiVersion18
		};

		/// <summary>
		/// The base URL for Facebook Graph API
		/// </summary>
		public const string GraphApiBaseUrl = "https://graph.facebook.com";

		/// <summary>
		/// The maximum number of quick replies allowed per message (Facebook limit).
		/// </summary>
		public const int MaxQuickReplies = 13;

		/// <summary>
		/// The maximum number of buttons per template message (Facebook limit).
		/// </summary>
		public const int MaxButtonsPerMessage = 3;

		/// <summary>
		/// The maximum number of carousel cards (generic template elements).
		/// </summary>
		public const int MaxCarouselCards = 10;

		/// <summary>
		/// The maximum number of buttons per carousel card.
		/// </summary>
		public const int MaxButtonsPerCard = 3;

		/// <summary>
		/// The maximum number of list items.
		/// </summary>
		public const int MaxListItems = 4;

		/// <summary>
		/// The minimum number of list items.
		/// </summary>
		public const int MinListItems = 2;
	}
}
