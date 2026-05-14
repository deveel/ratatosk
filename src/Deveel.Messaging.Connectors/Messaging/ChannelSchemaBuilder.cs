//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
    /// <summary>
    /// A fluent builder for constructing <see cref="ChannelSchema"/> instances.
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the builder with the specified
        /// channel provider, type, and version.
        /// </summary>
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
                var newAuthConfig = new AuthenticationConfiguration(authConfig.AuthenticationType, authConfig.DisplayName);

                foreach (var field in authConfig.RequiredFields)
                {
                    newAuthConfig.WithRequiredField(new AuthenticationField(field.FieldName, field.DataType)
                    {
                        IsSensitive = field.IsSensitive,
                        DisplayName = field.DisplayName,
                        Description = field.Description,
                        AuthenticationRole = field.AuthenticationRole,
                        AllowedValues = field.AllowedValues?.ToList()
                    });
                }

                foreach (var field in authConfig.OptionalFields)
                {
                    newAuthConfig.WithOptionalField(new AuthenticationField(field.FieldName, field.DataType)
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

        /// <summary>
        /// Creates a new builder pre-populated with the data from the given source schema.
        /// </summary>
        /// <param name="source">The source schema to copy from.</param>
        /// <param name="derivedDisplayName">
        /// An optional display name for the new schema to distinguish it from the source.
        /// </param>
        /// <returns>A new builder instance with the source schema's data.</returns>
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

        public ChannelSchemaBuilder AddAuthenticationType(AuthenticationType authenticationType)
        {
            var config = CreateBasicAuthenticationConfiguration(authenticationType);
            return AddAuthenticationConfiguration(config);
        }

        public ChannelSchemaBuilder AddAuthenticationConfiguration(AuthenticationConfiguration authenticationConfiguration)
        {
            ArgumentNullException.ThrowIfNull(authenticationConfiguration, nameof(authenticationConfiguration));

            if (_authenticationConfigurations.Any(c => c.AuthenticationType == authenticationConfiguration.AuthenticationType))
                throw new InvalidOperationException($"An authentication configuration for '{authenticationConfiguration.AuthenticationType}' authentication type already exists in the schema.");

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

        public ChannelSchemaBuilder RemoveAuthenticationType(AuthenticationType authenticationType)
        {
            var configToRemove = _authenticationConfigurations.FirstOrDefault(c => c.AuthenticationType == authenticationType);
            if (configToRemove != null)
                _authenticationConfigurations.Remove(configToRemove);
            return this;
        }

        public ChannelSchemaBuilder RemoveAuthenticationConfiguration(AuthenticationType authenticationType)
        {
            var configToRemove = _authenticationConfigurations.FirstOrDefault(c => c.AuthenticationType == authenticationType);
            if (configToRemove != null)
                _authenticationConfigurations.Remove(configToRemove);
            return this;
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
            ArgumentNullException.ThrowIfNull(allowedContentTypes, nameof(allowedContentTypes));
            _contentTypes.Clear();
            foreach (var contentType in allowedContentTypes)
                _contentTypes.Add(contentType);
            return this;
        }

        public ChannelSchemaBuilder RestrictAuthenticationTypes(params AuthenticationType[] allowedAuthenticationTypes)
        {
            ArgumentNullException.ThrowIfNull(allowedAuthenticationTypes, nameof(allowedAuthenticationTypes));
            var configurationsToRemove = _authenticationConfigurations
                .Where(c => !allowedAuthenticationTypes.Contains(c.AuthenticationType))
                .ToList();
            foreach (var config in configurationsToRemove)
                _authenticationConfigurations.Remove(config);
            return this;
        }

        public ChannelSchemaBuilder RestrictAuthenticationConfigurations(params AuthenticationConfiguration[] allowedConfigurations)
        {
            ArgumentNullException.ThrowIfNull(allowedConfigurations, nameof(allowedConfigurations));
            _authenticationConfigurations.Clear();
            foreach (var config in allowedConfigurations)
                _authenticationConfigurations.Add(config);
            return this;
        }

        public ChannelSchemaBuilder UpdateParameter(string parameterName, Action<ChannelParameter> updateAction)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(parameterName, nameof(parameterName));
            ArgumentNullException.ThrowIfNull(updateAction, nameof(updateAction));

            var parameter = _parameters.FirstOrDefault(p => string.Equals(p.Name, parameterName, StringComparison.OrdinalIgnoreCase));
            if (parameter == null)
                throw new InvalidOperationException($"Parameter with name '{parameterName}' not found in the schema.");

            updateAction(parameter);
            return this;
        }

        public ChannelSchemaBuilder UpdateMessageProperty(string propertyName, Action<MessagePropertyConfiguration> updateAction)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, nameof(propertyName));
            ArgumentNullException.ThrowIfNull(updateAction, nameof(updateAction));

            var property = _messageProperties.FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));
            if (property == null)
                throw new InvalidOperationException($"Message property with name '{propertyName}' not found in the schema.");

            updateAction(property);
            return this;
        }

        public ChannelSchemaBuilder UpdateEndpoint(EndpointType endpointType, Action<ChannelEndpointConfiguration> updateAction)
        {
            ArgumentNullException.ThrowIfNull(updateAction, nameof(updateAction));

            var endpoint = _endpoints.FirstOrDefault(e => e.Type == endpointType);
            if (endpoint == null)
                throw new InvalidOperationException($"Endpoint with type '{endpointType}' not found in the schema.");

            updateAction(endpoint);
            return this;
        }

        private static AuthenticationConfiguration CreateBasicAuthenticationConfiguration(AuthenticationType authenticationType)
        {
            return authenticationType switch
            {
                AuthenticationType.None => new AuthenticationConfiguration(AuthenticationType.None, "No Authentication"),
                AuthenticationType.Basic => Messaging.AuthenticationConfigurations.FlexibleBasicAuthentication(),
                AuthenticationType.ApiKey => Messaging.AuthenticationConfigurations.FlexibleApiKeyAuthentication(),
                AuthenticationType.Token => Messaging.AuthenticationConfigurations.FlexibleTokenAuthentication(),
                AuthenticationType.ClientCredentials => Messaging.AuthenticationConfigurations.ClientCredentialsAuthentication(),
                AuthenticationType.Certificate => Messaging.AuthenticationConfigurations.FlexibleCertificateAuthentication(),
                AuthenticationType.Custom => CreateFlexibleCustomAuthentication(),
                _ => throw new ArgumentException($"Unsupported authentication type: {authenticationType}", nameof(authenticationType))
            };
        }

        private static AuthenticationConfiguration CreateFlexibleCustomAuthentication()
        {
            var config = new FlexibleAuthenticationConfiguration(AuthenticationType.Custom, "Custom Authentication");

            config.WithOptionalField("CustomAuth", DataType.String, field =>
                {
                    field.DisplayName = "Custom Auth";
                    field.Description = "Custom authentication data";
                    field.AuthenticationRole = "CustomAuth";
                })
                .WithOptionalField("AuthenticationData", DataType.String, field =>
                {
                    field.DisplayName = "Authentication Data";
                    field.Description = "Custom authentication data";
                    field.AuthenticationRole = "AuthenticationData";
                })
                .WithOptionalField("Credentials", DataType.String, field =>
                {
                    field.DisplayName = "Credentials";
                    field.Description = "Custom credentials";
                    field.AuthenticationRole = "Credentials";
                })
                .WithOptionalField("AuthConfig", DataType.String, field =>
                {
                    field.DisplayName = "Auth Config";
                    field.Description = "Authentication configuration";
                    field.AuthenticationRole = "AuthConfig";
                })
                .WithOptionalField("SecretKey", DataType.String, field =>
                {
                    field.DisplayName = "Secret Key";
                    field.Description = "Secret key for authentication";
                    field.AuthenticationRole = "SecretKey";
                    field.IsSensitive = true;
                })
                .WithOptionalField("PrivateKey", DataType.String, field =>
                {
                    field.DisplayName = "Private Key";
                    field.Description = "Private key for authentication";
                    field.AuthenticationRole = "PrivateKey";
                    field.IsSensitive = true;
                })
                .WithOptionalField("Signature", DataType.String, field =>
                {
                    field.DisplayName = "Signature";
                    field.Description = "Authentication signature";
                    field.AuthenticationRole = "Signature";
                })
                .WithOptionalField("Hash", DataType.String, field =>
                {
                    field.DisplayName = "Hash";
                    field.Description = "Authentication hash";
                    field.AuthenticationRole = "Hash";
                });

            return config;
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
