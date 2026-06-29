using Jvedio.Core.Config;
using Jvedio.Core.Enums;
using Jvedio.Core.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Jvedio.Test.MetadataTest
{
    [TestClass]
    public class MetadataLookupCacheTest
    {
        [TestInitialize]
        public void TestInitialize()
        {
            if (ConfigManager.DownloadConfig == null)
                ConfigManager.DownloadConfig = DownloadConfig.CreateInstance();
        }

        [TestMethod]
        public void BuildCacheKey_NormalizesVid()
        {
            string key = MetadataLookupCache.BuildCacheKey("Crawler", " ipx-633 ", (int)VideoType.Normal);
            StringAssert.Contains(key, "IPX-633");
            StringAssert.Contains(key, MetadataLookupCache.KeyVersion);
        }

        [TestMethod]
        public void IsSuccessResult_RequiresNonEmptyTitle()
        {
            Assert.IsFalse(MetadataLookupCache.IsSuccessResult(null));
            Assert.IsFalse(MetadataLookupCache.IsSuccessResult(new Dictionary<string, object>()));
            Assert.IsFalse(MetadataLookupCache.IsSuccessResult(new Dictionary<string, object> {
                ["Title"] = "",
            }));
            Assert.IsTrue(MetadataLookupCache.IsSuccessResult(new Dictionary<string, object> {
                ["Title"] = "Test Title",
            }));
        }

        [TestMethod]
        public void ShouldBypassCache_WhenOverrideInfo_ReturnsTrue()
        {
            bool before = ConfigManager.DownloadConfig.OverrideInfo;
            try {
                ConfigManager.DownloadConfig.OverrideInfo = true;
                Assert.IsTrue(MetadataLookupCache.ShouldBypassCache());
                ConfigManager.DownloadConfig.OverrideInfo = false;
                Assert.IsFalse(MetadataLookupCache.ShouldBypassCache());
            } finally {
                ConfigManager.DownloadConfig.OverrideInfo = before;
            }
        }
    }
}
