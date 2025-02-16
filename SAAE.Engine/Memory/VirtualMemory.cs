namespace SAAE.Engine.Memory;

public sealed class VirtualMemory : IDisposable
{
    /// <summary>
    /// Total size of the virtual memory in bytes.
    /// </summary>
    private readonly ulong _size;
    
    public ulong Size => _size;

    /// <summary>
    /// The size of each page.
    /// </summary>
    private readonly ulong _pageSize;

    /// <summary>
    /// Defines the amount of concurrently loaded pages in memory.
    /// </summary>
    private readonly uint _maxLoadedPages;

    /// <summary>
    /// Total number of pages in the virtual memory. Includes
    /// loaded and unloaded pages.
    /// </summary>
    private readonly uint _totalPageCount;

    /// <summary>
    /// Array of loaded pages. To access this array, use
    /// the lookup table <see cref="_pageIndices"/>.
    /// Always has a size of <see cref="_maxLoadedPages"/>.
    /// If a slot is empty, it can be a <see langword="null"/>.
    /// </summary>
    private readonly Page?[] _loadedPages;

    /// <summary>
    /// Lookup table to find the index of a page in the
    /// <see cref="_loadedPages"/> array. It is always the size
    /// of <see cref="_totalPageCount"/>.
    /// </summary>
    private readonly int[] _pageIndices;

    /// <summary>
    /// Last access time of each loaded page. Used to
    /// define which page to unload when the memory is full.
    /// Is has the same size of <see cref="_loadedPages"/>.
    /// </summary>
    private readonly long[] _lastAccessTime;

    private readonly bool _collectDebugInfo;

    private readonly VirtualMemoryDebugInfo _debugInfo = new();
    
    private readonly IStorage _coldStorage;

    public VirtualMemory(VirtualMemoryConfiguration config)
    {
        _collectDebugInfo = config.CollectDebugInfo;
        _size = config.Size;
        _pageSize = config.PageSize;
        _maxLoadedPages = config.MaxLoadedPages;
        _totalPageCount = (uint)(_size / _pageSize);
        _loadedPages = new Page[_maxLoadedPages];
        Array.Fill(_loadedPages, null);
        _lastAccessTime = new long[_maxLoadedPages];
        Array.Fill(_lastAccessTime, 0);
        _pageIndices = new int[_totalPageCount];
        Array.Fill(_pageIndices, -1);
        if (config.ColdStorageOptimization) {
            _coldStorage = new OptimizedColdStorage(config);
        } else {
            _coldStorage = new ColdStorage(config);
        }
    }

    public byte ReadByte(ulong address)
    {
        // sanitizar input
        if(address >= _size) {
            throw new InvalidAddressException($"Address out of bounds. Expected Range: [0,{_size}[. Got: {address}");
        }
        
        int pageNumber = (int)(address / _pageSize);
        if(pageNumber < 0 || pageNumber >= _totalPageCount){
            throw new InvalidAddressException($"Page number out of bounds. Expected Range: [0,{_totalPageCount}[. Got: {pageNumber}");
        }

        int pageIndex = EnsureLoaded(pageNumber);
        _lastAccessTime[pageIndex] = DateTime.Now.Ticks;

        Page page = _loadedPages[pageIndex]!;
        int offset = (int)(address % _pageSize);
        byte data = page.Data[offset];
        if(data != 0)
            Console.WriteLine($"Read byte {data} from address {address}");
        return data;
    }

    public void WriteByte(ulong address, byte value) {
        if(address >= _size) {
            throw new InvalidAddressException($"Address out of bounds. Expected Range: [0,{_size}[. Got: {address}");
        }
        
        int pageNumber = (int)(address / _pageSize);
        if(pageNumber < 0 || pageNumber >= _totalPageCount){
            throw new InvalidAddressException($"Page number out of bounds. Expected Range: [0,{_totalPageCount}[. Got: {pageNumber}");
        }

        int pageIndex = EnsureLoaded(pageNumber);
        _lastAccessTime[pageIndex] = DateTime.Now.Ticks;

        Page page = _loadedPages[pageIndex]!;
        int offset = (int)(address % _pageSize);
        if (page.Data[offset] != value) {
            page.IsDirty = true;
        }
        page.Data[offset] = value;
    }

    public int ReadWord(ulong address) {
        // sanitizar input
        if (address >= _size) {
            throw new InvalidAddressException($"Address out of bounds. Expected Range: [0,{_size}[. Got: {address}");
        }

        int pageNumber = (int)(address / _pageSize);
        if (pageNumber < 0 || pageNumber >= _totalPageCount) {
            throw new InvalidAddressException($"Page number out of bounds. Expected Range: [0,{_totalPageCount}[. Got: {pageNumber}");
        }

        int pageIndex = EnsureLoaded(pageNumber);
        _lastAccessTime[pageIndex] = DateTime.Now.Ticks;

        Page page = _loadedPages[pageIndex]!;
        int offset = (int)(address % _pageSize);
        int data = BitConverter.ToInt32(page.Data, offset);
        if (data != 0)
            Console.WriteLine($"Read byte {data} from address {address}");
        return data;
    }

    public void WriteWord(ulong address, int value) {
        if (address >= _size) {
            throw new InvalidAddressException($"Address out of bounds. Expected Range: [0,{_size}[. Got: {address}");
        }

        int pageNumber = (int)(address / _pageSize);
        if (pageNumber < 0 || pageNumber >= _totalPageCount) {
            throw new InvalidAddressException($"Page number out of bounds. Expected Range: [0,{_totalPageCount}[. Got: {pageNumber}");
        }

        int pageIndex = EnsureLoaded(pageNumber);
        _lastAccessTime[pageIndex] = DateTime.Now.Ticks;

        Page page = _loadedPages[pageIndex]!;
        int offset = (int)(address % _pageSize);
        if (page.Data[offset] != value) {
            page.IsDirty = true;
        }
        byte[] data = BitConverter.GetBytes(value);
        for(int i=0;i<4;i++) {
            page.Data[offset+i] = data[i];
        }
    }

    /// <summary>
    /// Returns the index of the least recently used page.
    /// This index is relative to the <see cref="_loadedPages"/> array.
    /// </summary>
    /// <returns>The index of the least recently used page</returns>
    private int LeastRecentlyUsedPage() {
        _debugInfo.PageLoads++;
        long minTime = long.MaxValue;
        int minIndex = -1;
        for (int i = 0; i < _maxLoadedPages; i++) {
            if (_lastAccessTime[i] >= minTime) continue;
            minTime = _lastAccessTime[i];
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
        int pageIndex = _pageIndices[pageNumber];
        if (pageIndex != -1) return pageIndex;
        
        Console.Write($"Page fault: {pageNumber}. ");
        // nao esta carregado. acha um slot vazio
        int emptySlotIndex = Array.FindIndex(_loadedPages, p => p == null);
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
    /// Unloads a page from the <see cref="_loadedPages"/> array.
    /// </summary>
    /// <param name="index">The index of the page to unload</param>
    private void UnloadPage(int index){
        _debugInfo.PageUnloads++;
        
        Page p = _loadedPages[index]!;
        if(p.IsDirty){
            // save to disk
            _coldStorage.WritePage(p);
        }
        
        _loadedPages[index] = null;
        _lastAccessTime[index] = 0;
        int idx = Array.FindIndex(_pageIndices, i => i == index);
        _pageIndices[idx] = -1;
    }

    /// <summary>
    /// Loads a page into the <see cref="_loadedPages"/> array.
    /// </summary>
    /// <param name="pageIndex">The global number of the page</param>
    /// <param name="destinationIndex">The index of the slot to load the page. This corresponds to the <see cref="_loadedPages"/> array</param>
    private void LoadPage(int pageIndex, int destinationIndex){
        // logic
        Page page = _coldStorage.ReadPage(pageIndex);
        _loadedPages[destinationIndex] = page;
        _pageIndices[pageIndex] = destinationIndex;
        _lastAccessTime[destinationIndex] = DateTime.Now.Ticks;
    }

    public VirtualMemoryDebugInfo GetDebugInfo() {
        return (VirtualMemoryDebugInfo)_debugInfo.Clone();
    }

    public void Dispose() {
        foreach(Page? p in _loadedPages)
        {
            if (p is null) {
                continue;
            }
            if (p.IsDirty) {
                _coldStorage.WritePage(p);
            }
        }
        _coldStorage.Dispose();
    }
}