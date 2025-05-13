using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Test;

[TestClass]
public class ExtensionsTest {

    [TestMethod]
    public void TestLinqSplit() {
        List<int> nums = [1,2,3,4,0,1,4,2,0,1,20,15,0,1,0];
        List<IEnumerable<int>>? parts = nums.Split(0).ToList();
        Assert.AreEqual(4, parts.Count);
        CollectionAssert.AreEqual(new List<int>(){ 1,2,3,4}, parts[0].ToList());
        CollectionAssert.AreEqual(new List<int>() { 1,4,2}, parts[1].ToList());
        CollectionAssert.AreEqual(new List<int>() { 1,20,15}, parts[2].ToList());
        CollectionAssert.AreEqual(new List<int>() { 1}, parts[3].ToList());
    }

    [TestMethod]
    public void TestLinqSplitNonEnding() {
        List<int> nums = [1,2,3,0,1,4,2,0,1,20,15,0,1];
        List<IEnumerable<int>>? parts = nums.Split(0).ToList();
        Assert.AreEqual(4, parts.Count);
        CollectionAssert.AreEqual(new List<int>() { 1,2,3}, parts[0].ToList());
        CollectionAssert.AreEqual(new List<int>() { 1,4,2}, parts[1].ToList());
        CollectionAssert.AreEqual(new List<int>() { 1,20,15}, parts[2].ToList());
        CollectionAssert.AreEqual(new List<int>() { 1}, parts[3].ToList());
    }

    [TestMethod]
    public void TestLinqSplitPredicate() {
        List<int> nums = [1, 2, 3, 4, 0, 1, 4, 2, 0, 1, 20, 15, 0, 1, 0];
        List<IEnumerable<int>>? parts = nums.Split(x => x == 0).ToList();
        Assert.AreEqual(4, parts.Count);
        CollectionAssert.AreEqual(new List<int>() { 1, 2, 3, 4 }, parts[0].ToList());
        CollectionAssert.AreEqual(new List<int>() { 1, 4, 2 }, parts[1].ToList());
        CollectionAssert.AreEqual(new List<int>() { 1, 20, 15 }, parts[2].ToList());
        CollectionAssert.AreEqual(new List<int>() { 1 }, parts[3].ToList());
    }

    [TestMethod]
    public void TestLinqAfterItem() {
        List<int> nums = [1, 2, 3, 4, 0, 6, 8, -3, 99, 52, 20, 15, 12, 19, -2];
        Assert.AreEqual(6, nums.After(0));
        Assert.AreEqual(99, nums.After(-3));
        Assert.AreEqual(2, nums.After(1));
        Assert.ThrowsException<InvalidOperationException>(() => nums.After(-2));
    }

    [TestMethod]
    public void TestLinqBeforeItem() {
        List<int> nums = [1, 2, 3, 4, 0, 6, 8, -3, 99, 52, 20, 15, 12, 19, -2];
        Assert.AreEqual(4, nums.Before(0));
        Assert.AreEqual(0, nums.Before(6));
        Assert.AreEqual(3, nums.Before(4));
        Assert.AreEqual(19, nums.Before(-2));
        Assert.ThrowsException<InvalidOperationException>(() => nums.Before(1));
    }

    
}
