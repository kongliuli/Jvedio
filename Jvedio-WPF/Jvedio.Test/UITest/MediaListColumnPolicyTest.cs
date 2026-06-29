using Jvedio.Core.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jvedio.Test.UITest
{
    [TestClass]
    public class MediaListColumnPolicyTest
    {
        [TestMethod]
        public void VideoMode_ShowsVid_HidesGenreSeries()
        {
            Assert.IsTrue(MediaListColumnPolicy.IsVisible(MediaListMode.Video, MediaListColumn.Vid));
            Assert.IsFalse(MediaListColumnPolicy.IsVisible(MediaListMode.Video, MediaListColumn.Genre));
        }

        [TestMethod]
        public void PictureMode_ShowsPathSizeImportDate()
        {
            Assert.IsTrue(MediaListColumnPolicy.IsVisible(MediaListMode.Picture, MediaListColumn.Path));
            Assert.IsTrue(MediaListColumnPolicy.IsVisible(MediaListMode.Picture, MediaListColumn.Size));
            Assert.IsTrue(MediaListColumnPolicy.IsVisible(MediaListMode.Picture, MediaListColumn.LastScanDate));
            Assert.IsFalse(MediaListColumnPolicy.IsVisible(MediaListMode.Picture, MediaListColumn.Vid));
        }
    }
}
