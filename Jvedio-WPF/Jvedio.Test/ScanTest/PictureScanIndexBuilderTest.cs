using Jvedio.Core.Enums;
using Jvedio.Core.Scan;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace Jvedio.Test.ScanTest
{
    [TestClass]
    public class PictureScanIndexBuilderTest
    {
        [TestMethod]
        public void Build_OneAlbumPerFolderWithDirectImages()
        {
            string root = Path.Combine(Path.GetTempPath(), "jvedio-pic-b-" + Path.GetRandomFileName());
            string album = Path.Combine(root, "setA");
            Directory.CreateDirectory(album);
            File.WriteAllBytes(Path.Combine(album, "a.jpg"), new byte[] { 1, 2 });
            File.WriteAllBytes(Path.Combine(album, "b.png"), new byte[] { 3 });

            try {
                var result = PictureScanIndexBuilder.Build(new[] { root }, ScanExtensions.PICTURE_EXTENSIONS_LIST, DataType.Picture, 1);
                Assert.AreEqual(1, result.Albums.Count);
                Assert.AreEqual(album, result.Albums[0].Path);
                Assert.AreEqual(2, result.Albums[0].PicCount);
                Assert.IsTrue(result.Sidecar.FilesByAlbumPath[album].Count == 2);
                Assert.IsTrue(result.Sidecar.FolderNodes.Any(n => n.FullPath == album));
                Assert.IsTrue(result.Sidecar.FolderNodes.Any(n => n.FullPath == root));
            } finally {
                if (Directory.Exists(root))
                    Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void Build_SkipsEmptyIntermediateFoldersFromTreeLeaves()
        {
            string root = Path.Combine(Path.GetTempPath(), "jvedio-pic-b-" + Path.GetRandomFileName());
            string empty = Path.Combine(root, "empty");
            string album = Path.Combine(root, "nested", "pics");
            Directory.CreateDirectory(empty);
            Directory.CreateDirectory(album);
            File.WriteAllBytes(Path.Combine(album, "x.jpg"), new byte[] { 9 });

            try {
                var result = PictureScanIndexBuilder.Build(new[] { root }, ScanExtensions.PICTURE_EXTENSIONS_LIST, DataType.Picture, 1);
                Assert.AreEqual(1, result.Albums.Count);
                Assert.IsFalse(result.Sidecar.FolderNodes.Any(n => n.FullPath == empty));
                Assert.IsTrue(result.Sidecar.FolderNodes.Any(n => n.FullPath == album));
            } finally {
                if (Directory.Exists(root))
                    Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void PicturePathHelper_ToRelativePath_UsesForwardSlashes()
        {
            string root = @"C:\scan";
            string file = @"C:\scan\a\b.jpg";
            string rel = PicturePathHelper.ToRelativePath(root, file);
            StringAssert.Contains(rel, "a/b.jpg");
        }
    }
}
