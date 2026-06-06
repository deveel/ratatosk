namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Feature", "RetryPolicy")]
public class RetryPolicySendTests
{
    private sealed class RetryTestConnector : ChannelConnectorBase
    {
        private readonly int _failUntilCall;
        private readonly string _retryableErrorCode;
        private bool _failOnNonRetryable;

        public int CallCount;

        public RetryTestConnector(
            IChannelSchema schema,
            ConnectionSettings? settings = null,
            int failUntilCall = 0,
            string retryableErrorCode = "RATE_LIMITED")
            : base(schema, settings ?? new ConnectionSettings())
        {
            _failUntilCall = failUntilCall;
            _retryableErrorCode = retryableErrorCode;
        }

        public RetryTestConnector WithNonRetryableFailure()
        {
            _failOnNonRetryable = true;
            return this;
        }

        protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
        {
            SetState(ConnectorState.Ready);
            return ValueTask.CompletedTask;
        }

        protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
        {
            CallCount++;

            if (_failOnNonRetryable)
                throw new ConnectorException("INVALID_CONFIG", "Test", "Non-retryable error");

            if (CallCount < _failUntilCall)
                throw new ConnectorException(_retryableErrorCode, "Test", "Transient error");

            return Task.FromResult(new SendResult(message.Id, "remote-id")
            {
                Status = MessageStatus.Delivered
            });
        }

        protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
            => Task.FromResult(new StatusInfo("OK"));
    }

    private static IChannelSchema CreateSchema()
        => new ChannelSchemaBuilder("TestProvider", "test", "1.0")
            .WithCapabilities(ChannelCapability.SendMessages)
            .HandlesMessageEndpoint(EndpointType.PhoneNumber, e => { e.CanSend = true; e.CanReceive = true; })
            .Build();

    private static Message CreateTestMessage()
        => new Message
        {
            Id = "msg-1",
            Sender = new Endpoint(EndpointType.PhoneNumber, "+1234"),
            Receiver = new Endpoint(EndpointType.PhoneNumber, "+5678")
        };

    [Fact]
    public async Task Should_SucceedWithoutRetry_When_NoPolicyConfigured()
    {
        var schema = CreateSchema();
        var connector = new RetryTestConnector(schema);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var result = await connector.SendMessageAsync(CreateTestMessage(), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess());
        Assert.Equal(1, connector.CallCount);
        Assert.Equal(1, result.Value.GetRetryAttempts());
    }

    [Fact]
    public async Task Should_SucceedOnFirstAttempt_When_NoFailure()
    {
        var settings = new ConnectionSettings()
            .SetParameter(RetrySettingsKeys.MaxAttempts, 3)
            .SetParameter(RetrySettingsKeys.RetryableErrorCodes, "RATE_LIMITED");
        var schema = CreateSchema();
        var connector = new RetryTestConnector(schema, settings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var result = await connector.SendMessageAsync(CreateTestMessage(), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess());
        Assert.Equal(1, connector.CallCount);
        Assert.Equal(1, result.Value.GetRetryAttempts());
    }

    [Fact]
    public async Task Should_SucceedOnSecondAttempt_When_FirstFailsWithRetryableError()
    {
        var settings = new ConnectionSettings()
            .SetParameter(RetrySettingsKeys.MaxAttempts, 3)
            .SetParameter(RetrySettingsKeys.RetryableErrorCodes, "RATE_LIMITED");
        var schema = CreateSchema();
        var connector = new RetryTestConnector(schema, settings, failUntilCall: 2);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var result = await connector.SendMessageAsync(CreateTestMessage(), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess());
        Assert.Equal(2, connector.CallCount);
        Assert.Equal(2, result.Value.GetRetryAttempts());
    }

    [Fact]
    public async Task Should_SucceedOnThirdAttempt_When_FirstTwoFailWithRetryableError()
    {
        var settings = new ConnectionSettings()
            .SetParameter(RetrySettingsKeys.MaxAttempts, 5)
            .SetParameter(RetrySettingsKeys.RetryableErrorCodes, "RATE_LIMITED");
        var schema = CreateSchema();
        var connector = new RetryTestConnector(schema, settings, failUntilCall: 3);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var result = await connector.SendMessageAsync(CreateTestMessage(), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess());
        Assert.Equal(3, connector.CallCount);
        Assert.Equal(3, result.Value.GetRetryAttempts());
    }

    [Fact]
    public async Task Should_Fail_When_RetriesExhausted()
    {
        var settings = new ConnectionSettings()
            .SetParameter(RetrySettingsKeys.MaxAttempts, 3)
            .SetParameter(RetrySettingsKeys.RetryableErrorCodes, "RATE_LIMITED");
        var schema = CreateSchema();
        var connector = new RetryTestConnector(schema, settings, failUntilCall: 10);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var result = await connector.SendMessageAsync(CreateTestMessage(), TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess());
        Assert.Equal(ConnectorErrorCodes.RetryAttemptsExhausted, result.Error?.Code);
        Assert.Equal(3, connector.CallCount);
    }

    [Fact]
    public async Task Should_NotRetry_When_NonRetryableErrorOccurs()
    {
        var settings = new ConnectionSettings()
            .SetParameter(RetrySettingsKeys.MaxAttempts, 3)
            .SetParameter(RetrySettingsKeys.RetryableErrorCodes, "RATE_LIMITED");
        var schema = CreateSchema();
        var connector = new RetryTestConnector(schema, settings)
            .WithNonRetryableFailure();
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var result = await connector.SendMessageAsync(CreateTestMessage(), TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess());
        Assert.Equal("INVALID_CONFIG", result.Error?.Code);
        Assert.Equal(1, connector.CallCount);
    }

    [Fact]
    public async Task Should_ReturnCircuitBreakerOpen_When_CircuitBreakerTrips()
    {
        var schema = CreateSchema();
        var connector = new CircuitBreakerTestConnector(schema);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var result = await connector.SendMessageAsync(CreateTestMessage(), TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess());
        Assert.Equal(ConnectorErrorCodes.CircuitBreakerOpen, result.Error?.Code);
    }

    private sealed class CircuitBreakerTestConnector : ChannelConnectorBase
    {
        public int CallCount;

        public CircuitBreakerTestConnector(IChannelSchema schema, ConnectionSettings? settings = null)
            : base(schema, settings ?? new ConnectionSettings())
        {
        }

        protected override RetryPolicyOptions? GetDefaultRetryPolicy()
            => new RetryPolicyOptions
            {
                MaxRetryAttempts = 3,
                RetryableErrorCodes = { "RATE_LIMITED" },
                EnableCircuitBreaker = true,
                CircuitBreakerFailureRatio = 0.5,
                CircuitBreakerSamplingDuration = TimeSpan.FromSeconds(5),
                CircuitBreakerMinimumThroughput = 2,
                CircuitBreakerBreakDuration = TimeSpan.FromSeconds(10)
            };

        protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
        {
            SetState(ConnectorState.Ready);
            return ValueTask.CompletedTask;
        }

        protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
        {
            CallCount++;
            throw new ConnectorException("RATE_LIMITED", "Test", "Transient error");
        }

        protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
            => Task.FromResult(new StatusInfo("OK"));
    }

    [Fact]
    public async Task Should_NotRetry_When_MaxRetryAttemptsIsOne()
    {
        var settings = new ConnectionSettings()
            .SetParameter(RetrySettingsKeys.MaxAttempts, 1)
            .SetParameter(RetrySettingsKeys.RetryableErrorCodes, "RATE_LIMITED");
        var schema = CreateSchema();
        var connector = new RetryTestConnector(schema, settings, failUntilCall: 10);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var result = await connector.SendMessageAsync(CreateTestMessage(), TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess());
        Assert.Equal(1, connector.CallCount);
    }

    [Fact]
    public async Task Should_UseGetDefaultRetryPolicy_When_OverrideExists()
    {
        var schema = CreateSchema();
        var connector = new CustomRetryTestConnector(schema);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var result = await connector.SendMessageAsync(CreateTestMessage(), TestContext.Current.CancellationToken);

        // Connector fails 3 times, default retry is 5, so should succeed
        Assert.True(result.IsSuccess());
        Assert.Equal(4, connector.CallCount);
    }

    private sealed class CustomRetryTestConnector : ChannelConnectorBase
    {
        public int CallCount;

        public CustomRetryTestConnector(IChannelSchema schema, ConnectionSettings? settings = null)
            : base(schema, settings ?? new ConnectionSettings())
        {
        }

        protected override RetryPolicyOptions? GetDefaultRetryPolicy()
            => new RetryPolicyOptions
            {
                MaxRetryAttempts = 5,
                RetryableErrorCodes = { "RATE_LIMITED" }
            };

        protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
        {
            SetState(ConnectorState.Ready);
            return ValueTask.CompletedTask;
        }

        protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
        {
            CallCount++;
            if (CallCount < 4)
                throw new ConnectorException("RATE_LIMITED", "Test", "Transient error");

            return Task.FromResult(new SendResult(message.Id, "remote-id")
            {
                Status = MessageStatus.Delivered
            });
        }

        protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
            => Task.FromResult(new StatusInfo("OK"));
    }
}
