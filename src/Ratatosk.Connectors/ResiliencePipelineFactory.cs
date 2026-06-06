using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Ratatosk
{
    internal static class ResiliencePipelineFactory
    {
        public static ResiliencePipeline<T>? BuildPipeline<T>(RetryPolicyOptions? options)
        {
            if (options == null || options.MaxRetryAttempts <= 1)
                return null;

            var builder = new ResiliencePipelineBuilder<T>();

            var retryOptions = new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = options.MaxRetryAttempts - 1,
                Delay = options.BaseDelay,
                BackoffType = MapBackoffType(options.BackoffType),
                UseJitter = options.UseJitter,
                ShouldHandle = args => ValueTask.FromResult(args.Outcome.Exception is ConnectorException ex &&
                    options.RetryableErrorCodes.Contains(ex.ErrorCode))
            };

            builder.AddRetry(retryOptions);

            if (options.EnableCircuitBreaker)
            {
                var breakerOptions = new CircuitBreakerStrategyOptions<T>
                {
                    FailureRatio = options.CircuitBreakerFailureRatio,
                    SamplingDuration = options.CircuitBreakerSamplingDuration,
                    MinimumThroughput = options.CircuitBreakerMinimumThroughput,
                    BreakDuration = options.CircuitBreakerBreakDuration,
                    ShouldHandle = args => ValueTask.FromResult(args.Outcome.Exception is ConnectorException ex &&
                        options.RetryableErrorCodes.Contains(ex.ErrorCode))
                };

                builder.AddCircuitBreaker(breakerOptions);
            }

            return builder.Build();
        }

        private static DelayBackoffType MapBackoffType(RetryBackoffType type)
        {
            return type switch
            {
                RetryBackoffType.Constant => DelayBackoffType.Constant,
                RetryBackoffType.Linear => DelayBackoffType.Linear,
                RetryBackoffType.Exponential => DelayBackoffType.Exponential,
                _ => DelayBackoffType.Exponential
            };
        }
    }
}
