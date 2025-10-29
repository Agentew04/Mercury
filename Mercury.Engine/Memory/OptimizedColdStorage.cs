using System.Text;

namespace Mercury.Engine.Memory;

internal sealed class OptimizedColdStorage : IStorage {
    private FileStream fs;
    private BinaryReader br;
    private BinaryWriter bw;

    // VM\0COLD\0STORAGE\0
    private readonly byte[] _fileSignature = [86, 77, 0, 67, 79, 76, 68, 0, 83, 84, 79, 82, 65, 71, 69, 0];

    private readonly InfoHeader _header = new();
    /// <summary>
    /// Array that defines if a page is registered on the file or not
    /// </summary>
    private byte[] _registerTable = null!;
    private ulong _registerTableAddress = ulong.MinValue;

    private ulong[] _regionTable = null!;
    private ulong _regionTableAddress = ulong.MinValue;

    private const uint PagesPerJumpTable = 256;
    
    public OptimizedColdStorage(MemoryConfiguration config) {
        if(config.Size % config.PageSize != 0) {
            throw new ArgumentException("Size must be a multiple of PageSize.");
        }

        if (config.StorageType != StorageType.FileOptimized)
        {
            throw new Exception("Error in VirtualMemory logic! OptimizedColdStorage class is not the one that should be used. Check your configuration.");
        }

        if (File.Exists(config.ColdStoragePath)) {
            fs = new FileStream(config.ColdStoragePath, FileMode.Open);
            br = new BinaryReader(fs, Encoding.ASCII, true);
            bw = new BinaryWriter(fs, Encoding.ASCII, true);
            if(fs.Length == 0 || config.ForceColdStorageReset) {
                br.Close();
                bw.Close();
                fs.Close();
                File.Delete(config.ColdStoragePath);
                fs = new FileStream(config.ColdStoragePath, FileMode.CreateNew);
                br = new BinaryReader(fs, Encoding.ASCII, true);
                bw = new BinaryWriter(fs, Encoding.ASCII, true);
                CreateNew(config);
            } else {
                // read information from file
                ReadFromFile();
            }
        } else {
            fs = new FileStream(config.ColdStoragePath, FileMode.CreateNew);
            br = new BinaryReader(fs, Encoding.ASCII, true);
            bw = new BinaryWriter(fs, Encoding.ASCII, true);
            // create file
            CreateNew(config);
        }
    }

    private void ReadFromFile() {
        byte[] signature = br.ReadBytes(16);
        if (!signature.SequenceEqual(_fileSignature)) {
            throw new InvalidDataException("Invalid file signature.");
        }
        
        _header.PageSize = br.ReadUInt32();
        _header.PageCount = br.ReadUInt32();
        
        int registerTableByteSize = (int)Math.Ceiling(_header.PageCount / 8.0);
        _registerTableAddress = (ulong)fs.Position;
        _registerTable = br.ReadBytes(registerTableByteSize);
        uint regionCount = (uint)MathF.Ceiling(_header.PageCount / (float)PagesPerJumpTable);
        _regionTable = new ulong[regionCount];
        _regionTableAddress = (ulong)fs.Position;
        for (int i = 0; i < regionCount; i++) {
            _regionTable[i] = br.ReadUInt64();
        }
    }

    private void CreateNew(MemoryConfiguration config) {
        bw.Write(_fileSignature);
        _header.PageSize = (uint)config.PageSize;
        bw.Write(_header.PageSize);
        _header.PageCount = (uint)(config.Size / config.PageSize); // garantido que eh multiplo
        bw.Write(_header.PageCount);
        _registerTable = new byte[(int)Math.Ceiling(_header.PageCount / 8.0)];
        _registerTableAddress = (ulong)fs.Position;
        bw.Write(_registerTable);
        //Console.WriteLine($"RegisterTableAddress: {_regionTableAddress}");
        uint regionCount = (uint)MathF.Ceiling(_header.PageCount / (float)PagesPerJumpTable);
        _regionTable = new ulong[regionCount];
        _regionTableAddress = (ulong)fs.Position;
        //Console.WriteLine($"Region count: {regionCount}; RegionTable Address: {_regionTableAddress}");
        for (int i = 0; i < regionCount; i++) {
            _regionTable[i] = ulong.MinValue;
            bw.Write(_regionTable[i]);
        }
        bw.Flush();
    }

    private bool IsPageRegistered(int pageNumber) {
        if (pageNumber >= _header.PageCount) {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number out of bounds.");
        }
        int byteIndex = pageNumber / 8;
        int bitIndex = pageNumber % 8;
        byte mask = (byte)(1 << bitIndex);
        return (_registerTable[byteIndex] & mask) != 0;
    }

    private void RegisterPage(int pageNumber) {
        if (pageNumber >= _header.PageCount) {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number out of bounds.");
        }
        int byteIndex = pageNumber / 8;
        int bitIndex = pageNumber % 8;
        byte mask = (byte)(1 << bitIndex);
        _registerTable[byteIndex] |= mask;
        long pastPos = fs.Position;
        fs.Seek((long)_registerTableAddress+byteIndex, SeekOrigin.Begin);
        bw.Write(_registerTable[byteIndex]);
        bw.Flush();
        fs.Seek(pastPos, SeekOrigin.Begin);
    }

    public void WritePage(Page page) {
        if (page.Number < 0 || page.Number >= _header.PageCount) {
            throw new ArgumentOutOfRangeException(nameof(page),
                $"Page number out of range! Got: {page.Number}. Expected: [0,{_header.PageCount}[");
        }
        
        if (!page.IsDirty && IsPageRegistered(page.Number)) {
            Console.WriteLine($"Page {page.Number} is not dirty. Skipping write.");
            return;
        }
        
        uint regionIndex = (uint)page.Number / PagesPerJumpTable;
        uint pageNumberOffset = (uint)page.Number % PagesPerJumpTable;
        if (_regionTable[regionIndex] == ulong.MinValue) {
            Console.WriteLine("Jump table did not exist. Creating one at: {0}", fs.Position);
            // update region table
            _regionTable[regionIndex] = (ulong)fs.Length;
            fs.Seek((long)(_regionTableAddress + regionIndex * sizeof(ulong)), SeekOrigin.Begin);
            bw.Write(_regionTable[regionIndex]);
            bw.Flush();
            
            // create empty jump table
            fs.Seek((int)_regionTable[regionIndex], SeekOrigin.Begin);
            for (int i = 0; i < PagesPerJumpTable; i++) {
                bw.Write(ulong.MinValue);
            }
            bw.Flush();
            fs.Flush();
        }
        
        // write page
        if (IsPageRegistered(page.Number)) {
            // get page address
            ulong jumpTableAddress = _regionTable[regionIndex];
            fs.Seek((long)jumpTableAddress, SeekOrigin.Begin);
            fs.Seek(pageNumberOffset * sizeof(ulong), SeekOrigin.Current);
            ulong pageAddress = br.ReadUInt64();
            
            // overwrite data
            fs.Seek((long)pageAddress, SeekOrigin.Begin);
            Console.WriteLine("Address: " + pageAddress);
            fs.Write(page.Data);
            fs.Flush();
        }
        else {
            RegisterPage(page.Number);
            
            // update jump table
            ulong pageAddress = (ulong)fs.Length;
            fs.Seek((long)_regionTable[regionIndex], SeekOrigin.Begin);
            fs.Seek(pageNumberOffset * sizeof(ulong), SeekOrigin.Current);
            bw.Write(pageAddress);
            bw.Flush();
            //Console.WriteLine("Registered page {0} at address {1}", page.Number, pageAddress);
            
            // write page data
            fs.Seek((long)pageAddress, SeekOrigin.Begin);
            fs.Write(page.Data);
            fs.Flush();
        }
        page.IsDirty = false;
    }

    public Page ReadPage(int pageNumber) {
        if (pageNumber < 0 || pageNumber >= _header.PageCount) {
            throw new ArgumentOutOfRangeException(nameof(pageNumber),
                $"Page number out of range! Got: {pageNumber}. Expected: [0,{_header.PageCount}[");
        }
        
        uint regionIndex = (uint)pageNumber / PagesPerJumpTable;
        uint pageNumberOffset = (uint)pageNumber % PagesPerJumpTable;
        // check if jump table for region exists
        if (_regionTable[regionIndex] == ulong.MinValue) {
            //Console.WriteLine($"Creating jump table for region {regionIndex}");
            // update region table
            _regionTable[regionIndex] = (ulong)fs.Length;
            fs.Seek((long)(_regionTableAddress + regionIndex * sizeof(ulong)), SeekOrigin.Begin);
            bw.Write(_regionTable[regionIndex]);
            
            // create empty jump table
            fs.Seek((int)_regionTable[regionIndex], SeekOrigin.Begin);
            for (int i = 0; i < PagesPerJumpTable; i++) {
                bw.Write(ulong.MinValue);
            }
            bw.Flush();
        }
        
        if (IsPageRegistered(pageNumber)) {
            Console.WriteLine($"Page {pageNumber} already exists, reading");
            // page exists, read data
            ulong jumpTableAddress = _regionTable[regionIndex];
            fs.Seek((long)jumpTableAddress, SeekOrigin.Begin);
            fs.Seek(pageNumberOffset * sizeof(ulong), SeekOrigin.Current);
            ulong pageAddress = br.ReadUInt64();
            fs.Seek((long)pageAddress, SeekOrigin.Begin);
            byte[] data = br.ReadBytes((int)_header.PageSize);
            return new Page(_header.PageSize, pageNumber) {
                IsDirty = false,
                Data = data
            };
        }
        else {
            // page does not exist
            //Console.WriteLine($"Page {pageNumber} did not exist, creating");
            RegisterPage(pageNumber);
            // update jump table
            ulong pageAddress = (ulong)fs.Length;
            ulong jumpTableAddress = _regionTable[regionIndex];
            fs.Seek((long)jumpTableAddress, SeekOrigin.Begin);
            fs.Seek(pageNumberOffset * sizeof(ulong), SeekOrigin.Current);
            bw.Write(pageAddress);
            bw.Flush();

            // create empty and return
            fs.Seek((long)pageAddress, SeekOrigin.Begin);
            byte[] data = new byte[_header.PageSize];
            fs.Write(data);
            fs.Flush();
            return new Page(_header.PageSize, pageNumber) {
                Data = data,
                IsDirty = false
            };
        }
    }
    
    public void Dispose() {
        br.Dispose();
        bw.Dispose();
        fs.Dispose();
    }

    private class InfoHeader {
        
        /// <summary>
        /// The size of each page.
        /// </summary>
        public uint PageSize { get; set; }
        
        /// <summary>
        /// The total amount of pages that can be stored in the file.
        /// </summary>
        public uint PageCount { get; set; }
    }
}
