//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ratatosk
{
    /// <summary>
    /// An <see cref="IAuthenticationProvider"/> that extracts a username/password pair
    /// from the connection settings. It looks for fields with the <c>"principal"</c> role
    /// (e.g. Username, AccountSid) and pairs them with fields with the <c>"credential"</c> role
    /// (e.g. Password, AuthToken), trying each combination until it finds a complete pair.
    /// </summary>
    public class BasicAuthenticationProvider : AuthenticationProviderBase
    {
        private readonly ILogger<BasicAuthenticationProvider> _logger;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="logger">An optional logger.</param>
        public BasicAuthenticationProvider(ILogger<BasicAuthenticationProvider>? logger = null)
            : base(AuthenticationScheme.Basic, "Basic Authentication")
        {
            _logger = logger ?? NullLogger<BasicAuthenticationProvider>.Instance;
        }

        /// <inheritdoc/>
        public override Task<AuthenticationResult> ObtainCredentialAsync(ConnectionSettings connectionSettings, AuthenticationConfiguration configuration, CancellationToken cancellationToken = default)
        {
            var principalFields = configuration.GetFieldsByRole("principal").ToList();
            var credentialFields = configuration.GetFieldsByRole("credential").ToList();

            foreach (var principal in principalFields)
            {
                var username = connectionSettings.GetParameter(principal.FieldName)?.ToString();
                if (string.IsNullOrWhiteSpace(username))
                    continue;

                foreach (var credential in credentialFields)
                {
                    var password = connectionSettings.GetParameter(credential.FieldName)?.ToString();
                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        _logger.LogFoundCredentials(AuthenticationScheme.Basic, principal.FieldName);

                        var authCredential = AuthenticationCredential.ForBasic(username, password);
                        authCredential.Properties["UserField"] = principal.FieldName;
                        authCredential.Properties["PassField"] = credential.FieldName;
                        return Task.FromResult(Success(authCredential));
                    }
                }
            }

            var principalNames = string.Join(", ", principalFields.Select(f => f.FieldName));
            var credentialNames = string.Join(", ", credentialFields.Select(f => f.FieldName));
            return Task.FromResult(Failure(
                $"Basic authentication credentials not found. Expected combinations of principal fields [{principalNames}] with credential fields [{credentialNames}].",
                "MISSING_BASIC_CREDENTIALS"));
        }
    }
}
