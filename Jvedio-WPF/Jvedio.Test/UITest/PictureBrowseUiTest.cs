using Jvedio.Core.Scan;
using Jvedio.Core.UI;
using Jvedio.Entity;
using Jvedio.Entity.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SuperUtils.Framework.ORM.Wrapper;
using System.Collections.Generic;

namespace Jvedio.Test.UITest
{
    [TestClass]
    public class PictureBrowseUiTest
    {
        [TestMethod]
        public void FolderTree_BuildForest_LinksParentChild()
        {
            var nodes = new List<PictureFolderNode> {
                new PictureFolderNode { FullPath = @"D:\pics", ParentPath = null, Name = "pics" },
                new PictureFolderNode { FullPath = @"D:\pics\a", ParentPath = @"D:\pics", Name = "a" },
                new PictureFolderNode { FullPath = @"D:\pics\a\b", ParentPath = @"D:\pics\a", Name = "b" },
            };

            var forest = PictureFolderTreeService.BuildForest(nodes);
            Assert.AreEqual(1, forest.Count);
            Assert.AreEqual(1, forest[0].Children.Count);
            Assert.AreEqual("b", forest[0].Children[0].Children[0].Name);
        }

        [TestMethod]
        public void ListQuery_AlbumScope_UsesParentJoin()
        {
            var wrapper = new SelectWrapper<Video>();
            string sql = PictureListQuery.AlbumFromSql;
            PictureListQuery.ApplyFolderScope(
                wrapper,
                ref sql,
                @"D:\pics\root",
                PictureBrowseMode.Album,
                includeSubfolders: true);

            Assert.IsTrue(sql.Contains("pfn_scope"));
        }

        [TestMethod]
        public void NasPathHelper_DetectsUncAndZSpaceHint()
        {
            Assert.IsTrue(NasPathHelper.IsUncPath(@"\\zspace\photo"));
            Assert.IsTrue(NasPathHelper.IsLikelyZSpacePath(@"\\192.168.1.8\zspool\pics"));
            Assert.IsTrue(NasPathHelper.GetScanRecommendations(@"\\nas\share").Count >= 1);
        }

        [TestMethod]
        public void PathHelper_CombineFullPath_FromRelative()
        {
            string full = PicturePathHelper.CombineFullPath(@"D:\root", "a/b.jpg");
            Assert.IsTrue(full.EndsWith("a\\b.jpg") || full.EndsWith("a/b.jpg"));
        }
    }
}
