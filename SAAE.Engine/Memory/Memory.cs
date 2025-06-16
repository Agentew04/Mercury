using System.Buffers.Binary;

namespace SAAE.Engine.Memory;

public sealed class Memory : IDisposable, IMemory
{
    /// <summary>
    /// Total size of the memory in bytes.
    /// </summary>
    public ulong Size { get; }

    /// <summary>
    /// The size of each page.
    /// </summary>
    private readonly ulong pageSize;

    /// <summary>
    /// Defines the amount of concurrently loaded pages in memory.
    /// </summary>
    private readonly uint maxLoadedPages;

    /// <summary>
    /// Total number of pages in the memory. Includes
    /// loaded and unloaded pages.
    /// </summary>
    private readonly uint totalPageCount;

    /// <summary>
    /// Array of loaded pages. To access this array, use
    /// the lookup table <see cref="pageIndices"/>.
    /// Always has a size of <see cref="maxLoadedPages"/>.
    /// If a slot is empty, it can be a <see langword="null"/>.
    /// </summary>
    private readonly Page?[] loadedPages;

    /// <summary>
    /// Lookup table to find the index of a page in the
    /// <see cref="loadedPages"/> array. It is always the size
    /// of <see cref="totalPageCount"/>.
    /// </summary>
    private readonly int[] pageIndices;

    /// <summary>
    /// Last access time of each loaded page. Used to
    /// define which page to unload when the memory is full.
    /// Is has the same size of <see cref="loadedPages"/>.
    /// </summary>
    private readonly long[] lastAccessTime;
    
    public Endianess Endianess { get; init; }

    private readonly MemoryDebugInfo debugInfo = new();
    
    private readonly IStorage coldStorage;

    

    public Memory(MemoryConfiguration config)
    {
        Size = config.Size;
        pageSize = config.PageSize;
        maxLoadedPages = config.MaxLoadedPages;
        totalPageCount = (uint)(Size / pageSize);
        Endianess = config.Endianess;
        loadedPages = new Page[maxLoadedPages];
        Array.Fill(loadedPages, null);
        lastAccessTime = new long[maxLoadedPages];
        Array.Fill(lastAccessTime, 0);
        pageIndices = new int[totalPageCount];
        Array.Fill(pageIndices, -1);
        coldStorage = config.StorageType switch
        {
            StorageType.FileOriginal => new ColdStorage(config),
            StorageType.FileOptimized => new OptimizedColdStorage(config),
            StorageType.Volatile => new VolatileStorage(config),
            _ => throw new ArgumentException("StorageType not supported.", nameof(config))
        };
    }

    public byte ReadByte(ulong address)
    {
        // sanitizar input
        if(address >= Size) {
            throw new InvalidAddressException($"Address out of bounds. Expected Range: [0,{Size}[. Got: {address}");
        }
        
        int pageNumber = (int)(address / pageSize);
        if(pageNumber < 0 || pageNumber >= totalPageCount){
            throw new InvalidAddressException($"Page number out of bounds. Expected Range: [0,{totalPageCount}[. Got: {pageNumber}");
        }

        int pageIndex = EnsureLoaded(pageNumber);
        lastAccessTime[pageIndex] = DateTime.Now.Ticks;

        Page page = loadedPages[pageIndex]!;
        int offset = (int)(address % pageSize);
        byte data = page.Data[offset];
        // if(data != 0)
        //     Console.WriteLine($"Read byte {data} from address {address}");
        return data;
    }

    public void WriteByte(ulong address, byte value) {
        if(address >= Size) {
            throw new InvalidAddressException($"Address out of bounds. Expected Range: [0,{Size}[. Got: {address}");
        }
        
        int pageNumber = (int)(address / pageSize);
        if(pageNumber < 0 || pageNumber >= totalPageCount){
            throw new InvalidAddressException($"Page number out of bounds. Expected Range: [0,{totalPageCount}[. Got: {pageNumber}");
        }

        int pageIndex = EnsureLoaded(pageNumber);
        lastAccessTime[pageIndex] = DateTime.Now.Ticks;

        Page page = loadedPages[pageIndex]!;
        int offset = (int)(address % pageSize);
        if (page.Data[offset] != value) {
            page.IsDirty = true;
        }
        page.Data[offset] = value;
    }

    public int ReadWord(ulong address) {
        // sanitizar input
        if (address >= Size) {
            throw new InvalidAddressException($"Address out of bounds. Expected Range: [0,{Size}[. Got: {address}");
        }

        int pageNumber = (int)(address / pageSize);
        if (pageNumber < 0 || pageNumber >= totalPageCount) {
            throw new InvalidAddressException($"Page number out of bounds. Expected Range: [0,{totalPageCount}[. Got: {pageNumber}");
        }

        int pageIndex = EnsureLoaded(pageNumber);
        lastAccessTime[pageIndex] = DateTime.Now.Ticks;

        Page page = loadedPages[pageIndex]!;
        int offset = (int)(address % pageSize);
        Span<byte> bytes = page.Data.AsSpan()[offset..(offset+4)];
        int data = Endianess switch {
            Endianess.LittleEndian => BinaryPrimitives.ReadInt32LittleEndian(bytes),
            Endianess.BigEndian => BinaryPrimitives.ReadInt32BigEndian(bytes),
            _ => throw new ArgumentOutOfRangeException(nameof(address))
        };
        return data;
    }

    public void WriteWord(ulong address, int value) {
        if (address >= Size) {
            throw new InvalidAddressException($"Address out of bounds. Expected Range: [0,{Size}[. Got: {address}");
        }

        int pageNumber = (int)(address / pageSize);
        if (pageNumber < 0 || pageNumber >= totalPageCount) {
            throw new InvalidAddressException($"Page number out of bounds. Expected Range: [0,{totalPageCount}[. Got: {pageNumber}");
        }

        int pageIndex = EnsureLoaded(pageNumber);
        lastAccessTime[pageIndex] = DateTime.Now.Ticks;

        Page page = loadedPages[pageIndex]!;
        int offset = (int)(address % pageSize);
        if (page.Data[offset] != value) {
            page.IsDirty = true;
        }

        Span<byte> data = stackalloc byte[4];
        if (Endianess == Endianess.LittleEndian) {
            BinaryPrimitives.WriteInt32LittleEndian(data, value);
        } else {
            BinaryPrimitives.WriteInt32BigEndian(data, value);
        }
        for(int i=0;i<4;i++) {
            page.Data[offset+i] = data[i];
        }
    }
    
    public byte[] Read(ulong address, int length) {
        if (address >= Size) {
            throw new InvalidAddressException($"Address out of bounds. Expected Range: [0,{Size}[. Got: {address}");
        }

        if (address + (ulong)length >= Size) {
            throw new InvalidAddressException($"Data out of bounds. Expected Range: [0,{Size}[. Got: {address + (ulong)length}");
        }
        
        byte[] data = new byte[length];
        for (ulong i = 0; i < (ulong)length; i++) {
            data[i] = ReadByte(address + i);
        }
        return data;
    }

    public void Write(ulong address, byte[] data) {
        if (address >= Size) {
            throw new InvalidAddressException($"Address out of bounds. Expected Range: [0,{Size}[. Got: {address}");
        }

        if (address + (ulong)data.Length >= Size) {
            throw new InvalidAddressException($"Data out of bounds. Expected Range: [0,{Size}[. Got: {address + (ulong)data.Length}");
        }
        
        // vai ser lento, mas faz parte
        for (ulong i = 0; i < (ulong)data.Length; i++) {
            WriteByte(address + i, data[i]);
        }
    }

    public void Read(ulong address, Span<byte> bytes) {
        if (address >= Size) {
            throw new InvalidAddressException($"Address out of bounds. Expected Range: [0,{Size}[. Got: {address}");
        }

        if (address + (ulong)bytes.Length >= Size) {
            throw new InvalidAddressException($"Data out of bounds. Expected Range: [0,{Size}[. Got: {address + (ulong)bytes.Length}");
        }
        
        for (int i = 0; i < bytes.Length; i++) {
            bytes[i] = ReadByte(address + (ulong)i);
        }
    }

    public void Write(ulong address, Span<byte> bytes) {
        if (address >= Size) {
            throw new InvalidAddressException($"Address out of bounds. Expected Range: [0,{Size}[. Got: {address}");
        }

        if (address + (ulong)bytes.Length >= Size) {
            throw new InvalidAddressException($"Data out of bounds. Expected Range: [0,{Size}[. Got: {address + (ulong)bytes.Length}");
        }
        
        // vai ser lento, mas faz parte
        for (int i = 0; i < bytes.Length; i++) {
            WriteByte(address + (ulong)i, bytes[i]);
        }
    }

    public void Read(ulong address, Span<int> words) {
        if (address >= Size) {
            throw new InvalidAddressException($"Address out of bounds. Expected Range: [0,{Size}[. Got: {address}");
        }

        if (address + (ulong)(words.Length * 4) >= Size) {
            throw new InvalidAddressException($"Data out of bounds. Expected Range: [0,{Size}[. Got: {address + (ulong)(words.Length * 4)}");
        }
        
        for (int i = 0; i < words.Length; i++) {
            words[i] = ReadWord(address + (ulong)(i * 4));
        }
    }

    public void Write(ulong address, Span<int> words) {
        if (address >= Size) {
            throw new InvalidAddressException($"Address out of bounds. Expected Range: [0,{Size}[. Got: {address}");
        }

        if (address + (ulong)(words.Length * 4) >= Size) {
            throw new InvalidAddressException($"Data out of bounds. Expected Range: [0,{Size}[. Got: {address + (ulong)(words.Length * 4)}");
        }
        
        // vai ser lento, mas faz parte
        for (int i = 0; i < words.Length; i++) {
            WriteWord(address + (ulong)(i * 4), words[i]);
        }
    }

    /// <summary>
    /// Returns the index of the least recently used page.
    /// This index is relative to the <see cref="loadedPages"/> array.
    /// </summary>
    /// <returns>The index of the least recently used page</returns>
    private int LeastRecentlyUsedPage() {
        debugInfo.PageLoads++;
        long minTime = long.MaxValue;
        int minIndex = -1;
        for (int i = 0; i < maxLoadedPages; i++) {
            if (lastAccessTime[i] >= minTime) continue;
            minTime = lastAccessTime[i];
            minIndex = i;
        }
        return minIndex;
    }

    /// <summary>
    /// Checks if the page is loaded and loads it if not.
    /// Already unloads a page if the memory is full.
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <returns></returns>
    private int EnsureLoaded(int pageNumber) {
        int pageIndex = pageIndices[pageNumber];
        if (pageIndex != -1) return pageIndex;
        
        // nao esta carregado. acha um slot vazio
        int emptySlotIndex = Array.FindIndex(loadedPages, p => p == null);
        if (emptySlotIndex == -1) {
            // descarrega o mais antigo
            int lru = LeastRecentlyUsedPage();
            UnloadPage(lru);
            emptySlotIndex = lru;
        }
        LoadPage(pageNumber, emptySlotIndex);
        return emptySlotIndex;
    }

    /// <summary>
    /// Unloads a page from the <see cref="loadedPages"/> array.
    /// </summary>
    /// <param name="index">The index of the page to unload</param>
    private void UnloadPage(int index){
        debugInfo.PageUnloads++;
        
        Page p = loadedPages[index]!;
        if(p.IsDirty){
            // save to disk
            coldStorage.WritePage(p);
        }
        
        loadedPages[index] = null;
        lastAccessTime[index] = 0;
        int idx = Array.FindIndex(pageIndices, i => i == index);
        pageIndices[idx] = -1;
    }

    /// <summary>
    /// Loads a page into the <see cref="loadedPages"/> array.
    /// </summary>
    /// <param name="pageIndex">The global number of the page</param>
    /// <param name="destinationIndex">The index of the slot to load the page. This corresponds to the <see cref="loadedPages"/> array</param>
    private void LoadPage(int pageIndex, int destinationIndex){
        // logic
        Page page = coldStorage.ReadPage(pageIndex);
        loadedPages[destinationIndex] = page;
        pageIndices[pageIndex] = destinationIndex;
        lastAccessTime[destinationIndex] = DateTime.Now.Ticks;
    }

    public MemoryDebugInfo GetDebugInfo() {
        return (MemoryDebugInfo)debugInfo.Clone();
    }

    public void Dispose() {
        foreach(Page? p in loadedPages)
        {
            if (p is null) {
                continue;
            }
            if (p.IsDirty) {
                coldStorage.WritePage(p);
            }
        }
        coldStorage.Dispose();
    }
}