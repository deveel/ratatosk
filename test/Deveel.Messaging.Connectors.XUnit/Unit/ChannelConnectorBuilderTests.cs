//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Messaging.XUnit
{
	/// <summary>
	/// Tests for <see cref="ChannelConnectorBuilder{TConnector}"/> covering schema overrides,
	/// settings (fluent and configuration-based), factory replacement, and DI integration
	/// for both unnamed and named connector registrations.
	/// </summary>
	[Trait("Category", "Unit")]
	[Trait("Layer", "Application")]
	[Trait("Feature", "ChannelConnectorBuilder")]
	public class ChannelConnectorBuilderTests
	{
		private static IServiceCollection CreateServices() => new ServiceCollection();

		private static IConfiguration BuildConfig(IDictionary<string, string?> values) =>
			new ConfigurationBuilder()
				.AddInMemoryCollection(values)
				.Build();

		// ── MessagingBuilder property ─────────────────────────────────────────

		#region MessagingBuilder property

		[Fact]
		public void Should_ExposeMessagingBuilder_When_BuilderIsCreated()
		{
			// Arrange
			var services = CreateServices();
			MessagingBuilder? captured = null;

			// Act
			var mb = services.AddMessaging()
				.AddConnector<TestConnector>(b =>
				{
					captured = b.MessagingBuilder;
				});

			// Assert
			Assert.NotNull(captured);
			Assert.Same(mb, captured);
		}

		#endregion

		// ── Unnamed connector — fluent settings ───────────────────────────────

		#region AddConnector (unnamed) — fluent settings

		[Fact]
		public void Should_RegisterConnector_When_BuilderConfigureIsInvoked()
		{
			// Arrange
			var services = CreateServices();

			// Act
			services.AddMessaging()
				.AddConnector<TestConnector>(b => b
					.WithSetting("AccountSid", "ACtest")
					.WithSetting("AuthToken", "tok123"));

			var provider = services.BuildServiceProvider();

			// Assert
			var connector = provider.GetRequiredService<TestConnector>();
			Assert.NotNull(connector);
		}

		[Fact]
		public void Should_PassSettingsToConnector_When_WithSettingIsUsed()
		{
			// Arrange
			var services = CreateServices();

			// Act
			services.AddMessaging()
				.AddConnector<TestConnector>(b => b
					.WithSetting("AccountSid", "ACtest")
					.WithSetting("AuthToken", "tok123"));

			var provider = services.BuildServiceProvider();
			var connector = (TestConnector)provider.GetRequiredService<TestConnector>();

			// Assert
			Assert.Equal("ACtest", connector.ConnectionSettings.GetParameter("AccountSid"));
			Assert.Equal("tok123", connector.ConnectionSettings.GetParameter("AuthToken"));
		}

		[Fact]
		public void Should_MergeMultipleSettings_When_WithSettingsActionIsUsed()
		{
			// Arrange
			var services = CreateServices();

			// Act
			services.AddMessaging()
				.AddConnector<TestConnector>(b => b
					.WithSettings(d =>
					{
						d["AccountSid"] = "AC-from-action";
						d["Region"]     = "us-east-1";
					}));

			var provider = services.BuildServiceProvider();
			var connector = provider.GetRequiredService<TestConnector>();

			// Assert
			Assert.Equal("AC-from-action", connector.ConnectionSettings.GetParameter("AccountSid"));
			Assert.Equal("us-east-1",     connector.ConnectionSettings.GetParameter("Region"));
		}

		[Fact]
		public void Should_OverrideSchema_When_WithSchemaIsUsed()
		{
			// Arrange
			var services = CreateServices();
			var overrideSchema = new DummySchema("OverriddenProvider", "OverriddenType");

			// Act
			services.AddMessaging()
				.AddConnector<TestConnector>(b => b.WithSchema(overrideSchema));

			var provider = services.BuildServiceProvider();
			var connector = provider.GetRequiredService<TestConnector>();

			// Assert
			Assert.Equal("OverriddenProvider", connector.Schema.ChannelProvider);
			Assert.Equal("OverriddenType",     connector.Schema.ChannelType);
		}

		[Fact]
		public void Should_UseCustomFactory_When_UseFactoryIsSet()
		{
			// Arrange
			var factoryCalled = false;
			var services = CreateServices();

			// Act
			services.AddMessaging()
				.AddConnector<TestConnector>(b => b
					.UseFactory((sp, schema, settings) =>
					{
						factoryCalled = true;
						return new TestConnector(schema, settings);
					}));

			var provider = services.BuildServiceProvider();
			provider.GetRequiredService<TestConnector>();

			// Assert
			Assert.True(factoryCalled);
		}

		[Fact]
		public void Should_PassSettingsToCustomFactory_When_UseFactoryAndWithSettingAreCombined()
		{
			// Arrange
			var services = CreateServices();
			ConnectionSettings? captured = null;

			// Act
			services.AddMessaging()
				.AddConnector<TestConnector>(b => b
					.WithSetting("From", "+15550000001")
					.UseFactory((sp, schema, settings) =>
					{
						captured = settings;
						return new TestConnector(schema, settings);
					}));

			var provider = services.BuildServiceProvider();
			provider.GetRequiredService<TestConnector>();

			// Assert
			Assert.NotNull(captured);
			Assert.Equal("+15550000001", captured!.GetParameter("From"));
		}

		[Fact]
		public void Should_BeSingleton_When_ConnectorIsRegisteredWithBuilder()
		{
			// Arrange
			var services = CreateServices();
			services.AddMessaging()
				.AddConnector<TestConnector>(b => b.WithSetting("AccountSid", "AC1"));

			var provider = services.BuildServiceProvider();

			// Act
			var first  = provider.GetRequiredService<TestConnector>();
			var second = provider.GetRequiredService<TestConnector>();

			// Assert
			Assert.Same(first, second);
		}

		#endregion

		// ── Unnamed connector — configuration-based settings ──────────────────

		#region AddConnector (unnamed) — IConfiguration settings

		[Fact]
		public void Should_LoadSettingsFromConfig_When_WithSettingsConfigSectionIsUsed()
		{
			// Arrange
			var config = BuildConfig(new Dictionary<string, string?>
			{
				["Messaging:Test:AccountSid"] = "AC-from-config",
				["Messaging:Test:AuthToken"]  = "tok-from-config",
			});

			var services = CreateServices();
			services.AddSingleton<IConfiguration>(config);

			// Act
			services.AddMessaging()
				.AddConnector<TestConnector>(b => b
					.WithSettings("Messaging:Test"));

			var provider = services.BuildServiceProvider();
			var connector = provider.GetRequiredService<TestConnector>();

			// Assert
			Assert.Equal("AC-from-config",  connector.ConnectionSettings.GetParameter("AccountSid"));
			Assert.Equal("tok-from-config", connector.ConnectionSettings.GetParameter("AuthToken"));
		}

		[Fact]
		public void Should_PreferFluentOverConfig_When_BothAreProvided()
		{
			// Arrange
			var config = BuildConfig(new Dictionary<string, string?>
			{
				["Messaging:Test:AccountSid"] = "AC-from-config",
				["Messaging:Test:AuthToken"]  = "tok-from-config",
				["Messaging:Test:Region"]     = "eu-west-1",
			});

			var services = CreateServices();
			services.AddSingleton<IConfiguration>(config);

			// Act
			services.AddMessaging()
				.AddConnector<TestConnector>(b => b
					.WithSettings("Messaging:Test")
					.WithSetting("AuthToken", "tok-from-fluent") // overrides config
					.WithSetting("NewKey",    "extra-value"));    // new key not in config

			var provider = services.BuildServiceProvider();
			var connector = provider.GetRequiredService<TestConnector>();

			// Assert — fluent wins
			Assert.Equal("AC-from-config",  connector.ConnectionSettings.GetParameter("AccountSid"));
			Assert.Equal("tok-from-fluent", connector.ConnectionSettings.GetParameter("AuthToken"));
			Assert.Equal("eu-west-1",       connector.ConnectionSettings.GetParameter("Region"));
			Assert.Equal("extra-value",     connector.ConnectionSettings.GetParameter("NewKey"));
		}

		[Fact]
		public void Should_ReturnEmptySettings_When_IConfigurationIsNotRegistered()
		{
			// Arrange — no IConfiguration in DI
			var services = CreateServices();

			services.AddMessaging()
				.AddConnector<TestConnector>(b => b
					.WithSettings("Messaging:Missing"));

			var provider = services.BuildServiceProvider();

			// Act — should not throw
			var connector = provider.GetRequiredService<TestConnector>();

			// Assert
			Assert.NotNull(connector);
			Assert.Empty(connector.ConnectionSettings.Parameters);
		}

		#endregion

		// ── Named connector — fluent settings ─────────────────────────────────

		#region AddConnector (named) — fluent settings

		[Fact]
		public void Should_RegisterKeyedConnector_When_NamedBuilderIsUsed()
		{
			// Arrange
			var services = CreateServices();
			services.AddMessaging()
				.AddConnector<TestConnector>("marketing", b => b
					.WithSetting("PhoneNumber", "+15550001111"));

			var provider = services.BuildServiceProvider();

			// Act
			var connector = provider.GetRequiredKeyedService<IChannelConnector>("marketing");

			// Assert
			Assert.NotNull(connector);
			Assert.IsType<TestConnector>(connector);
		}

		[Fact]
		public void Should_PassSettingsToNamedConnector_When_WithSettingIsUsed()
		{
			// Arrange
			var services = CreateServices();
			services.AddMessaging()
				.AddConnector<TestConnector>("support", b => b
					.WithSetting("PhoneNumber", "+15550002222")
					.WithSetting("Region",      "us-west-2"));

			var provider = services.BuildServiceProvider();
			var connector = (TestConnector)provider.GetRequiredKeyedService<IChannelConnector>("support");

			// Assert
			Assert.Equal("+15550002222", connector.ConnectionSettings.GetParameter("PhoneNumber"));
			Assert.Equal("us-west-2",    connector.ConnectionSettings.GetParameter("Region"));
		}

		[Fact]
		public void Should_RegisterNamedConnectorDescriptor_When_NamedBuilderIsUsed()
		{
			// Arrange
			var services = CreateServices();
			services.AddMessaging()
				.AddConnector<TestConnector>("marketing", b => b
					.WithSetting("PhoneNumber", "+15550001111"));

			var provider = services.BuildServiceProvider();

			// Act
			var descriptors = provider.GetServices<NamedConnectorDescriptor>().ToList();

			// Assert
			Assert.Contains(descriptors, d => d.Name == "marketing");
		}

		[Fact]
		public void Should_IncludeSettingsInDescriptor_When_NamedBuilderIsUsed()
		{
			// Arrange
			var services = CreateServices();
			services.AddMessaging()
				.AddConnector<TestConnector>("support", b => b
					.WithSetting("PhoneNumber", "+15550002222"));

			var provider = services.BuildServiceProvider();
			var descriptor = provider.GetServices<NamedConnectorDescriptor>()
				.First(d => d.Name == "support");

			// Assert
			Assert.Equal("+15550002222", descriptor.GetSetting<string>("PhoneNumber"));
		}

		[Fact]
		public void Should_UseCustomFactory_When_NamedBuilderHasFactoryOverride()
		{
			// Arrange
			var factoryCalled = false;
			var services = CreateServices();
			services.AddMessaging()
				.AddConnector<TestConnector>("promo", b => b
					.UseFactory((sp, schema, settings) =>
					{
						factoryCalled = true;
						return new TestConnector(schema, settings);
					}));

			var provider = services.BuildServiceProvider();
			provider.GetRequiredKeyedService<IChannelConnector>("promo");

			// Assert
			Assert.True(factoryCalled);
		}

		[Fact]
		public void Should_OverrideSchemaForNamedConnector_When_WithSchemaIsUsed()
		{
			// Arrange
			var services = CreateServices();
			var overrideSchema = new DummySchema("CustomProvider", "CustomType");

			services.AddMessaging()
				.AddConnector<TestConnector>("restricted", b => b.WithSchema(overrideSchema));

			var provider = services.BuildServiceProvider();
			var connector = (TestConnector)provider.GetRequiredKeyedService<IChannelConnector>("restricted");

			// Assert
			Assert.Equal("CustomProvider", connector.Schema.ChannelProvider);
			Assert.Equal("CustomType",     connector.Schema.ChannelType);
		}

		[Fact]
		public void Should_RegisterMultipleNamedConnectors_When_MultipleBuilderCallsAreMade()
		{
			// Arrange
			var services = CreateServices();
			services.AddMessaging()
				.AddConnector<TestConnector>("marketing", b => b.WithSetting("PhoneNumber", "+1555001"))
				.AddConnector<TestConnector>("support",   b => b.WithSetting("PhoneNumber", "+1555002"));

			var provider = services.BuildServiceProvider();

			// Act
			var marketing = provider.GetRequiredKeyedService<IChannelConnector>("marketing");
			var support   = provider.GetRequiredKeyedService<IChannelConnector>("support");

			// Assert — both resolved, independently constructed
			Assert.IsType<TestConnector>(marketing);
			Assert.IsType<TestConnector>(support);
			Assert.NotSame(marketing, support);
		}

		#endregion

		// ── Named connector — configuration-based settings ────────────────────

		#region AddConnector (named) — IConfiguration settings

		[Fact]
		public void Should_LoadSettingsFromConfig_When_NamedBuilderWithSettingsConfigSectionIsUsed()
		{
			// Arrange
			var config = BuildConfig(new Dictionary<string, string?>
			{
				["Messaging:Marketing:PhoneNumber"] = "+15550001111",
				["Messaging:Marketing:Region"]      = "us-east-1",
			});

			var services = CreateServices();
			services.AddSingleton<IConfiguration>(config);

			services.AddMessaging()
				.AddConnector<TestConnector>("marketing", b => b
					.WithSettings("Messaging:Marketing"));

			var provider = services.BuildServiceProvider();
			var connector = (TestConnector)provider.GetRequiredKeyedService<IChannelConnector>("marketing");

			// Assert
			Assert.Equal("+15550001111", connector.ConnectionSettings.GetParameter("PhoneNumber"));
			Assert.Equal("us-east-1",   connector.ConnectionSettings.GetParameter("Region"));
		}

		[Fact]
		public void Should_PreferFluentOverConfigForNamedConnector_When_BothAreProvided()
		{
			// Arrange
			var config = BuildConfig(new Dictionary<string, string?>
			{
				["Messaging:Twilio:Marketing:PhoneNumber"] = "+1-config",
				["Messaging:Twilio:Marketing:Region"]      = "eu-west-1",
			});

			var services = CreateServices();
			services.AddSingleton<IConfiguration>(config);

			services.AddMessaging()
				.AddConnector<TestConnector>("marketing", b => b
					.WithSettings("Messaging:Twilio:Marketing")
					.WithSetting("PhoneNumber", "+1-fluent"));   // wins over config

			var provider = services.BuildServiceProvider();
			var connector = (TestConnector)provider.GetRequiredKeyedService<IChannelConnector>("marketing");

			// Assert
			Assert.Equal("+1-fluent",  connector.ConnectionSettings.GetParameter("PhoneNumber"));
			Assert.Equal("eu-west-1", connector.ConnectionSettings.GetParameter("Region"));
		}

		#endregion

		// ── Guard / validation ────────────────────────────────────────────────

		#region Guard conditions

		[Fact]
		public void Should_ThrowArgumentNullException_When_ConfigureActionIsNull()
		{
			// Arrange
			var services = CreateServices();
			var builder = services.AddMessaging();

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() =>
				builder.AddConnector<TestConnector>((Action<ChannelConnectorBuilder<TestConnector>>)null!));
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_NamedConfigureActionIsNull()
		{
			// Arrange
			var services = CreateServices();
			var builder = services.AddMessaging();

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() =>
				builder.AddConnector<TestConnector>("marketing", (Action<ChannelConnectorBuilder<TestConnector>>)null!));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_WithSettingKeyIsWhitespace()
		{
			// Arrange
			var services = CreateServices();

			// Act & Assert
			Assert.Throws<ArgumentException>(() =>
				services.AddMessaging().AddConnector<TestConnector>(b =>
					b.WithSetting("  ", "value")));
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_UseFactoryReceivesNull()
		{
			// Arrange
			var services = CreateServices();

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() =>
				services.AddMessaging().AddConnector<TestConnector>(b =>
					b.UseFactory(null!)));
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_WithSchemaReceivesNull()
		{
			// Arrange
			var services = CreateServices();

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() =>
				services.AddMessaging().AddConnector<TestConnector>(b =>
					b.WithSchema(null!)));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_WithSettingsConfigSectionIsWhitespace()
		{
			// Arrange
			var services = CreateServices();

			// Act & Assert
			Assert.Throws<ArgumentException>(() =>
				services.AddMessaging().AddConnector<TestConnector>(b =>
					b.WithSettings("  ")));
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_WithSettingsActionIsNull()
		{
			// Arrange
			var services = CreateServices();

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() =>
				services.AddMessaging().AddConnector<TestConnector>(b =>
					b.WithSettings((Action<IDictionary<string, object?>>)null!)));
		}

		#endregion

		// ── Test infrastructure ───────────────────────────────────────────────

		[ChannelSchema(typeof(TestSchemaFactory))]
		private sealed class TestConnector : IChannelConnector
		{
			public TestConnector(IChannelSchema schema, ConnectionSettings? settings = null)
			{
				Schema             = schema;
				ConnectionSettings = settings ?? new ConnectionSettings();
			}

			public IChannelSchema      Schema             { get; }
			public ConnectionSettings  ConnectionSettings { get; }
			public ConnectorState      State              => ConnectorState.Uninitialized;

			public Task<OperationResult<bool>>         InitializeAsync(CancellationToken ct)         => Task.FromResult(OperationResult<bool>.Success(true));
			public Task<OperationResult<bool>>         TestConnectionAsync(CancellationToken ct)     => Task.FromResult(OperationResult<bool>.Success(true));
			public Task<OperationResult<SendResult>>   SendMessageAsync(IMessage m, CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<BatchSendResult>> SendBatchAsync(IMessageBatch b, CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<StatusInfo>>   GetStatusAsync(CancellationToken ct)          => throw new NotImplementedException();
			public Task<OperationResult<StatusUpdatesResult>> GetMessageStatusAsync(string id, CancellationToken ct) => throw new NotImplementedException();
			public IAsyncEnumerable<ValidationResult>  ValidateMessageAsync(IMessage m, CancellationToken ct)        => throw new NotImplementedException();
			public Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource s, CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource s, CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<ConnectorHealth>> GetHealthAsync(CancellationToken ct)       => throw new NotImplementedException();
			public Task ShutdownAsync(CancellationToken ct) => Task.CompletedTask;
		}

		private sealed class TestSchemaFactory : IChannelSchemaFactory
		{
			public IChannelSchema CreateSchema() => new DummySchema("TestProvider", "TestType");
		}

		private sealed class DummySchema : IChannelSchema
		{
			public DummySchema(string channelProvider, string channelType)
			{
				ChannelProvider = channelProvider;
				ChannelType     = channelType;
			}
			public string ChannelProvider { get; }
			public string ChannelType     { get; }
			public string Version         => "1.0";
			public string? DisplayName    => null;
			public bool IsStrict          => false;
			public ChannelCapability Capabilities => ChannelCapability.SendMessages;
			public IReadOnlyList<ChannelEndpointConfiguration>  Endpoints              => [];
			public IReadOnlyList<ChannelParameter>             Parameters             => [];
			public IReadOnlyList<MessagePropertyConfiguration> MessageProperties      => [];
			public IReadOnlyList<MessageContentType>           ContentTypes           => [];
			public IReadOnlyList<AuthenticationConfiguration>  AuthenticationConfigurations => [];
		}
	}
}

