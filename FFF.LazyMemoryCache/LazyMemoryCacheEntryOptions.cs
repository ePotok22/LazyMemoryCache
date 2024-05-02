using FFF.LazyMemoryCache.Enum;
using Microsoft.Extensions.Caching.Memory;

namespace FFF.LazyMemoryCache
{
    /// <summary>
    /// Extends MemoryCacheEntryOptions to include expiration modes and immediate expiration calculations.
    /// </summary>
    public class LazyMemoryCacheEntryOptions : MemoryCacheEntryOptions
    {
        /// <summary>
        /// Gets or sets the expiration mode for the cache entry.
        /// </summary>
        public ExpirationMode ExpirationMode { get; set; }

        /// <summary>
        /// Gets or sets the time span after which the cache entry will expire relative to now.
        /// This is used both for directly setting a relative expiration and calculating
        /// the delay from an absolute point in time.
        /// </summary>
        public TimeSpan ImmediateAbsoluteExpirationRelativeToNow { get; set; }

        /// <summary>
        /// Creates cache entry options with a specified absolute expiration time.
        /// </summary>
        /// <param name="absoluteExpiration">The absolute expiration time as a DateTimeOffset.</param>
        /// <returns>A configured LazyMemoryCacheEntryOptions object.</returns>
        public static LazyMemoryCacheEntryOptions WithImmediateAbsoluteExpiration(DateTimeOffset absoluteExpiration)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            TimeSpan delay = absoluteExpiration - now;
            return new LazyMemoryCacheEntryOptions
            {
                AbsoluteExpiration = absoluteExpiration,
                ExpirationMode = ExpirationMode.ImmediateEviction,
                ImmediateAbsoluteExpirationRelativeToNow = delay
            };
        }

        /// <summary>
        /// Creates cache entry options with a specified expiration duration relative to now.
        /// </summary>
        /// <param name="absoluteExpiration">The relative expiration time as a TimeSpan.</param>
        /// <returns>A configured LazyMemoryCacheEntryOptions object.</returns>
        public static LazyMemoryCacheEntryOptions WithImmediateAbsoluteExpiration(TimeSpan absoluteExpiration)
        {
            return new LazyMemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration,
                ExpirationMode = ExpirationMode.ImmediateEviction,
                ImmediateAbsoluteExpirationRelativeToNow = absoluteExpiration
            };
        }
    }
}
