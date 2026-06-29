using Jvedio.Core.Enums;
using Jvedio.Core.Library;
using Jvedio.Core.Metadata;
using Jvedio.Core.Plugins.Crawler;
using Jvedio.Core.Scan;
using Jvedio.Core.Tasks;
using Jvedio.Core.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SuperUtils.Framework.Tasks;
using System.Collections.Generic;

namespace Jvedio.Test.UITest
{
    /// <summary>
    /// QA-001 清单项自动化代理（对应 docs/QA-001-smoke-checklist.md）。
    /// ponytail: 无 WinAppDriver 时覆盖逻辑层；实机 UI 点击仍可选跑 FourDataTypeAppiumSmokeTest。
    /// </summary>
    [TestClass]
    [TestCategory("QA-001")]
    [TestCategory("Smoke")]
    public class Qa001SmokeChecklistTest
    {
        [TestInitialize]
        public void Init()
        {
            Jvedio.App.Init();
            TaskHub.Instance.CancelAll();
        }

        [TestCleanup]
        public void Cleanup()
        {
            TaskHub.Instance.CancelAll();
        }

        [TestMethod]
        public void Checklist_Step1_LibraryContextPerType()
        {
            foreach (DataType dataType in MediaTypeSmokeValidator.LibraryTypes) {
                LibraryContext.Apply(dataType);
                Assert.AreEqual(dataType, LibraryContextBinding.Instance.CurrentDataType,
                    $"LibraryContextBinding for {dataType}");
                int idx = StartupLibraryMapping.SideIndexFromDataType(dataType);
                Assert.AreEqual(dataType, StartupLibraryMapping.DataTypeFromSideIndex(idx),
                    $"Startup mapping for {dataType}");
            }
        }

        [TestMethod]
        public void Checklist_Step2_SideNavExists()
        {
            foreach (DataType dataType in MediaTypeSmokeValidator.LibraryTypes)
                MediaTypeSmokeValidator.AssertProfileAndSideNav(dataType);
        }

        [TestMethod]
        public void Checklist_Step3_ListModePerType()
        {
            foreach (DataType dataType in MediaTypeSmokeValidator.LibraryTypes)
                MediaTypeSmokeValidator.AssertTabPipeline(dataType);
        }

        [TestMethod]
        public void Checklist_Step4_SettingsScanSection()
        {
            foreach (DataType dataType in MediaTypeSmokeValidator.LibraryTypes) {
                SettingsSectionMask mask = MediaUIHost.GetSettingsSections(dataType);
                Assert.IsTrue(mask.HasSection(SettingsSectionMask.Scan), dataType.ToString());
            }
        }

        [TestMethod]
        public void Checklist_Step5_TaskHubScanAndSubProgress()
        {
            var job = new ScanTask(new List<string>(), new List<string> { "sample.mp4" });
            job.ScanResult.TotalCount = 3;
            job.ScanResult.Import.Add("a.mp4");
            Assert.AreEqual("1/3", TaskDisplayHelper.GetSubTaskProgressText(job));

            foreach (DataType dataType in MediaTypeSmokeValidator.LibraryTypes)
                MediaTypeSmokeValidator.AssertTaskHubAcceptsScan(dataType);
        }

        [TestMethod]
        public void Checklist_Step6_TaskHubCancelClear()
        {
            ScanJobBase job = ScanFactory.ProduceScanner(DataType.Video, new List<string>(), null);
            TaskHub.Instance.AddTask(job, TaskKind.Scan);
            Assert.IsTrue(TaskHub.Instance.Scan.CurrentTasks.Count > 0);
            TaskHub.Instance.CancelAll();
            TaskHub.Instance.Scan.RemoveTask(
                System.Threading.Tasks.TaskStatus.Canceled | System.Threading.Tasks.TaskStatus.RanToCompletion);
            Assert.AreEqual(0, TaskHub.Instance.Scan.CurrentTasks.Count);
        }

        [TestMethod]
        public void Checklist_Video_CrawlerAndNfoSections()
        {
            SettingsSectionMask video = MediaUIHost.GetSettingsSections(DataType.Video);
            Assert.IsTrue(video.HasSection(SettingsSectionMask.Crawler));
            Assert.IsTrue(video.HasSection(SettingsSectionMask.NfoFfmpeg));
            Assert.IsTrue(MediaFeatureMaskExtensions.IsFeatureVisible(DataType.Video, "AddMovie"));
        }

        [TestMethod]
        public void Checklist_Video_ScrapeServiceUsesTaskHub()
        {
            Assert.AreEqual(0, MetadataScrapeService.Enqueue(new List<Jvedio.Entity.Video>(), scrapeOnly: true));
            Assert.IsNotNull(TaskHub.Instance.Download);
        }

        [TestMethod]
        public void Checklist_Video_Plg002ToggleExistsInConfig()
        {
            if (Jvedio.ConfigManager.PluginConfig == null)
                return;
            bool previous = Jvedio.ConfigManager.PluginConfig.RequireSignedCrawlerDll;
            try {
                Jvedio.ConfigManager.PluginConfig.RequireSignedCrawlerDll = true;
                Assert.IsTrue(Jvedio.ConfigManager.PluginConfig.RequireSignedCrawlerDll);
            } finally {
                Jvedio.ConfigManager.PluginConfig.RequireSignedCrawlerDll = previous;
            }
        }

        [TestMethod]
        public void Checklist_NonVideo_NoCrawlerTab()
        {
            foreach (DataType dataType in new[] { DataType.Picture }) {
                SettingsSectionMask mask = MediaUIHost.GetSettingsSections(dataType);
                Assert.IsFalse(mask.HasSection(SettingsSectionMask.Crawler), dataType.ToString());
            }
        }

        [TestMethod]
        public void Checklist_Picture_PicturePathsVisible()
        {
            SettingsSectionMask mask = MediaUIHost.GetSettingsSections(DataType.Picture);
            Assert.IsTrue(mask.HasSection(SettingsSectionMask.PicturePaths));
        }

        [TestMethod]
        public void Checklist_Game_WebMetadataSiteRule()
        {
            const string html = "<html><head><meta property='og:title' content='Game Title' /></head></html>";
            HttpMetadataSiteRule rule = HttpMetadataSiteRegistry.Match("https://vndb.org/g1");
            Dictionary<string, object> fields = HttpMetadataSiteRegistry.ParseHtml(html, rule);
            Assert.AreEqual("Game Title", fields["Title"]);
        }

        [TestMethod]
        public void Checklist_BothLibraries_FullPipeline()
        {
            foreach (DataType dataType in MediaTypeSmokeValidator.LibraryTypes)
                MediaTypeSmokeValidator.AssertFullSmoke(dataType);
        }
    }
}
