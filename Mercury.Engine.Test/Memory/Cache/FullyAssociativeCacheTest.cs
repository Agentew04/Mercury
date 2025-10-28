using Mercury.Engine.Memory;
using Mercury.Engine.Memory.Cache;

namespace Mercury.Engine.Test.Memory.Cache;

[TestClass]
public class FullyAssociativeCacheTest
{
    [TestMethod]
    public void CacheFifoTest()
    {
        MemoryConfiguration config = new()
        {
            ColdStoragePath = Path.GetTempFileName(),
            StorageType = StorageType.Volatile,
            MaxLoadedPages = 2,
            PageSize = 64,
            Size = 512
        };
        using Engine.Memory.Memory memory = new(config);
        using FullyAssociativeCache cache = new(memory, 4, 1, CacheWritePolicy.WriteThrough, SubstitutionStrategy.Fifo);

        int missCount = 0;
        int evictionCount = 0;

        cache.OnCacheMiss += (_, _) => missCount++;
        cache.OnCacheEvict += (_, _) => evictionCount++;
        
        uint[] accesses = [0, 1, 3, 2, 0, 5, 9, 5, 6, 3];
        foreach (uint address in accesses)
        {
            cache.ReadByte(address);
        }
        
        Assert.AreEqual(8, missCount);
        Assert.AreEqual(4, evictionCount);
    }

    [TestMethod]
    public void CacheLruTest()
    {
        MemoryConfiguration config = new()
        {
            ColdStoragePath = Path.GetTempFileName(),
            StorageType = StorageType.Volatile,
            MaxLoadedPages = 2,
            PageSize = 64,
            Size = 512
        };
        using Engine.Memory.Memory memory = new(config);
        using FullyAssociativeCache cache = new(memory, 4, 1, CacheWritePolicy.WriteThrough, SubstitutionStrategy.Fifo);

        int missCount = 0;
        int evictionCount = 0;

        cache.OnCacheMiss += (_, _) => missCount++;
        cache.OnCacheEvict += (_, _) => evictionCount++;

        ulong[] accesses = [0,1,2,0,1,3,3,3,4,5,6,4,4,4,7,8];
        
        foreach (ulong address in accesses)
        {
            cache.ReadByte(address);
        }
        
        Assert.AreEqual(9, missCount);
        Assert.AreEqual(5, evictionCount);
    }
    
    [TestMethod]
    public void CacheLfuTest()
    {
        MemoryConfiguration config = new()
        {
            ColdStoragePath = Path.GetTempFileName(),
            StorageType = StorageType.Volatile,
            MaxLoadedPages = 2,
            PageSize = 64,
            Size = 512
        };
        using Engine.Memory.Memory memory = new(config);
        using FullyAssociativeCache cache = new(memory, 4, 1, CacheWritePolicy.WriteThrough, SubstitutionStrategy.Fifo);

        int missCount = 0;
        int evictionCount = 0;

        cache.OnCacheMiss += (_, _) => missCount++;
        cache.OnCacheEvict += (_, _) => evictionCount++;

        ulong[] accesses = [0,1,2,3,1,1,1,2,2,3,4,4,4,4,4,5];
        
        foreach (ulong address in accesses)
        {
            cache.ReadByte(address);
        }
        
        Assert.AreEqual(6, missCount);
        Assert.AreEqual(2, evictionCount);
    }
    
    [TestMethod]
    public void CacheRandomTest()
    {
        MemoryConfiguration config = new()
        {
            ColdStoragePath = Path.GetTempFileName(),
            StorageType = StorageType.Volatile,
            MaxLoadedPages = 2,
            PageSize = 64,
            Size = 512
        };
        using Engine.Memory.Memory memory = new(config);
        using FullyAssociativeCache cache = new(memory, 4, 1, CacheWritePolicy.WriteThrough, SubstitutionStrategy.Fifo);

        int missCount = 0;
        int evictionCount = 0;

        cache.OnCacheMiss += (_, _) => missCount++;
        cache.OnCacheEvict += (_, _) => evictionCount++;

        ulong[] accesses = [0,1,2,3,4,5];
        
        foreach (ulong address in accesses)
        {
            cache.ReadByte(address);
        }
        
        Assert.AreEqual(6, missCount);
        Assert.AreEqual(2, evictionCount);
    }
    
    [TestMethod]
    public void CacheWriteThroughTest()
    {
        MemoryConfiguration config = new()
        {
            ColdStoragePath = Path.GetTempFileName(),
            StorageType = StorageType.Volatile,
            MaxLoadedPages = 2,
            PageSize = 64,
            Size = 512
        };
        using Engine.Memory.Memory memory = new(config);
        using FullyAssociativeCache cache = new(memory, 4, 1, CacheWritePolicy.WriteThrough, SubstitutionStrategy.Fifo);

        cache.WriteByte(0,0x01);
        cache.WriteByte(1,0x02);
        cache.WriteByte(2,0x03);
        cache.WriteByte(3,0x04);
        
        Assert.AreEqual(0x01, memory.ReadByte(0));
        Assert.AreEqual(0x02, memory.ReadByte(1));
        Assert.AreEqual(0x03, memory.ReadByte(2));
        Assert.AreEqual(0x04, memory.ReadByte(3));
    }
    
    [TestMethod]
    public void CacheWriteBackTest()
    {
        MemoryConfiguration config = new()
        {
            ColdStoragePath = Path.GetTempFileName(),
            StorageType = StorageType.Volatile,
            MaxLoadedPages = 2,
            PageSize = 64,
            Size = 512
        };
        using Engine.Memory.Memory memory = new(config);
        using FullyAssociativeCache cache = new(memory, 4, 1, CacheWritePolicy.WriteBack, SubstitutionStrategy.Fifo);

        cache.WriteByte(0,0x01);
        cache.WriteByte(1,0x02);
        cache.WriteByte(2,0x03);
        cache.WriteByte(3,0x04);
        cache.WriteByte(4,0x05);
        
        Assert.AreEqual(0x01, memory.ReadByte(0));
        Assert.AreEqual(0x00, memory.ReadByte(1));
        Assert.AreEqual(0x00, memory.ReadByte(2));
        Assert.AreEqual(0x00, memory.ReadByte(3));
        Assert.AreEqual(0x00, memory.ReadByte(4));
    }
}