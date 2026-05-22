namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "MessageIdGeneratorExtensions")]
public class MessageIdGeneratorExtensionsTests {
	[Fact]
	public void Should_SetMessageId_When_EnsureMessageIdCalled() {
		var message = new Message();
		var generator = new DefaultMessageIdGenerator();

		var id = generator.EnsureMessageId(message);

		Assert.NotNull(id);
		Assert.NotEmpty(id);
		Assert.Equal(id, message.Id);
	}

	[Fact]
	public void Should_KeepExistingId_When_MessageAlreadyHasId() {
		var message = new Message { Id = "existing-id" };
		var generator = new DefaultMessageIdGenerator();

		var id = generator.EnsureMessageId(message);

		Assert.Equal("existing-id", id);
	}

	[Fact]
	public void Should_SetBatchId_When_EnsureBatchIdCalled() {
		var batch = new MessageBatch();
		var generator = new DefaultMessageIdGenerator();

		var id = generator.EnsureBatchId(batch);

		Assert.NotNull(id);
		Assert.NotEmpty(id);
		Assert.Equal(id, batch.Id);
	}

	[Fact]
	public void Should_ThrowArgumentNullException_When_GeneratorIsNull() {
		IMessageIdGenerator? generator = null;

		Assert.Throws<ArgumentNullException>(() => generator!.EnsureMessageId(new Message()));
	}

	[Fact]
	public void Should_ThrowArgumentNullException_When_MessageIsNull() {
		var generator = new DefaultMessageIdGenerator();

		Assert.Throws<ArgumentNullException>(() => generator.EnsureMessageId(null!));
	}
}
