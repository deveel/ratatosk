//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk {
	/// <summary>
	/// An implementation of <see cref="IMessage"/> that is used to
	/// represent a message that can be sent or received.
	/// </summary>
	public class Message : IMessage {
		/// <summary>
		/// Constructs an empty message instance.
		/// </summary>
		public Message() {
		}

		/// <summary>
		/// Constructs a message instance from the given <paramref name="message"/>.
		/// </summary>
		/// <param name="message">
		/// The message that is used as source of the new instance.
		/// </param>
		public Message(IMessage message) {
			Id = message.Id;
			Sender = message.Sender != null
				? (message.Sender is ISender sender ? sender : message.Sender is IUnresolvedSender unresolved ? unresolved : new Endpoint(message.Sender))
				: null;
			Receiver = message.Receiver != null
				? (message.Receiver is ISender isender ? isender : message.Receiver is IUnresolvedSender unres ? unres : new Endpoint(message.Receiver))
				: null;
			Content = MessageContent.Create(message.Content);
			Properties = message.Properties?.ToDictionary(x => x.Key, x => new MessageProperty(x.Value));
		}

		/// <inheritdoc/>
		public string? Id { get; set; }

		/// <summary>
		/// Gets or sets the endpoint that is the sender of the message.
		/// </summary>
		public IEndpoint? Sender { get; set; }

		/// <summary>
		/// Gets or sets the endpoint that is the receiver of the message.
		/// </summary>
		public IEndpoint? Receiver { get; set; }

		IEndpoint? IMessage.Sender => Sender;

		IEndpoint? IMessage.Receiver => Receiver;

		/// <inheritdoc/>
		public MessageContent? Content { get; set; }

		IMessageContent? IMessage.Content => Content;

		IDictionary<string, IMessageProperty>? IMessage.Properties 
			=> Properties?.ToDictionary(x => x.Key, y => (IMessageProperty)y.Value);

		/// <inheritdoc/>
		public IDictionary<string, MessageProperty>? Properties { get; set; }
	}
}
