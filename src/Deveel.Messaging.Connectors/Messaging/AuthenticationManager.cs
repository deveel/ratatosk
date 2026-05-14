//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;

namespace Deveel.Messaging
{
    /// <summary>
    /// Default implementation of <see cref="IAuthenticationManager"/> that registers the
    /// built-in providers (<see cref="ApiKeyAuthenticationProvider"/>, <see cref="BearerTokenAuthenticationProvider"/>,
    /// <see cref="BasicAuthenticationProvider"/>, <see cref="ClientCredentialsAuthenticationProvider"/>)
    /// and provides in-memory credential caching.
    /// </summary>
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly List<IAuthenticationProvider> _providers;
        private readonly ILogger<AuthenticationManager> _logger;
        private readonly Dictionary<string, AuthenticationCredential> _credentialCache;
        private readonly object _cacheLock = new object();

        /// <summary>
        /// Initializes a new instance. If no <paramref name="providers"/> are supplied,
        /// the four built-in providers are registered automatically.
        /// </summary>
        /// <param name="providers">Optional initial set of providers.</param>
        /// <param name="logger">An optional logger.</param>
        public AuthenticationManager(IEnumerable<IAuthenticationProvider>? providers = null, ILogger<AuthenticationManager>? logger = null)
        {
            _providers = new List<IAuthenticationProvider>(providers ?? Enumerable.Empty<IAuthenticationProvider>());
            _logger = logger ?? NullLogger<AuthenticationManager>.Instance;
            _credentialCache = new Dictionary<string, AuthenticationCredential>();

            if (_providers.Count == 0)
            {
                RegisterDefaultProviders();
            }
        }

        /// <inheritdoc/>
        public void RegisterProvider(IAuthenticationProvider provider)
        {
            ArgumentNullException.ThrowIfNull(provider, nameof(provider));

            _providers.Add(provider);
            _logger.LogAuthenticationProviderRegistered(provider.DisplayName, provider.Scheme);
        }

        /// <inheritdoc/>
        public async Task<AuthenticationResult> AuthenticateAsync(ConnectionSettings connectionSettings, AuthenticationConfiguration configuration, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(connectionSettings, nameof(connectionSettings));
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            try
            {
                _logger.LogUsingAuthenticationConfiguration(configuration.Scheme);

                var provider = FindProvider(configuration);
                if (provider == null)
                {
                    _logger.LogAuthenticationProviderNotFound(configuration.Scheme);
                    return AuthenticationResult.Failure($"No authentication provider available for scheme '{configuration.Scheme}'", "NO_PROVIDER");
                }

                var cacheKey = CreateCacheKey(connectionSettings, configuration);
                var cachedCredential = GetCachedCredential(cacheKey);

                if (cachedCredential != null && !ShouldRefreshCredential(cachedCredential))
                {
                    _logger.LogUsingCachedCredential(configuration.Scheme);
                    return AuthenticationResult.Success(cachedCredential);
                }

                AuthenticationResult result;
                if (cachedCredential != null && ShouldRefreshCredential(cachedCredential))
                {
                    _logger.LogRefreshingAuthenticationCredential();
                    result = await provider.RefreshCredentialAsync(cachedCredential, connectionSettings, configuration, cancellationToken);
                }
                else
                {
                    _logger.LogObtainingNewCredential(configuration.Scheme);
                    result = await provider.ObtainCredentialAsync(connectionSettings, configuration, cancellationToken);
                }

                if (result.IsSuccessful && result.Credential != null)
                {
                    CacheCredential(cacheKey, result.Credential);
                    _logger.LogAuthenticationSuccessful(configuration.Scheme);
                }
                else
                {
                    _logger.LogAuthenticationFailedWithMessage(configuration.Scheme, result.ErrorMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogAuthenticationException(ex);
                return AuthenticationResult.Failure($"Authentication error: {ex.Message}", "AUTHENTICATION_ERROR");
            }
        }

        /// <inheritdoc/>
        public void ClearCache()
        {
            lock (_cacheLock)
            {
                _credentialCache.Clear();
                _logger.LogCacheCleared();
            }
        }

        /// <inheritdoc/>
        public void InvalidateCredential(ConnectionSettings connectionSettings, AuthenticationConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(connectionSettings, nameof(connectionSettings));
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            var cacheKey = CreateCacheKey(connectionSettings, configuration);

            lock (_cacheLock)
            {
                if (_credentialCache.Remove(cacheKey))
                {
                    _logger.LogCredentialInvalidated(configuration.Scheme);
                }
            }
        }

        private void RegisterDefaultProviders()
        {
            RegisterProvider(new ApiKeyAuthenticationProvider());
            RegisterProvider(new BearerTokenAuthenticationProvider());
            RegisterProvider(new BasicAuthenticationProvider());
            RegisterProvider(new ClientCredentialsAuthenticationProvider());

            _logger.LogDefaultProvidersRegistered();
        }

        private IAuthenticationProvider? FindProvider(AuthenticationConfiguration configuration)
        {
            return _providers.FirstOrDefault(p => p.CanHandle(configuration));
        }

        private string CreateCacheKey(ConnectionSettings connectionSettings, AuthenticationConfiguration configuration)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append(configuration.Scheme.Name);

            var relevantFields = configuration.GetAllFieldNames();
            foreach (var field in relevantFields.OrderBy(f => f))
            {
                var value = connectionSettings.GetParameter(field);
                if (value != null)
                {
                    keyBuilder.Append($"|{field}={value.GetHashCode()}");
                }
            }

            return keyBuilder.ToString();
        }

        private AuthenticationCredential? GetCachedCredential(string cacheKey)
        {
            lock (_cacheLock)
            {
                if (_credentialCache.TryGetValue(cacheKey, out var credential))
                {
                    if (!credential.IsExpired)
                    {
                        return credential;
                    }

                    _credentialCache.Remove(cacheKey);
                }
            }

            return null;
        }

        private void CacheCredential(string cacheKey, AuthenticationCredential credential)
        {
            lock (_cacheLock)
            {
                _credentialCache[cacheKey] = credential;
            }
        }

        private bool ShouldRefreshCredential(AuthenticationCredential credential)
        {
            return credential.IsExpired || credential.WillExpireSoon(TimeSpan.FromMinutes(5));
        }
    }
}
