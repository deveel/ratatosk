namespace Ratatosk
{
    public enum RetryBackoffType
    {
        Constant,
        Linear,
        Exponential
    }

    public class RetryPolicyOptions
    {
        public int MaxRetryAttempts { get; set; } = 3;

        public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(1);

        public RetryBackoffType BackoffType { get; set; } = RetryBackoffType.Exponential;

        public bool UseJitter { get; set; } = true;

        public IList<string> RetryableErrorCodes { get; set; } = new List<string>();

        public bool EnableCircuitBreaker { get; set; }

        public double CircuitBreakerFailureRatio { get; set; } = 0.5;

        public TimeSpan CircuitBreakerSamplingDuration { get; set; } = TimeSpan.FromSeconds(30);

        public int CircuitBreakerMinimumThroughput { get; set; } = 10;

        public TimeSpan CircuitBreakerBreakDuration { get; set; } = TimeSpan.FromSeconds(30);

        public RetryPolicyOptions WithMaxAttempts(int maxAttempts)
        {
            MaxRetryAttempts = maxAttempts;
            return this;
        }

        public RetryPolicyOptions WithBaseDelay(TimeSpan delay)
        {
            BaseDelay = delay;
            return this;
        }

        public RetryPolicyOptions WithExponentialBackoff()
        {
            BackoffType = RetryBackoffType.Exponential;
            return this;
        }

        public RetryPolicyOptions WithLinearBackoff()
        {
            BackoffType = RetryBackoffType.Linear;
            return this;
        }

        public RetryPolicyOptions WithConstantBackoff()
        {
            BackoffType = RetryBackoffType.Constant;
            return this;
        }

        public RetryPolicyOptions WithJitter(bool useJitter = true)
        {
            UseJitter = useJitter;
            return this;
        }

        public RetryPolicyOptions RetryOnErrorCodes(params string[] errorCodes)
        {
            foreach (var code in errorCodes)
            {
                if (!RetryableErrorCodes.Contains(code))
                    RetryableErrorCodes.Add(code);
            }
            return this;
        }

        public RetryPolicyOptions WithCircuitBreaker(double failureRatio, TimeSpan breakDuration, TimeSpan? samplingDuration = null, int? minimumThroughput = null)
        {
            EnableCircuitBreaker = true;
            CircuitBreakerFailureRatio = failureRatio;
            CircuitBreakerBreakDuration = breakDuration;
            if (samplingDuration.HasValue)
                CircuitBreakerSamplingDuration = samplingDuration.Value;
            if (minimumThroughput.HasValue)
                CircuitBreakerMinimumThroughput = minimumThroughput.Value;
            return this;
        }
    }
}
