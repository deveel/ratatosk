//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
    /// <summary>
    /// Represents the outcome of an authentication operation performed by an
    /// <see cref="IAuthenticationProvider"/>.
    /// </summary>
    public class AuthenticationResult
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="isSuccessful">Whether authentication succeeded.</param>
        /// <param name="credential">The obtained credential (on success).</param>
        /// <param name="errorMessage">An error message (on failure).</param>
        /// <param name="errorCode">An optional error code (on failure).</param>
        public AuthenticationResult(bool isSuccessful, AuthenticationCredential? credential = null, string? errorMessage = null, string? errorCode = null)
        {
            IsSuccessful = isSuccessful;
            Credential = credential;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
            Timestamp = DateTime.UtcNow;
            AdditionalData = new Dictionary<string, object?>();
        }

        /// <summary>
        /// Gets whether the authentication operation succeeded.
        /// </summary>
        public bool IsSuccessful { get; }

        /// <summary>
        /// Gets the credential obtained, if successful.
        /// </summary>
        public AuthenticationCredential? Credential { get; }

        /// <summary>
        /// Gets the error message, if the operation failed.
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>
        /// Gets the error code, if the operation failed.
        /// </summary>
        public string? ErrorCode { get; }

        /// <summary>
        /// Gets the UTC timestamp when this result was created.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Gets additional data attached to this result.
        /// </summary>
        public Dictionary<string, object?> AdditionalData { get; }

        /// <summary>
        /// Creates a successful result with the given <paramref name="credential"/>.
        /// </summary>
        /// <param name="credential">The obtained credential.</param>
        /// <returns>A successful result.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="credential"/> is <c>null</c>.</exception>
        public static AuthenticationResult Success(AuthenticationCredential credential)
        {
            ArgumentNullException.ThrowIfNull(credential, nameof(credential));
            return new AuthenticationResult(true, credential);
        }

        /// <summary>
        /// Creates a failed result with the given <paramref name="errorMessage"/>.
        /// </summary>
        /// <param name="errorMessage">A description of the failure.</param>
        /// <param name="errorCode">An optional error code.</param>
        /// <returns>A failed result.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="errorMessage"/> is <c>null</c> or empty.</exception>
        public static AuthenticationResult Failure(string errorMessage, string? errorCode = null)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(errorMessage, nameof(errorMessage));
            return new AuthenticationResult(false, null, errorMessage, errorCode);
        }
    }
}
