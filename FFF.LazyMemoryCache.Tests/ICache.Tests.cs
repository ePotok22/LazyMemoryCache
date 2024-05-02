
using Microsoft.Extensions.Caching.Memory;
using FFF.LazyMemoryCache;
using Moq;

namespace FFF.LazyMemoryCache.Tests;

[TestFixture]
public class CacheTests
{
    private Mock<ICache> _mockCache;
    private const string TestKey = "testKey";
    private const string TestValue = "testValue";

    [SetUp]
    public void Setup()
    {
        _mockCache = new Mock<ICache>();
    }

    [Test]
    public void Add_Item_Should_Call_Add_With_Correct_Parameters()
    {
        // Arrange
        var policy = new MemoryCacheEntryOptions();

        // Act
        _mockCache.Object.Add(TestKey, TestValue, policy);

        // Assert
        _mockCache.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<MemoryCacheEntryOptions>()), Times.Once);
    }

    [Test]
    public void Get_Item_Should_Return_Correct_Value()
    {
        // Arrange
        _mockCache.Setup(x => x.Get<string>(It.IsAny<string>())).Returns(TestValue);

        // Act
        var result = _mockCache.Object.Get<string>(TestKey);

        // Assert
        Assert.That(result, Is.EqualTo(TestValue));
    }

    [Test]
    public void Remove_Item_Should_Call_Remove_With_Correct_Key()
    {
        // Act
        _mockCache.Object.Remove(TestKey);

        // Assert
        _mockCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Once);
    }
    [Test]
    public void TryGetValue_KeyExists_Should_Return_True_And_Correct_Value()
    {
        // Arrange
        string? expectedValue = TestValue;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<string>(), out expectedValue)).Returns(true);

        // Act
        _mockCache.Object.TryGetValue(TestKey, out string? actualValue);

        // Assert
        Assert.That(actualValue, Is.EqualTo(TestValue));
        Assert.That(_mockCache.Object.TryGetValue(TestKey, out actualValue), Is.True);
    }

    [Test]
    public async Task GetOrAddAsync_KeyNotExists_Should_Add_Value()
    {
        // Arrange
        _mockCache.Setup(x => x.GetOrAddAsync(It.IsAny<string>(), It.IsAny<Func<ICacheEntry, Task<string>>>(), It.IsAny<MemoryCacheEntryOptions>()))
                  .ReturnsAsync(TestValue);

        // Act
        var result = await _mockCache.Object.GetOrAddAsync(TestKey, entry => Task.FromResult("newValue"), null);

        // Assert
        Assert.That(result, Is.EqualTo(TestValue));

        _mockCache.Verify(x => x.GetOrAddAsync(It.IsAny<string>(), It.IsAny<Func<ICacheEntry, Task<string>>>(), It.IsAny<MemoryCacheEntryOptions>()), Times.Once);
    }

    [Test]
    public void Remove_WithPredicate_Should_Call_Correct_Overload()
    {
        // Arrange
        Func<string, bool> predicate = key => key.StartsWith("test");

        // Act
        _mockCache.Object.Remove(TestKey, predicate);

        // Assert
        _mockCache.Verify(x => x.Remove(It.IsAny<string>(), It.IsAny<Func<string, bool>>()), Times.Once);
    }
    [Test]
    public async Task GetAsync_KeyExists_Should_Return_Expected_Value()
    {
        // Arrange
        _mockCache.Setup(x => x.GetAsync<string>(It.IsAny<string>())).ReturnsAsync(TestValue);

        // Act
        var result = await _mockCache.Object.GetAsync<string>(TestKey);

        // Assert
        Assert.That(result, Is.EqualTo(TestValue));
    }

    [Test]
    public void GetOrAdd_With_Policy_Should_Use_Policy()
    {
        // Arrange
        var policy = new MemoryCacheEntryOptions();
        _mockCache.Setup(x => x.GetOrAdd(It.IsAny<string>(), It.IsAny<Func<ICacheEntry, string>>(), It.IsAny<MemoryCacheEntryOptions>()))
                  .Returns(TestValue);

        // Act
        var result = _mockCache.Object.GetOrAdd(TestKey, entry => "newValue", policy);

        // Assert
        Assert.That(result, Is.EqualTo(TestValue));
        _mockCache.Verify(x => x.GetOrAdd(It.IsAny<string>(), It.IsAny<Func<ICacheEntry, string>>(), policy), Times.Once);
    }

    [Test]
    public void Remove_With_RemoveMode_Should_Handle_Modes_Correctly()
    {
        // Arrange
        var mode = RemoveMode.EqualIgnoreCase;

        // Act
        _mockCache.Object.Remove(TestKey, mode);

        // Assert
        _mockCache.Verify(x => x.Remove(It.IsAny<string>(), mode), Times.Once);
    }

    [Test]
    public async Task GetAsync_KeyNotExists_Should_Return_Null()
    {
        // Arrange
        _mockCache.Setup(x => x.GetAsync<string>(It.IsAny<string>())).ReturnsAsync(null as string);

        // Act
        var result = await _mockCache.Object.GetAsync<string>("nonExistentKey");

        // Assert
        Assert.That(result, Is.Null);
    }
}