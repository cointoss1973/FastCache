using Jitbit.Utils;
using Xunit;

namespace FastCache.Tests;

public class UnitTests
{
    [Fact]
    public async Task TestGetSetCleanup()
    {
        // Arrange
        var _cache = new FastCache<int, int>(cleanupJobInterval: 200);

        // Act
        _cache.AddOrUpdate(42, 42, TimeSpan.FromMilliseconds(100));

        // Assert
        Assert.True(_cache.TryGet(42, out int v));
        Assert.Equal(42, v);

        await Task.Delay(300);
        Assert.True(_cache.Count == 0); //cleanup job has run?
    }

    [Fact]
    public async Task Shortdelay()
    {
        // Arrange
        var cache = new FastCache<int, int>();

        // Act
        cache.AddOrUpdate(42, 42, TimeSpan.FromMilliseconds(500));
        await Task.Delay(50);

        // Assert
        Assert.True(cache.TryGet(42, out int result)); //not evicted
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task TestWithDefaultJobInterval()
    {
        // Arrange
        var cache = new FastCache<string, int>();

        // Act
        cache.AddOrUpdate("key", 42, TimeSpan.FromMilliseconds(100));

        // Assert
        Assert.True(cache.TryGet("key", out _));
        await Task.Delay(150);
        Assert.False(cache.TryGet("key", out _));
    }

    [Fact]
    public void TestRemove()
    {
        // Arrange
        var cache = new FastCache<string, int>();

        // Act
        cache.AddOrUpdate("key", 42, TimeSpan.FromMilliseconds(100));
        cache.Remove("key");
        bool result = cache.TryGet("key", out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task TestTryAdd()
    {
        // Arrange
        var cache = new FastCache<string, int>();

        // Act, Assert
        Assert.True(cache.TryAdd("key", 42, TimeSpan.FromMilliseconds(100)));
        Assert.False(cache.TryAdd("key", 42, TimeSpan.FromMilliseconds(100)));

        await Task.Delay(120); //wait for it to expire

        Assert.True(cache.TryAdd("key", 42, TimeSpan.FromMilliseconds(100)));
    }

    [Fact]
    public async Task TestGetOrAdd()
    {
        // Arrange
        var cache = new FastCache<string, int>();

        // Act
        cache.GetOrAdd("key", k => 1024, TimeSpan.FromMilliseconds(100));

        // Assert
        Assert.True(cache.TryGet("key", out int value));
        Assert.Equal(1024, value);
        await Task.Delay(110);

        Assert.False(cache.TryGet("key", out _));
    }

    [Fact]
    public async Task TestTryAddAtomicness()
    {
        // Arrange
        int i = 0;

        var cache = new FastCache<int, int>();
        cache.TryAdd(42, 42, TimeSpan.FromMilliseconds(50)); //add item with short TTL

        // Act
        await Task.Delay(100); //wait for tha value to expire

        await TestHelper.RunConcurrently(20, () =>
        {
            if (cache.TryAdd(42, 42, TimeSpan.FromSeconds(1)))
                i++;
        });

        // Assert
        Assert.True(i == 1, i.ToString());
    }

    [Fact]
    public async Task TestGetOrAddAtomicNess()
    {
        // Arrange
        int i = 0;

        var cache = new FastCache<int, int>();
        cache.GetOrAdd(42, k =>
        {
            i++;
            return 1024;
        }, TimeSpan.FromMilliseconds(100));

        // Act
        await TestHelper.RunConcurrently(20, () =>
        {
            if (cache.TryAdd(42, 42, TimeSpan.FromSeconds(1)))
                i++;
        });

        // Assert
        // test that add factory was called only once
        Assert.True(i == 1, i.ToString());
    }

    [Fact]
    public async Task Enumerator()
    {
        // Arrange
        var cache = new FastCache<string, int>(); //now with default cleanup interval
        cache.GetOrAdd("key", k => 1024, TimeSpan.FromMilliseconds(100));

        // Act
        int value = cache.FirstOrDefault().Value;

        // Assert
        Assert.Equal(1024, value);

        await Task.Delay(110);

        Assert.False(cache.Any());
    }

    [Fact]
    public async Task TestTtlExtended()
    {
        // Arrange
        var _cache = new FastCache<int, int>();

        // Act
        _cache.AddOrUpdate(42, 42, TimeSpan.FromMilliseconds(300));
        await Task.Delay(50);
        Assert.True(_cache.TryGet(42, out int result)); //not evicted
        Assert.True(result == 42);

        _cache.AddOrUpdate(42, 42, TimeSpan.FromMilliseconds(300));

        await Task.Delay(250);

        // Assert
        Assert.True(_cache.TryGet(42, out int result2)); //still not evicted
        Assert.True(result2 == 42);
    }
}