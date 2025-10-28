using Mercury.Engine.Memory;
using Mercury.Engine.Memory.Cache;

namespace Mercury.Engine.Test.Memory.Cache;

[TestClass]
public class DirectAccessCacheTest
{   
    [TestMethod]
    public void CacheMissTest1()
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
        DirectAccessCache cache = new(memory, 4, 1, CacheWritePolicy.WriteThrough);

        int missCount = 0;
        cache.OnCacheMiss += (_,_) => missCount++;

        ulong[] access = [0, 1, 2, 3, 5, 1, 6, 2];
        foreach (ulong address in access)
        {
            cache.ReadByte(address);
        }
        Assert.AreEqual(8, missCount);
    }

    [TestMethod]
    public void CacheMissTest2()
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
        using DirectAccessCache cache = new(memory, 4, 1, CacheWritePolicy.WriteThrough);
        
        int missCount = 0;
        cache.OnCacheMiss += (_,_) => missCount++;

        ulong[] access = [0,1,2,3,2,3,4,5,6,2,3];
        foreach (ulong address in access)
        {
            cache.ReadByte(address);
        }
        Assert.AreEqual(8, missCount);
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
        using DirectAccessCache cache = new(memory, 4, 1, CacheWritePolicy.WriteThrough);
        
        cache.WriteByte(0, 0x01);
        cache.WriteByte(1, 0x02);
        cache.WriteByte(2, 0x03);
        cache.WriteByte(3, 0x04);
        
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
        DirectAccessCache cache = new(memory, 4, 1, CacheWritePolicy.WriteBack);
        
        cache.WriteByte(0, 0x01);
        cache.WriteByte(1, 0x02);
        cache.WriteByte(2, 0x03);
        cache.WriteByte(3, 0x04);
        cache.WriteByte(4, 0x05);

        Assert.AreEqual(0x01, memory.ReadByte(0));
        Assert.AreEqual(0x00, memory.ReadByte(1));
        Assert.AreEqual(0x00, memory.ReadByte(2));
        Assert.AreEqual(0x00, memory.ReadByte(3));
        Assert.AreEqual(0x00, memory.ReadByte(4));
        
        // Dispose the cache to write back the data
        cache.Dispose();
        Assert.AreEqual(0x01, memory.ReadByte(0));
        Assert.AreEqual(0x02, memory.ReadByte(1));
        Assert.AreEqual(0x03, memory.ReadByte(2));
        Assert.AreEqual(0x04, memory.ReadByte(3));
        Assert.AreEqual(0x05, memory.ReadByte(4));
    }
}