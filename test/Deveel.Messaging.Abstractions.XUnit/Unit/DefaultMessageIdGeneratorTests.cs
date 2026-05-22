namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "DefaultMessageIdGenerator")]
public class DefaultMessageIdGeneratorTests {
	[Fact]
	public void Should_GenerateNonEmptyId_When_GenerateMessageId() {
		var generator = new DefaultMessageIdGenerator();

		var id = generator.GenerateMessageId();

		Assert.NotNull(id);
		Assert.NotEmpty(id);
	}

	[Fact]
	public void Should_GenerateNonEmptyBatchId_When_GenerateBatchId() {
		var generator = new DefaultMessageIdGenerator();

		var id = generator.GenerateBatchId();

		Assert.NotNull(id);
		Assert.NotEmpty(id);
	}

	[Fact]
	public void Should_GenerateUniqueIds_When_GenerateMessageIdCalledMultipleTimes() {
		var generator = new DefaultMessageIdGenerator();

		var id1 = generator.GenerateMessageId();
		var id2 = generator.GenerateMessageId();

		Assert.NotEqual(id1, id2);
	}
}
