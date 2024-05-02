
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace FFF.LazyMemoryCache.Tests;

[TestFixture]
public class ICacheProviderTests
{
    private Mock<ICacheProvider> _mockCacheProvider;
    private MemoryCacheEntryOptions _cacheEntryOptions;

    [SetUp]
    public void Setup()
    {
        _mockCacheProvider = new Mock<ICacheProvider>();
        _cacheEntryOptions = new MemoryCacheEntryOptions();
    }

    [Test]
    public void GetKeys_GenericType_ShouldCallGetKeys()
    {
        // Arrange
        _mockCacheProvider.Setup(x => x.GetKeys<string>())
                          .Returns(new List<string> { "key1", "key2" });

        // Act
        var keys = _mockCacheProvider.Object.GetKeys<string>();

        // Assert
        Assert.That(keys, Is.Not.Null);
        Assert.That(((List<string>)keys).Count, Is.EqualTo(2));
    }

    [Test]
    public void TryGetValue_ExistingKey_ShouldReturnTrueAndItem()
    {
        // Arrange
        var key = "existingKey";
        var expectedItem = "value";

        _mockCacheProvider.Setup(x => x.TryGetValue(key, out expectedItem))
                          .Returns(true);

        // Act
        var result = _mockCacheProvider.Object.TryGetValue(key, out string? value);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(value, Is.EqualTo(expectedItem));
    }

    [Test]
    public void Get_NonExistingKey_ShouldReturnNull()
    {
        // Arrange
        var key = "nonExistingKey";
        string? expectedItem = null;

        _mockCacheProvider.Setup(x => x.Get<string>(key)).Returns(expectedItem);

        // Act
        var result = _mockCacheProvider.Object.Get<string>(key);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Set_ItemToCache_ShouldCallSet()
    {
        // Arrange
        var key = "key";
        var value = "value";

        _mockCacheProvider.Setup(x => x.Set(key, value, _cacheEntryOptions));

        // Act
        _mockCacheProvider.Object.Set(key, value, _cacheEntryOptions);

        // Assert
        _mockCacheProvider.Verify(x => x.Set(key, value, _cacheEntryOptions), Times.Once);
    }

    [Test]
    public void Remove_ExistingKey_ShouldCallRemove()
    {
        // Arrange
        var key = "existingKey";

        _mockCacheProvider.Setup(x => x.Remove(key));

        // Act
        _mockCacheProvider.Object.Remove(key);

        // Assert
        _mockCacheProvider.Verify(x => x.Remove(key), Times.Once);
    }

    [Test]
    public void GetKeys_NonGenericType_ShouldCallGetKeys()
    {
        // Arrange
        _mockCacheProvider.Setup(x => x.GetKeys())
                          .Returns(new List<string> { "key1", "key2" });

        // Act
        var keys = _mockCacheProvider.Object.GetKeys();

        // Assert
        Assert.That(keys, Is.Not.Null);
        Assert.That(((List<string>)keys).Count, Is.EqualTo(2));
    }

    [Test]
    public void TryGetValue_NonExistingKey_ShouldReturnFalseAndDefaultItem()
    {
        // Arrange
        var key = "nonExistingKey";
        string? expectedItem = null;

        _mockCacheProvider.Setup(x => x.TryGetValue(key, out expectedItem))
                          .Returns(false);

        // Act
        var result = _mockCacheProvider.Object.TryGetValue(key, out string? value);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(value, Is.Null);
    }

    [Test]
    public void GetOrCreate_CacheItemExists_ShouldReturnCachedItem()
    {
        // Arrange
        var key = "existingKey";
        var expectedItem = "cachedValue";

        _mockCacheProvider.Setup(x => x.GetOrCreate(key, It.IsAny<Func<ICacheEntry, string>>(), null))
                          .Returns(expectedItem);

        // Act
        var result = _mockCacheProvider.Object.GetOrCreate(key, entry => expectedItem);

        // Assert
        Assert.That(result, Is.EqualTo(expectedItem));
    }

    [Test]
    public void GetOrCreate_CacheItemDoesNotExist_ShouldCreateAndReturnNewItem()
    {
        // Arrange
        var key = "nonExistingKey";
        var newItem = "newValue";

        _mockCacheProvider.Setup(x => x.GetOrCreate(key, It.IsAny<Func<ICacheEntry, string>>(), null))
                          .Returns(newItem);

        // Act
        var result = _mockCacheProvider.Object.GetOrCreate(key, entry => newItem);

        // Assert
        Assert.That(result, Is.EqualTo(newItem));
    }

    [Test]
    public void Remove_NonExistingKey_ShouldNotCallRemove()
    {
        // Arrange
        var key = "nonExistingKey";

        // Act
        _mockCacheProvider.Object.Remove(key);

        // Assert
        _mockCacheProvider.Verify(x => x.Remove(key), Times.Once);
    }

    [Test]
    public void Set_ItemToCache_WithPolicy_ShouldCallSet()
    {
        // Arrange
        var key = "key";
        var value = "value";
        var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10));

        _mockCacheProvider.Setup(x => x.Set(key, value, cacheEntryOptions));

        // Act
        _mockCacheProvider.Object.Set(key, value, cacheEntryOptions);

        // Assert
        _mockCacheProvider.Verify(x => x.Set(key, value, cacheEntryOptions), Times.Once);
    }

    [Test]
    public async Task GetOrCreate_ThreadSafety_ShouldReturnSameValue()
    {
        // Arrange
        var key = "key";
        var initialValue = "initialValue";
        var newValue = "newValue";

        _mockCacheProvider.SetupSequence(x => x.GetOrCreate(key, It.IsAny<Func<ICacheEntry, string>>(), null))
                          .Returns(newValue)
                          .Returns(initialValue);

        // Act
        var task1 = await Task.Run(() => _mockCacheProvider.Object.GetOrCreate(key, entry => initialValue));
        var task2 = await Task.Run(() => _mockCacheProvider.Object.GetOrCreate(key, entry => newValue));

        // Assert
        Assert.That(task1, Is.EqualTo(newValue));
        Assert.That(task2, Is.EqualTo(initialValue));
    }
}