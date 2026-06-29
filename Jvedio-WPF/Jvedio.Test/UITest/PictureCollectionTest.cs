using Jvedio.Core.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jvedio.Test.UITest
{
    [TestClass]
    public class PictureCollectionTest
    {
        [TestMethod]
        public void BuildCollectionScopeSql_AlbumMode_ContainsAlbumAndFolderClauses()
        {
            string sql = PictureCollectionService.BuildCollectionScopeSql(3, PictureBrowseMode.Album);
            Assert.IsTrue(sql.Contains("CollectionID=3"));
            Assert.IsTrue(sql.Contains("ItemType='album'"));
            Assert.IsTrue(sql.Contains("ItemType='folder'"));
        }

        [TestMethod]
        public void BuildCollectionScopeSql_SingleImageMode_ContainsFileClause()
        {
            string sql = PictureCollectionService.BuildCollectionScopeSql(2, PictureBrowseMode.SingleImage);
            Assert.IsTrue(sql.Contains("pf.FID IN"));
            Assert.IsTrue(sql.Contains("ItemType='file'"));
        }

        [TestMethod]
        public void ItemTypes_AreStable()
        {
            Assert.AreEqual("album", PictureCollectionItemTypes.Album);
            Assert.AreEqual("file", PictureCollectionItemTypes.File);
            Assert.AreEqual("folder", PictureCollectionItemTypes.Folder);
        }

        [TestMethod]
        public void BrowseContext_CollectionAndFolder_AreMutuallyExclusive()
        {
            PictureBrowseContext.SetFolder(@"D:\pics\a");
            Assert.IsFalse(string.IsNullOrEmpty(PictureBrowseContext.SelectedFolderPath));
            Assert.AreEqual(0, PictureBrowseContext.SelectedCollectionId);

            PictureBrowseContext.SetCollection(5, "test");
            Assert.AreEqual(5, PictureBrowseContext.SelectedCollectionId);
            Assert.IsTrue(string.IsNullOrEmpty(PictureBrowseContext.SelectedFolderPath));
        }
    }
}
