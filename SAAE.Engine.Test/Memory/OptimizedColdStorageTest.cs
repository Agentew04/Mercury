using SAAE.Engine.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Test.Memory;
[TestClass]
public class OptimizedColdStorageTest {

    [TestMethod]
    public void TestSmallSplit() {
        string? tempPath = Path.GetTempFileName();
        MemoryConfiguration config = new() {
            ColdStoragePath = tempPath,
            StorageType = StorageType.FileOriginal,
            Size = 4096,
            PageSize = 256
        };
        byte[] p1Data = new byte[256];
        byte[] p2Data = new byte[256];
        byte[] p3Data = new byte[256];
        byte[] p4Data = new byte[256];
        Random.Shared.NextBytes(p1Data);
        Random.Shared.NextBytes(p2Data);
        Random.Shared.NextBytes(p3Data);
        Random.Shared.NextBytes(p4Data);

        Page p1 = new(256, 0) {
            Data = p1Data,
            IsDirty = true
        };
        Page p2 = new(256, 15) {
            Data = p2Data,
            IsDirty = true
        };
        Page p3 = new(256, 7) {
            Data = p3Data,
            IsDirty = true
        };
        Page p4 = new(256, 1) {
            Data = p4Data,
            IsDirty = true
        };

        // write pages
        using (ColdStorage storage = new(config)) {
            storage.WritePage(p1);
            storage.WritePage(p2);
            storage.WritePage(p3);
            storage.WritePage(p4);
        }
        // read back
        using (ColdStorage storage = new(config)) {
            Page p1r = storage.ReadPage(0);
            Page p2r = storage.ReadPage(15);
            Page p3r = storage.ReadPage(7);
            Page p4r = storage.ReadPage(1);

            Assert.IsTrue(p1r.Data.SequenceEqual(p1Data));
            Assert.IsTrue(p2r.Data.SequenceEqual(p2Data));
            Assert.IsTrue(p3r.Data.SequenceEqual(p3Data));
            Assert.IsTrue(p4r.Data.SequenceEqual(p4Data));
        }

        // delete the file
        File.Delete(config.ColdStoragePath);
    }

    [TestMethod]
    public void TestSmallJoined() {
        string? tempPath = Path.GetTempFileName();
        MemoryConfiguration config = new() {
            ColdStoragePath = tempPath,
            StorageType = StorageType.FileOriginal,
            Size = 4096,
            PageSize = 256
        };
        byte[] p1Data = new byte[256];
        byte[] p2Data = new byte[256];
        byte[] p3Data = new byte[256];
        byte[] p4Data = new byte[256];
        Random.Shared.NextBytes(p1Data);
        Random.Shared.NextBytes(p2Data);
        Random.Shared.NextBytes(p3Data);
        Random.Shared.NextBytes(p4Data);

        Page p1 = new(256, 0) {
            Data = p1Data,
            IsDirty = true
        };
        Page p2 = new(256, 15) {
            Data = p2Data,
            IsDirty = true
        };
        Page p3 = new(256, 7) {
            Data = p3Data,
            IsDirty = true
        };
        Page p4 = new(256, 1) {
            Data = p4Data,
            IsDirty = true
        };

        // write pages
        using (ColdStorage storage = new(config)) {
            storage.WritePage(p1);
            storage.WritePage(p2);
            storage.WritePage(p3);
            storage.WritePage(p4);

            Page p1r = storage.ReadPage(0);
            Page p2r = storage.ReadPage(15);
            Page p3r = storage.ReadPage(7);
            Page p4r = storage.ReadPage(1);

            Assert.IsTrue(p1r.Data.SequenceEqual(p1Data));
            Assert.IsTrue(p2r.Data.SequenceEqual(p2Data));
            Assert.IsTrue(p3r.Data.SequenceEqual(p3Data));
            Assert.IsTrue(p4r.Data.SequenceEqual(p4Data));
        }

        // delete the file
        File.Delete(config.ColdStoragePath);
    }

    [TestMethod]
    public void TestBigSplit() {
        const int MB = 1024 * 1024;
        int fileSize = 5 * MB;

        // generate random data
        Span<byte> data = new byte[fileSize];
        Random.Shared.NextBytes(data);

        string tempPath = Path.GetTempFileName();
        if (File.Exists(tempPath)) {
            File.Delete(tempPath);
        }
        MemoryConfiguration config = new() {
            ColdStoragePath = tempPath,
            StorageType = StorageType.FileOptimized,
            Size = 64 * MB,
            PageSize = 1024
        };

        int baseAddress = 0x0;
        // write all data
        using (OptimizedColdStorage storage = new(config)) {
            for (int i = 0; i < fileSize; i += 1024) {
                Page page = new(1024, baseAddress + i / 1024) {
                    Data = data.Slice(i, 1024).ToArray(),
                    IsDirty = true
                };
                storage.WritePage(page);
            }
        }

        using (OptimizedColdStorage storage = new(config)) {
            Span<byte> readData = new byte[fileSize];
            for (int i = 0; i < fileSize; i += 1024) {
                Page page = storage.ReadPage(baseAddress + i / 1024);
                page.Data.CopyTo(readData.Slice(i, 1024));
            }

            Assert.IsTrue(data.SequenceEqual(readData));
        }

        File.Delete(tempPath);
    }

    [TestMethod]
    public void TestBigJoined() {
        const int MB = 1024 * 1024;
        int fileSize = 5 * MB;

        // generate random data
        Span<byte> data = new byte[fileSize];
        Random.Shared.NextBytes(data);

        string tempPath = Path.GetTempFileName();
        if (File.Exists(tempPath)) {
            File.Delete(tempPath);
        }
        MemoryConfiguration config = new() {
            ColdStoragePath = tempPath,
            StorageType = StorageType.FileOptimized,
            Size = 64 * MB,
            PageSize = 1024
        };

        int baseAddress = 0x0;
        // write all data
        using (OptimizedColdStorage storage = new(config)) {
            for (int i = 0; i < fileSize; i += 1024) {
                Page page = new(1024, baseAddress + i / 1024) {
                    Data = data.Slice(i, 1024).ToArray(),
                    IsDirty = true
                };
                storage.WritePage(page);
            }

            Span<byte> readData = new byte[fileSize];
            for (int i = 0; i < fileSize; i += 1024) {
                Page page = storage.ReadPage(baseAddress + i / 1024);
                page.Data.CopyTo(readData.Slice(i, 1024));
            }

            Assert.IsTrue(data.SequenceEqual(readData));
        }

        File.Delete(tempPath);
    }
}