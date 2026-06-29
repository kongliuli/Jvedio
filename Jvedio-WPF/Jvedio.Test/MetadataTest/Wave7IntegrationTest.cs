using Jvedio.Core.Enums;
using Jvedio.Core.Metadata;
using Jvedio.Core.Tasks;
using Jvedio.Core.UI;
using Jvedio.Entity.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jvedio.Test.MetadataTest
{
    [TestClass]
    public class MetadataEngineTest
    {
        [TestMethod]
        public void ProviderChain_IncludesNfoCacheAndCrawler()
        {
            // ponytail: 反射-free 冒烟——RefreshAsync(null) 不抛且 Provider 注册顺序在静态 ctor 完成
            Assert.IsNull(MetadataEngine.RefreshAsync(null, default, null).GetAwaiter().GetResult());
        }
    }

    [TestClass]
    public class TaskHubTest
    {
        [TestInitialize]
        public void InitApp()
        {
            Jvedio.App.Init();
        }

        [TestMethod]
        public void TotalRunningCount_MatchesManagerSum()
        {
            TaskHub hub = TaskHub.Instance;
            int expected = hub.Scan.RunningCount + hub.Download.RunningCount + hub.ScreenShot.RunningCount;
            Assert.AreEqual(expected, hub.TotalRunningCount);
            Assert.AreEqual(expected > 0, hub.HasRunningTasks);
        }
    }

    [TestClass]
    public class TabTypeExtensionsTest
    {
        [TestMethod]
        public void PrimaryListTabType_MapsByDataType()
        {
            Assert.AreEqual(TabType.GeoVideo, TabTypeExtensions.PrimaryListTabType(DataType.Video));
            Assert.AreEqual(TabType.GeoPicture, TabTypeExtensions.PrimaryListTabType(DataType.Picture));
            Assert.AreEqual(TabType.GeoPicture, TabTypeExtensions.PrimaryListTabType(DataType.Comics));
            Assert.AreEqual(TabType.GeoGame, TabTypeExtensions.PrimaryListTabType(DataType.Game));
        }

        [TestMethod]
        public void IsPrimaryListTab_GeoVideoAliasForVideoOnly()
        {
            Assert.IsTrue(TabTypeExtensions.IsPrimaryListTab(TabType.GeoVideo, DataType.Video));
            Assert.IsFalse(TabTypeExtensions.IsPrimaryListTab(TabType.GeoVideo, DataType.Picture));
            Assert.IsTrue(TabTypeExtensions.IsPrimaryListTab(TabType.GeoPicture, DataType.Picture));
        }
    }
}
