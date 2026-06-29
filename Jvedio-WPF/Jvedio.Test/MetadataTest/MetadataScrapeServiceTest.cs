using Jvedio.Core.Config;
using Jvedio.Core.Metadata;
using Jvedio.Core.WindowConfig;
using Jvedio.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jvedio.Test.MetadataTest
{
    [TestClass]
    public class MetadataScrapeServiceTest
    {
        [TestInitialize]
        public void TestInitialize()
        {
            if (ConfigManager.DownloadConfig == null)
                ConfigManager.DownloadConfig = DownloadConfig.CreateInstance();
            if (ConfigManager.Settings == null)
                ConfigManager.Settings = Settings.CreateInstance();
        }

        [TestMethod]
        public void ShouldScrapeOnly_AllImagesDisabled_ReturnsTrue()
        {
            Assert.IsTrue(MetadataScrapeService.ShouldScrapeOnly(false, false, false));
        }

        [TestMethod]
        public void ShouldScrapeOnly_AnyImageEnabled_ReturnsFalse()
        {
            Assert.IsFalse(MetadataScrapeService.ShouldScrapeOnly(true, false, false));
            Assert.IsFalse(MetadataScrapeService.ShouldScrapeOnly(false, true, false));
            Assert.IsFalse(MetadataScrapeService.ShouldScrapeOnly(false, false, true));
        }

        [TestMethod]
        public void NeedsMetadataDownload_WhenTitleNullMode_OnlyChecksTitle()
        {
            ConfigManager.Settings.DownloadWhenTitleNull = true;
            Assert.IsTrue(Video.NeedsMetadataDownload(null, "http://x", "http://img"));
            Assert.IsFalse(Video.NeedsMetadataDownload("Title", null, null));
        }

        [TestMethod]
        public void EnqueueAfterScan_WhenDownloadInfoDisabled_ReturnsZero()
        {
            bool before = ConfigManager.DownloadConfig.DownloadInfo;
            try {
                ConfigManager.DownloadConfig.DownloadInfo = false;
                Assert.AreEqual(0, MetadataScrapeService.EnqueueAfterScan(null));
            } finally {
                ConfigManager.DownloadConfig.DownloadInfo = before;
            }
        }
    }
}
