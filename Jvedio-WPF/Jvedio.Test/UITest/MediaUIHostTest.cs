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
        }

        [TestMethod]
        public void SettingsSections_Picture_ExcludesNfoAndCrawler()
        {
            SettingsSectionMask picture = MediaUIHost.GetSettingsSections(DataType.Picture);
            Assert.IsFalse(picture.HasSection(SettingsSectionMask.NfoFfmpeg));
            Assert.IsFalse(picture.HasSection(SettingsSectionMask.Crawler));
            Assert.IsTrue(picture.HasSection(SettingsSectionMask.Scan));
        }

        [TestMethod]
        public void ScanCompletionPolicy_Video_IncludesScreenShot()
        {
            ScanCompletionPolicy video = MediaUIHost.GetScanCompletionPolicy(DataType.Video);
            Assert.IsTrue(video.HasPolicy(ScanCompletionPolicy.ScreenShotAfterImport));
            ScanCompletionPolicy picture = MediaUIHost.GetScanCompletionPolicy(DataType.Picture);
            Assert.IsFalse(picture.HasPolicy(ScanCompletionPolicy.ScreenShotAfterImport));
        }

        [TestMethod]
        public void SettingsSections_Picture_IncludesPicturePaths()
        {
            SettingsSectionMask picture = MediaUIHost.GetSettingsSections(DataType.Picture);
            Assert.IsTrue(picture.HasSection(SettingsSectionMask.PicturePaths));
            Assert.IsFalse(picture.HasSection(SettingsSectionMask.NfoFfmpeg));
        }

        [TestMethod]
        public void StartupLibraryMapping_SideIndexRoundTrip()
        {
            Assert.AreEqual(DataType.Picture, StartupLibraryMapping.DataTypeFromSideIndex(1));
            Assert.AreEqual(1, StartupLibraryMapping.SideIndexFromDataType(DataType.Picture));
        }

        [TestMethod]
        public void MediaFeatureMask_AddGamePathOnly_Hidden()
        {
            Assert.IsFalse(MediaFeatureMaskExtensions.IsFeatureVisible(DataType.Picture, "AddGamePathOnly"));
            Assert.IsFalse(MediaFeatureMaskExtensions.IsFeatureVisible(DataType.Video, "AddGamePathOnly"));
        }

        [TestMethod]
        public void MediaUIProfile_Factories_NonNull()
        {
            Assert.IsNotNull(MediaUIHost.GetProfile(DataType.Picture).CreateSideNavigation());
            Assert.IsNotNull(MediaUIHost.GetProfile(DataType.Picture).CreateTabFactory());
            Assert.IsFalse(MediaUIHost.GetProfile(DataType.Picture).UseVideoDetailsWindow);
        }

        [TestMethod]
        public void MediaUIProfile_SideNavigation_DistinctByDataType()
        {
            Assert.AreNotSame(
                MediaUIHost.GetProfile(DataType.Video).CreateSideNavigation(),
                MediaUIHost.GetProfile(DataType.Picture).CreateSideNavigation());
        }

        [TestMethod]
        public void SettingsSections_VideoVsPicture_CrawlerTabMask()
        {
            Assert.IsTrue(MediaUIHost.GetSettingsSections(DataType.Video).HasSection(SettingsSectionMask.Crawler));
            Assert.IsFalse(MediaUIHost.GetSettingsSections(DataType.Picture).HasSection(SettingsSectionMask.Crawler));
            Assert.IsTrue(MediaUIHost.GetSettingsSections(DataType.Picture).HasSection(SettingsSectionMask.PicturePaths));
        }

        [TestMethod]
        public void MediaUIProfile_TabFactory_PerDataType()
        {
            Assert.IsNotNull(MediaUIHost.GetProfile(DataType.Video).CreateTabFactory());
            Assert.IsNotNull(MediaUIHost.GetProfile(DataType.Picture).CreateTabFactory());
            Assert.AreNotSame(
                MediaUIHost.GetProfile(DataType.Video).CreateTabFactory(),
                MediaUIHost.GetProfile(DataType.Picture).CreateTabFactory());
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
            Assert.IsFalse(string.IsNullOrEmpty(DataTypeDisplay.GetLabel(DataType.Picture)));
        }
    }
}
