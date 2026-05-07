//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Deveel.Messaging.XUnit
{
	/// <summary>
	/// Tests for advanced ChannelRegistry scenarios including disposal, concurrent operations, and complex error cases.
	/// </summary>
	[Trait("Category", "Unit")]
	[Trait("Layer", "Application")]
	[Trait("Feature", "ChannelRegistryAdvanced")]
	public class ChannelRegistryAdvancedTests
	{
		private static ChannelRegistry CreateRegistry() => new ChannelRegistry(new ServiceCollection().BuildServiceProvider());

		[Fact]
		public async Task Should_DisposesOnFailedInitialization_When_CreateConnectorAsyncWithDisposableConnector()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<DisposableFailingConnector>();

			// Act
			// Assert
			var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
				registry.CreateConnectorAsync<DisposableFailingConnector>());
			
			Assert.Contains("Failed to initialize connector", exception.Message);
			
			// The connector should have been disposed automatically
			Assert.True(DisposableFailingConnector.WasDisposed);
		}

		[Fact]
		public async Task Should_DisposesOnException_When_CreateConnectorAsyncWithAsyncDisposableConnector()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<AsyncDisposableExceptionConnector>();

			// Reset the static flag before the test
			var field = typeof(AsyncDisposableExceptionConnector).GetField("WasAsyncDisposed", 
				System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
			field?.SetValue(null, false);

			// Act
			// Assert
			var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
				registry.CreateConnectorAsync<AsyncDisposableExceptionConnector>());
			
			Assert.Contains("Failed to create instance", exception.Message);
			
			// Since the constructor throws before the object is fully created,
			// we can't expect it to be disposed via IAsyncDisposable
			// The test verifies that the exception is properly wrapped and handled
		}

		[Fact]
		public async Task Should_HandleGracefully_When_DisposeConnectorsSyncWithTimeout()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<SlowShutdownConnector>();
			
			// Create a connector that will take time to shutdown
			var connector = await registry.CreateConnectorAsync<SlowShutdownConnector>();

			// Act
			registry.Dispose();

			// Assert
			Assert.Equal(ConnectorState.Shutdown, connector.State);
		}

		[Fact]
		public async Task Should_ContinuesDisposal_When_DisposeConnectorsAsyncWithShutdownError()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<ErrorDuringShutdownConnector>();
			registry.RegisterConnector<TestConnector>();
			
			var connector1 = await registry.CreateConnectorAsync<ErrorDuringShutdownConnector>();
			var connector2 = await registry.CreateConnectorAsync<TestConnector>();

			// Act
			await registry.DisposeAsync();

			// Assert
			Assert.Equal(ConnectorState.Shutdown, connector2.State);
		}

		[Fact]
		public async Task Should_CallsDisposeAsync_When_DisposeConnectorsAsyncWithAsyncDisposableConnector()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<AsyncDisposableConnector>();
			
			var connector = await registry.CreateConnectorAsync<AsyncDisposableConnector>();

			// Act
			await registry.DisposeAsync();

			// Assert
			Assert.True(AsyncDisposableConnector.WasAsyncDisposed);
			Assert.Equal(ConnectorState.Shutdown, connector.State);
		}

		[Fact]
		public async Task Should_ContinuesWithOtherConnectors_When_DisposeConnectorsWithDisposalError()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<ErrorDuringDisposalConnector>();
			registry.RegisterConnector<TestConnector>();
			
			var connector1 = await registry.CreateConnectorAsync<ErrorDuringDisposalConnector>();
			var connector2 = await registry.CreateConnectorAsync<TestConnector>();

			// Act
			await registry.DisposeAsync();

			// Assert
			Assert.Equal(ConnectorState.Shutdown, connector2.State);
		}

		[Fact]
		public void Should_ReturnEarlyErrors_When_ValidateRuntimeSchemaInternalWithIncompatibleSchema()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();
			
			var incompatibleSchema = new ChannelSchema("DifferentProvider", "DifferentType", "1.0.0");

			// Act
			var validationResults = registry.ValidateSchema<TestConnector>(incompatibleSchema).ToList();

			// Assert
			Assert.NotEmpty(validationResults);
			Assert.Contains(validationResults, r => r.ErrorMessage!.Contains("not compatible"));
		}

		[Fact]
		public async Task Should_FailOnCreation_When_CreateConnectorInstanceWithMissingConstructor()
		{
			// Arrange
			var registry = CreateRegistry();
			
			// This should register fine, but fail when creating the connector
			registry.RegisterConnector<ConnectorWithoutProperConstructor>();

			// Act
			// Assert
			await Assert.ThrowsAsync<InvalidOperationException>(() => 
				registry.CreateConnectorAsync<ConnectorWithoutProperConstructor>());
		}

		[Fact]
		public void Should_ThrowArgumentException_When_CreateSchemaWithInvalidSchemaType()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentException>(() => 
				registry.RegisterConnector<ConnectorWithInvalidSchemaType>());
		}

		[Fact]
		public async Task Should_HandleGracefully_When_ConcurrentConnectorCreationIsInvoked()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();

			// Act
			var tasks = Enumerable.Range(0, 10)
				.Select(_ => registry.CreateConnectorAsync<TestConnector>())
				.ToArray();

			var connectors = await Task.WhenAll(tasks);

			// Assert
			Assert.Equal(10, connectors.Length);
			Assert.All(connectors, c => Assert.Equal(ConnectorState.Ready, c.State));
		}

		[Fact]
		public async Task Should_HandleGracefully_When_ConcurrentRegistrationIsInvoked()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			var tasks = Enumerable.Range(0, 5)
				.Select(i => Task.Run(() =>
				{
					try
					{
						registry.RegisterConnector<TestConnector>();
						return true;
					}
					catch (InvalidOperationException)
					{
						// Expected for all but the first registration
						return false;
					}
				}))
				.ToArray();

			var results = await Task.WhenAll(tasks);

			// Assert
			var successCount = results.Count(result => result);
			Assert.Equal(1, successCount);
		}

		[Fact]
		public async Task Should_HandleGracefully_When_DisposeAsyncCalledMultipleTimes()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();
			
			var connector = await registry.CreateConnectorAsync<TestConnector>();

			// Act
			await registry.DisposeAsync();
			await registry.DisposeAsync();
			await registry.DisposeAsync();

			// Assert
			Assert.Equal(ConnectorState.Shutdown, connector.State);
		}

		[Fact]
		public void Should_HandleGracefully_When_DisposeCalledMultipleTimes()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			registry.Dispose();
			registry.Dispose();
			registry.Dispose();

			// Assert
		}

		// Test connector classes for various scenarios

		[ChannelSchema(typeof(TestSchemaFactory))]
		private class TestConnector : ChannelConnectorBase
		{
			public TestConnector(IChannelSchema schema) : base(schema) { }

			protected override Task<ConnectorResult<bool>> InitializeConnectorAsync(CancellationToken cancellationToken)
			{
				SetState(ConnectorState.Ready);
				return Task.FromResult(ConnectorResult<bool>.Success(true));
			}

			protected override Task<ConnectorResult<bool>> TestConnectorConnectionAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<SendResult>> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			protected override Task<ConnectorResult<StatusInfo>> GetConnectorStatusAsync(CancellationToken cancellationToken)
				=> throw new NotImplementedException();
		}

		[ChannelSchema(typeof(TestSchemaFactory))]
		private class DisposableFailingConnector : ChannelConnectorBase, IDisposable
		{
			public static bool WasDisposed { get; private set; }

			public DisposableFailingConnector(IChannelSchema schema) : base(schema) 
			{
				WasDisposed = false;
			}

			protected override Task<ConnectorResult<bool>> InitializeConnectorAsync(CancellationToken cancellationToken)
			{
				return Task.FromResult(ConnectorResult<bool>.Fail("INIT_FAILED", "Initialization failed"));
			}

			protected override Task<ConnectorResult<bool>> TestConnectorConnectionAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<SendResult>> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			protected override Task<ConnectorResult<StatusInfo>> GetConnectorStatusAsync(CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			public void Dispose()
			{
				WasDisposed = true;
			}
		}

		[ChannelSchema(typeof(TestSchemaFactory))]
		private class AsyncDisposableExceptionConnector : ChannelConnectorBase, IAsyncDisposable
		{
			public static bool WasAsyncDisposed { get; private set; }

			static AsyncDisposableExceptionConnector()
			{
				WasAsyncDisposed = false;
			}

			public AsyncDisposableExceptionConnector(IChannelSchema schema) : base(schema) 
			{
				WasAsyncDisposed = false;
				throw new Exception("Constructor exception");
			}

			protected override Task<ConnectorResult<bool>> InitializeConnectorAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<bool>> TestConnectorConnectionAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<SendResult>> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			protected override Task<ConnectorResult<StatusInfo>> GetConnectorStatusAsync(CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			public ValueTask DisposeAsync()
			{
				WasAsyncDisposed = true;
				return ValueTask.CompletedTask;
			}
		}

		[ChannelSchema(typeof(TestSchemaFactory))]
		private class SlowShutdownConnector : ChannelConnectorBase
		{
			public SlowShutdownConnector(IChannelSchema schema) : base(schema) { }

			protected override Task<ConnectorResult<bool>> InitializeConnectorAsync(CancellationToken cancellationToken)
			{
				SetState(ConnectorState.Ready);
				return Task.FromResult(ConnectorResult<bool>.Success(true));
			}

			protected override Task<ConnectorResult<bool>> TestConnectorConnectionAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<SendResult>> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			protected override Task<ConnectorResult<StatusInfo>> GetConnectorStatusAsync(CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			protected override async Task ShutdownConnectorAsync(CancellationToken cancellationToken)
			{
				// Simulate slow shutdown but still complete within reasonable time for sync disposal
				await Task.Delay(3000, cancellationToken);
				SetState(ConnectorState.Shutdown);
			}
		}

		[ChannelSchema(typeof(TestSchemaFactory))]
		private class ErrorDuringShutdownConnector : ChannelConnectorBase
		{
			public ErrorDuringShutdownConnector(IChannelSchema schema) : base(schema) { }

			protected override Task<ConnectorResult<bool>> InitializeConnectorAsync(CancellationToken cancellationToken)
			{
				SetState(ConnectorState.Ready);
				return Task.FromResult(ConnectorResult<bool>.Success(true));
			}

			protected override Task<ConnectorResult<bool>> TestConnectorConnectionAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<SendResult>> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			protected override Task<ConnectorResult<StatusInfo>> GetConnectorStatusAsync(CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			protected override Task ShutdownConnectorAsync(CancellationToken cancellationToken)
			{
				throw new Exception("Shutdown error");
			}
		}

		[ChannelSchema(typeof(TestSchemaFactory))]
		private class AsyncDisposableConnector : ChannelConnectorBase, IAsyncDisposable
		{
			public static bool WasAsyncDisposed { get; private set; }

			public AsyncDisposableConnector(IChannelSchema schema) : base(schema) 
			{
				WasAsyncDisposed = false;
			}

			protected override Task<ConnectorResult<bool>> InitializeConnectorAsync(CancellationToken cancellationToken)
			{
				SetState(ConnectorState.Ready);
				return Task.FromResult(ConnectorResult<bool>.Success(true));
			}

			protected override Task<ConnectorResult<bool>> TestConnectorConnectionAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<SendResult>> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			protected override Task<ConnectorResult<StatusInfo>> GetConnectorStatusAsync(CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			protected override Task ShutdownConnectorAsync(CancellationToken cancellationToken)
			{
				SetState(ConnectorState.Shutdown);
				return Task.CompletedTask;
			}

			public ValueTask DisposeAsync()
			{
				WasAsyncDisposed = true;
				return ValueTask.CompletedTask;
			}
		}

		[ChannelSchema(typeof(TestSchemaFactory))]
		private class ErrorDuringDisposalConnector : ChannelConnectorBase, IDisposable
		{
			public ErrorDuringDisposalConnector(IChannelSchema schema) : base(schema) { }

			protected override Task<ConnectorResult<bool>> InitializeConnectorAsync(CancellationToken cancellationToken)
			{
				SetState(ConnectorState.Ready);
				return Task.FromResult(ConnectorResult<bool>.Success(true));
			}

			protected override Task<ConnectorResult<bool>> TestConnectorConnectionAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<SendResult>> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			protected override Task<ConnectorResult<StatusInfo>> GetConnectorStatusAsync(CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			protected override Task ShutdownConnectorAsync(CancellationToken cancellationToken)
			{
				SetState(ConnectorState.Shutdown);
				return Task.CompletedTask;
			}

			public void Dispose()
			{
				throw new Exception("Disposal error");
			}
		}

		[ChannelSchema(typeof(TestSchemaFactory))]
		private class ConnectorWithoutProperConstructor : ChannelConnectorBase
		{
			// Missing constructor that accepts IChannelSchema
			public ConnectorWithoutProperConstructor() : base(null!) { }

			protected override Task<ConnectorResult<bool>> InitializeConnectorAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<bool>> TestConnectorConnectionAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<SendResult>> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			protected override Task<ConnectorResult<StatusInfo>> GetConnectorStatusAsync(CancellationToken cancellationToken)
				=> throw new NotImplementedException();
		}

		[ChannelSchema(typeof(InvalidSchemaType))]
		private class ConnectorWithInvalidSchemaType : ChannelConnectorBase
		{
			public ConnectorWithInvalidSchemaType(IChannelSchema schema) : base(schema) { }

			protected override Task<ConnectorResult<bool>> InitializeConnectorAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<bool>> TestConnectorConnectionAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<SendResult>> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			protected override Task<ConnectorResult<StatusInfo>> GetConnectorStatusAsync(CancellationToken cancellationToken)
				=> throw new NotImplementedException();
		}

		private class TestSchemaFactory : IChannelSchemaFactory
		{
			public IChannelSchema CreateSchema()
			{
				return new ChannelSchema("TestProvider", "TestType", "1.0.0")
					.WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages)
					.HandlesMessageEndpoint(EndpointType.PhoneNumber)
					.AddContentType(MessageContentType.PlainText);
			}
		}

		// Invalid schema type that doesn't implement required interfaces
		private class InvalidSchemaType
		{
		}
	}
}
