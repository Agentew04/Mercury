namespace Mercury.Engine.Memory;

internal sealed class VolatileStorage : IStorage
{
    private Dictionary<int,Page> Pages { get; } = [];
    private MemoryConfiguration Config { get; }

    public VolatileStorage(MemoryConfiguration config)
    {
        Config = config;
        if (config.Size % config.PageSize != 0)
        {
            throw new ArgumentException("The total size of the memory must be a multiple of the page size.", nameof(config));
        }
        
        if (config.PageSize == 0)
        {
            throw new ArgumentException("The page size cannot be zero.", nameof(config));
        }
        
        if (config.Size == 0)
        {
            throw new ArgumentException("The total size of the memory cannot be zero.", nameof(config));
        }

        if (config.StorageType != StorageType.Volatile)
        {
            throw new ArgumentException("The storage type must be Volatile for this storage implementation.", nameof(config));
        }
    }
    
    public void Dispose()
    {
        // nao faz nada.
    }

    public void WritePage(Page page)
    {
        if (page.Number < 0 || page.Number >= (int)(Config.Size / Config.PageSize))
        {
            throw new ArgumentOutOfRangeException(nameof(page), $"The page number is out of bounds. Expected range: [0,{Config.Size / Config.PageSize}[. Got: {page.Number}.");
        }
        Pages[page.Number] = page;
    }

    public Page ReadPage(int pageNumber)
    {
        if (pageNumber < 0 || pageNumber >= (int)(Config.Size / Config.PageSize))
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), $"The page number is out of bounds. Expected range: [0,{Config.Size / Config.PageSize}[. Got: {pageNumber}.");
        }

        if (Pages.TryGetValue(pageNumber, out Page? page))
        {
            return page;
        }

        page = new Page(Config.PageSize, pageNumber);
        Pages[page.Number] = page;
        return page;
    }
}