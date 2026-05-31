//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk {
	/// <summary>
	/// An endpoint is a node in a network that is able 
	/// to send or receive messages
	/// </summary>
	[System.Text.Json.Serialization.JsonDerivedType(typeof(Endpoint), typeDiscriminator: "endpoint")]
	[System.Text.Json.Serialization.JsonDerivedType(typeof(Sender), typeDiscriminator: "sender")]
	[System.Text.Json.Serialization.JsonDerivedType(typeof(SenderRef), typeDiscriminator: "senderref")]
	[System.Text.Json.Serialization.JsonDerivedType(typeof(PhoneSender), typeDiscriminator: "phone")]
	[System.Text.Json.Serialization.JsonDerivedType(typeof(AlphaNumericSender), typeDiscriminator: "alphanumeric")]
	[System.Text.Json.Serialization.JsonDerivedType(typeof(EmailSender), typeDiscriminator: "email")]
	[System.Text.Json.Serialization.JsonDerivedType(typeof(BotSender), typeDiscriminator: "bot")]
	public interface IEndpoint {
		/// <summary>
		/// Gets the type of the endpoint that is used.
		/// </summary>
		EndpointType Type { get; }

		/// <summary>
		/// Gets the address of the endpoint.
		/// </summary>
		string Address { get; }
	}
}