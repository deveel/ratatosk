//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
    public class ChannelSchemaBuilder
    {
        private string _channelProvider;
        private string _channelType;
        private string _version;
        private string? _displayName;
        private bool _isStrict = true;
        private ChannelCapability _capabilities = ChannelCapability.SendMessages;
        private readonly List<ChannelParameter> _parameters = new();
        private readonly List<MessagePropertyConfiguration> _messageProperties = new();
        private readonly List<MessageContentType> _contentTypes = new();
        private readonly List<AuthenticationConfiguration> _authenticationConfigurations = new();
        private readonly List<ChannelEndpointConfiguration> _endpoints = new();

        public ChannelSchemaBuilder(string channelProvider, string channelType, string version)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(channelProvider, nameof(channelProvider));
            ArgumentException.ThrowIfNullOrWhiteSpace(channelType, nameof(channelType));
            ArgumentException.ThrowIfNullOrWhiteSpace(version, nameof(version));

            _channelProvider = channelProvider;
            _channelType = channelType;
            _version = version;
        }

        private ChannelSchemaBuilder(IChannelSchema source)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            _channelProvider = source.ChannelProvider;
            _channelType = source.ChannelType;
            _version = source.Version;
            _isStrict = source.IsStrict;
            _displayName = source.DisplayName;
            _capabilities = source.Capabilities;

            foreach (var param in source.Parameters)
            {
                _parameters.Add(new ChannelParameter(param.Name, param.DataType)
                {
                    IsRequired = param.IsRequired,
                    IsSensitive = param.IsSensitive,
                    DefaultValue = param.DefaultValue,
                    Description = param.Description,
                    AllowedValues = param.AllowedValues?.ToArray()
                });
            }

            foreach (var msgProp in source.MessageProperties)
            {
                _messageProperties.Add(new MessagePropertyConfiguration(msgProp.Name, msgProp.DataType)
                {
                    IsRequired = msgProp.IsRequired,
                    IsSensitive = msgProp.IsSensitive,
                    Description = msgProp.Description
                });
            }

            _contentTypes = new List<MessageContentType>(source.ContentTypes);

            foreach (var authConfig in source.AuthenticationConfigurations)
            {
                var newAuthConfig = new AuthenticationConfiguration(authConfig.Scheme, authConfig.DisplayName);

                foreach (var field in authConfig.Fields)
                {
                    newAuthConfig.WithField(new AuthenticationField(field.FieldName, field.DataType)
                    {
                        IsSensitive = field.IsSensitive,
                        DisplayName = field.DisplayName,
                        Description = field.Description,
                        AuthenticationRole = field.AuthenticationRole,
                        AllowedValues = field.AllowedValues?.ToList()
                    });
                }

                _authenticationConfigurations.Add(newAuthConfig);
            }

            foreach (var endpoint in source.Endpoints)
            {
                _endpoints.Add(new ChannelEndpointConfiguration(endpoint.Type)
                {
                    CanSend = endpoint.CanSend,
                    CanReceive = endpoint.CanReceive,
                    IsRequired = endpoint.IsRequired
                });
            }
        }

        public static ChannelSchemaBuilder From(IChannelSchema source, string? derivedDisplayName = null)
        {
            var builder = new ChannelSchemaBuilder(source);
            builder._displayName = derivedDisplayName ?? $"{source.DisplayName} (Copy)";
            return builder;
        }

        public ChannelSchemaBuilder WithId(string id) => this;

        public ChannelSchemaBuilder WithDisplayName(string? displayName)
        {
            _displayName = displayName;
            return this;
        }

        public ChannelSchemaBuilder WithCapabilities(ChannelCapability capabilities)
        {
            _capabilities = capabilities;
            return this;
        }

        public ChannelSchemaBuilder WithCapability(ChannelCapability capability)
        {
            _capabilities |= capability;
            return this;
        }

        public ChannelSchemaBuilder WithStrictMode(bool isStrict)
        {
            _isStrict = isStrict;
            return this;
        }

        public ChannelSchemaBuilder WithStrictMode()
        {
            return WithStrictMode(true);
        }

        public ChannelSchemaBuilder WithFlexibleMode()
        {
            return WithStrictMode(false);
        }

        public ChannelSchemaBuilder AddParameter(ChannelParameter parameter)
        {
            ArgumentNullException.ThrowIfNull(parameter, nameof(parameter));
            _parameters.Add(parameter);
            return this;
        }

        public ChannelSchemaBuilder AddParameter(string parameterName, DataType parameterType, Action<ChannelParameter>? configure = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(parameterName, nameof(parameterName));
            var parameter = new ChannelParameter(parameterName, parameterType);
            configure?.Invoke(parameter);
            return AddParameter(parameter);
        }

        public ChannelSchemaBuilder AddRequiredParameter(string parameterName, DataType parameterType, bool sensitive = false)
            => AddParameter(parameterName, parameterType, param => { param.IsRequired = true; param.IsSensitive = sensitive; });

        public ChannelSchemaBuilder AddMessageProperty(MessagePropertyConfiguration property)
        {
            ArgumentNullException.ThrowIfNull(property, nameof(property));

            if (_messageProperties.Any(p => string.Equals(p.Name, property.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"A message property configuration with name '{property.Name}' already exists in the schema.");

            _messageProperties.Add(property);
            return this;
        }

        public ChannelSchemaBuilder AddMessageProperty(string propertyName, DataType propertyType, Action<MessagePropertyConfiguration>? configure = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, nameof(propertyName));
            var property = new MessagePropertyConfiguration(propertyName, propertyType);
            configure?.Invoke(property);
            return AddMessageProperty(property);
        }

        public ChannelSchemaBuilder AddContentType(MessageContentType contentType)
        {
            _contentTypes.Add(contentType);
            return this;
        }

        /// <summary>
        /// Adds a pre-built authentication configuration for the given <paramref name="scheme"/>,
        /// using sensible default field mappings for well-known schemes.
        /// Scheme fields are <b>not</b> automatically added as schema parameters;
        /// use <see cref="AddAuthenticationConfiguration(AuthenticationConfiguration)"/>
        /// with explicit fields if you need them in the parameter list.
        /// </summary>
        /// <param name="scheme">The scheme to add.</param>
        /// <returns>This builder for chaining.</returns>
        public ChannelSchemaBuilder AddAuthenticationScheme(AuthenticationScheme scheme)
        {
            var config = BuildDefaultConfiguration(scheme);
            _authenticationConfigurations.Add(config);
            return this;
        }

        /// <summary>
        /// Adds a pre-built authentication configuration for the given <paramref name="authenticationType"/>.
        /// </summary>
        /// <param name="authenticationType">The authentication type to add.</param>
        /// <returns>This builder for chaining.</returns>
        [Obsolete("Use AddAuthenticationScheme instead")]
        public ChannelSchemaBuilder AddAuthenticationType(AuthenticationScheme authenticationType)
        {
            return AddAuthenticationScheme(authenticationType);
        }

        public ChannelSchemaBuilder AddAuthenticationConfiguration(AuthenticationConfiguration authenticationConfiguration)
        {
            ArgumentNullException.ThrowIfNull(authenticationConfiguration, nameof(authenticationConfiguration));

            if (_authenticationConfigurations.Any(c => c.Scheme == authenticationConfiguration.Scheme))
                throw new InvalidOperationException($"An authentication configuration for scheme '{authenticationConfiguration.Scheme}' already exists in the schema.");

            foreach (var field in authenticationConfiguration.Fields)
            {
                if (!string.Equals(field.AuthenticationRole, "principal", StringComparison.OrdinalIgnoreCase))
                    continue;

                var existing = _parameters.FirstOrDefault(p => string.Equals(p.Name, field.FieldName, StringComparison.OrdinalIgnoreCase));
                if (existing == null)
                {
                    _parameters.Add(new ChannelParameter(field.FieldName, field.DataType)
                    {
                        IsRequired = false,
                        IsSensitive = field.IsSensitive,
                        Description = field.Description ?? $"The '{field.FieldName}' parameter for authentication",
                        AllowedValues = field.AllowedValues?.ToArray()
                    });
                }
            }

            _authenticationConfigurations.Add(authenticationConfiguration);
            return this;
        }

        public ChannelSchemaBuilder AddAuthenticationConfiguration(Func<AuthenticationConfiguration> configurationFactory)
        {
            ArgumentNullException.ThrowIfNull(configurationFactory, nameof(configurationFactory));
            var config = configurationFactory();
            return AddAuthenticationConfiguration(config);
        }

        public ChannelSchemaBuilder HandlesMessageEndpoint(ChannelEndpointConfiguration endpoint)
        {
            ArgumentNullException.ThrowIfNull(endpoint, nameof(endpoint));

            if (_endpoints.Any(e => e.Type == endpoint.Type))
                throw new InvalidOperationException($"An endpoint configuration with type '{endpoint.Type}' already exists in the schema.");

            _endpoints.Add(endpoint);
            return this;
        }

        public ChannelSchemaBuilder HandlesMessageEndpoint(EndpointType endpointType, Action<ChannelEndpointConfiguration>? configure = null)
        {
            var endpoint = new ChannelEndpointConfiguration(endpointType);
            configure?.Invoke(endpoint);
            return HandlesMessageEndpoint(endpoint);
        }

        public ChannelSchemaBuilder AllowsAnyMessageEndpoint()
        {
            if (_endpoints.Any(e => e.Type == EndpointType.Any))
                throw new InvalidOperationException($"An endpoint configuration with type '{EndpointType.Any}' already exists in the schema.");

            _endpoints.Add(new ChannelEndpointConfiguration(EndpointType.Any)
            {
                CanSend = true,
                CanReceive = true
            });
            return this;
        }

        public ChannelSchemaBuilder RemoveParameter(string parameterName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(parameterName, nameof(parameterName));
            var parameter = _parameters.FirstOrDefault(p => string.Equals(p.Name, parameterName, StringComparison.OrdinalIgnoreCase));
            if (parameter != null)
                _parameters.Remove(parameter);
            return this;
        }

        public ChannelSchemaBuilder RemoveMessageProperty(string propertyName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, nameof(propertyName));
            var property = _messageProperties.FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));
            if (property != null)
                _messageProperties.Remove(property);
            return this;
        }

        public ChannelSchemaBuilder RemoveContentType(MessageContentType contentType)
        {
            _contentTypes.Remove(contentType);
            return this;
        }

        private static AuthenticationConfiguration BuildDefaultConfiguration(AuthenticationScheme scheme)
        {
            if (scheme == AuthenticationScheme.None)
                return new AuthenticationConfiguration(scheme, "No Authentication");
            if (scheme == AuthenticationScheme.Basic)
                return BuildFlexibleBasic();
            if (scheme == AuthenticationScheme.ApiKey)
                return BuildFlexibleApiKey();
            if (scheme == AuthenticationScheme.Bearer)
                return BuildFlexibleBearerToken();
            if (scheme == AuthenticationScheme.OAuthClientCredentials)
                return new AuthenticationConfiguration(scheme, "Client Credentials (OAuth 2.0)")
                    .WithField("ClientId", DataType.String, f => { f.DisplayName = "Client ID"; f.AuthenticationRole = "principal"; })
                    .WithField("ClientSecret", DataType.String, f => { f.DisplayName = "Client Secret"; f.AuthenticationRole = "credential"; f.IsSensitive = true; });
            if (scheme == AuthenticationScheme.Certificate)
                return BuildFlexibleCertificate();
            if (scheme == AuthenticationScheme.Digest)
                return new AuthenticationConfiguration(scheme, "Digest Authentication")
                    .WithField("Username", DataType.String, f => f.AuthenticationRole = "principal")
                    .WithField("Password", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; })
                    .WithField("Realm", DataType.String, f => f.AuthenticationRole = "realm");

            return new AuthenticationConfiguration(scheme, scheme.Name);
        }

        private static AuthenticationConfiguration BuildFlexibleBasic()
        {
            return new AuthenticationConfiguration(AuthenticationScheme.Basic, "Flexible Basic Authentication")
                .WithField("Username", DataType.String, f => { f.AuthenticationRole = "principal"; })
                .WithField("Password", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; })
                .WithField("AccountSid", DataType.String, f => { f.AuthenticationRole = "principal"; })
                .WithField("AuthToken", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; })
                .WithField("User", DataType.String, f => { f.AuthenticationRole = "principal"; })
                .WithField("Pass", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; })
                .WithField("ClientId", DataType.String, f => { f.AuthenticationRole = "principal"; })
                .WithField("ClientSecret", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; });
        }

        private static AuthenticationConfiguration BuildFlexibleApiKey()
        {
            return new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "Flexible API Key Authentication")
                .WithField("ApiKey", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; })
                .WithField("Key", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; })
                .WithField("AccessKey", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; });
        }

        private static AuthenticationConfiguration BuildFlexibleBearerToken()
        {
            return new AuthenticationConfiguration(AuthenticationScheme.Bearer, "Flexible Bearer Token Authentication")
                .WithField("Token", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; })
                .WithField("AccessToken", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; })
                .WithField("BearerToken", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; })
                .WithField("AuthToken", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; });
        }

        private static AuthenticationConfiguration BuildFlexibleCertificate()
        {
            return new AuthenticationConfiguration(AuthenticationScheme.Certificate, "Flexible Certificate Authentication")
                .WithField("Certificate", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; })
                .WithField("CertificatePath", DataType.String, f => { f.AuthenticationRole = "principal"; })
                .WithField("CertificateThumbprint", DataType.String, f => { f.AuthenticationRole = "principal"; })
                .WithField("PfxFile", DataType.String, f => { f.AuthenticationRole = "principal"; })
                .WithField("PfxPassword", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; })
                .WithField("CertificatePassword", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; });
        }

        public ChannelSchemaBuilder RemoveCapability(ChannelCapability capability)
        {
            _capabilities &= ~capability;
            return this;
        }

        public ChannelSchemaBuilder RemoveEndpoint(EndpointType endpointType)
        {
            var endpoint = _endpoints.FirstOrDefault(e => e.Type == endpointType);
            if (endpoint != null)
                _endpoints.Remove(endpoint);
            return this;
        }

        public ChannelSchemaBuilder RestrictCapabilities(ChannelCapability allowedCapabilities)
        {
            _capabilities &= allowedCapabilities;
            return this;
        }

        public ChannelSchemaBuilder RestrictContentTypes(params MessageContentType[] allowedContentTypes)
        {
            _contentTypes.Clear();
            foreach (var contentType in allowedContentTypes)
                _contentTypes.Add(contentType);
            return this;
        }

        public ChannelSchemaBuilder RestrictAuthenticationSchemes(params AuthenticationScheme[] allowedSchemes)
        {
            var configurationsToRemove = _authenticationConfigurations
                .Where(c => !allowedSchemes.Contains(c.Scheme))
                .ToList();
            foreach (var config in configurationsToRemove)
                _authenticationConfigurations.Remove(config);
            return this;
        }

        public ChannelSchemaBuilder RestrictAuthenticationConfigurations(params AuthenticationConfiguration[] allowedConfigurations)
        {
            _authenticationConfigurations.Clear();
            foreach (var config in allowedConfigurations)
                _authenticationConfigurations.Add(config);
            return this;
        }

        public ChannelSchemaBuilder UpdateParameter(string parameterName, Action<ChannelParameter> updateAction)
        {
            var parameter = _parameters.FirstOrDefault(p => string.Equals(p.Name, parameterName, StringComparison.OrdinalIgnoreCase));
            if (parameter == null)
                throw new InvalidOperationException($"Parameter with name '{parameterName}' not found in the schema.");
            updateAction(parameter);
            return this;
        }

        public ChannelSchemaBuilder UpdateMessageProperty(string propertyName, Action<MessagePropertyConfiguration> updateAction)
        {
            var property = _messageProperties.FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));
            if (property == null)
                throw new InvalidOperationException($"Message property with name '{propertyName}' not found in the schema.");
            updateAction(property);
            return this;
        }

        public ChannelSchemaBuilder UpdateEndpoint(EndpointType endpointType, Action<ChannelEndpointConfiguration> updateAction)
        {
            var endpoint = _endpoints.FirstOrDefault(e => e.Type == endpointType);
            if (endpoint == null)
                throw new InvalidOperationException($"Endpoint with type '{endpointType}' not found in the schema.");
            updateAction(endpoint);
            return this;
        }

        /// <summary>
        /// Builds a <see cref="ChannelSchema"/> instance from the current builder state.
        /// </summary>
        /// <returns>A new <see cref="ChannelSchema"/> instance.</returns>
        public ChannelSchema Build()
        {
            return new ChannelSchema(
                _channelProvider,
                _channelType,
                _version,
                _displayName,
                _isStrict,
                _capabilities,
                _parameters,
                _messageProperties,
                _contentTypes,
                _authenticationConfigurations,
                _endpoints);
        }
    }
}
