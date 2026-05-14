namespace Deveel.Messaging
{
    /// <summary>
    /// Provides a fluent API to build instances of <see cref="Message"/>.
    /// </summary>
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
        /// <param name="id">The unique identifier to assign to the message.</param>
        /// <returns>This instance for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="id"/> is null or whitespace.</exception>
        public MessageBuilder WithId(string id)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);
            _id = id;
            return this;
        }

        /// <summary>
        /// Sets the sender endpoint of the message.
        /// </summary>
        /// <param name="endpoint">The endpoint representing the sender.</param>
        /// <returns>This instance for chaining.</returns>
        public MessageBuilder From(IEndpoint endpoint)
        {
            _sender = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            return this;
        }

        /// <summary>
        /// Sets the sender as an email address endpoint.
        /// </summary>
        /// <param name="email">The email address of the sender.</param>
        /// <returns>This instance for chaining.</returns>
        public MessageBuilder FromEmail(string email)
            => From(Endpoint.EmailAddress(email));

        /// <summary>
        /// Sets the sender as a phone number endpoint.
        /// </summary>
        /// <param name="phone">The phone number of the sender.</param>
        /// <returns>This instance for chaining.</returns>
        public MessageBuilder FromPhone(string phone)
            => From(Endpoint.PhoneNumber(phone));

        /// <summary>
        /// Sets the receiver endpoint of the message.
        /// </summary>
        /// <param name="endpoint">The endpoint representing the receiver.</param>
        /// <returns>This instance for chaining.</returns>
        public MessageBuilder To(IEndpoint endpoint)
        {
            _receiver = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            return this;
        }

        /// <summary>
        /// Sets the receiver as an email address endpoint.
        /// </summary>
        /// <param name="email">The email address of the receiver.</param>
        /// <returns>This instance for chaining.</returns>
        public MessageBuilder ToEmail(string email)
            => To(Endpoint.EmailAddress(email));

        /// <summary>
        /// Sets the receiver as a phone number endpoint.
        /// </summary>
        /// <param name="phone">The phone number of the receiver.</param>
        /// <returns>This instance for chaining.</returns>
        public MessageBuilder ToPhone(string phone)
            => To(Endpoint.PhoneNumber(phone));

        /// <summary>
        /// Sets the content of the message.
        /// </summary>
        /// <param name="content">The content to include in the message.</param>
        /// <returns>This instance for chaining.</returns>
        public MessageBuilder WithContent(IMessageContent content)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            return this;
        }

        /// <summary>
        /// Sets the content as plain text.
        /// </summary>
        /// <param name="text">The text content.</param>
        /// <param name="encoding">An optional encoding for the text.</param>
        /// <returns>This instance for chaining.</returns>
        public MessageBuilder WithText(string text, string? encoding = null)
            => WithContent(new TextContent(text, encoding));

        /// <summary>
        /// Sets the content as HTML.
        /// </summary>
        /// <param name="html">The HTML content.</param>
        /// <returns>This instance for chaining.</returns>
        public MessageBuilder WithHtml(string html)
            => WithContent(new HtmlContent(html));

        /// <summary>
        /// Sets the content as HTML with optional configuration.
        /// </summary>
        /// <param name="html">The HTML content.</param>
        /// <param name="configure">An action to configure the HTML content.</param>
        /// <returns>This instance for chaining.</returns>
        public MessageBuilder WithHtml(string html, Action<HtmlContent>? configure)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(html, nameof(html));
            var content = new HtmlContent(html);
            configure?.Invoke(content);
            return WithContent(content);
        }

        /// <summary>
        /// Sets the subject of the message as a property.
        /// </summary>
        /// <param name="subject">The subject text.</param>
        /// <returns>This instance for chaining.</returns>
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
        /// <returns>This instance for chaining.</returns>
        public MessageBuilder WithProperty(string name, object value)
        {
            if (_properties == null)
                _properties = new Dictionary<string, MessageProperty>(StringComparer.OrdinalIgnoreCase);
            _properties[name] = new MessageProperty(name, value);
            return this;
        }

        /// <summary>
        /// Adds a collection of properties to the message.
        /// </summary>
        /// <param name="properties">The dictionary of properties to add.</param>
        /// <returns>This instance for chaining.</returns>
        public MessageBuilder WithProperties(IDictionary<string, MessageProperty> properties)
        {
            ArgumentNullException.ThrowIfNull(properties, nameof(properties));
            if (_properties == null)
                _properties = new Dictionary<string, MessageProperty>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in properties)
                _properties[kvp.Key] = kvp.Value;
            return this;
        }

        /// <summary>
        /// Adds a collection of properties from a dictionary of objects.
        /// </summary>
        /// <param name="properties">The dictionary of property names and values.</param>
        /// <returns>This instance for chaining.</returns>
        public MessageBuilder WithProperties(IDictionary<string, object> properties)
        {
            ArgumentNullException.ThrowIfNull(properties, nameof(properties));
            if (_properties == null)
                _properties = new Dictionary<string, MessageProperty>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in properties)
                _properties[kvp.Key] = new MessageProperty(kvp.Key, kvp.Value);
            return this;
        }

        /// <summary>
        /// Sets the remote identifier of the message.
        /// </summary>
        /// <param name="messageId">The remote message identifier.</param>
        /// <returns>This instance for chaining.</returns>
        public MessageBuilder WithRemoteId(string messageId)
            => WithProperty(KnownMessageProperties.RemoteMessageId, messageId);

        /// <summary>
        /// Sets the message identifier that this message is replying to.
        /// </summary>
        /// <param name="messageId">The identifier of the original message.</param>
        /// <returns>This instance for chaining.</returns>
        public MessageBuilder WithReplyTo(string messageId)
            => WithProperty(KnownMessageProperties.ReplyTo, messageId);

        /// <summary>
        /// Builds the <see cref="Message"/> instance with the configured values.
        /// </summary>
        /// <returns>The constructed <see cref="Message"/>.</returns>
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
