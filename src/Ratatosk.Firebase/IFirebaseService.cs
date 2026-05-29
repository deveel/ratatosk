//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace Ratatosk
{
    /// <summary>
    /// Defines the contract for interacting with Firebase Cloud Messaging services.
    /// </summary>
    /// <remarks>
    /// This interface abstracts the Firebase Admin SDK operations to enable
    /// testability and provide a consistent API for Firebase operations.
    /// </remarks>
    public interface IFirebaseService
    {
        /// <summary>
        /// Initializes the Firebase application with the provided service account credentials.
        /// </summary>
        /// <param name="serviceAccountKey">The service account key JSON string.</param>
        /// <param name="projectId">The Firebase project ID.</param>
        /// <returns>A task representing the initialization operation.</returns>
        Task InitializeAsync(string serviceAccountKey, string projectId);

        /// <summary>
        /// Sends a single push notification message.
        /// </summary>
        /// <param name="message">The Firebase message to send.</param>
        /// <param name="dryRun">Whether to send in dry-run mode for testing.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task containing the message ID if successful.</returns>
        Task<string> SendAsync(FirebaseAdmin.Messaging.Message message, bool dryRun = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends multiple push notification messages to different tokens.
        /// </summary>
        /// <param name="messages">The collection of Firebase messages to send.</param>
        /// <param name="dryRun">Whether to send in dry-run mode for testing.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task containing the batch response with individual results.</returns>
        Task<BatchResponse> SendEachAsync(IEnumerable<FirebaseAdmin.Messaging.Message> messages, bool dryRun = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a single message to multiple device tokens.
        /// </summary>
        /// <param name="message">The multicast message to send.</param>
        /// <param name="dryRun">Whether to send in dry-run mode for testing.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task containing the batch response with individual results.</returns>
        Task<BatchResponse> SendMulticastAsync(MulticastMessage message, bool dryRun = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Tests the connection to Firebase services.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the connection test operation.</returns>
        Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the Firebase application instance.
        /// </summary>
        FirebaseApp? App { get; }

        /// <summary>
        /// Gets a value indicating whether the service is initialized.
        /// </summary>
        bool IsInitialized { get; }
    }
}