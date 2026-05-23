namespace Deveel.Messaging
{
    public interface IFirebaseMessagingClient
    {
        Task<string> SendAsync(FirebaseAdmin.Messaging.Message message, bool dryRun, CancellationToken cancellationToken);

        Task<FirebaseAdmin.Messaging.BatchResponse> SendEachAsync(IEnumerable<FirebaseAdmin.Messaging.Message> messages, bool dryRun, CancellationToken cancellationToken);

        Task<FirebaseAdmin.Messaging.BatchResponse> SendMulticastAsync(FirebaseAdmin.Messaging.MulticastMessage message, bool dryRun, CancellationToken cancellationToken);
    }
}
