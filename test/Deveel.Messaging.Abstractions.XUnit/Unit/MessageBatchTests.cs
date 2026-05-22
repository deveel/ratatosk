namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "MessageBatch")]
public class MessageBatchTests {
	[Fact]
	public void Should_BeEmpty_When_DefaultConstructor() {
		var batch = new MessageBatch();

		Assert.NotNull(batch.Messages);
		Assert.Empty(batch.Messages);
		Assert.Null(batch.Id);
		Assert.NotNull(batch.Properties);
		Assert.Empty(batch.Properties);
	}

	[Fact]
	public void Should_AddMessages_When_MessagesAddedToList() {
		var batch = new MessageBatch();
		batch.Messages.Add(new Message { Id = "msg-1" });
		batch.Messages.Add(new Message { Id = "msg-2" });

		Assert.Equal(2, batch.Messages.Count);
	}

	[Fact]
	public void Should_SetId_When_IdPropertySet() {
		var batch = new MessageBatch { Id = "batch-1" };

		Assert.Equal("batch-1", batch.Id);
	}

	[Fact]
	public void Should_SetProperties_When_PropertiesDictionaryAssigned() {
		var batch = new MessageBatch {
			Properties = new Dictionary<string, object> {
				["key"] = "value"
			}
		};

		Assert.NotNull(batch.Properties);
		Assert.Equal("value", batch.Properties["key"]);
	}

	[Fact]
	public void Should_SetProperties_When_AddProperty() {
		var batch = new MessageBatch();
		batch.Properties["key"] = "value";

		Assert.Equal("value", batch.Properties["key"]);
	}

	[Fact]
	public void Should_SupportIMessageBatchInterface() {
		IMessageBatch batch = new MessageBatch {
			Id = "batch-1",
			Messages = { new Message { Id = "msg-1" } },
			Properties = { ["k"] = "v" }
		};

		Assert.Equal("batch-1", batch.Id);
		Assert.Single(batch.Messages);
		Assert.Equal("v", batch.Properties["k"]);
	}
}
