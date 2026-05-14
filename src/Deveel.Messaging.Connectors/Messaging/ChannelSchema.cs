//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.ComponentModel.DataAnnotations;

namespace Deveel.Messaging
{
    /// <summary>
    /// Provides a base implementation of <see cref="IChannelSchema"/> 
    /// that defines the schema for a communication channel.
    /// </summary>
    /// <remarks>
    /// This class serves as a foundation for creating channel schemas 
    /// with customizable properties, capabilities, and configuration parameters.
    /// Use <see cref="ChannelSchemaBuilder"/> to construct instances with a fluent API.
    /// </remarks>
    public class ChannelSchema : IChannelSchema
    {
        private readonly IList<ChannelParameter> parameters;
        private readonly IList<MessagePropertyConfiguration> messageProperties;
        private readonly IList<MessageContentType> contentTypes;
        private readonly IList<AuthenticationConfiguration> authenticationConfigurations;
        private readonly IList<ChannelEndpointConfiguration> endpoints;

        /// <summary>
        /// Constructs a new channel schema with the specified provider, type, and version.
        /// </summary>
        /// <param name="channelProvider">The channel provider identifier.</param>
        /// <param name="channelType">The type of communication channel.</param>
        /// <param name="version">The version of the schema.</param>
        public ChannelSchema(string channelProvider, string channelType, string version)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(channelProvider, nameof(channelProvider));
            ArgumentException.ThrowIfNullOrWhiteSpace(channelType, nameof(channelType));
            ArgumentException.ThrowIfNullOrWhiteSpace(version, nameof(version));

            ChannelProvider = channelProvider;
            ChannelType = channelType;
            Version = version;
            IsStrict = true;
            parameters = new List<ChannelParameter>();
            messageProperties = new List<MessagePropertyConfiguration>();
            contentTypes = new List<MessageContentType>();
            authenticationConfigurations = new List<AuthenticationConfiguration>();
            endpoints = new List<ChannelEndpointConfiguration>();
            Capabilities = ChannelCapability.SendMessages;
        }

        /// <summary>
        /// Constructs a new channel schema by copying from an existing schema.
        /// </summary>
        /// <param name="sourceSchema">The source schema to copy from.</param>
        /// <param name="derivedDisplayName">
        /// An optional display name for the derived schema; if not provided,
        /// a default copy label is used.
        /// </param>
        public ChannelSchema(IChannelSchema sourceSchema, string? derivedDisplayName = null)
        {
            ArgumentNullException.ThrowIfNull(sourceSchema, nameof(sourceSchema));

            ChannelProvider = sourceSchema.ChannelProvider;
            ChannelType = sourceSchema.ChannelType;
            Version = sourceSchema.Version;
            IsStrict = sourceSchema.IsStrict;
            DisplayName = derivedDisplayName ?? $"{sourceSchema.DisplayName} (Copy)";
            Capabilities = sourceSchema.Capabilities;

            parameters = new List<ChannelParameter>();
            foreach (var param in sourceSchema.Parameters)
            {
                parameters.Add(new ChannelParameter(param.Name, param.DataType)
                {
                    IsRequired = param.IsRequired,
                    IsSensitive = param.IsSensitive,
                    DefaultValue = param.DefaultValue,
                    Description = param.Description,
                    AllowedValues = param.AllowedValues?.ToArray()
                });
            }

            messageProperties = new List<MessagePropertyConfiguration>();
            foreach (var msgProp in sourceSchema.MessageProperties)
            {
                messageProperties.Add(new MessagePropertyConfiguration(msgProp.Name, msgProp.DataType)
                {
                    IsRequired = msgProp.IsRequired,
                    IsSensitive = msgProp.IsSensitive,
                    Description = msgProp.Description
                });
            }

            contentTypes = new List<MessageContentType>(sourceSchema.ContentTypes);

            authenticationConfigurations = new List<AuthenticationConfiguration>();
            foreach (var authConfig in sourceSchema.AuthenticationConfigurations)
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

                authenticationConfigurations.Add(newAuthConfig);
            }

            endpoints = new List<ChannelEndpointConfiguration>();
            foreach (var endpoint in sourceSchema.Endpoints)
            {
                endpoints.Add(new ChannelEndpointConfiguration(endpoint.Type)
                {
                    CanSend = endpoint.CanSend,
                    CanReceive = endpoint.CanReceive,
                    IsRequired = endpoint.IsRequired
                });
            }
        }

        internal ChannelSchema(
            string channelProvider,
            string channelType,
            string version,
            string? displayName,
            bool isStrict,
            ChannelCapability capabilities,
            IEnumerable<ChannelParameter> parameters,
            IEnumerable<MessagePropertyConfiguration> messageProperties,
            IEnumerable<MessageContentType> contentTypes,
            IEnumerable<AuthenticationConfiguration> authenticationConfigurations,
            IEnumerable<ChannelEndpointConfiguration> endpoints)
        {
            ChannelProvider = channelProvider;
            ChannelType = channelType;
            Version = version;
            DisplayName = displayName;
            IsStrict = isStrict;
            Capabilities = capabilities;
            this.parameters = new List<ChannelParameter>(parameters);
            this.messageProperties = new List<MessagePropertyConfiguration>(messageProperties);
            this.contentTypes = new List<MessageContentType>(contentTypes);
            this.authenticationConfigurations = new List<AuthenticationConfiguration>(authenticationConfigurations);
            this.endpoints = new List<ChannelEndpointConfiguration>(endpoints);
        }

        /// <inheritdoc/>
        public string ChannelProvider { get; }

        /// <inheritdoc/>
        public string ChannelType { get; }

        /// <inheritdoc/>
        public string Version { get; }

        /// <inheritdoc/>
        public string? DisplayName { get; set; }

        /// <inheritdoc/>
        public bool IsStrict { get; set; }

        /// <inheritdoc/>
        public ChannelCapability Capabilities { get; set; }

        /// <inheritdoc/>
        public IReadOnlyList<ChannelParameter> Parameters => parameters.AsReadOnly();

        /// <inheritdoc/>
        public IReadOnlyList<MessagePropertyConfiguration> MessageProperties => messageProperties.AsReadOnly();

        /// <inheritdoc/>
        public IReadOnlyList<MessageContentType> ContentTypes => contentTypes.AsReadOnly();

        /// <inheritdoc/>
        public IReadOnlyList<AuthenticationConfiguration> AuthenticationConfigurations => authenticationConfigurations.AsReadOnly();

        /// <inheritdoc/>
        public IReadOnlyList<ChannelEndpointConfiguration> Endpoints => endpoints.AsReadOnly();

        /// <summary>
        /// Gets the distinct authentication types supported by the schema.
        /// </summary>
        public IEnumerable<AuthenticationType> AuthenticationTypes =>
            AuthenticationConfigurations.Select(c => c.AuthenticationType).Distinct();
    }
}
