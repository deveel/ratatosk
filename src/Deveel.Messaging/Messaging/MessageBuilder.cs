namespace Deveel.Messaging
{
    /// <summary>
    /// Provides a fluent API for constructing <see cref="Message"/> instances.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this builder to create messages with a clear, chainable syntax
    /// instead of setting properties directly on the <see cref="Message"/> class.
    /// </para>
    /// <example>
    /// <code>
    /// var message = new MessageBuilder()
    ///     .WithId("msg-1")
    ///     .FromPhone("+15551234567")
    ///     .ToEmail("user@example.com")
    ///     .WithText("Hello!")
    ///     .WithSubject("Greeting")
    ///     .Build();
    /// </code>
    /// </example>
    /// </remarks>
    public class MessageBuilder
    {
        private string _id = "";
        private IEndpoint? _sender;
        private IEndpoint? _receiver;
        private IMessageContent? _content;
        private Dictionary<string, MessageProperty>? _properties;

        /// <summary>
        /// Sets the unique identifier of the message.
        /// </summary>
        /// <param name="id">
        /// The unique identifier to assign to the message. Cannot be
        /// <c>null</c> or whitespace.
        /// </param>
        /// <returns>
        /// Returns the current builder instance to allow chaining.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="id"/> is <c>null</c> or whitespace.
        /// </exception>
        public MessageBuilder WithId(string id)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);
            _id = id;
            return this;
        }

        /// <summary>
        /// Sets the sender endpoint of the message.
        /// </summary>
        /// <param name="endpoint">
        /// The endpoint that identifies the sender.
        /// </param>
        /// <returns>
        /// Returns the current builder instance to allow chaining.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="endpoint"/> is <c>null</c>.
        /// </exception>
        public MessageBuilder From(IEndpoint endpoint)
        {
            _sender = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            return this;
        }

        /// <summary>
        /// Sets the sender to an email address.
        /// </summary>
        /// <param name="email">The email address of the sender.</param>
        /// <returns>
        /// Returns the current builder instance to allow chaining.
        /// </returns>
        public MessageBuilder FromEmail(string email)
            => From(Endpoint.EmailAddress(email));

        /// <summary>
        /// Sets the sender to a phone number.
        /// </summary>
        /// <param name="phone">The phone number of the sender.</param>
        /// <returns>
        /// Returns the current builder instance to allow chaining.
        /// </returns>
        public MessageBuilder FromPhone(string phone)
            => From(Endpoint.PhoneNumber(phone));

        /// <summary>
        /// Sets the receiver endpoint of the message.
        /// </summary>
        /// <param name="endpoint">
        /// The endpoint that identifies the receiver.
        /// </param>
        /// <returns>
        /// Returns the current builder instance to allow chaining.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="endpoint"/> is <c>null</c>.
        /// </exception>
        public MessageBuilder To(IEndpoint endpoint)
        {
            _receiver = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            return this;
        }

        /// <summary>
        /// Sets the receiver to an email address.
        /// </summary>
        /// <param name="email">The email address of the receiver.</param>
        /// <returns>
        /// Returns the current builder instance to allow chaining.
        /// </returns>
        public MessageBuilder ToEmail(string email)
            => To(Endpoint.EmailAddress(email));

        /// <summary>
        /// Sets the receiver to a phone number.
        /// </summary>
        /// <param name="phone">The phone number of the receiver.</param>
        /// <returns>
        /// Returns the current builder instance to allow chaining.
        /// </returns>
        public MessageBuilder ToPhone(string phone)
            => To(Endpoint.PhoneNumber(phone));

        /// <summary>
        /// Sets the content of the message.
        /// </summary>
        /// <param name="content">
        /// The content to assign to the message.
        /// </param>
        /// <returns>
        /// Returns the current builder instance to allow chaining.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="content"/> is <c>null</c>.
        /// </exception>
        public MessageBuilder WithContent(IMessageContent content)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            return this;
        }

        /// <summary>
        /// Sets the content to a plain text body.
        /// </summary>
        /// <param name="text">The plain text content of the message.</param>
        /// <param name="encoding">
        /// An optional encoding identifier for the text.
        /// </param>
        /// <returns>
        /// Returns the current builder instance to allow chaining.
        /// </returns>
        public MessageBuilder WithText(string text, string? encoding = null)
            => WithContent(new TextContent(text, encoding));

        /// <summary>
        /// Sets the content to an HTML body.
        /// </summary>
        /// <param name="html">The HTML content of the message.</param>
        /// <returns>
        /// Returns the current builder instance to allow chaining.
        /// </returns>
        public MessageBuilder WithHtml(string html)
            => WithContent(new HtmlContent(html));

        /// <summary>
        /// Adds a subject property to the message.
        /// </summary>
        /// <param name="subject">The subject line of the message.</param>
        /// <returns>
        /// Returns the current builder instance to allow chaining.
        /// </returns>
        public MessageBuilder WithSubject(string subject)
        {
            if (_properties == null)
                _properties = new Dictionary<string, MessageProperty>(StringComparer.OrdinalIgnoreCase);
            _properties[KnownMessageProperties.Subject] = new MessageProperty(KnownMessageProperties.Subject, subject);
            return this;
        }

        /// <summary>
        /// Adds a custom property to the message.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <returns>
        /// Returns the current builder instance to allow chaining.
        /// </returns>
        public MessageBuilder WithProperty(string name, object value)
        {
            if (_properties == null)
                _properties = new Dictionary<string, MessageProperty>(StringComparer.OrdinalIgnoreCase);
            _properties[name] = new MessageProperty(name, value);
            return this;
        }

        /// <summary>
        /// Builds a <see cref="Message"/> instance from the configured values.
        /// </summary>
        /// <returns>
        /// Returns a new <see cref="Message"/> populated with the values
        /// set through the builder's fluent methods.
        /// </returns>
        public Message Build()
        {
            return new Message
            {
                Id = _id,
                Sender = _sender != null ? new Endpoint(_sender.Type, _sender.Address) : null,
                Receiver = _receiver != null ? new Endpoint(_receiver.Type, _receiver.Address) : null,
                Content = _content != null ? MessageContent.Create(_content) : null,
                Properties = _properties?.ToDictionary(x => x.Key, x => x.Value)
            };
        }
    }
}
