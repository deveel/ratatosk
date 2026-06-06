namespace Ratatosk
{
    public static class RetrySettingsKeys
    {
        public const string MaxAttempts = "Retry.MaxAttempts";
        public const string BackoffType = "Retry.BackoffType";
        public const string BaseDelay = "Retry.BaseDelay";
        public const string UseJitter = "Retry.UseJitter";
        public const string RetryableErrorCodes = "Retry.RetryableErrorCodes";
        public const string EnableCircuitBreaker = "Retry.EnableCircuitBreaker";
        public const string CircuitBreakerFailureRatio = "Retry.CircuitBreaker.FailureRatio";
        public const string CircuitBreakerSamplingDuration = "Retry.CircuitBreaker.SamplingDuration";
        public const string CircuitBreakerMinimumThroughput = "Retry.CircuitBreaker.MinimumThroughput";
        public const string CircuitBreakerBreakDuration = "Retry.CircuitBreaker.BreakDuration";
    }
}
