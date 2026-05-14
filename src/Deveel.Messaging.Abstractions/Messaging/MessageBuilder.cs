namespace Deveel.Messaging
{
    public class MessageBuilder
    {
        private string _id = "";
        private IEndpoint? _sender;
        private IEndpoint? _receiver;
        private IMessageContent? _content;
        private Dictionary<string, MessageProperty>? _properties;

        public MessageBuilder WithId(string id)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);
            _id = id;
            return this;
        }

        public MessageBuilder From(IEndpoint endpoint)
        {
            _sender = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            return this;
        }

        public MessageBuilder FromEmail(string email)
            => From(Endpoint.EmailAddress(email));

        public MessageBuilder FromPhone(string phone)
            => From(Endpoint.PhoneNumber(phone));

        public MessageBuilder To(IEndpoint endpoint)
        {
            _receiver = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            return this;
        }

        public MessageBuilder ToEmail(string email)
            => To(Endpoint.EmailAddress(email));

        public MessageBuilder ToPhone(string phone)
            => To(Endpoint.PhoneNumber(phone));

        public MessageBuilder WithContent(IMessageContent content)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            return this;
        }

        public MessageBuilder WithText(string text, string? encoding = null)
            => WithContent(new TextContent(text, encoding));

        public MessageBuilder WithHtml(string html)
            => WithContent(new HtmlContent(html));

        public MessageBuilder WithHtml(string html, Action<HtmlContent>? configure)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(html, nameof(html));
            var content = new HtmlContent(html);
            configure?.Invoke(content);
            return WithContent(content);
        }

        public MessageBuilder WithSubject(string subject)
        {
            if (_properties == null)
                _properties = new Dictionary<string, MessageProperty>(StringComparer.OrdinalIgnoreCase);
            _properties[KnownMessageProperties.Subject] = new MessageProperty(KnownMessageProperties.Subject, subject);
            return this;
        }

        public MessageBuilder WithProperty(string name, object value)
        {
            if (_properties == null)
                _properties = new Dictionary<string, MessageProperty>(StringComparer.OrdinalIgnoreCase);
            _properties[name] = new MessageProperty(name, value);
            return this;
        }

        public MessageBuilder WithProperties(IDictionary<string, MessageProperty> properties)
        {
            ArgumentNullException.ThrowIfNull(properties, nameof(properties));
            if (_properties == null)
                _properties = new Dictionary<string, MessageProperty>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in properties)
                _properties[kvp.Key] = kvp.Value;
            return this;
        }

        public MessageBuilder WithProperties(IDictionary<string, object> properties)
        {
            ArgumentNullException.ThrowIfNull(properties, nameof(properties));
            if (_properties == null)
                _properties = new Dictionary<string, MessageProperty>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in properties)
                _properties[kvp.Key] = new MessageProperty(kvp.Key, kvp.Value);
            return this;
        }

        public MessageBuilder WithRemoteId(string messageId)
            => WithProperty(KnownMessageProperties.RemoteMessageId, messageId);

        public MessageBuilder WithReplyTo(string messageId)
            => WithProperty(KnownMessageProperties.ReplyTo, messageId);

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
