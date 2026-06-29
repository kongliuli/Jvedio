using Jvedio.Core.Scan;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Jvedio.Test.UITest
{
    [TestClass]
    public class ScanDiscoveryHelperTest
    {
        [TestMethod]
        public void GroupByScanRoots_AssignsFilesToLongestMatchingRoot()
        {
            var roots = new List<string> { @"C:\lib\album1", @"C:\lib\album1\sub" };
            var files = new List<string> {
                @"C:\lib\album1\a.jpg",
                @"C:\lib\album1\sub\b.jpg",
            };

            Dictionary<string, List<string>> grouped = ScanDiscoveryHelper.GroupByScanRoots(roots, files);

            Assert.AreEqual(1, grouped[@"C:\lib\album1"].Count);
            Assert.AreEqual(1, grouped[@"C:\lib\album1\sub"].Count);
        }

        [TestMethod]
        public void RemoveEmptyPathGroups_DropsRootsWithNoFiles()
        {
            var dict = new Dictionary<string, List<string>> {
                { @"C:\a", new List<string>() },
                { @"C:\b", new List<string> { "x" } },
            };

            ScanDiscoveryHelper.RemoveEmptyPathGroups(dict);

            Assert.IsFalse(dict.ContainsKey(@"C:\a"));
            Assert.AreEqual(1, dict.Count);
        }
    }
}
