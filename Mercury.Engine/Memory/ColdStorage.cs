namespace Mercury.Engine.Memory;

internal sealed class ColdStorage : IStorage {

    private readonly FileStream fs;

    private readonly ulong pageCount;
    private readonly ulong pageSize;

    public ColdStorage(MemoryConfiguration config) {
        if (config.StorageType != StorageType.FileOriginal) {
            throw new InvalidOperationException("ColdStorage class is not the optimized one. Error in VirtualMemory logic!");
        }
        if(config.Size % config.PageSize != 0) {
            throw new ArgumentException("The total size of the memory must be a multiple of the page size.", nameof(config));
        }
        if (config.ForceColdStorageReset && File.Exists(config.ColdStoragePath)) {
            File.Delete(config.ColdStoragePath);
        }
        fs = File.Open(config.ColdStoragePath, FileMode.OpenOrCreate);
        fs.SetLength((long)config.Size);
        pageSize = config.PageSize;
        pageCount = config.Size / config.PageSize;
    }

    public void Dispose() {
        fs.Dispose();
    }

    public Page ReadPage(int pageNumber) {
        if(pageNumber < 0 || (ulong)pageNumber >= pageCount) {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), $"The page number is out of bounds. Expected range: [0,{pageCount}[. Got: {pageNumber}.");
        }
        byte[] data = new byte[pageSize];
        fs.Seek((long)((ulong)pageNumber * pageSize), SeekOrigin.Begin);
        fs.ReadExactly(data);
        return new Page(pageSize, pageNumber) {
            Data = data,
            IsDirty = false
        };
    }

    public void WritePage(Page page) {
        if (page.Number < 0 || (ulong)page.Number >= pageCount) {
            throw new ArgumentOutOfRangeException(nameof(page), $"The page number is out of bounds. Expected range: [0,{pageCount}[. Got: {page.Number}.");
        }

        fs.Seek((long)((ulong)page.Number * pageSize), SeekOrigin.Begin);
        if(page.Data.Length != (int)pageSize) {
            throw new ArgumentOutOfRangeException(nameof(page), $"The size of the page data is invalid! Expected: {pageSize}. Got: {page.Data.Length}.");
        }
        fs.Write(page.Data);
    }
}
