using System.ComponentModel.DataAnnotations;

namespace Deveel.Messaging;

/// <summary>
/// Tests for the <see cref="ChannelConnectorBase"/> abstract class to verify
/// its state management, capability validation, and default implementations.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
[Trait("Feature", "ChannelConnectorBase")]
public class ChannelConnectorBaseTests
{
	[Fact]
	public void Should_SetSchemaCorrectly_When_ConstructorWithValidSchema()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0");

		// Act
		var connector = new TestConnector(schema);

		// Assert
		Assert.Same(schema, connector.Schema);
		Assert.Equal(ConnectorState.Uninitialized, connector.State);
	}

	[Fact]
	public void Should_ThrowArgumentNullException_When_ConstructorWithNullSchema()
	{
		// Act
		// Assert
		Assert.Throws<ArgumentNullException>(() => new TestConnector(null!));
	}

	[Fact]
	public async Task Should_TransitionToInitializingThenReady_When_InitializeAsyncWhenUninitialized()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0");
		var connector = new TestConnector(schema);

		// Act
		var result = await connector.InitializeAsync(TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess());
		Assert.Equal(ConnectorState.Ready, connector.State);
	}

	[Fact]
	public async Task Should_ReturnFailure_When_InitializeAsyncWhenAlreadyInitialized()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0");
		var connector = new TestConnector(schema);
		await connector.InitializeAsync(TestContext.Current.CancellationToken);

		// Act
		var result = await connector.InitializeAsync(TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess());
		Assert.Equal("ALREADY_INITIALIZED", result.Error?.Code);
	}

	[Fact]
	public async Task Should_TransitionToErrorState_When_InitializeAsyncWhenInitializationFails()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0");
		var connector = new TestConnector(schema) { ShouldFailInitialization = true };

		// Act
		var result = await connector.InitializeAsync(TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess());
		Assert.Equal(ConnectorState.Error, connector.State);
	}

	[Fact]
	public async Task Should_TransitionToErrorState_When_InitializeAsyncWhenInitializationThrows()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0");
		var connector = new TestConnector(schema) { ShouldThrowOnInitialization = true };

		// Act
		var result = await connector.InitializeAsync(TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess());
		Assert.Equal(ConnectorState.Error, connector.State);
		Assert.Equal("INITIALIZATION_ERROR", result.Error?.Code);
	}

	[Fact]
	public async Task Should_ThrowInvalidOperationException_When_TestConnectionAsyncWhenNotOperational()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0");
		var connector = new TestConnector(schema);

		// Act
		// Assert
		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			connector.TestConnectionAsync(TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task Should_ReturnResult_When_TestConnectionAsyncWhenOperational()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0");
		var connector = new TestConnector(schema);
		await connector.InitializeAsync(TestContext.Current.CancellationToken);

		// Act
		var result = await connector.TestConnectionAsync(TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess());
		Assert.True(result.Value);
	}

	[Fact]
	public async Task Should_ThrowNotSupportedException_When_SendMessageAsyncWithoutSendCapability()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0")
			.WithCapabilities(ChannelCapability.ReceiveMessages); // No send capability
		var connector = new TestConnector(schema);
		await connector.InitializeAsync(TestContext.Current.CancellationToken);
		var message = new Message
		{
			Id = Guid.NewGuid().ToString(),
			Content = new TextContent("This is a test message.")
		};

		// Act
		// Assert
		await Assert.ThrowsAsync<NotSupportedException>(() =>
			connector.SendMessageAsync(message, TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task Should_ThrowArgumentNullException_When_SendMessageAsyncWithNullMessage()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0");
		var connector = new TestConnector(schema);
		await connector.InitializeAsync(TestContext.Current.CancellationToken);

		// Act
		// Assert
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			connector.SendMessageAsync(null!, TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task Should_ReturnResult_When_SendMessageAsyncWhenSupported()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0");
		var connector = new TestConnector(schema);
		await connector.InitializeAsync(TestContext.Current.CancellationToken);
		var message = new Message
		{
			Id = Guid.NewGuid().ToString(),
			Content = new TextContent("This is a test message.")
		};

		// Act
		var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess());
		Assert.NotNull(result.Value);
		Assert.Equal(message.Id, result.Value.MessageId);
	}

	[Fact]
	public async Task Should_ThrowNotSupportedException_When_SendBatchAsyncWithoutBulkCapability()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0");
		var connector = new TestConnector(schema);
		await connector.InitializeAsync(TestContext.Current.CancellationToken);
		var batch = new MessageBatch {
			Id = Guid.NewGuid().ToString(),
			Messages = new List<IMessage> { new Message { Id = Guid.NewGuid().ToString() } }
		};

		// Act
		// Assert
		await Assert.ThrowsAsync<NotSupportedException>(() =>
			connector.SendBatchAsync(batch, TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task Should_CallsImplementation_When_SendBatchAsyncWithCapability()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0")
			.WithCapability(ChannelCapability.BulkMessaging);
		var connector = new TestConnector(schema);
		await connector.InitializeAsync(TestContext.Current.CancellationToken);
		var batch = new MessageBatch {
			Id = Guid.NewGuid().ToString(),
			Messages = new List<IMessage> {
				new Message {
					Id = Guid.NewGuid().ToString(),
					Content = new TextContent("This is a test message.")
				}
			}
		};

		// Act
		var result = await connector.SendBatchAsync(batch, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess());
		Assert.Contains("Batch sending is not supported", result.Error!.Message!);
	}

	[Fact]
	public async Task Should_ThrowNotSupportedException_When_GetMessageStatusAsyncWithoutCapability()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0");
		var connector = new TestConnector(schema);
		await connector.InitializeAsync(TestContext.Current.CancellationToken);

		// Act
		// Assert
		await Assert.ThrowsAsync<NotSupportedException>(() =>
			connector.GetMessageStatusAsync("test-message", TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task Should_ThrowArgumentNullException_When_GetMessageStatusAsyncWithNullMessageId()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0")
			.WithCapability(ChannelCapability.MessageStatusQuery);
		var connector = new TestConnector(schema);
		await connector.InitializeAsync(TestContext.Current.CancellationToken);

		// Act
		// Assert
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			connector.GetMessageStatusAsync(null!, TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task Should_ReturnValidationResults_When_ValidateMessageAsyncWithMessage()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0")
			.AddContentType(MessageContentType.PlainText);
		var connector = new TestConnector(schema);
		var message = new Message
		{
			Id = Guid.NewGuid().ToString(),
			Content = new TextContent("This is a test message.")
		};

		// Act
		var results = new List<ValidationResult>();
		await foreach (var result in connector.ValidateMessageAsync(message, TestContext.Current.CancellationToken))
		{
			results.Add(result);
		}

		// Assert
		Assert.Single(results);
		Assert.Equal(ValidationResult.Success, results[0]);
	}

	[Fact]
	public async Task Should_ThrowNotSupportedException_When_ReceiveStatusAsyncWithoutCapability()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0");
		var connector = new TestConnector(schema);
		await connector.InitializeAsync(TestContext.Current.CancellationToken);

		// Act
		// Assert
		await Assert.ThrowsAsync<NotSupportedException>(() =>
			connector.ReceiveMessageStatusAsync(MessageSource.Text("test content"), TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task Should_ThrowNotSupportedException_When_ReceiveMessagesAsyncWithoutCapability()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0");
		var connector = new TestConnector(schema);
		await connector.InitializeAsync(TestContext.Current.CancellationToken);

		// Act
		// Assert
		await Assert.ThrowsAsync<NotSupportedException>(() =>
			connector.ReceiveMessagesAsync(MessageSource.Text("test content"), TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task Should_ThrowNotSupportedException_When_GetHealthAsyncWithoutCapability()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0");
		var connector = new TestConnector(schema);

		// Act
		// Assert
		await Assert.ThrowsAsync<NotSupportedException>(() =>
			connector.GetHealthAsync(TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task Should_ReturnHealthInfo_When_GetHealthAsyncWithCapability()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0")
			.WithCapability(ChannelCapability.HealthCheck);
		var connector = new TestConnector(schema);
		await connector.InitializeAsync(TestContext.Current.CancellationToken);

		// Act
		var result = await connector.GetHealthAsync(TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess());
		Assert.NotNull(result.Value);
		Assert.Equal(ConnectorState.Ready, result.Value.State);
		Assert.True(result.Value.IsHealthy);
	}

	[Fact]
	public async Task Should_ReturnUnhealthyStatus_When_GetHealthAsyncWhenNotReady()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0")
			.WithCapability(ChannelCapability.HealthCheck);
		var connector = new TestConnector(schema) { ShouldFailInitialization = true };
		await connector.InitializeAsync(TestContext.Current.CancellationToken); // This will put it in Error state

		// Act
		var result = await connector.GetHealthAsync(TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess());
		Assert.NotNull(result.Value);
		Assert.Equal(ConnectorState.Error, result.Value.State);
		Assert.False(result.Value.IsHealthy);
		Assert.Contains("Connector is in Error state", result.Value.Issues);
	}

	[Fact]
	public async Task Should_TransitionToShutdownState_When_ShutdownAsyncIsInvoked()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0");
		var connector = new TestConnector(schema);
		await connector.InitializeAsync(TestContext.Current.CancellationToken);

		// Act
		await connector.ShutdownAsync(TestContext.Current.CancellationToken);

		// Assert
		Assert.Equal(ConnectorState.Shutdown, connector.State);
	}

	[Fact]
	public async Task Should_DoNothing_When_ShutdownAsyncWhenAlreadyShutdown()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0");
		var connector = new TestConnector(schema);
		await connector.InitializeAsync(TestContext.Current.CancellationToken);
		await connector.ShutdownAsync(TestContext.Current.CancellationToken);

		// Act
		await connector.ShutdownAsync(TestContext.Current.CancellationToken);

		// Assert
		Assert.Equal(ConnectorState.Shutdown, connector.State);
	}

	[Theory]
	[InlineData(ConnectorState.Uninitialized)]
	[InlineData(ConnectorState.Initializing)]
	[InlineData(ConnectorState.ShuttingDown)]
	[InlineData(ConnectorState.Shutdown)]
	public async Task Should_ThrowInvalidOperationException_When_OperationalMethodsWithNonOperationalState(ConnectorState state)
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0");
		var connector = new TestConnector(schema);
		connector.SetStatePublic(state);
		var message = new Message {
			Id = Guid.NewGuid().ToString(),
			Content = new TextContent("This is a test message.")
		};

		// Act
		// Assert
		if (state != ConnectorState.Shutdown && state != ConnectorState.ShuttingDown)
		{
			await Assert.ThrowsAsync<InvalidOperationException>(() =>
				connector.TestConnectionAsync(TestContext.Current.CancellationToken));
		}

		if (state != ConnectorState.Shutdown && state != ConnectorState.ShuttingDown)
		{
			await Assert.ThrowsAsync<InvalidOperationException>(() =>
				connector.SendMessageAsync(message, TestContext.Current.CancellationToken));
		}
	}

	[Fact]
	public async Task Should_ReturnStatus_When_GetStatusAsyncIsInvoked()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0");
		var connector = new TestConnector(schema);

		// Act
		var result = await connector.GetStatusAsync(TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess());
		Assert.Equal("Test Status", result.Value.Status);
	}

	[Fact]
	public async Task Should_ReturnValidationFailure_When_SendMessageAsyncWithValidationErrors()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0")
			.AddContentType(MessageContentType.Html); // Only supports HTML, not PlainText
		var connector = new TestConnector(schema);
		await connector.InitializeAsync(TestContext.Current.CancellationToken);
		var message = new Message
		{
			Id = Guid.NewGuid().ToString(),
			Content = new TextContent("This is a test message.") // Invalid content type
		};

		// Act
		var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess());
		Assert.Equal("MESSAGE_VALIDATION_FAILED", result.Error?.Code);
		var validationError = Assert.IsAssignableFrom<IValidationError>(result.Error);
		Assert.NotEmpty(validationError.ValidationResults);
		Assert.Contains(validationError.ValidationResults, 
			r => 
				r.ErrorMessage != null &&
				r.ErrorMessage.Contains("not supported") && 
				r.ErrorMessage.Contains("PlainText"));
	}

	[Fact]
	public async Task Should_ReturnValidationFailure_When_SendBatchAsyncWithValidationErrors()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0")
			.WithCapability(ChannelCapability.BulkMessaging)
			.AddContentType(MessageContentType.Html); // Only supports HTML
		var connector = new TestConnector(schema);
		await connector.InitializeAsync(TestContext.Current.CancellationToken);

		var batch = new MessageBatch {
			Id = Guid.NewGuid().ToString(),
			Messages = new List<IMessage> {
				new Message {
					Id = Guid.NewGuid().ToString(),
					Content = new TextContent()
				}
			}
		};

		// Act
		var result = await connector.SendBatchAsync(batch, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess());
		Assert.Equal("BATCH_VALIDATION_FAILED", result.Error?.Code);
		var validationError = Assert.IsAssignableFrom<IValidationError>(result.Error);
		Assert.NotEmpty(validationError.ValidationResults);
		Assert.Contains(validationError.ValidationResults, 
			r => 
				r.ErrorMessage != null &&
				r.ErrorMessage.Contains("not supported") && 
				r.ErrorMessage.Contains("PlainText"));
	}

	[Fact]
	public async Task Should_ReturnValidationError_When_ValidateMessageAsyncWithInvalidContentType()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0")
			.AddContentType(MessageContentType.Html); // Only supports HTML
		var connector = new TestConnector(schema);
		var message = new Message
		{
			Id = Guid.NewGuid().ToString(),
			Content = new TextContent("This is a test message.") // Invalid content type
		};

		// Act
		var results = new List<ValidationResult>();
		await foreach (var result in connector.ValidateMessageAsync(message, TestContext.Current.CancellationToken))
		{
			results.Add(result);
		}

		// Assert
		Assert.NotEmpty(results);
		var errorResult = results.FirstOrDefault(r => r != ValidationResult.Success);
		Assert.NotNull(errorResult);
		Assert.Contains("not supported", errorResult.ErrorMessage);
		Assert.Contains("PlainText", errorResult.ErrorMessage);
	}

	[Fact]
	public async Task Should_ReturnValidationError_When_ValidateMessageAsyncWithMissingMessageId()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0");
		var connector = new TestConnector(schema);
		var message = new Message {
			Id = ""
		}; // Empty ID

		// Act
		var results = new List<ValidationResult>();
		await foreach (var result in connector.ValidateMessageAsync(message, TestContext.Current.CancellationToken))
		{
			results.Add(result);
		}

		// Assert
		Assert.NotEmpty(results);
		var errorResult = results.FirstOrDefault(r => r != ValidationResult.Success);
		Assert.NotNull(errorResult);
		Assert.Contains("Message ID is required", errorResult.ErrorMessage);
		Assert.Contains("Id", errorResult.MemberNames);
	}

	[Fact]
	public async Task Should_ReturnSuccess_When_ValidateMessageAsyncWithValidMessage()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0")
			.AddContentType(MessageContentType.PlainText); // Supports PlainText
		var connector = new TestConnector(schema);
		var message = new Message
		{
			Content = new TextContent("This is a test message."),
			Id = Guid.NewGuid().ToString()
		};

		// Act
		var results = new List<ValidationResult>();
		await foreach (var result in connector.ValidateMessageAsync(message, TestContext.Current.CancellationToken))
		{
			results.Add(result);
		}

		// Assert
		Assert.Single(results);
		Assert.Equal(ValidationResult.Success, results[0]);
	}

	[Fact]
	public void Should_ReturnEndpointTypeProperty_When_GetEndpointTypeIsInvoked()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0");
		var connector = new TestConnector(schema);
		var endpoint = Endpoint.EmailAddress("test@example.com");

		// Act
		var endpointType = connector.GetEndpointTypePublic(endpoint);

		// Assert
		Assert.Equal("email", endpointType);
	}

	[Fact]
	public void Should_ReturnTrue_When_IsEndpointTypeSupportedWithMatchingEndpointType()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0")
			.HandlesMessageEndpoint(EndpointType.EmailAddress, e =>
			{
				e.CanSend = true;
				e.CanReceive = false;
			});
		var connector = new TestConnector(schema);

		// Act
		var isSupported = connector.IsEndpointTypeSupportedPublic(EndpointType.EmailAddress, asSender: true);

		// Assert
		Assert.True(isSupported);
	}

	[Fact]
	public void Should_ReturnFalse_When_IsEndpointTypeSupportedWithNonMatchingEndpointType()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0")
			.HandlesMessageEndpoint(EndpointType.EmailAddress, e =>
			{
				e.CanSend = true;
				e.CanReceive = false;
			});
		var connector = new TestConnector(schema);

		// Act
		var isSupported = connector.IsEndpointTypeSupportedPublic(EndpointType.PhoneNumber, asSender: true);

		// Assert
		Assert.False(isSupported);
	}

	[Fact]
	public async Task Should_WorksCorrectly_When_ValidateMessageAsyncWithEndpointTypeValidation()
	{
		// Arrange
		var schema = new ChannelSchema("TestProvider", "Email", "1.0.0")
			.AddContentType(MessageContentType.PlainText)
			.HandlesMessageEndpoint(EndpointType.EmailAddress, e =>
			{
				e.CanSend = true;
				e.CanReceive = false;
			});

		var connector = new TestConnector(schema);
		var validMessage = new Message
		{
			Id = Guid.NewGuid().ToString(),
			Content = new TextContent("This is a valid message."),
			Sender = Endpoint.EmailAddress("sender@test.com"),
			Receiver = Endpoint.EmailAddress("receiver@test.com")  // This should fail validation
		};

		// Act
		var results = new List<ValidationResult>();
		await foreach (var result in connector.ValidateMessageAsync(validMessage, TestContext.Current.CancellationToken))
		{
			results.Add(result);
		}

		// Assert
		var errorResults = results.Where(r => r != ValidationResult.Success).ToList();
		Assert.Single(errorResults);
		var errorResult = errorResults.First();
		Assert.Contains("Receiver endpoint type", errorResult.ErrorMessage);
		Assert.Contains("not supported", errorResult.ErrorMessage);
	}

	// Test connector implementation for testing
	private class TestConnector : ChannelConnectorBase
	{
		public bool ShouldFailInitialization { get; set; }
		public bool ShouldThrowOnInitialization { get; set; }

		public TestConnector(IChannelSchema schema) : base(schema)
		{
		}

		public void SetStatePublic(ConnectorState state) => SetState(state);

		// Expose protected methods for testing
		public string? GetEndpointTypePublic(IEndpoint endpoint) => GetEndpointType(endpoint);

		public bool IsEndpointTypeSupportedPublic(EndpointType endpointType, bool asSender = false, bool asReceiver = false) =>
			IsEndpointTypeSupported(endpointType, asSender, asReceiver);

		protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
		{
			if (ShouldThrowOnInitialization)
				throw new InvalidOperationException("Test initialization failure");

			if (ShouldFailInitialization)
                throw new MessagingException("INIT_FAILED", "Initialization failed");

            return ValueTask.CompletedTask;
		}

		protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
		{
            return ValueTask.CompletedTask;
		}

		protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
		{
			var result = new SendResult(message.Id, $"remote-{message.Id}");
			return Task.FromResult(result);
		}

		protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
		{
			var status = new StatusInfo("Test Status");
			return Task.FromResult(status);
		}
	}
}
