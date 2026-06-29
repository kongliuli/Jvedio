using Jvedio.Core.Scan;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jvedio.Test.ScanTest
{
    [TestClass]
    public class ScanResultDiscoverTest
    {
        [TestMethod]
        public void FormatDiscoverSummary_WhenNoDiscoverData_ReturnsNull()
        {
            var result = new ScanResult();
            Assert.IsNull(result.FormatDiscoverSummary());
        }

        [TestMethod]
        public void FormatDiscoverSummary_IncludesCountMsAndCache()
        {
            var result = new ScanResult {
                DiscoverFileCount = 42,
                DiscoverEnumerateMs = 150,
                DiscoverFromCache = true,
            };
            StringAssert.Contains(result.FormatDiscoverSummary(), "42 files");
            StringAssert.Contains(result.FormatDiscoverSummary(), "150ms");
            StringAssert.Contains(result.FormatDiscoverSummary(), "cache=True");
        }
    }
}
