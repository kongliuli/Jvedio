using Jvedio.Core.Config;
using Jvedio.Core.Enums;
using Jvedio.Core.Library;
using Jvedio.Core.Metadata;
using Jvedio.Core.Plugins.Crawler;
using Jvedio.Test.UITest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Jvedio.Test.MetadataTest
{
    [TestClass]
    public class Wave13OpenItemsTest
    {
        [TestMethod]
        public void LibraryContextBinding_NotifiesOnApply()
        {
            DataType before = LibraryContextBinding.Instance.CurrentDataType;
            bool notified = false;
            LibraryContextBinding.Instance.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(LibraryContextBinding.CurrentDataType))
                    notified = true;
            };
            try {
                LibraryContext.Apply(DataType.Picture);
                Assert.IsTrue(notified);
                Assert.AreEqual(DataType.Picture, LibraryContextBinding.Instance.CurrentDataType);
            } finally {
                LibraryContext.Apply(before);
            }
        }

        [TestMethod]
        public void CrawlerPluginSecurity_AllowsKnownSuffix()
        {
            Assert.IsTrue(CrawlerPluginSecurity.IsAllowedPublicType(typeof(FakeSampleCrawler)));
            Assert.IsFalse(CrawlerPluginSecurity.IsAllowedPublicType(typeof(string)));
        }

        [TestMethod]
        public void CrawlerPluginSecurity_UnsignedDll_AllowedWhenDisabled()
        {
            if (Jvedio.ConfigManager.PluginConfig == null)
            {
                string dll = Assembly.GetExecutingAssembly().Location;
                Assert.IsTrue(CrawlerPluginSecurity.VerifyDll(dll));
                return;
            }

            bool previous = Jvedio.ConfigManager.PluginConfig.RequireSignedCrawlerDll;
            try {
                Jvedio.ConfigManager.PluginConfig.RequireSignedCrawlerDll = false;
                string dll = Assembly.GetExecutingAssembly().Location;
                Assert.IsTrue(CrawlerPluginSecurity.VerifyDll(dll));
            } finally {
                Jvedio.ConfigManager.PluginConfig.RequireSignedCrawlerDll = previous;
            }
        }

        [TestMethod]
        public void HttpMetadataSiteRegistry_MatchesVndbHost()
        {
            HttpMetadataSiteRule rule = HttpMetadataSiteRegistry.Match("https://vndb.org/v1234");
            Assert.IsNotNull(rule);
            Assert.AreEqual("vndb.org", rule.HostContains);
        }

        [TestMethod]
        public void HttpMetadataSiteRegistry_ParseOgTitle()
        {
            const string html = "<html><head><meta property='og:title' content='VN Title' /></head></html>";
            HttpMetadataSiteRule rule = HttpMetadataSiteRegistry.Match("https://vndb.org/x");
            Dictionary<string, object> fields = HttpMetadataSiteRegistry.ParseHtml(html, rule);
            Assert.IsNotNull(fields);
            Assert.AreEqual("VN Title", fields["Title"]);
        }

        [TestMethod]
        public void HttpMetadataOptions_SyncFromConfig()
        {
            var cfg = HttpMetadataConfig.CreateInstance();
            cfg.EnableSiteRules = true;
            cfg.SiteRules = new List<HttpMetadataSiteRule> {
                new HttpMetadataSiteRule { HostContains = "example.test", OgPropertyMap = { ["og:title"] = "Title" } },
            };
            HttpMetadataOptions.SyncFromConfig(cfg);
            Assert.IsTrue(HttpMetadataOptions.EnableSiteRules);
            Assert.AreEqual(1, HttpMetadataOptions.SiteRules.Count);
        }

        [TestMethod]
        public void FourLibraryTypes_W13Smoke()
        {
            foreach (DataType dataType in MediaTypeSmokeValidator.LibraryTypes)
                MediaTypeSmokeValidator.AssertFullSmoke(dataType);
        }
    }

    public class FakeSampleCrawler
    {
    }
}
