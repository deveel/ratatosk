namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Feature", "RetryPolicy")]
public class RetryPolicyOptionsTests
{
    [Fact]
    public void Should_HaveDefaultValues_When_Created()
    {
        var options = new RetryPolicyOptions();

        Assert.Equal(3, options.MaxRetryAttempts);
        Assert.Equal(TimeSpan.FromSeconds(1), options.BaseDelay);
        Assert.Equal(RetryBackoffType.Exponential, options.BackoffType);
        Assert.True(options.UseJitter);
        Assert.Empty(options.RetryableErrorCodes);
        Assert.False(options.EnableCircuitBreaker);
        Assert.Equal(0.5, options.CircuitBreakerFailureRatio);
        Assert.Equal(TimeSpan.FromSeconds(30), options.CircuitBreakerSamplingDuration);
        Assert.Equal(10, options.CircuitBreakerMinimumThroughput);
        Assert.Equal(TimeSpan.FromSeconds(30), options.CircuitBreakerBreakDuration);
    }

    [Fact]
    public void Should_SetMaxAttempts_When_WithMaxAttemptsCalled()
    {
        var options = new RetryPolicyOptions().WithMaxAttempts(5);
        Assert.Equal(5, options.MaxRetryAttempts);
    }

    [Fact]
    public void Should_SetBaseDelay_When_WithBaseDelayCalled()
    {
        var options = new RetryPolicyOptions().WithBaseDelay(TimeSpan.FromSeconds(3));
        Assert.Equal(TimeSpan.FromSeconds(3), options.BaseDelay);
    }

    [Fact]
    public void Should_SetExponentialBackoff_When_WithExponentialBackoffCalled()
    {
        var options = new RetryPolicyOptions().WithExponentialBackoff();
        Assert.Equal(RetryBackoffType.Exponential, options.BackoffType);
    }

    [Fact]
    public void Should_SetLinearBackoff_When_WithLinearBackoffCalled()
    {
        var options = new RetryPolicyOptions().WithLinearBackoff();
        Assert.Equal(RetryBackoffType.Linear, options.BackoffType);
    }

    [Fact]
    public void Should_SetConstantBackoff_When_WithConstantBackoffCalled()
    {
        var options = new RetryPolicyOptions().WithConstantBackoff();
        Assert.Equal(RetryBackoffType.Constant, options.BackoffType);
    }

    [Fact]
    public void Should_DisableJitter_When_WithJitterFalse()
    {
        var options = new RetryPolicyOptions().WithJitter(false);
        Assert.False(options.UseJitter);
    }

    [Fact]
    public void Should_AddErrorCodes_When_RetryOnErrorCodesCalled()
    {
        var options = new RetryPolicyOptions()
            .RetryOnErrorCodes("RATE_LIMITED", "SERVICE_UNAVAILABLE");

        Assert.Contains("RATE_LIMITED", options.RetryableErrorCodes);
        Assert.Contains("SERVICE_UNAVAILABLE", options.RetryableErrorCodes);
    }

    [Fact]
    public void Should_EnableCircuitBreaker_When_WithCircuitBreakerCalled()
    {
        var options = new RetryPolicyOptions()
            .WithCircuitBreaker(0.3, TimeSpan.FromSeconds(60));

        Assert.True(options.EnableCircuitBreaker);
        Assert.Equal(0.3, options.CircuitBreakerFailureRatio);
        Assert.Equal(TimeSpan.FromSeconds(60), options.CircuitBreakerBreakDuration);
    }

    [Fact]
    public void Should_SetCircuitBreakerDetails_When_WithCircuitBreakerCalledWithOptionalParams()
    {
        var options = new RetryPolicyOptions()
            .WithCircuitBreaker(0.2, TimeSpan.FromSeconds(45), TimeSpan.FromSeconds(15), 5);

        Assert.Equal(0.2, options.CircuitBreakerFailureRatio);
        Assert.Equal(TimeSpan.FromSeconds(45), options.CircuitBreakerBreakDuration);
        Assert.Equal(TimeSpan.FromSeconds(15), options.CircuitBreakerSamplingDuration);
        Assert.Equal(5, options.CircuitBreakerMinimumThroughput);
    }

    [Fact]
    public void Should_NotDuplicateErrorCodes_When_RetryOnErrorCodesCalledMultipleTimes()
    {
        var options = new RetryPolicyOptions()
            .RetryOnErrorCodes("RATE_LIMITED")
            .RetryOnErrorCodes("RATE_LIMITED");

        Assert.Single(options.RetryableErrorCodes);
    }
}
