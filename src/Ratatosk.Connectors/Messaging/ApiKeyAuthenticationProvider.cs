//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ratatosk
{
    /// <summary>
    /// An <see cref="IAuthenticationProvider"/> that extracts an API key from the
    /// connection settings using fields with the <c>"principal"</c> role.
    /// </summary>
    public class ApiKeyAuthenticationProvider : AuthenticationProviderBase
    {
        private readonly ILogger<ApiKeyAuthenticationProvider> _logger;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="logger">An optional logger.</param>
        public ApiKeyAuthenticationProvider(ILogger<ApiKeyAuthenticationProvider>? logger = null)
            : base(AuthenticationScheme.ApiKey, "API Key Authentication")
        {
            _logger = logger ?? NullLogger<ApiKeyAuthenticationProvider>.Instance;
        }

        /// <inheritdoc/>
        public override Task<AuthenticationResult> ObtainCredentialAsync(ConnectionSettings connectionSettings, AuthenticationConfiguration configuration, CancellationToken cancellationToken = default)
        {
            var principalFields = configuration.GetFieldsByRole("principal");

            foreach (var field in principalFields)
            {
                var value = connectionSettings.GetParameter(field.FieldName)?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _logger.LogFoundCredentials(AuthenticationScheme.ApiKey, field.FieldName);
                    var credential = AuthenticationCredential.ForApiKey(value);
                    credential.Properties["FieldName"] = field.FieldName;
                    return Task.FromResult(Success(credential));
                }
            }

            var fieldNames = string.Join(", ", principalFields.Select(f => f.FieldName));
            return Task.FromResult(Failure($"API key not found. Expected one of: {fieldNames}", "MISSING_API_KEY"));
        }
    }
}
