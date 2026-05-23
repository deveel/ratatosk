using System.Diagnostics.CodeAnalysis;

namespace Deveel.Messaging
{
    [ExcludeFromCodeCoverage]
    public class FirebaseMessagingClient : IFirebaseMessagingClient
    {
        private readonly FirebaseAdmin.Messaging.FirebaseMessaging _messaging;

        public FirebaseMessagingClient(FirebaseAdmin.Messaging.FirebaseMessaging messaging)
        {
            _messaging = messaging ?? throw new ArgumentNullException(nameof(messaging));
        }

        public Task<string> SendAsync(FirebaseAdmin.Messaging.Message message, bool dryRun, CancellationToken cancellationToken)
            => _messaging.SendAsync(message, dryRun, cancellationToken);

        public Task<FirebaseAdmin.Messaging.BatchResponse> SendEachAsync(IEnumerable<FirebaseAdmin.Messaging.Message> messages, bool dryRun, CancellationToken cancellationToken)
            => _messaging.SendEachAsync(messages, dryRun, cancellationToken);

        public Task<FirebaseAdmin.Messaging.BatchResponse> SendMulticastAsync(FirebaseAdmin.Messaging.MulticastMessage message, bool dryRun, CancellationToken cancellationToken)
            => _messaging.SendMulticastAsync(message, dryRun, cancellationToken);
    }
}
