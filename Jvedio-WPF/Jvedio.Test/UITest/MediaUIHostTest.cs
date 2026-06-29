using Jvedio.Core.Enums;
using Jvedio.Core.Library;
using Jvedio.Core.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jvedio.Test.UITest
{
    [TestClass]
    public class MediaUIHostTest
    {
        [TestMethod]
        public void GetProfile_AllDataTypes_ReturnsProfile()
        {
            Assert.IsNotNull(MediaUIHost.GetProfile(DataType.Video));
            Assert.IsNotNull(MediaUIHost.GetProfile(DataType.Picture));
            Assert.IsNotNull(MediaUIHost.GetProfile(DataType.Game));
            Assert.IsNotNull(MediaUIHost.GetProfile(DataType.Comics));
        }

        [TestMethod]
        public void SettingsSections_Game_ExcludesNfoAndCrawler()
        {
            SettingsSectionMask game = MediaUIHost.GetSettingsSections(DataType.Game);
            Assert.IsFalse(game.HasSection(SettingsSectionMask.NfoFfmpeg));
            Assert.IsFalse(game.HasSection(SettingsSectionMask.Crawler));
            Assert.IsTrue(game.HasSection(SettingsSectionMask.Scan));
        }

        [TestMethod]
        public void ScanCompletionPolicy_Video_IncludesScreenShot()
        {
            ScanCompletionPolicy video = MediaUIHost.GetScanCompletionPolicy(DataType.Video);
            Assert.IsTrue(video.HasPolicy(ScanCompletionPolicy.ScreenShotAfterImport));
            ScanCompletionPolicy game = MediaUIHost.GetScanCompletionPolicy(DataType.Game);
            Assert.IsFalse(game.HasPolicy(ScanCompletionPolicy.ScreenShotAfterImport));
        }

        [TestMethod]
        public void SettingsSections_Picture_IncludesPicturePaths()
        {
            SettingsSectionMask picture = MediaUIHost.GetSettingsSections(DataType.Picture);
            Assert.IsTrue(picture.HasSection(SettingsSectionMask.PicturePaths));
            Assert.IsFalse(picture.HasSection(SettingsSectionMask.NfoFfmpeg));
        }

        [TestMethod]
        public void SettingsSections_Comics_SameAsPicture()
        {
            SettingsSectionMask comics = MediaUIHost.GetSettingsSections(DataType.Comics);
            Assert.IsTrue(comics.HasSection(SettingsSectionMask.Scan));
            Assert.IsFalse(comics.HasSection(SettingsSectionMask.Crawler));
        }

        [TestMethod]
        public void StartupLibraryMapping_SideIndexRoundTrip()
        {
            Assert.AreEqual(DataType.Game, StartupLibraryMapping.DataTypeFromSideIndex(2));
            Assert.AreEqual(1, StartupLibraryMapping.SideIndexFromDataType(DataType.Picture));
        }

        [TestMethod]
        public void MediaFeatureMask_Game_HasAddMediaPath()
        {
            Assert.IsTrue(MediaFeatureMaskExtensions.IsFeatureVisible(DataType.Game, "AddGamePathOnly"));
            Assert.IsFalse(MediaFeatureMaskExtensions.IsFeatureVisible(DataType.Video, "AddGamePathOnly"));
        }

        [TestMethod]
        public void MediaUIProfile_Factories_NonNull()
        {
            Assert.IsNotNull(MediaUIHost.GetProfile(DataType.Picture).CreateSideNavigation());
            Assert.IsNotNull(MediaUIHost.GetProfile(DataType.Game).CreateTabFactory());
            Assert.IsFalse(MediaUIHost.GetProfile(DataType.Game).UseVideoDetailsWindow);
        }

        [TestMethod]
        public void MediaUIProfile_SideNavigation_DistinctByDataType()
        {
            Assert.AreNotSame(
                MediaUIHost.GetProfile(DataType.Video).CreateSideNavigation(),
                MediaUIHost.GetProfile(DataType.Picture).CreateSideNavigation());
            Assert.AreNotSame(
                MediaUIHost.GetProfile(DataType.Picture).CreateSideNavigation(),
                MediaUIHost.GetProfile(DataType.Game).CreateSideNavigation());
        }

        [TestMethod]
        public void SettingsSections_VideoVsGame_CrawlerTabMask()
        {
            Assert.IsTrue(MediaUIHost.GetSettingsSections(DataType.Video).HasSection(SettingsSectionMask.Crawler));
            Assert.IsFalse(MediaUIHost.GetSettingsSections(DataType.Game).HasSection(SettingsSectionMask.Crawler));
            Assert.IsTrue(MediaUIHost.GetSettingsSections(DataType.Picture).HasSection(SettingsSectionMask.PicturePaths));
        }

        [TestMethod]
        public void MediaUIProfile_TabFactory_PerDataType()
        {
            Assert.IsNotNull(MediaUIHost.GetProfile(DataType.Video).CreateTabFactory());
            Assert.IsNotNull(MediaUIHost.GetProfile(DataType.Game).CreateTabFactory());
            Assert.AreNotSame(
                MediaUIHost.GetProfile(DataType.Video).CreateTabFactory(),
                MediaUIHost.GetProfile(DataType.Game).CreateTabFactory());
        }

        [TestMethod]
        public void MediaUIProfile_Video_UsesDetailsWindow()
        {
            Assert.IsTrue(MediaUIHost.GetProfile(DataType.Video).UseVideoDetailsWindow);
            Assert.IsFalse(MediaUIHost.GetProfile(DataType.Picture).UseVideoDetailsWindow);
        }

        [TestMethod]
        public void DataTypeDisplay_ReturnsNonEmpty()
        {
            Assert.IsFalse(string.IsNullOrEmpty(DataTypeDisplay.GetLabel(DataType.Game)));
        }
    }
}
