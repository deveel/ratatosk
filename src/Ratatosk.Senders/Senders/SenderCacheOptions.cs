namespace Ratatosk.Senders
{
    /// <summary>
    /// Configuration options for the distributed sender cache.
    /// </summary>
    public class SenderCacheOptions
    {
        /// <summary>
        /// Gets or sets the default time-to-live for cached sender entries.
        /// Defaults to 5 minutes.
        /// </summary>
        public TimeSpan DefaultTtl { get; set; } = TimeSpan.FromMinutes(5);
    }
}
