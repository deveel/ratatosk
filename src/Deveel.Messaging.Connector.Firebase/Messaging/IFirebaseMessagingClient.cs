namespace Deveel.Messaging
{
    /// <summary>
    /// Provides an abstraction over the Firebase Admin SDK for sending messages.
    /// </summary>
    public interface IFirebaseMessagingClient
    {
        /// <summary>
        /// Sends a single Firebase Cloud Message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="dryRun">If <c>true</c>, the message is validated but not actually sent.</param>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>The message ID assigned by Firebase.</returns>
        Task<string> SendAsync(FirebaseAdmin.Messaging.Message message, bool dryRun, CancellationToken cancellationToken);

        /// <summary>
        /// Sends multiple messages individually via Firebase Cloud Messaging.
        /// </summary>
        /// <param name="messages">The messages to send.</param>
        /// <param name="dryRun">If <c>true</c>, messages are validated but not actually sent.</param>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>A <see cref="BatchResponse"/> with per-message results.</returns>
        Task<FirebaseAdmin.Messaging.BatchResponse> SendEachAsync(IEnumerable<FirebaseAdmin.Messaging.Message> messages, bool dryRun, CancellationToken cancellationToken);

        /// <summary>
        /// Sends a multicast message to multiple recipients via Firebase Cloud Messaging.
        /// </summary>
        /// <param name="message">The multicast message to send.</param>
        /// <param name="dryRun">If <c>true</c>, the message is validated but not actually sent.</param>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>A <see cref="BatchResponse"/> with per-recipient results.</returns>
        Task<FirebaseAdmin.Messaging.BatchResponse> SendMulticastAsync(FirebaseAdmin.Messaging.MulticastMessage message, bool dryRun, CancellationToken cancellationToken);
    }
}
