namespace Ratatosk.Testing
{
    /// <summary>
    /// A fake connector that simulates timeout scenarios for unit testing.
    /// </summary>
    /// <remarks>
    /// This connector is designed for testing timeout handling in the framework.
    /// It can be configured to timeout on specific operations (send, receive, status query).
    /// </remarks>
    public class FakeTimeoutConnector : ChannelConnectorBase
    {
        private readonly TimeSpan _delay;
        private readonly bool _timeoutOnSend;
        private readonly bool _timeoutOnReceive;
        private readonly bool _timeoutOnStatusQuery;

        /// <summary>
        /// Creates a new instance of the fake timeout connector.
        /// </summary>
        /// <param name="schema">The channel schema for the connector.</param>
        /// <param name="connectionSettings">The connection settings.</param>
        /// <param name="delay">
        /// The delay to simulate before timing out. Default is 10 seconds.
        /// Set to a value longer than your timeout to ensure timeout occurs.
        /// </param>
        /// <param name="timeoutOnSend">If true, send operations will timeout.</param>
        /// <param name="timeoutOnReceive">If true, receive operations will timeout.</param>
        /// <param name="timeoutOnStatusQuery">If true, status query operations will timeout.</param>
        public FakeTimeoutConnector(
            IChannelSchema schema,
            ConnectionSettings? connectionSettings = null,
            TimeSpan? delay = null,
            bool timeoutOnSend = true,
            bool timeoutOnReceive = false,
            bool timeoutOnStatusQuery = false)
            : base(schema, connectionSettings)
        {
            _delay = delay ?? TimeSpan.FromSeconds(10);
            _timeoutOnSend = timeoutOnSend;
            _timeoutOnReceive = timeoutOnReceive;
            _timeoutOnStatusQuery = timeoutOnStatusQuery;
        }

        /// <summary>
        /// Gets a value indicating whether this connector should timeout on send operations.
        /// </summary>
        public bool TimeoutOnSend => _timeoutOnSend;

        /// <summary>
        /// Gets a value indicating whether this connector should timeout on receive operations.
        /// </summary>
        public bool TimeoutOnReceive => _timeoutOnReceive;

        /// <summary>
        /// Gets a value indicating whether this connector should timeout on status query operations.
        /// </summary>
        public bool TimeoutOnStatusQuery => _timeoutOnStatusQuery;

        /// <inheritdoc/>
        protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
        {
            return SimulateTimeoutAsync(
                _timeoutOnSend,
                () => new SendResult(message.Id ?? "unknown", $"fake-{message.Id}"),
                cancellationToken);
        }

        /// <inheritdoc/>
        protected override Task<ReceiveResult> ReceiveMessagesCoreAsync(MessageSource source, CancellationToken cancellationToken)
        {
            return SimulateTimeoutAsync(
                _timeoutOnReceive,
                () => new ReceiveResult("fake-batch", Array.Empty<IMessage>()),
                cancellationToken);
        }

        /// <inheritdoc/>
        protected override Task<StatusUpdatesResult> GetMessageStatusCoreAsync(string messageId, CancellationToken cancellationToken)
        {
            return SimulateTimeoutAsync(
                _timeoutOnStatusQuery,
                () => new StatusUpdatesResult(messageId),
                cancellationToken);
        }

        /// <inheritdoc/>
        protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new StatusInfo("OK", "Fake connector is operational"));
        }

        private async Task<T> SimulateTimeoutAsync<T>(bool shouldTimeout, Func<T> resultFactory, CancellationToken cancellationToken)
        {
            if (shouldTimeout)
            {
                try
                {
                    await Task.Delay(_delay, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected - timeout occurred
                    throw;
                }
            }

            return resultFactory();
        }
    }
}
