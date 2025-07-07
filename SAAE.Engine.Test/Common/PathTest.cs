using System.Xml.Serialization;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using SAAE.Editor.Extensions;

namespace SAAE.Engine.Test.Common;

[TestClass]
public class PathTest {

    [TestMethod]
    public void TestRelativeDirectoryParts() {
        string path = "bin/test/folder";
        PathObject obj = path.ToDirectoryPath();
        
        Assert.IsFalse(obj.IsAbsolute);
        Assert.IsTrue(obj.IsDirectory);
        Assert.IsFalse(obj.IsFile);
        CollectionAssert.AreEqual(new[]{"bin", "test", "folder"}, obj.Parts);
    }

    [TestMethod]
    public void TestAbsoluteDirectoryPartsWindows() {
        string path = "C:\\Test\\Folder";
        PathObject obj = path.ToDirectoryPath();
        
        Assert.IsTrue(obj.IsAbsolute);
        Assert.IsFalse(obj.IsFile);
        Assert.IsTrue(obj.IsDirectory);
        CollectionAssert.AreEqual(new[]{"C:", "Test", "Folder"}, obj.Parts);
    }
    
    [TestMethod]
    public void TestAbsoluteDirectoryPartsLinux() {
        string path = "/Test/Folder/obj";
        PathObject obj = path.ToDirectoryPath();
        
        Assert.IsTrue(obj.IsAbsolute);
        Assert.IsFalse(obj.IsFile);
        Assert.IsTrue(obj.IsDirectory);
        CollectionAssert.AreEqual(new[]{"Test", "Folder", "obj"}, obj.Parts);
    }
    
    [TestMethod]
    public void TestDirectoryPartsMixed() {
        string path = "/Test/Folder\\dir";
        PathObject obj = path.ToDirectoryPath();
        
        Assert.IsTrue(obj.IsAbsolute);
        Assert.IsFalse(obj.IsFile);
        Assert.IsTrue(obj.IsDirectory);
        CollectionAssert.AreEqual(new[]{"Test", "Folder", "dir"}, obj.Parts);
    }

    [TestMethod]
    public void TestDirectoryTrailingSlash() {
        string path = "folder1/folder2/";
        PathObject obj = path.ToDirectoryPath();
        
        Assert.IsFalse(obj.IsAbsolute);
        Assert.IsFalse(obj.IsFile);
        Assert.IsTrue(obj.IsDirectory);
        CollectionAssert.AreEqual(new[]{"folder1", "folder2"}, obj.Parts);
    }

    [TestMethod]
    public void TestFileWithExtension() {
        string path = "folder1/file.txt";
        PathObject obj = path.ToFilePath();
        
        Assert.IsFalse(obj.IsAbsolute);
        Assert.IsTrue(obj.IsFile);
        Assert.IsFalse(obj.IsDirectory);
        CollectionAssert.AreEqual(new[]{"folder1"}, obj.Parts);
        Assert.AreEqual("file", obj.Filename);
        Assert.AreEqual(".txt", obj.Extension);
        Assert.AreEqual("file.txt", obj.FullFileName);
    }
    
    [TestMethod]
    public void TestFileWithExtensionTrailing() {
        string path = "folder1/file.txt/";
        Assert.ThrowsException<NotSupportedException>(() => {
            _ = path.ToFilePath();
        });
    }

    [TestMethod]
    public void TestFileWithoutExtension() {
        string path = "folder1/folder2/file";
        PathObject obj = path.ToFilePath();
        
        Assert.IsFalse(obj.IsAbsolute);
        Assert.IsTrue(obj.IsFile);
        Assert.IsFalse(obj.IsDirectory);
        CollectionAssert.AreEqual(new[]{"folder1", "folder2"}, obj.Parts);
        Assert.AreEqual(string.Empty, obj.Extension);
        Assert.AreEqual("file", obj.Filename);
        Assert.AreEqual("file", obj.FullFileName);
    }
    
    [TestMethod]
    public void TestFileWithoutExtensionTrailing() {
        string path = "folder1/file/";
        Assert.ThrowsException<NotSupportedException>(() => {
            _ = path.ToFilePath();
        });
    }
    
    [DataRow("folder1/folder2", true)]
    [DataRow("folder1/folder2/", true)]
    [DataRow("/folder1/folder2", false)]
    [DataRow("/folder1/folder2/", false)]
    [DataTestMethod]
    public void TestDirectoryAppendLinux(string path, bool relative) {
        PathObject obj1 = path.ToDirectoryPath();

        PathObject obj2 = obj1.Folders("folder3", "folder4");
        
        Assert.AreEqual(relative, !obj2.IsAbsolute);
        Assert.IsFalse(obj2.IsFile);
        Assert.IsTrue(obj2.IsDirectory);
        CollectionAssert.AreEqual(new[]{"folder1", "folder2", "folder3", "folder4"}, obj2.Parts);
    }
    
    [DataRow("C:\\folder1/folder2", false)]
    [DataRow("C:\\folder1/folder2/", false)]
    [DataRow("C:/folder1/folder2", false)]
    [DataRow("C:/folder1/folder2/", false)]
    [DataTestMethod]
    public void TestDirectoryAppendWindows(string path, bool relative) {
        PathObject obj1 = path.ToDirectoryPath();

        PathObject obj2 = obj1.Folders("folder3", "folder4");
        
        Assert.AreEqual(relative, !obj2.IsAbsolute);
        Assert.IsFalse(obj2.IsFile);
        Assert.IsTrue(obj2.IsDirectory);
        CollectionAssert.AreEqual(new[]{"C:", "folder1", "folder2", "folder3", "folder4"}, obj2.Parts);
    }

    [TestMethod]
    public void TestFileFromDirectory() {
        string path = "/folder1/folder2";
        PathObject objDir = path.ToDirectoryPath();
        PathObject objFile = objDir.File("file.txt");
        
        Assert.IsTrue(objFile.IsFile);
        Assert.IsFalse(objFile.IsDirectory);
        Assert.IsTrue(objFile.IsAbsolute);
        Assert.AreEqual("file", objFile.Filename);
        Assert.AreEqual(".txt", objFile.Extension);
        Assert.AreEqual("file.txt", objFile.FullFileName);
    }

    [TestMethod]
    public void TestFileFromFileThrows() {
        string path = "folder1/folder2/file.txt";
        PathObject obj1 = path.ToFilePath();
        Assert.ThrowsException<NotSupportedException>(() => {
            _ = obj1.File("file2.txt");
        });
    }
    
    [TestMethod]
    public void TestDirectoryFromFileThrows() {
        string path = "folder1/folder2/file.txt";
        PathObject obj1 = path.ToFilePath();
        Assert.ThrowsException<NotSupportedException>(() => {
            _ = obj1.Folder("folder3");
        });
    }

    [TestMethod]
    public void TestDirectoryToString() {
        string path = "folder1/folder2";
        PathObject obj = path.ToDirectoryPath();
        Assert.AreEqual($"folder1{Path.DirectorySeparatorChar}folder2{Path.DirectorySeparatorChar}", obj.ToString());
    }
    
    [TestMethod]
    public void TestFileToString() {
        string path = "folder1/folder2/file.txt";
        PathObject obj = path.ToFilePath();
        Assert.AreEqual($"folder1{Path.DirectorySeparatorChar}folder2{Path.DirectorySeparatorChar}file.txt", obj.ToString());
    }

    [TestMethod]
    public void TestDirectorySerialization() {
        using MemoryStream ms = new();
        XmlSerializer serializer = new(typeof(PathObject));
        string path = "folder1/folder2/file.txt";
        PathObject obj = path.ToFilePath();
        serializer.Serialize(ms, obj);

        ms.Seek(0, SeekOrigin.Begin);
        using StreamReader sr = new(ms, leaveOpen: true);
        sr.ReadLine();
        Console.WriteLine(sr.ReadToEnd());
        sr.DiscardBufferedData();
        ms.Seek(0, SeekOrigin.Begin);
        PathObject? obj2 = (PathObject?)serializer.Deserialize(ms);
        if (obj2 is null) {
            Assert.Fail();
            return;
        }
        Assert.AreEqual(obj,obj2);
    }
}