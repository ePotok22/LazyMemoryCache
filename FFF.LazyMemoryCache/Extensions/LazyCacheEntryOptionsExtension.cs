using FFF.LazyMemoryCache.Enum;

namespace FFF.LazyMemoryCache;

/// <summary>
/// Provides extension methods for the LazyMemoryCacheEntryOptions class to set absolute expiration details
/// for cache entries. These methods enhance the functionality of LazyMemoryCacheEntryOptions by allowing
/// more flexible setting of expiration times, either as a specific point in time or relative to the current time.
/// </summary>
public static class LazyCacheEntryOptionsExtension
{
    /// <summary>
    /// Sets the absolute expiration of the cache entry options to a specific point in time.
    /// This method configures the cache entry to expire at the provided absolute expiration date and time.
    /// </summary>
    /// <param name="option">The cache entry options to modify.</param>
    /// <param name="absoluteExpiration">The exact date and time when the cache entry should expire.</param>
    /// <param name="mode">The mode of expiration to apply, influencing how the cache behaves when the entry expires.</param>
    /// <returns>The updated LazyMemoryCacheEntryOptions with the set expiration configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provided options are null.</exception>
    public static LazyMemoryCacheEntryOptions SetAbsoluteExpiration(this LazyMemoryCacheEntryOptions option, DateTimeOffset absoluteExpiration,
        ExpirationMode mode)
    {
        if (option == null) throw new ArgumentNullException(nameof(option), "Cache entry options cannot be null.");

        TimeSpan delay = absoluteExpiration - DateTimeOffset.UtcNow;
        option.AbsoluteExpiration = absoluteExpiration;
        option.ExpirationMode = mode;
        option.ImmediateAbsoluteExpirationRelativeToNow = delay;
        return option;
    }

    /// <summary>
    /// Sets the absolute expiration of the cache entry options relative to the current time.
    /// This method allows setting a timespan after which the cache entry will expire, starting from the moment
    /// this method is called.
    /// </summary>
    /// <param name="option">The cache entry options to modify.</param>
    /// <param name="absoluteExpiration">The duration after which the cache entry should expire, starting from now.</param>
    /// <param name="mode">The mode of expiration to apply, influencing how the cache behaves when the entry expires.</param>
    /// <returns>The updated LazyMemoryCacheEntryOptions with the set expiration configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provided options are null.</exception>
    public static LazyMemoryCacheEntryOptions SetAbsoluteExpiration(this LazyMemoryCacheEntryOptions option, TimeSpan absoluteExpiration,
        ExpirationMode mode)
    {
        if (option is null) throw new ArgumentNullException(nameof(option), "Cache entry options cannot be null.");

        option.AbsoluteExpirationRelativeToNow = absoluteExpiration;
        option.ExpirationMode = mode;
        option.ImmediateAbsoluteExpirationRelativeToNow = absoluteExpiration;
        return option;
    }
}
