using SAAE.Engine.Memory;
using SAAE.Engine.Memory.Cache;

namespace SAAE.Engine.Test.Memory.Cache;

[TestClass]
public class FullyAssociativeCacheTest
{
    [TestMethod]
    public void CacheFifoTest()
    {
        MemoryConfiguration config = new()
        {
            ColdStoragePath = Path.GetTempFileName(),
            ColdStorageOptimization = true,
            MaxLoadedPages = 2,
            PageSize = 64,
            Size = 512
        };
        using Engine.Memory.Memory memory = new(config);
        using FullyAssociativeCache cache = new(memory, 4, 1, CacheWritePolicy.WriteThrough, SubstitutionStrategy.Fifo);

        int missCount = 0;
        int evictionCount = 0;

        cache.OnCacheMiss += (_, e) => missCount++;
        cache.OnCacheEvict += (_, e) => evictionCount++;
        
        uint[] accesses = [0, 1, 3, 2, 0, 5, 9, 5, 6, 3];
        foreach (uint address in accesses)
        {
            cache.ReadByte(address);
        }
        
        Assert.AreEqual(8, missCount);
        Assert.AreEqual(4, evictionCount);
    }
    
}