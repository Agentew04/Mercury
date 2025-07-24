using SAAE.Engine.Memory;

namespace SAAE.Engine.Common.Builders;

using Memory = Memory.Memory;

/// <summary>
/// Builder responsible for a friendly way to create a <see cref="Memory"/> instance.
/// </summary>
public class MemoryBuilder : IBuilder<Memory>
{
    private ulong pageSize = 4096; // Default page size
    private ulong size = 1024ul * 1024 * 1024 * 4; // Default size (4 GB)
    private Endianess endianess = Endianess.LittleEndian;
    private int pageCapacity = 16;
    private string storagePath = "memory.bin";
    private StorageType storageType;

    public MemoryBuilder WithPageSize(ulong pageSize)
    {
        this.pageSize = pageSize;
        return this;
    }
    
    public MemoryBuilder WithPageCapacity(int pageCapacity)
    {
        if (pageCapacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageCapacity), "Page capacity must be at least 1.");
        }
        this.pageCapacity = pageCapacity;
        return this;
    }
    
    public MemoryBuilder WithSize(ulong size)
    {
        this.size = size;
        return this;
    }

    public MemoryBuilder WithBigEndian()
    {
        endianess = Endianess.BigEndian;
        return this;
    }

    public MemoryBuilder WithLittleEndian()
    {
        endianess = Endianess.LittleEndian;
        return this;
    }

    public MemoryBuilder WithEndianess(Endianess endianess) {
        return endianess switch {
            Endianess.BigEndian => WithBigEndian(),
            Endianess.LittleEndian => WithLittleEndian(),
            _ => throw new NotSupportedException("Endianness not supported")
        };
    }

    public MemoryBuilder With4Gb() => WithSize(1024ul * 1024 * 1024 * 4);

    public MemoryBuilder WithFileStorage(string path, bool optimized = true)
    {
        storagePath = path;
        storageType = optimized ? StorageType.FileOptimized : StorageType.FileOriginal;
        return this;
    }
    
    public MemoryBuilder WithVolatileStorage()
    {
        storageType = StorageType.Volatile;
        return this;
    }
    
    public Memory Build()
    {
        MemoryConfiguration config = new()
        {
            PageSize = pageSize,
            Size = size,
            Endianess = endianess,
            StorageType = storageType,
            ColdStoragePath = storagePath,
            MaxLoadedPages = (uint)pageCapacity,
        };
        return new Memory(config);
    }
}