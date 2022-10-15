using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Jitbit.Utils;
using System.Runtime.Caching;

#if DEBUG
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).RunAll(new DebugInProcessConfig());
#else
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).RunAll();
#endif

[ShortRunJob]
[MemoryDiagnoser(displayGenColumns: false)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
//[CategoriesColumn]
public class CacheBenchmark
{
    private static readonly FastCache<string, int> _cache = new FastCache<string, int>(1000 * 60);

    private static readonly DateTime _after10Minutes = DateTime.Now.AddMinutes(10);


    [GlobalSetup]
    public void Setup()
    {
        for (int i = 0; i < 1000; i++)
        {
            _cache.AddOrUpdate("test" + i, i, TimeSpan.FromMinutes(1));
            MemoryCache.Default.Add("test" + i, i, _after10Minutes);
        }
    }


    // Get

    [BenchmarkCategory("Get"), Benchmark(Description = "FastCache(Get)", Baseline = true)]
    public int FastCacheLookup()
    {
        _cache.TryGet("test123", out int value);
        return value;
    }

    [BenchmarkCategory("Get"), Benchmark(Description = "MemoryCache(Get)")]
    public int MemoryCacheLookup()
    {
        //object value = MemoryCache.Default["test123"];
        object value = MemoryCache.Default.Get("test123");
        return (int)value;
    }


    // Add

    [BenchmarkCategory("Add"), Benchmark(Description = "FastCache(Add)", Baseline = true)]
    public void FastCacheAdd()
    {
        _cache.AddOrUpdate("1111", 42, TimeSpan.FromMinutes(1));
    }

    [BenchmarkCategory("Add"), Benchmark(Description = "MemoryCache(Add)")]
    public void MemoryCacheAdd()
    {
        MemoryCache.Default.Add("1111", 42, _after10Minutes);
    }


    // Update

    [BenchmarkCategory("Update"), Benchmark(Description = "FastCache(Update)", Baseline = true)]
    public void FastCacheUpdate()
    {
        _cache.AddOrUpdate("test0", 42, TimeSpan.FromMinutes(1));
    }

    [BenchmarkCategory("Update"), Benchmark(Description = "MemoryCache(Update)")]
    public void MemoryCacheUpdate()
    {
        MemoryCache.Default.Set("test0", 42, _after10Minutes);
    }


    // Remove

    [BenchmarkCategory("Remove"), Benchmark(Description = "FastCache(Remove)", Baseline = true)]
    public void FastCacheRemove()
    {
        _cache.Remove("test1");
    }

    [BenchmarkCategory("Remove"), Benchmark(Description = "MemoryCache(Remove)")]
    public void MemoryCacheRemove()
    {
        MemoryCache.Default.Remove("test1");
    }
}