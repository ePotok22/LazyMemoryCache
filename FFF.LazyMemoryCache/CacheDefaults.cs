using Microsoft.Extensions.Caching.Memory;

namespace FFF.LazyMemoryCache
{
    /// <summary>
    /// Provides default settings and utility methods for caching operations. This class encapsulates
    /// the creation of cache entry options with predefined settings, allowing for consistent cache behavior
    /// across different parts of an application.
    /// </summary>
    public class CacheDefaults
    {
        /// <summary>
        /// Gets or sets the default cache duration. The duration is expressed as a TimeSpan,
        /// which allows for clear and flexible configuration of time intervals.
        /// </summary>
        /// <value>
        /// The default duration for cache entries, initially set to 20 minutes.
        /// </value>
        public virtual TimeSpan DefaultCacheDuration { get; set; } = TimeSpan.FromMinutes(20);

        /// <summary>
        /// Constructs and returns MemoryCacheEntryOptions configured with an absolute expiration time
        /// relative to now, based on the DefaultCacheDuration property. This method centralizes the
        /// configuration of cache entries, promoting consistent behavior and reducing redundancy.
        /// </summary>
        /// <returns>
        /// A new instance of MemoryCacheEntryOptions with an absolute expiration set to a time
        /// relative to the current moment, derived from the DefaultCacheDuration.
        /// </returns>
        internal MemoryCacheEntryOptions BuildCacheOptions()
        {
            return new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = DefaultCacheDuration
            };
        }
    }

}
