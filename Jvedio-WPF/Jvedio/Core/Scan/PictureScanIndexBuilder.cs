using Jvedio.Core.Enums;
using Jvedio.Entity.Data;
using SuperUtils.IO;
using SuperUtils.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Jvedio.Core.Scan
{
    public sealed class PictureScanSidecar
    {
        public List<string> ScannedRoots { get; } = new List<string>();
        public List<PictureFolderNode> FolderNodes { get; } = new List<PictureFolderNode>();
        public Dictionary<string, List<PictureFile>> FilesByAlbumPath { get; } =
            new Dictionary<string, List<PictureFile>>(StringComparer.OrdinalIgnoreCase);
    }

    internal static class PictureScanIndexBuilder
    {
        internal sealed class BuildResult
        {
            public List<Picture> Albums { get; set; } = new List<Picture>();
            public PictureScanSidecar Sidecar { get; set; } = new PictureScanSidecar();
            public List<string> NotImport { get; set; } = new List<string>();
        }

        public static BuildResult Build(IEnumerable<string> scanRoots, List<string> fileExt, DataType dataType, long dbId)
        {
            var result = new BuildResult();
            if (scanRoots == null)
                return result;

            HashSet<string> imageExt = new HashSet<string>(fileExt ?? ScanExtensions.PICTURE_EXTENSIONS_LIST, StringComparer.OrdinalIgnoreCase);

            foreach (string rawRoot in scanRoots) {
                if (string.IsNullOrWhiteSpace(rawRoot))
                    continue;
                string scanRoot = PicturePathHelper.NormalizeDir(rawRoot);
                if (!Directory.Exists(scanRoot))
                    continue;

                result.Sidecar.ScannedRoots.Add(scanRoot);
                Merge(BuildFromRoot(scanRoot, imageExt, dataType, dbId), result);
            }

            return result;
        }

        private static BuildResult BuildFromRoot(string scanRoot, HashSet<string> imageExt, DataType dataType, long dbId)
        {
            var result = new BuildResult();
            var dirsWithDirectImages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var dirsInTree = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string dir in EnumerateDirectoriesSafe(scanRoot)) {
                List<string> directImages = ListDirectFiles(dir, imageExt);
                List<string> directVideos = ListDirectVideos(dir);

                foreach (string file in Directory.GetFiles(dir)) {
                    string ext = Path.GetExtension(file).ToLower();
                    if (!imageExt.Contains(ext) && !ScanExtensions.VIDEO_EXTENSIONS_LIST.Contains(ext))
                        result.NotImport.Add(file);
                }

                if (directImages.Count == 0)
                    continue;

                dirsWithDirectImages.Add(PicturePathHelper.NormalizeDir(dir));
                Picture album = CreateAlbum(dir, scanRoot, directImages, directVideos, dataType, dbId);
                result.Albums.Add(album);

                var files = new List<PictureFile>();
                foreach (string imgPath in directImages) {
                    files.Add(new PictureFile {
                        FileName = Path.GetFileName(imgPath),
                        RelativePath = PicturePathHelper.ToRelativePath(scanRoot, imgPath),
                        Size = new FileInfo(imgPath).Length,
                    });
                }
                result.Sidecar.FilesByAlbumPath[album.Path] = files;
            }

            foreach (string dir in dirsWithDirectImages) {
                string current = dir;
                while (!string.IsNullOrEmpty(current) &&
                    current.StartsWith(scanRoot, StringComparison.OrdinalIgnoreCase)) {
                    dirsInTree.Add(current);
                    if (string.Equals(current, scanRoot, StringComparison.OrdinalIgnoreCase))
                        break;
                    current = PicturePathHelper.ParentDir(current);
                }
            }

            foreach (string dir in dirsInTree.OrderBy(d => d, StringComparer.OrdinalIgnoreCase)) {
                result.Sidecar.FolderNodes.Add(new PictureFolderNode {
                    DBId = dbId,
                    ScanRoot = scanRoot,
                    FullPath = dir,
                    ParentPath = ParentPathForTree(scanRoot, dir),
                    Name = Path.GetFileName(dir),
                    Depth = PicturePathHelper.DepthFromRoot(scanRoot, dir),
                    HasImages = 1,
                });
            }

            return result;
        }

        private static void Merge(BuildResult source, BuildResult target)
        {
            target.Albums.AddRange(source.Albums);
            target.NotImport.AddRange(source.NotImport);
            target.Sidecar.ScannedRoots.AddRange(source.Sidecar.ScannedRoots);
            target.Sidecar.FolderNodes.AddRange(source.Sidecar.FolderNodes);
            foreach (var kv in source.Sidecar.FilesByAlbumPath)
                target.Sidecar.FilesByAlbumPath[kv.Key] = kv.Value;
        }

        private static string ParentPathForTree(string scanRoot, string dir)
        {
            scanRoot = PicturePathHelper.NormalizeDir(scanRoot);
            dir = PicturePathHelper.NormalizeDir(dir);
            if (string.Equals(dir, scanRoot, StringComparison.OrdinalIgnoreCase))
                return null;
            string parent = PicturePathHelper.ParentDir(dir);
            if (string.IsNullOrEmpty(parent) || parent.Length < scanRoot.Length)
                return scanRoot;
            return parent;
        }

        private static Picture CreateAlbum(
            string dir,
            string scanRoot,
            List<string> directImages,
            List<string> directVideos,
            DataType dataType,
            long dbId)
        {
            dir = PicturePathHelper.NormalizeDir(dir);
            long totalSize = directImages.Sum(p => new FileInfo(p).Length);
            var hashPaths = new List<string>(directImages);
            hashPaths.AddRange(directVideos);

            return new Picture {
                DataType = dataType,
                DBId = dbId,
                Title = Path.GetFileName(dir),
                Path = dir,
                PicCount = directImages.Count,
                PicPaths = string.Join(SuperUtils.Values.ConstValues.SeparatorString,
                    directImages.Select(Path.GetFileName)),
                VideoPaths = string.Join(SuperUtils.Values.ConstValues.SeparatorString,
                    directVideos.Select(Path.GetFileName)),
                Size = totalSize,
                Hash = Encrypt.FasterDirMD5(hashPaths),
            };
        }

        private static List<string> ListDirectFiles(string dir, HashSet<string> imageExt)
        {
            var list = new List<string>();
            try {
                foreach (string file in Directory.GetFiles(dir)) {
                    if (imageExt.Contains(Path.GetExtension(file).ToLower()))
                        list.Add(file);
                }
            } catch (Exception) {
                // ponytail: skip unreadable folders
            }
            return list;
        }

        private static List<string> ListDirectVideos(string dir)
        {
            var list = new List<string>();
            try {
                foreach (string file in Directory.GetFiles(dir)) {
                    if (ScanExtensions.VIDEO_EXTENSIONS_LIST.Contains(Path.GetExtension(file).ToLower()))
                        list.Add(file);
                }
            } catch (Exception) {
            }
            return list;
        }

        private static IEnumerable<string> EnumerateDirectoriesSafe(string scanRoot)
        {
            yield return PicturePathHelper.NormalizeDir(scanRoot);
            string[] subDirs;
            try {
                subDirs = Directory.GetDirectories(scanRoot, "*", SearchOption.AllDirectories);
            } catch (Exception) {
                yield break;
            }
            foreach (string dir in subDirs)
                yield return PicturePathHelper.NormalizeDir(dir);
        }
    }
}
