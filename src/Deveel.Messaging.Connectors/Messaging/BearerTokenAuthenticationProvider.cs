//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Deveel.Messaging
{
    /// <summary>
    /// An <see cref="IAuthenticationProvider"/> that extracts a bearer token from the
    /// connection settings using fields with the <c>"principal"</c> role.
    /// </summary>
    public class BearerTokenAuthenticationProvider : AuthenticationProviderBase
    {
        private readonly ILogger<BearerTokenAuthenticationProvider> _logger;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="logger">An optional logger.</param>
        public BearerTokenAuthenticationProvider(ILogger<BearerTokenAuthenticationProvider>? logger = null)
            : base(AuthenticationScheme.Bearer, "Bearer Token Authentication")
        {
            _logger = logger ?? NullLogger<BearerTokenAuthenticationProvider>.Instance;
        }

        /// <inheritdoc/>
        public override Task<AuthenticationResult> ObtainCredentialAsync(ConnectionSettings connectionSettings, AuthenticationConfiguration configuration, CancellationToken cancellationToken = default)
        {
            var principalFields = configuration.GetFieldsByRole("principal");

            foreach (var field in principalFields)
            {
                var value = GetStringParameter(connectionSettings, field.FieldName);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _logger.LogDebug("Found bearer token in field: {Field}", field.FieldName);

                    var tokenType = GetStringParameter(connectionSettings, "TokenType") ?? "Bearer";

                    DateTime? expiresAt = null;
                    var expirationString = GetStringParameter(connectionSettings, "ExpiresAt");
                    if (!string.IsNullOrWhiteSpace(expirationString) && DateTime.TryParse(expirationString, out var expirationDate))
                    {
                        expiresAt = expirationDate;
                    }

                    var credential = AuthenticationCredential.ForBearerToken(value, expiresAt, tokenType);
                    credential.Properties["FieldName"] = field.FieldName;
                    return Task.FromResult(Success(credential));
                }
            }

            var fieldNames = string.Join(", ", principalFields.Select(f => f.FieldName));
            return Task.FromResult(Failure($"Bearer token not found. Expected one of: {fieldNames}", "MISSING_TOKEN"));
        }
    }
}
