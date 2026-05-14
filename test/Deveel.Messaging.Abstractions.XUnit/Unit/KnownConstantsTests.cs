namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "KnownConstants")]
public class KnownMessagePropertiesTests
{
    [Fact]
    public void Should_HaveCorrectValue_When_KnownMessagePropertiesSubject()
    {
        // Act
        // Assert
        Assert.Equal("subject", KnownMessageProperties.Subject);
    }

    [Fact]
    public void Should_HaveCorrectValue_When_KnownMessagePropertiesRemoteMessageId()
    {
        // Act
        // Assert
        Assert.Equal("remoteMessageId", KnownMessageProperties.RemoteMessageId);
    }

    [Fact]
    public void Should_HaveCorrectValue_When_KnownMessagePropertiesReplyTo()
    {
        // Act
        // Assert
        Assert.Equal("replyTo", KnownMessageProperties.ReplyTo);
    }

    [Fact]
    public void Should_HaveCorrectValue_When_KnownMessagePropertiesCorrelationId()
    {
        // Act
        // Assert
        Assert.Equal("correlationId", KnownMessageProperties.CorrelationId);
    }

    [Fact]
    public void Should_BeDistinct_When_KnownMessagePropertiesAllValues()
    {
        // Arrange
        var values = new[]
        {
            KnownMessageProperties.Subject,
            KnownMessageProperties.RemoteMessageId,
            KnownMessageProperties.ReplyTo,
            KnownMessageProperties.CorrelationId
        };

        // Act
        var distinctValues = values.Distinct().ToList();

        // Assert
        Assert.Equal(values.Length, distinctValues.Count);
    }

    [Fact]
    public void Should_BeNotNullOrEmpty_When_KnownMessagePropertiesAllValues()
    {
        // Arrange
        var values = new[]
        {
            KnownMessageProperties.Subject,
            KnownMessageProperties.RemoteMessageId,
            KnownMessageProperties.ReplyTo,
            KnownMessageProperties.CorrelationId
        };

        // Act
        // Assert
        foreach (var value in values)
        {
            Assert.False(string.IsNullOrEmpty(value), $"Message property value should not be null or empty: {value}");
            Assert.False(string.IsNullOrWhiteSpace(value), $"Message property value should not be whitespace: {value}");
        }
    }

    [Fact]
    public void Should_InDictionary_When_KnownMessagePropertiesCanBeUsedAsKeys()
    {
        // Arrange
        var properties = new Dictionary<string, object>();

        // Act
        properties[KnownMessageProperties.Subject] = "Test Subject";
        properties[KnownMessageProperties.RemoteMessageId] = "remote-123";
        properties[KnownMessageProperties.ReplyTo] = "original-456";
        properties[KnownMessageProperties.CorrelationId] = "correlation-789";

        // Assert
        Assert.Equal("Test Subject", properties[KnownMessageProperties.Subject]);
        Assert.Equal("remote-123", properties[KnownMessageProperties.RemoteMessageId]);
        Assert.Equal("original-456", properties[KnownMessageProperties.ReplyTo]);
        Assert.Equal("correlation-789", properties[KnownMessageProperties.CorrelationId]);
    }
}