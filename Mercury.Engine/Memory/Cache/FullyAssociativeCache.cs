using System.Buffers.Binary;
using System.Numerics;

namespace Mercury.Engine.Memory.Cache;

/// <summary>
/// Represents a fully associative cache. There are no index bits, so any block can be placed in any row.
/// </summary>
public class FullyAssociativeCache : ICache {
    
    /// <summary>
    /// The backing memory for this cache.
    /// </summary>
    private readonly IMemory backingMemory;
    /// <summary>
    /// The amount of blocks this cache has.
    /// </summary>
    private readonly int blockCount;
    /// <summary>
    /// The amount of bytes each block has.
    /// </summary>
    private readonly int blockSize;

    /// <summary>
    /// The write policy of this cache.
    /// </summary>
    public CacheWritePolicy WritePolicy { get; init; }

    public event EventHandler<CacheMissEventArgs>? OnCacheMiss;
    public event EventHandler<CacheEvictionEventArgs>? OnCacheEvict;

    /// <summary>
    /// Represents the substitution strategy of this cache. Manages
    /// which block will be evicted when a new block is loaded and
    /// there are no free blocks available.
    /// </summary>
    public SubstitutionStrategy SubstitutionStrategy { get; init; }

    public Endianess Endianess => backingMemory.Endianess;
    

    public FullyAssociativeCache(IMemory backingMemory, int blockCount, int blockSize, CacheWritePolicy writePolicy, SubstitutionStrategy substitutionStrategy) {
        this.backingMemory = backingMemory;
        WritePolicy = writePolicy;
        SubstitutionStrategy = substitutionStrategy;
        this.blockCount = blockCount;
        this.blockSize = blockSize;

        switch (substitutionStrategy)
        {
            case SubstitutionStrategy.Fifo:
                fifoBlockQueue = new Queue<uint>(blockCount);
                break;
            case SubstitutionStrategy.Lru:
                lruTimestamps = new List<uint>(blockCount);
                for (int i = 0; i < blockCount; i++) {
                    lruTimestamps.Add(0);
                }
                break;
            case SubstitutionStrategy.Lfu:
                lfuFrequencies = new List<uint>(blockCount);
                for (int i = 0; i < blockCount; i++) {
                    lfuFrequencies.Add(0);
                }
                break;
            case SubstitutionStrategy.Random:
                rng = new Random();
                break;
            default:
                throw new NotSupportedException("SubstitutionStrategy not supported.");
        }
        
        blocks = new List<CacheBlock>(blockCount);
        for (int i = 0; i < blockCount; i++) {
            blocks.Add(new CacheBlock {
                Data = new byte[blockSize],
                Valid = false,
                Modified = false,
                Tag = 0
            });
        }
    }

    private readonly List<CacheBlock> blocks;
    
    private readonly Queue<uint>? fifoBlockQueue;
    private readonly List<uint>? lruTimestamps;
    private uint lruHead;
    private uint lfuAccessCounter;
    private const uint LfuHalflife = 64;
    private readonly List<uint>? lfuFrequencies;
    private readonly Random? rng;

    /// <summary>
    /// Decides which block to evict based on the substitution strategy.
    /// </summary>
    /// <returns></returns>
    private uint GetBlockToEvict()
    {
        switch (SubstitutionStrategy)
        {
            case SubstitutionStrategy.Fifo:
                return fifoBlockQueue!.Peek();
            case SubstitutionStrategy.Lru:
                uint oldestTimestamp = uint.MaxValue;
                uint oldestBlockIndex = 0;
                for (uint i = 0; i < blockCount; i++)
                {
                    if (lruTimestamps![(int)i] < oldestTimestamp)
                    {
                        oldestTimestamp = lruTimestamps[(int)i];
                        oldestBlockIndex = i;
                    }
                }
                return oldestBlockIndex;
            case SubstitutionStrategy.Lfu:
                uint leastFrequent = uint.MaxValue;
                uint leastFrequentBlockIndex = 0;
                for (uint i = 0; i < blockCount; i++)
                {
                    if (lfuFrequencies![(int)i] < leastFrequent)
                    {
                        leastFrequent = lfuFrequencies[(int)i];
                        leastFrequentBlockIndex = i;
                    }
                }
                return leastFrequentBlockIndex;
            case SubstitutionStrategy.Random:
                return (uint)rng!.Next(blockCount);
            default:
                throw new NotSupportedException();
        }
    }
    
    private ulong GetAddressTag(ulong address) {
        //4 bytes -> 2 bit skip
        // n bytes -> skip = n / 4
        int skip = BitOperations.Log2((uint)blockSize);
        
        ulong tagMask = (unchecked((ulong)-1) >> skip) << skip;
        ulong tag = (address & tagMask) >> skip;
        return tag;
    }

    private bool IsHit(ulong tag, out int index)
    {
        index = blocks.FindIndex(x => x.Tag == tag && x.Valid);
        return index != -1;
    }

    private int LoadBlock(ulong tag)
    {
        // find an invalid block
        int index = blocks.FindIndex(x => !x.Valid);
        int tagShift = BitOperations.Log2((uint)blockSize);
        ulong baseAddress = tag << tagShift;
        Span<byte> buffer = stackalloc byte[blockSize];
        if (index != -1)
        {
            // just load the block into invalid block
            blocks[index].Tag = tag;
            blocks[index].Valid = true;
            blocks[index].Modified = false;
            backingMemory.Read(baseAddress, buffer);
            if (SubstitutionStrategy == SubstitutionStrategy.Fifo)
            {
                fifoBlockQueue!.Enqueue((uint)index);
            }
            return index;
        }
        
        index = (int)GetBlockToEvict();
        OnCacheEvict?.Invoke(this, 
            new CacheEvictionEventArgs(baseAddress, blocks[index].Tag << tagShift));
        StoreBlock(index);

        // load new block
        blocks[index].Tag = tag;
        blocks[index].Valid = true;
        blocks[index].Modified = false;
        backingMemory.Read(baseAddress, buffer);

        switch (SubstitutionStrategy)
        {
            case SubstitutionStrategy.Fifo:
                _ = fifoBlockQueue!.Dequeue(); // remove the oldest block
                fifoBlockQueue!.Enqueue((uint)index); // add the new block to the end
                break;
            case SubstitutionStrategy.Lru:
                lruTimestamps![index] = lruHead;
                break;
            case SubstitutionStrategy.Lfu:
                lfuFrequencies![index] = 0; // reset frequency to 1
                break;
        }
        
        return index;
    }

    private void StoreBlock(int index)
    {
        CacheBlock block = blocks[index];
        if (!block.Valid)
        {
            throw new InvalidOperationException("Cannot store an invalid block.");
        }

        if (block.Modified && WritePolicy == CacheWritePolicy.WriteBack)
        {
            ulong baseAddress = block.Tag << BitOperations.Log2((uint)blockSize);
            backingMemory.Write(baseAddress, block.Data);
        }
        
        // Reset the block
        block.Valid = false;
        block.Modified = false;
        block.Tag = 0;
    }

    private byte GetByteFromBlock(CacheBlock block, ulong address)
    {
        int offset = (int)(address % (ulong)blockSize);
        if (offset < 0 || offset >= blockSize)
        {
            throw new ArgumentOutOfRangeException(nameof(address), "Address is out of bounds for the block size.");
        }
        return block.Data[offset];
    }
    
    #region Memory Access

    public byte ReadByte(ulong address) {
        ulong tag = GetAddressTag(address);

        if (!IsHit(tag, out int index))
        {
            // miss
            OnCacheMiss?.Invoke(this, new CacheMissEventArgs(address));
            index = LoadBlock(tag);
        }
        
        // hit
        switch (SubstitutionStrategy)
        {
            case SubstitutionStrategy.Lru:
                lruTimestamps![index] = lruHead++;
                break;
            case SubstitutionStrategy.Lfu:
                lfuFrequencies![index]++;
                if(lfuAccessCounter++ >= LfuHalflife) {
                    // halve the frequencies
                    for (int i = 0; i < lfuFrequencies!.Count; i++) {
                        lfuFrequencies[i] /= 2;
                    }
                    lfuAccessCounter = 0;
                }
                break;
        }
        
        return GetByteFromBlock(blocks[index], address);
    }

    public void WriteByte(ulong address, byte value) {
        ulong tag = GetAddressTag(address);

        if (!IsHit(tag, out int index))
        {
            // miss
            OnCacheMiss?.Invoke(this, new CacheMissEventArgs(address));
            index = LoadBlock(tag);
        }
        
        // hit
        switch (SubstitutionStrategy)
        {
            case SubstitutionStrategy.Lru:
                lruTimestamps![index] = lruHead++;
                break;
            case SubstitutionStrategy.Lfu:
                lfuFrequencies![index]++;
                if(lfuAccessCounter++ >= LfuHalflife) {
                    // halve the frequencies
                    for (int i = 0; i < lfuFrequencies!.Count; i++) {
                        lfuFrequencies[i] /= 2;
                    }
                    lfuAccessCounter = 0;
                }
                break;
        }

        if (WritePolicy == CacheWritePolicy.WriteBack)
        {
            blocks[index].Modified = true;
            blocks[index].Data[address % (ulong)blockSize] = value;
        }
        else if (WritePolicy == CacheWritePolicy.WriteThrough)
        {
            backingMemory.WriteByte(address, value);
            blocks[index].Data[address % (ulong)blockSize] = value;
        }
    }

    public int ReadWord(ulong address) {
        Span<byte> buffer = stackalloc byte[4];
        buffer[0] = ReadByte(address);
        buffer[1] = ReadByte(address + 1);
        buffer[2] = ReadByte(address + 2);
        buffer[3] = ReadByte(address + 3);

        return Endianess switch
        {
            Endianess.LittleEndian => BinaryPrimitives.ReadInt32LittleEndian(buffer),
            Endianess.BigEndian => BinaryPrimitives.ReadInt32BigEndian(buffer),
            _ => throw new NotSupportedException("Unsupported endianness.")
        };
    }

    public void WriteWord(ulong address, int value) {
        Span<byte> data = stackalloc byte[4];
        if (Endianess == Endianess.LittleEndian) {
            BinaryPrimitives.WriteInt32LittleEndian(data, value);
        }
        else if (Endianess == Endianess.BigEndian) {
            BinaryPrimitives.WriteInt32BigEndian(data, value);
        }
        else {
            throw new NotSupportedException("Unsupported endianness.");
        }

        WriteByte(address, data[0]);
        WriteByte(address + 1, data[1]);
        WriteByte(address + 2, data[2]);
        WriteByte(address + 3, data[3]);
    }

    public byte[] Read(ulong address, int length) {
        if (length <= 0) {
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than zero.");
        }

        byte[] result = new byte[length];
        for (int i = 0; i < length; i++) {
            result[i] = ReadByte(address + (ulong)i);
        }
        return result;
    }

    public void Write(ulong address, byte[] bytes) {
        if (bytes == null) {
            throw new ArgumentNullException(nameof(bytes), "Bytes cannot be null.");
        }
        if (bytes.Length <= 0) {
            throw new ArgumentOutOfRangeException(nameof(bytes), "Bytes length must be greater than zero.");
        }

        for (int i = 0; i < bytes.Length; i++) {
            WriteByte(address + (ulong)i, bytes[i]);
        }
    }

    public void Read(ulong address, Span<byte> bytes) {
        if (bytes == null || bytes.Length <= 0) {
            throw new ArgumentOutOfRangeException(nameof(bytes), "Bytes span must be greater than zero.");
        }

        for (int i = 0; i < bytes.Length; i++) {
            bytes[i] = ReadByte(address + (ulong)i);
        }
    }

    public void Write(ulong address, Span<byte> bytes) {
        if (bytes == null || bytes.Length <= 0) {
            throw new ArgumentOutOfRangeException(nameof(bytes), "Bytes span must be greater than zero.");
        }

        for (int i = 0; i < bytes.Length; i++) {
            WriteByte(address + (ulong)i, bytes[i]);
        }
    }

    public void Read(ulong address, Span<int> words) {
        if (words == null || words.Length <= 0) {
            throw new ArgumentOutOfRangeException(nameof(words), "Words span must be greater than zero.");
        }

        for (int i = 0; i < words.Length; i++) {
            words[i] = ReadWord(address + (ulong)(i * 4));
        }
    }

    public void Write(ulong address, Span<int> words) {
        if (words == null || words.Length <= 0) {
            throw new ArgumentOutOfRangeException(nameof(words), "Words span must be greater than zero.");
        }

        for (int i = 0; i < words.Length; i++) {
            WriteWord(address + (ulong)(i * 4), words[i]);
        }
    }

    #endregion

    public void Dispose()
    {
        if (WritePolicy != CacheWritePolicy.WriteBack)
        {
            return;
        }
        for(int i=0;i<blocks.Count;i++)
        {
            CacheBlock block = blocks[i];
            if(!block.Valid || !block.Modified) continue;   
            StoreBlock(i);
        }
        GC.SuppressFinalize(this);
    }

    private class CacheBlock
    {
        public ulong Tag { get; set; }
        public bool Modified { get; set; }
        public bool Valid { get; set; }
        public required byte[] Data { get; init; }
    }
}