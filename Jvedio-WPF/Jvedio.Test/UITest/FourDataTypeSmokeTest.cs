using Jvedio.Core.Enums;
using Jvedio.Core.Library;
using Jvedio.Core.Scan;
using Jvedio.Core.Tasks;
using Jvedio.Core.UI;
using Jvedio.Entity.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SuperUtils.Framework.Tasks;
using System;
using System.Collections.Generic;

namespace Jvedio.Test.UITest
{
    /// <summary>
    /// 四库类型冒烟校验：侧栏 Profile → TabType → 列表模式 → 扫描器 → TaskHub。
    /// ponytail: 无 WPF/Appium 的可重复单测；完整 UI 自动化见 <see cref="FourDataTypeAppiumSmokeTest"/>。
    /// </summary>
    internal static class MediaTypeSmokeValidator
    {
        private static readonly DataType[] AllLibraryTypes = {
            DataType.Video,
            DataType.Picture,
            DataType.Game,
            DataType.Comics,
        };

        public static IReadOnlyList<DataType> LibraryTypes => AllLibraryTypes;

        public static void AssertFullSmoke(DataType dataType)
        {
            AssertProfileAndSideNav(dataType);
            AssertTabPipeline(dataType);
            AssertScanPipeline(dataType);
            AssertTaskHubAcceptsScan(dataType);
        }

        public static void AssertProfileAndSideNav(DataType dataType)
        {
            IMediaUIProfile profile = MediaUIHost.GetProfile(dataType);
            Assert.AreEqual(dataType, profile.DataType);
            Assert.IsNotNull(profile.CreateSideNavigation(), $"SideNav null for {dataType}");
            Assert.IsNotNull(profile.CreateTabFactory(), $"TabFactory null for {dataType}");

            int sideIdx = StartupLibraryMapping.SideIndexFromDataType(dataType);
            Assert.AreEqual(dataType, StartupLibraryMapping.DataTypeFromSideIndex(sideIdx));
        }

        public static void AssertTabPipeline(DataType dataType)
        {
            TabType primary = TabTypeExtensions.PrimaryListTabType(dataType);
            MediaListMode listMode = MediaListModeExtensions.FromDataType(dataType);

            switch (dataType) {
                case DataType.Video:
                    Assert.AreEqual(TabType.GeoVideo, primary);
                    Assert.AreEqual(MediaListMode.Video, listMode);
                    Assert.IsTrue(MediaUIHost.GetProfile(dataType).UseVideoDetailsWindow);
                    break;
                case DataType.Picture:
                    Assert.AreEqual(TabType.GeoPicture, primary);
                    Assert.AreEqual(MediaListMode.Picture, listMode);
                    Assert.IsFalse(MediaUIHost.GetProfile(dataType).UseVideoDetailsWindow);
                    break;
                case DataType.Comics:
                    Assert.AreEqual(TabType.GeoPicture, primary);
                    Assert.AreEqual(MediaListMode.Comics, listMode);
                    break;
                case DataType.Game:
                    Assert.AreEqual(TabType.GeoGame, primary);
                    Assert.AreEqual(MediaListMode.Game, listMode);
                    Assert.IsFalse(MediaUIHost.GetProfile(dataType).UseVideoDetailsWindow);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType));
            }

            SettingsSectionMask sections = MediaUIHost.GetSettingsSections(dataType);
            Assert.IsTrue(sections.HasSection(SettingsSectionMask.Scan), $"Scan settings missing for {dataType}");
        }

        public static void AssertScanPipeline(DataType dataType)
        {
            ScanJobBase job = ScanFactory.ProduceScanner(dataType, new List<string>(), null);
            Assert.IsNotNull(job, $"ScanFactory returned null for {dataType}");

            switch (dataType) {
                case DataType.Video:
                    Assert.IsInstanceOfType(job, typeof(ScanTask));
                    break;
                case DataType.Picture:
                    Assert.IsInstanceOfType(job, typeof(PictureScan));
                    break;
                case DataType.Comics:
                    Assert.IsInstanceOfType(job, typeof(ComicScan));
                    break;
                case DataType.Game:
                    Assert.IsInstanceOfType(job, typeof(GameScan));
                    break;
            }
        }

        public static void AssertTaskHubAcceptsScan(DataType dataType)
        {
            ScanJobBase job = ScanFactory.ProduceScanner(dataType, new List<string>(), null);
            int before = TaskHub.Instance.Scan.CurrentTasks?.Count ?? 0;
            TaskHub.Instance.AddTask(job, TaskKind.Scan);
            int after = TaskHub.Instance.Scan.CurrentTasks?.Count ?? 0;
            Assert.IsTrue(after >= before, $"TaskHub.Scan did not accept job for {dataType}");
            TaskHub.Instance.Scan.RemoveTask(System.Threading.Tasks.TaskStatus.Canceled |
                System.Threading.Tasks.TaskStatus.RanToCompletion);
        }
    }

    [TestClass]
    [TestCategory("Smoke")]
    public class FourDataTypeSmokeTest
    {
        [TestInitialize]
        public void InitApp()
        {
            Jvedio.App.Init();
            TaskHub.Instance.CancelAll();
        }

        [TestCleanup]
        public void CleanupTasks()
        {
            TaskHub.Instance.CancelAll();
        }

        [TestMethod]
        public void AllLibraryTypes_FullSmokePass()
        {
            foreach (DataType dataType in MediaTypeSmokeValidator.LibraryTypes)
                MediaTypeSmokeValidator.AssertFullSmoke(dataType);
        }

        [TestMethod]
        public void Video_FullSmokePass()
        {
            MediaTypeSmokeValidator.AssertFullSmoke(DataType.Video);
        }

        [TestMethod]
        public void Picture_FullSmokePass()
        {
            MediaTypeSmokeValidator.AssertFullSmoke(DataType.Picture);
        }

        [TestMethod]
        public void Game_FullSmokePass()
        {
            MediaTypeSmokeValidator.AssertFullSmoke(DataType.Game);
        }

        [TestMethod]
        public void Comics_FullSmokePass()
        {
            MediaTypeSmokeValidator.AssertFullSmoke(DataType.Comics);
        }

        [TestMethod]
        public void Comics_UsesPictureSideNavigation()
        {
            Assert.AreSame(
                MediaUIHost.GetProfile(DataType.Picture).CreateSideNavigation(),
                MediaUIHost.GetProfile(DataType.Comics).CreateSideNavigation());
        }

        [TestMethod]
        public void TabFactories_AreDistinctPerProfile()
        {
            Assert.AreNotSame(
                MediaUIHost.GetProfile(DataType.Video).CreateTabFactory(),
                MediaUIHost.GetProfile(DataType.Game).CreateTabFactory());
        }
    }
}
