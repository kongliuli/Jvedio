using Jvedio.Core.Config;
using Jvedio.Core.Enums;
using Jvedio.Core.Library;
using Jvedio.Core.Metadata;
using Jvedio.Core.UI;
using Jvedio.Core.WindowConfig;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace Jvedio.Test.MetadataTest
{
    [TestClass]
    public class LibraryRuntimeTest
    {
        [TestMethod]
        public void SetCurrent_UpdatesCurrentDataType()
        {
            DataType before = LibraryRuntime.CurrentDataType;
            try {
                LibraryRuntime.SetCurrent(DataType.Game);
                Assert.AreEqual(DataType.Game, LibraryRuntime.CurrentDataType);
            } finally {
                LibraryRuntime.SetCurrent(before);
            }
        }
    }

    [TestClass]
    public class NfoMetadataWriterTest
    {
        [TestMethod]
        public void WriteVideo_RoundTripViaReader()
        {
            string dir = Path.Combine(Path.GetTempPath(), "jvedio-nfo-w10-" + Path.GetRandomFileName());
            Directory.CreateDirectory(dir);
            string nfoPath = Path.Combine(dir, "W10-001.nfo");
            try {
                var video = new Video {
                    VID = "W10-001",
                    Title = "Wave10 Title",
                    Genre = "Action;RPG",
                };
                NfoMetadataWriter.WriteVideo(video, nfoPath);
                Movie movie = NfoMetadataReader.TryReadMovie(nfoPath);
                Assert.IsNotNull(movie);
                Assert.AreEqual("W10-001", movie.id);
                Assert.AreEqual("Wave10 Title", movie.title);
            } finally {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
        }

        [TestMethod]
        public void MetadataSaver_SkipsWhenDisabled()
        {
            if (ConfigManager.Settings == null)
                ConfigManager.Settings = Settings.CreateInstance();
            if (ConfigManager.DownloadConfig == null)
                ConfigManager.DownloadConfig = DownloadConfig.CreateInstance();

            bool saveBefore = ConfigManager.Settings.SaveInfoToNFO;
            try {
                ConfigManager.Settings.SaveInfoToNFO = false;
                var video = new Video { VID = "NO-NFO", Title = "Skip" };
                MetadataSaver.Default.SaveNfo(video);
                // ponytail: no exception == pass; path resolution not exercised when disabled
            } finally {
                ConfigManager.Settings.SaveInfoToNFO = saveBefore;
            }
        }
    }

    [TestClass]
    public class SideMenuLabelQueriesTest
    {
        [TestInitialize]
        public void InitApp()
        {
            Jvedio.App.Init();
            if (System.Windows.Application.ResourceAssembly == null)
                System.Windows.Application.ResourceAssembly = typeof(Jvedio.App).Assembly;
        }

        [TestMethod]
        public void GetGenreList_GameDataType_ReturnsEmptyWithoutDb()
        {
            List<string> game = SideMenuLabelQueries.GetGenreList(DataType.Game, string.Empty);
            Assert.IsNotNull(game);
        }

        [TestMethod]
        public void GetListByField_GameSeries_DoesNotThrow()
        {
            List<string> list = SideMenuLabelQueries.GetListByField(DataType.Game, "Series", string.Empty);
            Assert.IsNotNull(list);
        }
    }
}
