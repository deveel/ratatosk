//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Deveel.Messaging
{
    /// <summary>
    /// An authentication provider for Firebase that handles Google Service Account authentication
    /// by passing through the service account key for Firebase SDK initialization.
    /// </summary>
    /// <remarks>
    /// This provider handles service account authentication by extracting the service account
    /// JSON from connection settings and creating a credential that can be used by the Firebase SDK.
    /// It doesn't perform token exchange itself but validates and prepares the service account
    /// information for the Firebase SDK to use.
    /// </remarks>
    public class FirebaseServiceAccountAuthenticationProvider : AuthenticationProviderBase
    {
        private readonly ILogger<FirebaseServiceAccountAuthenticationProvider> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirebaseServiceAccountAuthenticationProvider"/> class.
        /// </summary>
        /// <param name="logger">Optional logger for diagnostic purposes.</param>
        public FirebaseServiceAccountAuthenticationProvider(ILogger<FirebaseServiceAccountAuthenticationProvider>? logger = null)
            : base(AuthenticationScheme.Certificate, "Firebase Service Account Authentication")
        {
            _logger = logger ?? NullLogger<FirebaseServiceAccountAuthenticationProvider>.Instance;
        }

        /// <inheritdoc/>
        public override bool CanHandle(AuthenticationConfiguration configuration)
        {
            return configuration.Scheme == AuthenticationScheme.Certificate ||
                   (configuration.Scheme == AuthenticationScheme.Custom &&
                    configuration.GetAllFieldNames().Any(f => f.Contains("ServiceAccount", StringComparison.OrdinalIgnoreCase)));
        }

        /// <inheritdoc/>
        public override Task<AuthenticationResult> ObtainCredentialAsync(ConnectionSettings connectionSettings, AuthenticationConfiguration configuration, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogObtainingServiceAccountCredential();

                var serviceAccountKey = GetStringParameter(connectionSettings, "ServiceAccountKey") ??
                                      GetStringParameter(connectionSettings, "ServiceAccountJson") ??
                                      GetStringParameter(connectionSettings, "Certificate");

                if (string.IsNullOrWhiteSpace(serviceAccountKey))
                {
                    return Task.FromResult(Failure("Service account key is required. Provide ServiceAccountKey, ServiceAccountJson, or Certificate parameter.", "MISSING_SERVICE_ACCOUNT_KEY"));
                }

                if (serviceAccountKey.EndsWith(".json", StringComparison.OrdinalIgnoreCase) && 
                    !serviceAccountKey.Contains("{"))
                {
                    if (!File.Exists(serviceAccountKey))
                    {
                        return Task.FromResult(Failure($"Service account key file not found: {serviceAccountKey}", "SERVICE_ACCOUNT_FILE_NOT_FOUND"));
                    }
                    
                    _logger.LogUsingServiceAccountKeyFile();
                }
                else
                {
                    try
                    {
                        System.Text.Json.JsonDocument.Parse(serviceAccountKey);
                        _logger.LogUsingServiceAccountKeyJson();
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        return Task.FromResult(Failure("Service account key is not valid JSON", "INVALID_SERVICE_ACCOUNT_JSON"));
                    }
                }

                var credential = new AuthenticationCredential(AuthenticationScheme.Certificate, serviceAccountKey);
                credential.Properties["CredentialType"] = "ServiceAccount";
                credential.Properties["Provider"] = "Firebase";

                var projectId = GetStringParameter(connectionSettings, "ProjectId");
                if (!string.IsNullOrWhiteSpace(projectId))
                {
                    credential.Properties["ProjectId"] = projectId;
                }

                _logger.LogFirebaseServiceAccountCredentialPrepared();
                return Task.FromResult(Success(credential));
            }
            catch (Exception ex)
            {
                _logger.LogFirebaseServiceAccountCredentialError(ex);
                return Task.FromResult(Failure($"Error preparing credential: {ex.Message}", "CREDENTIAL_ERROR"));
            }
        }

        /// <inheritdoc/>
        public override Task<AuthenticationResult> RefreshCredentialAsync(AuthenticationCredential existingCredential, ConnectionSettings connectionSettings, AuthenticationConfiguration configuration, CancellationToken cancellationToken = default)
        {
            if (existingCredential.Scheme == AuthenticationScheme.Certificate &&
                !string.IsNullOrWhiteSpace(existingCredential.Value))
            {
                _logger.LogServiceAccountCredentialValid();
                return Task.FromResult(Success(existingCredential));
            }

            _logger.LogServiceAccountCredentialInvalid();
            return ObtainCredentialAsync(connectionSettings, configuration, cancellationToken);
        }
    }
}