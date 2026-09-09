using Mercury.Engine.Memory;

namespace Mercury.Engine.Common.Builders;

using Memory = Memory.Memory;

/// <summary>
/// Builder responsible for a friendly way to create a <see cref="Memory"/> instance.
/// </summary>
public class MemoryBuilder : IBuilder<Memory>
{
    private ulong blockSize = 4096; // Default page size
    private ulong size = 1024ul * 1024 * 1024 * 4; // Default size (4 GB)
    private Endianness endianness = Endianness.LittleEndian;
    private int blockCapacity = 16;
    private string storagePath = "memory.bin";
    private StorageType storageType;

    public MemoryBuilder WithBlockSize(ulong blockSize)
    {
        this.blockSize = blockSize;
        return this;
    }
    
    public MemoryBuilder WithBlockCapacity(int blockCapacity)
    {
        if (blockCapacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(blockCapacity), "Page capacity must be at least 1.");
        }
        this.blockCapacity = blockCapacity;
        return this;
    }
    
    public MemoryBuilder WithSize(ulong size)
    {
        this.size = size;
        return this;
    }

    public MemoryBuilder WithBigEndian()
    {
        endianness = Endianness.BigEndian;
        return this;
    }

    public MemoryBuilder WithLittleEndian()
    {
        endianness = Endianness.LittleEndian;
        return this;
    }

    public MemoryBuilder WithEndianess(Endianess endianess) {
        return endianess switch {
            Endianess.BigEndian => WithBigEndian(),
            Endianess.LittleEndian => WithLittleEndian(),
    public MemoryBuilder WithEndianess(Endianness endianness) {
        return endianness switch {
            Endianness.BigEndian => WithBigEndian(),
            Endianness.LittleEndian => WithLittleEndian(),
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
        if (storageType == StorageType.NotSet) {
            throw new NotSupportedException("Storage Type must be set.");
        }
        
        MemoryConfiguration config = new()
        {
            PageSize = blockSize,
            Size = size,
            Endianness = endianness,
            StorageType = storageType,
            ColdStoragePath = storagePath,
            MaxLoadedPages = (uint)blockCapacity,
        };
        return new Memory(config);
    }
}