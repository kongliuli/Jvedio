using Jvedio.Core.Config;
using Jvedio.Core.Enums;
using Jvedio.Core.Library;
using Jvedio.Core.Metadata;
using Jvedio.Core.Plugins;
using Jvedio.Core.Rename;
using Jvedio.Core.Scan;
using Jvedio.Core.Tasks;
using Jvedio.Core.UI;
using Jvedio.Entity;
using Jvedio.Entity.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Jvedio.Test.MetadataTest
{
    [TestClass]
    public class Wave11OpenItemsTest
    {
        [TestMethod]
        public void TaskParallelismConfig_Defaults_ArePositive()
        {
            var cfg = TaskParallelismConfig.CreateInstance();
            Assert.IsTrue(cfg.GenerateTaskCount >= 1);
            Assert.IsTrue(cfg.DownloadTaskCount >= 1);
        }

        [TestMethod]
        public void NonVideoMetadataEngine_Game_ReadsWebUrl()
        {
            var game = new Game { Title = "G1", WebUrl = "http://example/game" };
            Dictionary<string, object> fields = NonVideoMetadataEngine.ReadLocalFields(game, DataType.Game);
            Assert.IsNotNull(fields);
            Assert.AreEqual("http://example/game", fields["WebUrl"]);
        }

        [TestMethod]
        public void StartupLibraryMapping_ResolveDataType_PrefersStored()
        {
            Assert.AreEqual(DataType.Game, StartupLibraryMapping.ResolveDataType(0, (int)DataType.Game));
        }

        [TestMethod]
        public void NfoMetadataReaderAdapter_RoundTrip()
        {
            string dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "jvedio-w11-" + System.IO.Path.GetRandomFileName());
            System.IO.Directory.CreateDirectory(dir);
            string path = System.IO.Path.Combine(dir, "X.nfo");
            try {
                var video = new Video { VID = "X", Title = "T" };
                NfoMetadataWriter.WriteVideo(video, path);
                Assert.IsNotNull(NfoMetadataReaderAdapter.Default.TryReadMovie(path));
            } finally {
                if (System.IO.Directory.Exists(dir))
                    System.IO.Directory.Delete(dir, true);
            }
        }

        [TestMethod]
        public void PluginHookRegistry_InvokePostMetadata_SafeWhenNull()
        {
            PluginHookRegistry.InvokePostMetadata(null, null);
            PluginHookRegistry.InvokePostMetadata(new Video(), new Dictionary<string, object>());
        }

        [TestMethod]
        public void RenameEngine_BuildTargetPaths_DelegatesToVideo()
        {
            if (Jvedio.ConfigManager.RenameConfig == null)
                Jvedio.ConfigManager.RenameConfig = Jvedio.Core.Config.RenameConfig.CreateInstance();
            var video = new Video { Path = @"C:\fake\ABC-123.mp4", VID = "ABC-123", Title = "T" };
            string[] paths = RenameEngine.BuildTargetPaths(video);
            Assert.IsNotNull(paths);
            Assert.IsTrue(paths.Length >= 1);
        }

        [TestMethod]
        public void ScanJobBase_ImplementsIBackgroundTask()
        {
            Assert.IsTrue(typeof(IBackgroundTask).IsAssignableFrom(typeof(ScanJobBase)));
        }

        [TestMethod]
        public void SettingsSectionMask_AllTypes_HaveScanSection()
        {
            foreach (DataType t in new[] { DataType.Video, DataType.Picture, DataType.Game, DataType.Comics }) {
                Assert.IsTrue(MediaUIHost.GetSettingsSections(t).HasSection(SettingsSectionMask.Scan),
                    t.ToString());
            }
        }
    }
}
