//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
	/// <summary>
	/// A set of constants used in the Twilio Connector scope
	/// </summary>
	public static class TwilioConnectorConstants
	{
		/// <summary>
		/// The name of the provider (twilio)
		/// </summary>
		public const string Provider = "twilio";

		/// <summary>
		/// The SMS channel type
		/// </summary>
		public const string SmsChannel = "sms";

		/// <summary>
		/// The WhatsApp channel type
		/// </summary>
		public const string WhatsAppChannel = "whatsapp";

		/// <summary>
		/// The Twilio REST API version used in API request URLs.
		/// This is the stable long-lived version of the Twilio Programmable Messaging API.
		/// </summary>
		public const string ApiVersion = "2010-04-01";

		/// <summary>
		/// The current Twilio C# Helper Library major version supported by this connector.
		/// Schema versions are aligned to the SDK major version to reflect the feature set
		/// available at each version.
		/// </summary>
		public const string SdkVersion = "7.0";

		/// <summary>
		/// The Twilio C# Helper Library version 6.x schema version.
		/// Introduced Messaging Services, improved status callbacks, and WhatsApp Business API support.
		/// </summary>
		public const string SdkVersion6 = "6.0";

		/// <summary>
		/// The Twilio C# Helper Library version 5.x schema version.
		/// Provides basic Programmable SMS and early WhatsApp support.
		/// </summary>
		public const string SdkVersion5 = "5.0";

		/// <summary>
		/// The connector schema version, aligned to the current SDK major version.
		/// </summary>
		public const string ConnectorSchemaVersion = SdkVersion;

		/// <summary>
		/// The base URL for the Twilio REST API.
		/// </summary>
		public const string TwilioApiBaseUrl = "https://api.twilio.com";

		/// <summary>
		/// Gets the Twilio SDK versions that have explicit schema variants defined by this connector.
		/// </summary>
		public static IReadOnlyList<string> SupportedSchemaVersions { get; } = new[]
		{
			ConnectorSchemaVersion,
			SdkVersion6,
			SdkVersion5
		};
	}
}
