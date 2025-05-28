using SAAE.Engine.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Test.Memory;

[TestClass]
public class MemoryTest {

    [TestMethod]
    public void TestSmallSplitBase() {
        Span<byte> data = new byte[256];
        Random.Shared.NextBytes(data);

        string tempPath = Path.GetTempFileName();
        MemoryConfiguration config = new() {
            ColdStoragePath = tempPath,
            ColdStorageOptimization = true,
            MaxLoadedPages = 2,
            PageSize = 64,
            Size = 512
        };
        int baseAddress = 0x0;
        using (Engine.Memory.Memory memory = new(config)) {
            int address = baseAddress;
            foreach(byte b in data) {
                memory.WriteByte((ulong)address, b);
                address++;
            }
        }
        using (Engine.Memory.Memory memory = new(config)) {
            int address = baseAddress;
            foreach(byte b in data) {
                Assert.AreEqual(b, memory.ReadByte((ulong)address));
                address++;
            }
        }
        File.Delete(tempPath);
    }

    [TestMethod]
    public void TestSmallSplitBroad() {
        Span<byte> data = new byte[256];
        Random.Shared.NextBytes(data);

        string tempPath = Path.GetTempFileName();
        MemoryConfiguration config = new() {
            ColdStoragePath = tempPath,
            ColdStorageOptimization = true,
            MaxLoadedPages = 2,
            PageSize = 64,
            Size = 512
        };
        int baseAddress = 0x2;
        using (Engine.Memory.Memory memory = new(config)) {
            int address = baseAddress;
            foreach (byte b in data) {
                memory.WriteByte((ulong)address, b);
                address++;
            }
        }
        using (Engine.Memory.Memory memory = new(config)) {
            int address = baseAddress;
            foreach (byte b in data) {
                Assert.AreEqual(b, memory.ReadByte((ulong)address));
                address++;
            }
        }
        File.Delete(tempPath);
    }
    
    [TestMethod]
    public void TestSmallJoinBase() {
        Span<byte> data = new byte[256];
        Random.Shared.NextBytes(data);

        string tempPath = Path.GetTempFileName();
        MemoryConfiguration config = new() {
            ColdStoragePath = tempPath,
            ColdStorageOptimization = true,
            MaxLoadedPages = 2,
            PageSize = 64,
            Size = 512
        };
        int baseAddress = 0x0;
        using (Engine.Memory.Memory memory = new(config)) {
            int address = baseAddress;
            foreach (byte b in data) {
                memory.WriteByte((ulong)address, b);
                address++;
            }
            address = baseAddress;
            foreach (byte b in data) {
                Assert.AreEqual(b, memory.ReadByte((ulong)address));
                address++;
            }
        }
        File.Delete(tempPath);
    }
    
    [TestMethod]
    public void TestSmallJoinBroad() {
        Span<byte> data = new byte[256];
        Random.Shared.NextBytes(data);

        string tempPath = Path.GetTempFileName();
        MemoryConfiguration config = new() {
            ColdStoragePath = tempPath,
            ColdStorageOptimization = true,
            MaxLoadedPages = 2,
            PageSize = 64,
            Size = 512
        };
        int baseAddress = 0x2;
        using (Engine.Memory.Memory memory = new(config)){
            int address = baseAddress;
            foreach (byte b in data) {
                memory.WriteByte((ulong)address, b);
                address++;
            }
            address = baseAddress;
            foreach (byte b in data) {
                Assert.AreEqual(b, memory.ReadByte((ulong)address));
                address++;
            }
        }
        File.Delete(tempPath);
    }

    [TestMethod]
    public void TestBigSplitBase() {
        int mb = 1024 * 1024;
        int size = 5* mb;
        Span<byte> data = new byte[size];
        Random.Shared.NextBytes(data);

        string tempPath = Path.GetTempFileName();
        MemoryConfiguration config = new() {
            ColdStoragePath = tempPath,
            ColdStorageOptimization = true,
            MaxLoadedPages = 4,
            PageSize = 4096,
            Size = 64ul*(ulong)mb
        };
        int baseAddress = 0x0;
        using (Engine.Memory.Memory memory = new(config)) {
            int address = baseAddress;
            foreach (byte b in data) {
                memory.WriteByte((ulong)address, b);
                address++;
            }
        }
        using (Engine.Memory.Memory memory = new(config)) {
            int address = baseAddress;
            foreach (byte b in data) {
                Assert.AreEqual(b, memory.ReadByte((ulong)address));
                address++;
            }
        }
        File.Delete(tempPath);
    }

    [TestMethod]
    public void TestBigSplitBroad() {
        int mb = 1024 * 1024;
        int size = 5 * mb;
        Span<byte> data = new byte[size];
        Random.Shared.NextBytes(data);

        string tempPath = Path.GetTempFileName();
        MemoryConfiguration config = new() {
            ColdStoragePath = tempPath,
            ColdStorageOptimization = true,
            MaxLoadedPages = 4,
            PageSize = 4096,
            Size = 64ul * (ulong)mb
        };
        int baseAddress = 0x20*mb;
        using (Engine.Memory.Memory memory = new(config)) {
            int address = baseAddress;
            foreach (byte b in data) {
                memory.WriteByte((ulong)address, b);
                address++;
            }
        }
        using (Engine.Memory.Memory memory = new(config)) {
            int address = baseAddress;
            foreach (byte b in data) {
                Assert.AreEqual(b, memory.ReadByte((ulong)address));
                address++;
            }
        }
        File.Delete(tempPath);
    }
    
    [TestMethod]
    public void TestBigJoinBase() {
        int mb = 1024 * 1024;
        int size = 5 * mb;
        Span<byte> data = new byte[size];
        Random.Shared.NextBytes(data);

        string tempPath = Path.GetTempFileName();
        MemoryConfiguration config = new() {
            ColdStoragePath = tempPath,
            ColdStorageOptimization = true,
            MaxLoadedPages = 4,
            PageSize = 4096,
            Size = 64ul * (ulong)mb
        };
        int baseAddress = 0x0;
        using (Engine.Memory.Memory memory = new(config)) {
            int address = baseAddress;
            foreach (byte b in data) {
                memory.WriteByte((ulong)address, b);
                address++;
            }
            address = baseAddress;
            foreach (byte b in data) {
                Assert.AreEqual(b, memory.ReadByte((ulong)address));
                address++;
            }
        }
        File.Delete(tempPath);
    }
    
    [TestMethod]
    public void TestBigJoinBroad() {
        int mb = 1024 * 1024;
        int size = 5 * mb;
        Span<byte> data = new byte[size];
        Random.Shared.NextBytes(data);

        string tempPath = Path.GetTempFileName();
        MemoryConfiguration config = new() {
            ColdStoragePath = tempPath,
            ColdStorageOptimization = true,
            MaxLoadedPages = 4,
            PageSize = 4096,
            Size = 64ul * (ulong)mb
        };
        int baseAddress = 0x20*mb;
        using (Engine.Memory.Memory memory = new(config)) {
            int address = baseAddress;
            foreach (byte b in data) {
                memory.WriteByte((ulong)address, b);
                address++;
            }
            address = baseAddress;
            foreach (byte b in data) {
                Assert.AreEqual(b, memory.ReadByte((ulong)address));
                address++;
            }
        }
        File.Delete(tempPath);
    }

    [TestMethod]
    public void TestBigEndian() {
        string tempPath = Path.GetTempFileName();
        MemoryConfiguration config = new() {
            ColdStoragePath = tempPath,
            ColdStorageOptimization = true,
            MaxLoadedPages = 2,
            PageSize = 64,
            Size = 512,
            Endianess = Endianess.BigEndian
        };
        using Engine.Memory.Memory memory = new(config);
        const int expectedData = 0x01020304;
        byte[] expectedBytes = [
            0x01, 0x02, 0x03, 0x04,
        ];
        memory.Write(0, expectedBytes);
            
        Assert.AreEqual(0x01020304, memory.ReadWord(0));
            
        memory.WriteWord(4, expectedData);

        byte[] read = memory.Read(4, 4);
        CollectionAssert.AreEqual(expectedBytes, read);
    }

    [TestMethod]
    public void TestLittleEndian() {
        string tempPath = Path.GetTempFileName();
        MemoryConfiguration config = new() {
            ColdStoragePath = tempPath,
            ColdStorageOptimization = true,
            MaxLoadedPages = 2,
            PageSize = 64,
            Size = 512,
            Endianess = Endianess.LittleEndian
        };
        using Engine.Memory.Memory memory = new(config);
        const int expectedData = 0x01020304;
        byte[] expectedBytes = [
            0x04, 0x03, 0x02, 0x01,
        ];
        memory.Write(0, expectedBytes);
            
        Assert.AreEqual(0x01020304, memory.ReadWord(0));
            
        memory.WriteWord(4, expectedData);

        byte[] read = memory.Read(4, 4);
        CollectionAssert.AreEqual(expectedBytes, read);
    }
}
