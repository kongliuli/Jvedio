using Jvedio.Core.Enums;
using Jvedio.Core.Library;
using Jvedio.Core.Metadata;
using Jvedio.Core.Rename;
using Jvedio.Core.Scan;
using Jvedio.Core.Tasks;
using Jvedio.Entity;
using Jvedio.Entity.Data;
using Jvedio.Test.UITest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SuperUtils.Framework.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jvedio.Test.MetadataTest
{
    [TestClass]
    public class Wave12OpenItemsTest
    {
        [TestMethod]
        public void UnifiedTaskWorkerPool_MaxWorkers_IsPositive()
        {
            UnifiedTaskWorkerPool.RefreshLimits();
            Assert.IsTrue(UnifiedTaskWorkerPool.MaxWorkers >= 1);
        }

        [TestMethod]
        public void TaskDisplayHelper_ScanJob_ShowsSubProgress()
        {
            var job = new ScanTask(new List<string>(), new List<string> { "a.mp4" });
            job.ScanResult.TotalCount = 10;
            job.ScanResult.Import.Add("a.mp4");
            string text = TaskDisplayHelper.GetSubTaskProgressText(job);
            Assert.AreEqual("1/10", text);
        }

        [TestMethod]
        public void LibraryContext_Apply_SyncsRuntime()
        {
            DataType before = LibraryRuntime.CurrentDataType;
            try {
                LibraryContext.Apply(DataType.Comics);
                Assert.AreEqual(DataType.Comics, LibraryContext.Current.DataType);
                Assert.AreEqual(DataType.Comics, LibraryRuntime.CurrentDataType);
            } finally {
                LibraryContext.Apply(before);
            }
        }

        [TestMethod]
        public void RenameEngine_BuildTargetPaths_UsesFormatString()
        {
            if (Jvedio.ConfigManager.RenameConfig == null)
                Jvedio.ConfigManager.RenameConfig = Jvedio.Core.Config.RenameConfig.CreateInstance();
            var video = new Video { Path = @"C:\fake\ABC-123.mp4", VID = "ABC-123", Title = "T" };
            string[] paths = RenameEngine.BuildTargetPaths(video);
            Assert.IsNotNull(paths);
            Assert.IsTrue(paths.Length >= 1);
            Assert.IsFalse(string.IsNullOrEmpty(paths[0]));
        }

        [TestMethod]
        public void ScanReasonText_ContainsRepeatedVideo()
        {
            string text = ScanReasonText.Get(NotImportReason.RepetitiveVideo);
            Assert.AreEqual(NotImportReason.RepetitiveVideo.ToString(), text);
        }

        [TestMethod]
        public async Task NonVideoMetadataEngine_ReadWithWeb_MergesLocal()
        {
            var game = new Game { Title = "Local", WebUrl = "http://127.0.0.1:1/not-reachable" };
            Dictionary<string, object> fields = await NonVideoMetadataEngine.ReadWithWebAsync(
                game, DataType.Game, null, null);
            Assert.IsNotNull(fields);
            Assert.AreEqual("Local", fields["Title"]);
        }

        [TestMethod]
        public void HttpMetadataProvider_CanProvide_WhenWebUrlSet()
        {
            var provider = new HttpMetadataProvider();
            Assert.IsTrue(provider.CanProvide(new Video { WebUrl = "http://example.com" }));
            Assert.IsFalse(provider.CanProvide(new Video()));
        }

        [TestMethod]
        public void FourLibraryTypes_Wave12Smoke()
        {
            foreach (DataType dataType in MediaTypeSmokeValidator.LibraryTypes)
                MediaTypeSmokeValidator.AssertFullSmoke(dataType);
        }
    }
}
