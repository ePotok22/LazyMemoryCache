using FFF.LazyMemoryCache.Enum;
using Microsoft.Extensions.Caching.Memory;
using System.Collections;

namespace FFF.LazyMemoryCache;

//public sealed class CachingService : ICache
//{
//    private readonly Lazy<ICacheProvider> cacheProvider;

//    private readonly int[] keyLocks;

//    public static Lazy<ICacheProvider> DefaultCacheProvider { get; set; }
//        = new Lazy<ICacheProvider>(() =>
//            new MemoryCacheProvider(
//                new MemoryCache(
//                    new MemoryCacheOptions())
//            ));

//    public CacheDefaults DefaultCachePolicy { get; set; } = new CacheDefaults();

//    public CachingService() : this(DefaultCacheProvider) { }

//    public CachingService(Lazy<ICacheProvider> cacheProvider)
//    {
//        this.cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
//        var lockCount = Math.Max(Environment.ProcessorCount * 8, 32);
//        keyLocks = new int[lockCount];
//    }

//    public CachingService(Func<ICacheProvider> cacheProviderFactory)
//    {
//        if (cacheProviderFactory == null) throw new ArgumentNullException(nameof(cacheProviderFactory));
//        cacheProvider = new Lazy<ICacheProvider>(cacheProviderFactory);
//        var lockCount = Math.Max(Environment.ProcessorCount * 8, 32);
//        keyLocks = new int[lockCount];

//    }

//    public CachingService(ICacheProvider cache) : this(() => cache)
//    {
//        if (cache == null)
//            throw new ArgumentNullException(nameof(cache));
//    }

//    public void Add<T>(string key, T item, MemoryCacheEntryOptions policy)
//    {
//        if (Equals(item, default(T)))
//            throw new ArgumentNullException(nameof(item));
//        ValidateKey(key);

//        CacheProvider.Set(key, item!, policy);
//    }

//    public IEnumerable<T> GetKeys<T>() =>
//        CacheProvider.GetKeys<T>();

//    public ICollection GetKeys() =>
//        CacheProvider.GetKeys();

//    public T? Get<T>(string key)
//    {
//        ValidateKey(key);

//        T? item = CacheProvider.Get<T>(key);

//        return GetValueFromLazy<T>(item!, out _);
//    }

//    public Task<T?> GetAsync<T>(string key)
//    {
//        ValidateKey(key);

//        T? item = CacheProvider.Get<T>(key);

//        return GetValueFromAsyncLazy<T>(item!, out _);
//    }

//    public bool TryGetValue<T>(string key, out T? value)
//    {
//        ValidateKey(key);

//        return CacheProvider.TryGetValue(key, out value);
//    }

//    public T? GetOrAdd<T>(string key, Func<ICacheEntry, T> addItemFactory) =>
//        GetOrAdd(key, addItemFactory, default);

//    public T? GetOrAdd<T>(string key, Func<ICacheEntry, T> addItemFactory, MemoryCacheEntryOptions? policy)
//    {
//        ValidateKey(key);

//        object? cacheItem;

//        object CacheFactory(ICacheEntry entry) =>
//            new Lazy<T>(() =>
//            {
//                var result = addItemFactory(entry);
//                SetAbsoluteExpirationFromRelative(entry);
//                EnsureEvictionCallbackDoesNotReturnTheAsyncOrLazy<T>(entry.PostEvictionCallbacks);
//                return result;
//            });

//        // acquire lock per key
//        uint hash = (uint)key.GetHashCode() % (uint)keyLocks.Length;
//        while (Interlocked.CompareExchange(ref keyLocks[hash], 1, 0) == 1) { Thread.Yield(); }

//        try
//        {
//            if (policy != null)
//                cacheItem = CacheProvider.GetOrCreate(key, CacheFactory, policy);
//            else
//                cacheItem = CacheProvider.GetOrCreate(key, CacheFactory);
//        }
//        finally
//        {
//            keyLocks[hash] = 0;
//        }

//        try
//        {
//            T? result = GetValueFromLazy<T>(cacheItem!, out bool valueHasChangedType);

//            // if we get a cache hit but for something with the wrong type we need to evict it, start again and cache the new item instead
//            if (valueHasChangedType)
//            {
//                CacheProvider.Remove(key);

//                // acquire lock again
//                hash = (uint)key.GetHashCode() % (uint)keyLocks.Length;
//                while (Interlocked.CompareExchange(ref keyLocks[hash], 1, 0) == 1) { Thread.Yield(); }

//                try
//                {
//                    cacheItem = CacheProvider.GetOrCreate(key, CacheFactory);
//                }
//                finally
//                {
//                    keyLocks[hash] = 0;
//                }
//                result = GetValueFromLazy<T>(cacheItem!, out _ /* we just evicted so type change cannot happen this time */);
//            }

//            return result;
//        }
//        catch //addItemFactory errored so do not cache the exception
//        {
//            CacheProvider.Remove(key);
//            throw;
//        }
//    }

//    public void Remove(string key)
//    {
//        ValidateKey(key);
//        CacheProvider.Remove(key);
//    }
//    public void Remove(string key, RemoveMode mode)
//    {
//        ValidateKey(key);

//        IEnumerable<string> keys = mode switch
//        {
//            RemoveMode.StartWith => GetKeys<string>().Where(_ => _.StartsWith(key)),
//            RemoveMode.StartWithIgnoreCase => GetKeys<string>().Where(_ => _.StartsWith(key, StringComparison.InvariantCultureIgnoreCase)),
//            RemoveMode.EndWith => GetKeys<string>().Where(_ => _.StartsWith(key)),
//            RemoveMode.EndWithIgnoreCase => GetKeys<string>().Where(_ => _.EndsWith(key, StringComparison.InvariantCultureIgnoreCase)),
//            RemoveMode.Equal => GetKeys<string>().Where(_ => _.StartsWith(key)),
//            RemoveMode.EqualIgnoreCase => GetKeys<string>().Where(_ => _.Equals(key, StringComparison.InvariantCultureIgnoreCase)),
//            RemoveMode.Contain => GetKeys<string>().Where(_ => _.Contains(key)),
//            _ => throw new NotImplementedException()
//        };
//        foreach (var item in keys)
//            CacheProvider.Remove(item);
//    }

//    public void Remove(string key, Func<string, bool> predicate)
//    {
//        ValidateKey(key);
//        foreach (var item in GetKeys<string>().Where(predicate))
//            CacheProvider.Remove(item);
//    }

//    public ICacheProvider CacheProvider =>
//        cacheProvider.Value;

//    public Task<T?> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> addItemFactory) =>
//         GetOrAddAsync(key, addItemFactory, default);

//    public async Task<T?> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> addItemFactory, MemoryCacheEntryOptions? policy)
//    {
//        ValidateKey(key);

//        object? cacheItem;

//        // Ensure only one thread can place an item into the cache provider at a time.
//        // We are not evaluating the addItemFactory inside here - that happens outside the lock,
//        // below, and guarded using the async lazy. Here we just ensure only one thread can place
//        // the AsyncLazy into the cache at one time

//        // acquire lock
//        uint hash = (uint)key.GetHashCode() % (uint)keyLocks.Length;
//        while (Interlocked.CompareExchange(ref keyLocks[hash], 1, 0) == 1) { Thread.Yield(); }

//        object CacheFactory(ICacheEntry entry) =>
//            new AsyncLazy<T>(async () =>
//            {
//                var result = await addItemFactory(entry).ConfigureAwait(false);
//                SetAbsoluteExpirationFromRelative(entry);
//                EnsureEvictionCallbackDoesNotReturnTheAsyncOrLazy<T>(entry.PostEvictionCallbacks);
//                return result;
//            });

//        try
//        {
//            if (policy != null)
//                cacheItem = CacheProvider.GetOrCreate(key, CacheFactory, policy);
//            else
//                cacheItem = CacheProvider.GetOrCreate(key, CacheFactory);
//        }
//        finally
//        {
//            keyLocks[hash] = 0;
//        }

//        try
//        {
//            Task<T?> result = GetValueFromAsyncLazy<T>(cacheItem, out var valueHasChangedType);

//            // if we get a cache hit but for something with the wrong type we need to evict it, start again and cache the new item instead
//            if (valueHasChangedType)
//            {
//                CacheProvider.Remove(key);

//                // acquire lock
//                hash = (uint)key.GetHashCode() % (uint)keyLocks.Length;
//                while (Interlocked.CompareExchange(ref keyLocks[hash], 1, 0) == 1) { Thread.Yield(); }

//                try
//                {
//                    cacheItem = CacheProvider.GetOrCreate(key, CacheFactory);
//                }
//                finally
//                {
//                    keyLocks[hash] = 0;
//                }
//                result = GetValueFromAsyncLazy<T>(cacheItem, out _ /* we just evicted so type change cannot happen this time */);
//            }

//            if (result.IsCanceled || result.IsFaulted)
//                CacheProvider.Remove(key);

//            return await result.ConfigureAwait(false);
//        }
//        catch //addItemFactory errored so do not cache the exception
//        {
//            CacheProvider.Remove(key);
//            throw;
//        }
//    }

//    private T? GetValueFromLazy<T>(object item, out bool valueHasChangedType)
//    {
//        valueHasChangedType = false;

//        switch (item)
//        {
//            case Lazy<T> lazy:
//                return lazy.Value;
//            case T variable:
//                return variable;
//            case AsyncLazy<T> asyncLazy:
//                // this is async to sync - and should not really happen as long as GetOrAddAsync is used for an async
//                // value. Only happens when you cache something async and then try and grab it again later using
//                // the non async methods.
//                return asyncLazy.Value.ConfigureAwait(false).GetAwaiter().GetResult();
//            case Task<T> task:
//                return task.Result;
//        }

//        // if they have cached something else with the same key we need to tell caller to reset the cached item
//        // although this is probably not the fastest this should not get called on the main use case
//        // where you just hit the first switch case above.
//        var itemsType = item?.GetType();
//        if (itemsType != null && itemsType.IsGenericType && itemsType.GetGenericTypeDefinition() == typeof(Lazy<>))
//        {
//            valueHasChangedType = true;
//        }

//        return default(T);
//    }

//    private Task<T?> GetValueFromAsyncLazy<T>(object? item, out bool valueHasChangedType)
//    {
//        valueHasChangedType = false;

//        switch (item)
//        {
//            case AsyncLazy<T?> asyncLazy:
//                return asyncLazy.Value;
//            case Task<T?> task:
//                return task;
//            // this is sync to async and only happens if you cache something sync and then get it later async
//            case Lazy<T?> lazy:
//                return Task.FromResult(lazy.Value);
//            case T variable:
//                return Task.FromResult<T?>(variable);
//        }

//        // if they have cached something else with the same key we need to tell caller to reset the cached item
//        // although this is probably not the fastest this should not get called on the main use case
//        // where you just hit the first switch case above.
//        var itemsType = item?.GetType();
//        if (itemsType != null && itemsType.IsGenericType && itemsType.GetGenericTypeDefinition() == typeof(AsyncLazy<>))
//            valueHasChangedType = true;

//        return Task.FromResult(default(T));
//    }

//    private void EnsureEvictionCallbackDoesNotReturnTheAsyncOrLazy<T>(IList<PostEvictionCallbackRegistration> callbackRegistrations)
//    {
//        if (callbackRegistrations != null)
//            foreach (var item in callbackRegistrations)
//            {
//                var originalCallback = item.EvictionCallback;
//                item.EvictionCallback = (key, value, reason, state) =>
//                {
//                    // before the original callback we need to unwrap the Lazy that holds the cache item
//                    if (value is AsyncLazy<T?> asyncCacheItem)
//                        value = asyncCacheItem.IsValueCreated ? asyncCacheItem.Value : Task.FromResult(default(T));
//                    else if (value is Lazy<T?> cacheItem)
//                        value = cacheItem.IsValueCreated ? cacheItem.Value : default(T);

//                    // pass the unwrapped cached value to the original callback
//                    originalCallback?.Invoke(key, value, reason, state);
//                };
//            }
//    }

//    private void ValidateKey(string key)
//    {
//        if (key == null)
//            throw new ArgumentNullException(nameof(key));

//        if (string.IsNullOrWhiteSpace(key))
//            throw new ArgumentOutOfRangeException(nameof(key), "Cache keys cannot be empty or whitespace");
//    }

//    private static void SetAbsoluteExpirationFromRelative(ICacheEntry entry)
//    {
//        if (!entry.AbsoluteExpirationRelativeToNow.HasValue) return;

//        var absoluteExpiration = DateTimeOffset.UtcNow + entry.AbsoluteExpirationRelativeToNow.Value;
//        if (!entry.AbsoluteExpiration.HasValue || absoluteExpiration < entry.AbsoluteExpiration)
//            entry.AbsoluteExpiration = absoluteExpiration;
//    }

//}

/// <summary>
/// Provides a caching service that utilizes an ICacheProvider to manage cached data.
/// This class supports various caching operations with concurrency control.
/// </summary>
public sealed class CachingService : ICache
{
    private readonly Lazy<ICacheProvider> _cacheProvider;
    private readonly int[] _keyLocks;

    /// <summary>
    /// Static default cache provider that initializes a MemoryCacheProvider with default options.
    /// </summary>
    public static Lazy<ICacheProvider> DefaultCacheProvider { get; set; }
        = new Lazy<ICacheProvider>(() => new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())));

    /// <summary>
    /// Default caching policy settings applied to items stored in the cache.
    /// </summary>
    public CacheDefaults DefaultCachePolicy { get; set; } = new CacheDefaults();

    public ICacheProvider CacheProvider =>
        _cacheProvider.Value;

    /// <summary>
    /// Initializes a new instance of the CachingService using the default cache provider.
    /// </summary>
    public CachingService() : this(DefaultCacheProvider) { }

    /// <summary>
    /// Initializes a new instance of the CachingService with a specific cache provider.
    /// </summary>
    /// <param name="cacheProvider">The cache provider to use for caching operations.</param>
    public CachingService(Lazy<ICacheProvider> cacheProvider)
    {
        _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
        var lockCount = Math.Max(Environment.ProcessorCount * 8, 32);
        _keyLocks = new int[lockCount];
    }

    /// <summary>
    /// Initializes a new instance of the CachingService with a factory function to create the cache provider.
    /// </summary>
    /// <param name="cacheProviderFactory">A factory function to create the cache provider.</param>
    public CachingService(Func<ICacheProvider> cacheProviderFactory)
    {
        if (cacheProviderFactory == null) throw new ArgumentNullException(nameof(cacheProviderFactory));
        _cacheProvider = new Lazy<ICacheProvider>(cacheProviderFactory);
        var lockCount = Math.Max(Environment.ProcessorCount * 8, 32);
        _keyLocks = new int[lockCount];
    }

    /// <summary>
    /// Initializes a new instance of the CachingService with a given ICacheProvider.
    /// </summary>
    /// <param name="cache">The ICacheProvider instance to use.</param>
    public CachingService(ICacheProvider cache) : this(() => cache)
    {
        if (cache == null)
            throw new ArgumentNullException(nameof(cache));
    }

    /// <summary>
    /// Adds an item to the cache with an optional cache entry policy.
    /// </summary>
    /// <typeparam name="T">The type of the item to add to the cache.</typeparam>
    /// <param name="key">The key associated with the cached item.</param>
    /// <param name="item">The item to add to the cache.</param>
    /// <param name="policy">The policy under which the item should be cached.</param>
    public void Add<T>(string key, T item, MemoryCacheEntryOptions policy)
    {
        if (Equals(item, default(T)))
            throw new ArgumentNullException(nameof(item));
        ValidateKey(key);

        CacheProvider.Set(key, item!, policy);
    }

    /// <summary>
    /// Retrieves all keys of a specified type from the cache.
    /// </summary>
    /// <typeparam name="T">The type of the keys to retrieve.</typeparam>
    /// <returns>An enumerable of all keys of type T in the cache.</returns>
    public IEnumerable<T> GetKeys<T>() =>
        CacheProvider.GetKeys<T>();

    /// <summary>
    /// Retrieves all keys from the cache.
    /// </summary>
    /// <returns>An ICollection of all keys in the cache.</returns>
    public ICollection GetKeys() =>
        CacheProvider.GetKeys();

    /// <summary>
    /// Attempts to retrieve an item from the cache.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve.</typeparam>
    /// <param name="key">The key associated with the cached item.</param>
    /// <returns>The cached item if found; otherwise, null.</returns>
    public T? Get<T>(string key)
    {
        ValidateKey(key);

        T? item = CacheProvider.Get<T>(key);

        return GetValueFromLazy<T>(item!, out _);
    }

    /// <summary>
    /// Asynchronously retrieves an item from the cache.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve.</typeparam>
    /// <param name="key">The key associated with the cached item.</param>
    /// <returns>A task that, when completed, will return the cached item if found; otherwise, null.</returns>
    public Task<T?> GetAsync<T>(string key)
    {
        ValidateKey(key);

        T? item = CacheProvider.Get<T>(key);

        return GetValueFromAsyncLazy<T>(item!, out _);
    }

    /// <summary>
    /// Attempts to retrieve a value from the cache and indicates whether the retrieval was successful.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key associated with the cached item.</param>
    /// <param name="value">When this method returns, contains the cached item associated with the specified key, if found.</param>
    /// <returns>True if an item with the specified key was found in the cache; otherwise, false.</returns>
    public bool TryGetValue<T>(string key, out T? value)
    {
        ValidateKey(key);

        return CacheProvider.TryGetValue(key, out value);
    }

    /// <summary>
    /// Retrieves an item from the cache by its key or adds it if it does not exist.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve or add.</typeparam>
    /// <param name="key">The key of the cache item.</param>
    /// <param name="addItemFactory">A delegate that defines the method to create the item.</param>
    /// <returns>The cached or newly added item.</returns>
    public T? GetOrAdd<T>(string key, Func<ICacheEntry, T> addItemFactory) =>
        GetOrAdd(key, addItemFactory, default);

    /// <summary>
    /// Retrieves an item from the cache by its key or adds it if it does not exist, using the specified caching policy.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve or add.</typeparam>
    /// <param name="key">The key of the cache item.</param>
    /// <param name="addItemFactory">A delegate that defines the method to create the item.</param>
    /// <param name="policy">The policy under which the item should be cached.</param>
    /// <returns>The cached or newly added item.</returns>
    public T? GetOrAdd<T>(string key, Func<ICacheEntry, T> addItemFactory, MemoryCacheEntryOptions? policy)
    {
        ValidateKey(key);

        object? cacheItem;

        object CacheFactory(ICacheEntry entry) =>
            new Lazy<T>(() =>
            {
                var result = addItemFactory(entry);
                SetAbsoluteExpirationFromRelative(entry);
                EnsureEvictionCallbackDoesNotReturnTheAsyncOrLazy<T>(entry.PostEvictionCallbacks);
                return result;
            });

        // Acquire lock per key
        uint hash = (uint)key.GetHashCode() % (uint)_keyLocks.Length;
        while (Interlocked.CompareExchange(ref _keyLocks[hash], 1, 0) == 1) { Thread.Yield(); }

        try
        {
            if (policy != null)
                cacheItem = CacheProvider.GetOrCreate(key, CacheFactory, policy);
            else
                cacheItem = CacheProvider.GetOrCreate(key, CacheFactory);
        }
        finally
        {
            _keyLocks[hash] = 0;
        }

        try
        {
            T? result = GetValueFromLazy<T>(cacheItem!, out bool valueHasChangedType);

            // If we get a cache hit but for something with the wrong type we need to evict it, start again and cache the new item instead
            if (valueHasChangedType)
            {
                CacheProvider.Remove(key);

                // Acquire lock again
                hash = (uint)key.GetHashCode() % (uint)_keyLocks.Length;
                while (Interlocked.CompareExchange(ref _keyLocks[hash], 1, 0) == 1) { Thread.Yield(); }

                try
                {
                    cacheItem = CacheProvider.GetOrCreate(key, CacheFactory);
                }
                finally
                {
                    _keyLocks[hash] = 0;
                }
                result = GetValueFromLazy<T>(cacheItem!, out _ /* we just evicted so type change cannot happen this time */);
            }

            return result;
        }
        catch // addItemFactory errored so do not cache the exception
        {
            CacheProvider.Remove(key);
            throw;
        }
    }

    /// <summary>
    /// Removes an item from the cache.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    public void Remove(string key)
    {
        ValidateKey(key);
        CacheProvider.Remove(key);
    }

    /// <summary>
    /// Removes items from the cache based on a specific mode such as start with, end with, etc.
    /// </summary>
    /// <param name="key">The key or part of the key to match for removal.</param>
    /// <param name="mode">The mode that defines how keys are matched for removal.</param>
    public void Remove(string key, RemoveMode mode)
    {
        ValidateKey(key);

        IEnumerable<string> keys = mode switch
        {
            RemoveMode.StartWith => GetKeys<string>().Where(_ => _.StartsWith(key)),
            RemoveMode.StartWithIgnoreCase => GetKeys<string>().Where(_ => _.StartsWith(key, StringComparison.InvariantCultureIgnoreCase)),
            RemoveMode.EndWith => GetKeys<string>().Where(_ => _.StartsWith(key)),
            RemoveMode.EndWithIgnoreCase => GetKeys<string>().Where(_ => _.EndsWith(key, StringComparison.InvariantCultureIgnoreCase)),
            RemoveMode.Equal => GetKeys<string>().Where(_ => _.StartsWith(key)),
            RemoveMode.EqualIgnoreCase => GetKeys<string>().Where(_ => _.Equals(key, StringComparison.InvariantCultureIgnoreCase)),
            RemoveMode.Contain => GetKeys<string>().Where(_ => _.Contains(key)),
            _ => throw new NotImplementedException()
        };
        foreach (var item in keys)
            CacheProvider.Remove(item);
    }

    /// <summary>
    /// Removes items from the cache that match a given predicate.
    /// </summary>
    /// <param name="key">The key or part of the key to match for removal.</param>
    /// <param name="predicate">A function to test each key for a condition for removal.</param>
    public void Remove(string key, Func<string, bool> predicate)
    {
        ValidateKey(key);
        foreach (var item in GetKeys<string>().Where(predicate))
            CacheProvider.Remove(item);
    }

    /// <summary>
    /// Asynchronously retrieves an item from the cache by its key or adds it if it does not exist.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve or add.</typeparam>
    /// <param name="key">The key of the cache item.</param>
    /// <param name="addItemFactory">A delegate that defines the method to create the item.</param>
    /// <returns>A task that, when completed, will return the cached or newly added item.</returns>
    public Task<T?> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> addItemFactory) =>
        GetOrAddAsync(key, addItemFactory, default);

    /// <summary>
    /// Asynchronously retrieves an item from the cache by its key or adds it if it does not exist, using the specified caching policy.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve or add.</typeparam>
    /// <param name="key">The key of the cache item.</param>
    /// <param name="addItemFactory">A delegate that defines the method to create the item.</param>
    /// <param name="policy">The policy under which the item should be cached.</param>
    /// <returns>A task that, when completed, will return the cached or newly added item.</returns>
    public async Task<T?> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> addItemFactory, MemoryCacheEntryOptions? policy)
    {
        ValidateKey(key);

        object? cacheItem;

        // Ensure only one thread can place an item into the cache provider at a time.
        // We are not evaluating the addItemFactory inside here - that happens outside the lock,
        // below, and guarded using the async lazy. Here we just ensure only one thread can place
        // the AsyncLazy into the cache at one time

        // Acquire lock
        uint hash = (uint)key.GetHashCode() % (uint)_keyLocks.Length;
        while (Interlocked.CompareExchange(ref _keyLocks[hash], 1, 0) == 1) { Thread.Yield(); }

        object CacheFactory(ICacheEntry entry) =>
            new AsyncLazy<T>(async () =>
            {
                var result = await addItemFactory(entry).ConfigureAwait(false);
                SetAbsoluteExpirationFromRelative(entry);
                EnsureEvictionCallbackDoesNotReturnTheAsyncOrLazy<T>(entry.PostEvictionCallbacks);
                return result;
            });

        try
        {
            if (policy != null)
                cacheItem = CacheProvider.GetOrCreate(key, CacheFactory, policy);
            else
                cacheItem = CacheProvider.GetOrCreate(key, CacheFactory);
        }
        finally
        {
            _keyLocks[hash] = 0;
        }

        try
        {
            Task<T?> result = GetValueFromAsyncLazy<T>(cacheItem, out var valueHasChangedType);

            // if we get a cache hit but for something with the wrong type we need to evict it, start again and cache the new item instead
            if (valueHasChangedType)
            {
                CacheProvider.Remove(key);

                // acquire lock
                hash = (uint)key.GetHashCode() % (uint)_keyLocks.Length;
                while (Interlocked.CompareExchange(ref _keyLocks[hash], 1, 0) == 1) { Thread.Yield(); }

                try
                {
                    cacheItem = CacheProvider.GetOrCreate(key, CacheFactory);
                }
                finally
                {
                    _keyLocks[hash] = 0;
                }
                result = GetValueFromAsyncLazy<T>(cacheItem, out _ /* we just evicted so type change cannot happen this time */);
            }

            if (result.IsCanceled || result.IsFaulted)
                CacheProvider.Remove(key);

            return await result.ConfigureAwait(false);
        }
        catch // addItemFactory errored so do not cache the exception
        {
            CacheProvider.Remove(key);
            throw;
        }
    }

    /// <summary>
    /// Extracts a value from an object that might be a direct value, a Lazy, or an AsyncLazy.
    /// </summary>
    /// <typeparam name="T">The type of the value to extract.</typeparam>
    /// <param name="item">The item that might be Lazy or AsyncLazy wrapped.</param>
    /// <param name="valueHasChangedType">Outputs whether the item was of an unexpected type.</param>
    /// <returns>A task containing the value, or the default value if extraction failed.</returns>
    private Task<T?> GetValueFromAsyncLazy<T>(object? item, out bool valueHasChangedType)
    {
        valueHasChangedType = false;

        switch (item)
        {
            case AsyncLazy<T?> asyncLazy:
                return asyncLazy.Value;
            case Task<T?> task:
                return task;
            case Lazy<T?> lazy:
                return Task.FromResult(lazy.Value);
            case T value:
                return Task.FromResult<T?>(value);
        }

        // Check if the cached item is a different type of AsyncLazy
        if (item?.GetType() is Type itemType && itemType.IsGenericType &&
            itemType.GetGenericTypeDefinition() == typeof(AsyncLazy<>))
        {
            valueHasChangedType = true;
        }

        return Task.FromResult(default(T));
    }

    /// <summary>
    /// Extracts a value from an object that might be wrapped in a Lazy<T> or AsyncLazy<T>.
    /// </summary>
    /// <typeparam name="T">The type of the value to extract.</typeparam>
    /// <param name="item">The object that may contain the value wrapped.</param>
    /// <param name="valueHasChangedType">Outputs whether the item was of an unexpected type.</param>
    /// <returns>The extracted value, or the default value if the type does not match.</returns>
    private T? GetValueFromLazy<T>(object item, out bool valueHasChangedType)
    {
        valueHasChangedType = false;

        switch (item)
        {
            case Lazy<T> lazy:
                return lazy.Value;
            case T value:
                return value;
            case AsyncLazy<T> asyncLazy:
                return asyncLazy.Value.GetAwaiter().GetResult(); // Note: this can block!
            case Task<T> task:
                return task.Result; // Note: this can block!
        }

        // Check if the cached item is a different type of Lazy
        if (item?.GetType() is Type itemType && itemType.IsGenericType &&
            itemType.GetGenericTypeDefinition() == typeof(Lazy<>))
        {
            valueHasChangedType = true;
        }

        return default;
    }

    /// <summary>
    /// Validates the given cache key for null or whitespace and throws an appropriate exception if invalid.
    /// </summary>
    /// <param name="key">The cache key to validate.</param>
    private void ValidateKey(string key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentOutOfRangeException(nameof(key), "Cache keys cannot be empty or whitespace.");
    }

    /// <summary>
    /// Converts a relative expiration time to an absolute expiration time and sets it on the cache entry.
    /// </summary>
    /// <param name="entry">The cache entry to modify.</param>
    private static void SetAbsoluteExpirationFromRelative(ICacheEntry entry)
    {
        if (!entry.AbsoluteExpirationRelativeToNow.HasValue) return;

        var absoluteExpiration = DateTimeOffset.UtcNow + entry.AbsoluteExpirationRelativeToNow.Value;
        if (!entry.AbsoluteExpiration.HasValue || absoluteExpiration < entry.AbsoluteExpiration)
            entry.AbsoluteExpiration = absoluteExpiration;
    }

    /// <summary>
    /// Modifies eviction callbacks to unwrap Lazy and AsyncLazy objects before invoking the original eviction callback.
    /// This prevents callbacks from mistakenly operating on wrapped objects.
    /// </summary>
    /// <typeparam name="T">The type of the cached item.</typeparam>
    /// <param name="callbackRegistrations">The list of callback registrations to modify.</param>
    private void EnsureEvictionCallbackDoesNotReturnTheAsyncOrLazy<T>(IList<PostEvictionCallbackRegistration> callbackRegistrations)
    {
        if (callbackRegistrations != null)
            foreach (var item in callbackRegistrations)
            {
                var originalCallback = item.EvictionCallback;
                item.EvictionCallback = (key, value, reason, state) =>
                {
                    // before the original callback we need to unwrap the Lazy that holds the cache item
                    if (value is AsyncLazy<T?> asyncCacheItem)
                        value = asyncCacheItem.IsValueCreated ? asyncCacheItem.Value : Task.FromResult(default(T));
                    else if (value is Lazy<T?> cacheItem)
                        value = cacheItem.IsValueCreated ? cacheItem.Value : default(T);

                    // pass the unwrapped cached value to the original callback
                    originalCallback?.Invoke(key, value, reason, state);
                };
            }
    }
}