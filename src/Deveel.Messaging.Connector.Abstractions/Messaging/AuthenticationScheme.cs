//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
    /// <summary>
    /// Identifies an authentication mechanism used by a channel connector.
    /// </summary>
    /// <remarks>
    /// This is an extensible record type that replaces the old <c>AuthenticationType</c> enum.
    /// Pre-defined instances are provided for common schemes; custom schemes can be created
    /// by calling <c>AuthenticationScheme.Of("my-scheme")</c>.
    /// </remarks>
    public record AuthenticationScheme(string Name)
    {
        /// <summary>No authentication is required.</summary>
        public static AuthenticationScheme None => new("none");

        /// <summary>Authentication using an API key (typically via HTTP header or query parameter).</summary>
        public static AuthenticationScheme ApiKey => new("api-key");

        /// <summary>Bearer token authentication (RFC 6750).</summary>
        public static AuthenticationScheme Bearer => new("bearer");

        /// <summary>HTTP Basic authentication (RFC 7617), using a username and password pair.</summary>
        public static AuthenticationScheme Basic => new("basic");

        /// <summary>OAuth 2.0 Client Credentials flow (RFC 6749).</summary>
        public static AuthenticationScheme OAuthClientCredentials => new("oauth-client-credentials");

        /// <summary>Certificate-based authentication (e.g. TLS client certificates or service account keys).</summary>
        public static AuthenticationScheme Certificate => new("certificate");

        /// <summary>HTTP Digest authentication (RFC 7616).</summary>
        public static AuthenticationScheme Digest => new("digest");

        /// <summary>A fallback for provider-specific or custom authentication mechanisms.</summary>
        public static AuthenticationScheme Custom => new("custom");

        /// <summary>
        /// Creates a custom authentication scheme with the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The unique name of the scheme.</param>
        /// <returns>A new <see cref="AuthenticationScheme"/> instance.</returns>
        public static AuthenticationScheme Of(string name) => new(name);

        /// <inheritdoc/>
        public override string ToString() => Name;

        /// <summary>
        /// Implicitly converts an <see cref="AuthenticationScheme"/> to its string name.
        /// </summary>
        /// <param name="scheme">The scheme to convert.</param>
        public static implicit operator string(AuthenticationScheme scheme) => scheme.Name;
    }
}
