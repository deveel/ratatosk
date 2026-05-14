using System.ComponentModel.DataAnnotations;

namespace Deveel.Messaging;

/// <summary>
/// Integration tests demonstrating how to use the <see cref="ChannelConnectorBase"/>
/// abstract class to create concrete connector implementations.
/// </summary>
public class ChannelConnectorUsageExamples
{
	[Fact]
	public async Task EmailConnector_Example_CanSendMessage()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("SMTP", "Email", "1.0.0")
			.WithDisplayName("Email Connector")
			.WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.HealthCheck)
			.AddRequiredParameter("Host", DataType.String)
			.AddParameter("Port", DataType.Integer, param => param.DefaultValue = 587)
			.AddContentType(MessageContentType.PlainText)
			.AddContentType(MessageContentType.Html)
			.AddAuthenticationScheme(AuthenticationScheme.Basic)
			.Build();

		var connector = new ExampleEmailConnector(schema);

		// Act
		await connector.InitializeAsync(TestContext.Current.CancellationToken);
		var message = new Message
		{
			Id = Guid.NewGuid().ToString(),
			Receiver = new Endpoint(EndpointType.EmailAddress, "test@example.com"),
			Content = new TextContent("Hello World")
		};
		var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess());
		Assert.NotNull(result.Value);
		Assert.Equal(message.Id, result.Value.MessageId);
		Assert.StartsWith("email-", result.Value.RemoteMessageId);
	}

	[Fact]
	public async Task SmsConnector_Example_SupportsStatusQueries()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Twilio", "SMS", "2.0.0")
			.WithDisplayName("SMS Connector")
			.WithCapabilities(
				ChannelCapability.SendMessages |
				ChannelCapability.MessageStatusQuery |
				ChannelCapability.HealthCheck)
			.AddRequiredParameter("AccountSid", DataType.String)
			.AddRequiredParameter("AuthToken", DataType.String, true)
			.AddContentType(MessageContentType.PlainText)
			.AddAuthenticationScheme(AuthenticationScheme.Bearer)
			.Build();

		var connector = new ExampleSmsConnector(schema);

		// Act
		await connector.InitializeAsync(TestContext.Current.CancellationToken);
		var message = new Message
		{
			Id = Guid.NewGuid().ToString(),
			Receiver = new Endpoint(EndpointType.PhoneNumber, "+1234567890"),
			Content = new TextContent("Hello SMS")
		};
		var sendResult = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);
		var statusResult = await connector.GetMessageStatusAsync(sendResult.Value!.MessageId, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(sendResult.IsSuccess());
		Assert.True(statusResult.IsSuccess());
		Assert.Equal(message.Id, statusResult.Value!.MessageId);
		Assert.Single(statusResult.Value.Updates);
	}

	[Fact]
	public async Task ConnectorWithHealthCheck_Example_ReturnsHealthStatus()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Custom", "Health", "1.0.0")
			.WithCapabilities(ChannelCapability.HealthCheck)
			.Build();

		var connector = new ExampleHealthConnector(schema);

		// Act
		await connector.InitializeAsync(TestContext.Current.CancellationToken);
		var healthResult = await connector.GetHealthAsync(TestContext.Current.CancellationToken);

		// Assert
		Assert.True(healthResult.IsSuccess());
		Assert.NotNull(healthResult.Value);
		Assert.True(healthResult.Value.IsHealthy);
		Assert.Equal(ConnectorState.Ready, healthResult.Value.State);
		Assert.Contains("connections", healthResult.Value.Metrics.Keys);
	}

	// Example Email Connector Implementation
	private class ExampleEmailConnector : ChannelConnectorBase
	{
		public ExampleEmailConnector(IChannelSchema schema) : base(schema) { }

		protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
		{
			// Simulate email server configuration
            return ValueTask.CompletedTask;
		}

		protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
		{
			// Simulate connection test to email server
            return ValueTask.CompletedTask;
		}

		protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
		{
			// Simulate sending email
			var result = new SendResult(message.Id, $"email-{Guid.NewGuid()}");
			result.Status = MessageStatus.Sent;
			return Task.FromResult(result);
		}

		protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
		{
			var status = new StatusInfo("Connected");
			return Task.FromResult(status);
		}
	}

	// Example SMS Connector Implementation with Status Query Support
	private class ExampleSmsConnector : ChannelConnectorBase
	{
		public ExampleSmsConnector(IChannelSchema schema) : base(schema) { }

		protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
		{
            return ValueTask.CompletedTask;
		}

		protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
		}

		protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
		{
			var result = new SendResult(message.Id, $"sms-{Guid.NewGuid()}");
			result.Status = MessageStatus.Queued;
			return Task.FromResult(result);
		}

		protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
		{
			var status = new StatusInfo("Active");
			return Task.FromResult(status);
		}

		// Override to provide status query capability
		protected override Task<StatusUpdatesResult> GetMessageStatusCoreAsync(string messageId, CancellationToken cancellationToken)
		{
			var statusUpdate = new StatusUpdateResult(messageId, MessageStatus.Delivered);
			var result = new StatusUpdatesResult(messageId, new[] { statusUpdate });
			return Task.FromResult(result);
		}
	}

	// Example Health-focused Connector Implementation
	private class ExampleHealthConnector : ChannelConnectorBase
	{
		public ExampleHealthConnector(IChannelSchema schema) : base(schema) { }

		protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
		{
            return ValueTask.CompletedTask;
		}

		protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
		}

		protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
		{
			// This connector doesn't support sending
			throw new NotSupportedException("This connector is for health monitoring only");
		}

		protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
		{
			var status = new StatusInfo("Monitoring");
			return Task.FromResult(status);
		}

		// Override to provide custom health information
		protected override Task<ConnectorHealth> GetConnectorHealthAsync(CancellationToken cancellationToken)
		{
			var health = new ConnectorHealth
			{
				State = State,
				IsHealthy = State == ConnectorState.Ready,
				LastHealthCheck = DateTime.UtcNow,
				Uptime = TimeSpan.FromHours(1), // Simulate 1 hour uptime
			};

			// Add custom metrics
			health.Metrics["connections"] = 5;
			health.Metrics["memory_usage"] = "120MB";
			health.Metrics["cpu_usage"] = "15%";

			return Task.FromResult(health);
		}
	}
}
