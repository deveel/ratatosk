namespace Ratatosk
{
    public static class ResultRetryExtensions
    {
        public static int GetRetryAttempts(this SendResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            if (result.AdditionalData.TryGetValue(ResultMetadataKeys.RetryAttempts, out var value) && value is int attempts)
                return attempts;
            return 1;
        }

        public static int GetRetryAttempts(this StatusUpdateResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            if (result.AdditionalData.TryGetValue(ResultMetadataKeys.RetryAttempts, out var value) && value is int attempts)
                return attempts;
            return 1;
        }

        public static int GetRetryAttempts(this StatusInfo result)
        {
            if (result.AdditionalData.TryGetValue(ResultMetadataKeys.RetryAttempts, out var value) && value is int attempts)
                return attempts;
            return 1;
        }
    }
}
