//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using FirebaseAdmin;
using FirebaseAdmin.Messaging;

using Google.Apis.Auth.OAuth2;

namespace Deveel.Messaging
{
	/// <summary>
	/// Default implementation of <see cref="IFirebaseService"/> using the Firebase Admin SDK.
	/// </summary>
	public class FirebaseService : IFirebaseService
    {
        private FirebaseApp? _app;
        private FirebaseMessaging? _messaging;

        /// <inheritdoc/>
        public FirebaseApp? App => _app;

        /// <inheritdoc/>
        public bool IsInitialized => _app != null && _messaging != null;

        /// <inheritdoc/>
        public async Task InitializeAsync(string serviceAccountKey, string projectId)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(serviceAccountKey, nameof(serviceAccountKey));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(projectId, nameof(projectId));

            try
            {
                // Clean up existing app if already initialized
                if (_app != null)
                {
                    _app.Delete();
                    _app = null;
                    _messaging = null;
                }

                // Create credential from service account key
                var credential = GoogleCredential.FromJson(serviceAccountKey);

                // Initialize Firebase app
                _app = FirebaseApp.Create(new AppOptions
                {
                    Credential = credential,
                    ProjectId = projectId
                });

                // Initialize messaging service
                _messaging = FirebaseMessaging.GetMessaging(_app);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize Firebase service: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<string> SendAsync(FirebaseAdmin.Messaging.Message message, bool dryRun = false, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();
            ArgumentNullException.ThrowIfNull(message, nameof(message));

            try
            {
                return await _messaging!.SendAsync(message, dryRun, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send Firebase message: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<BatchResponse> SendEachAsync(IEnumerable<FirebaseAdmin.Messaging.Message> messages, bool dryRun = false, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();
            ArgumentNullException.ThrowIfNull(messages, nameof(messages));

            try
            {
                return await _messaging!.SendEachAsync(messages, dryRun, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send Firebase messages: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<BatchResponse> SendMulticastAsync(MulticastMessage message, bool dryRun = false, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();
            ArgumentNullException.ThrowIfNull(message, nameof(message));

            try
            {
                return await _messaging!.SendMulticastAsync(message, dryRun, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send Firebase multicast message: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (!IsInitialized)
                return false;

            try
            {
                // Create a simple test message to validate the connection
                // We'll use dry-run mode so no actual message is sent
                var testMessage = new FirebaseAdmin.Messaging.Message
                {
                    Token = "test-token-for-connection-validation",
                    Notification = new Notification
                    {
                        Title = "Test",
                        Body = "Connection test"
                    }
                };

                // This will validate credentials and connectivity without sending
                await _messaging!.SendAsync(testMessage, dryRun: true, cancellationToken);
                return true;
            }
            catch (FirebaseMessagingException ex) when (ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument)
            {
                // Invalid token is expected in connection test, but it means connection works
                return true;
            }
            catch
            {
                // Any other exception means connection failed
                return false;
            }
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Firebase service is not initialized. Call InitializeAsync first.");
            }
        }
    }
}