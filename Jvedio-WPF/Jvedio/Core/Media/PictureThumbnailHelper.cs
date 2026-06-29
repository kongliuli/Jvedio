using Jvedio.Core.Scan;
using Jvedio.Entity;
using SuperUtils.IO;
using SuperUtils.Values;
using System;
using System.IO;
using System.Linq;

namespace Jvedio.Core.Media
{
    public static class PictureThumbnailHelper
    {
        public static void ApplyListImage(ref Video video, bool singleImageMode, string picPaths = null)
        {
            if (video == null)
                return;

            if (singleImageMode) {
                if (File.Exists(video.Path)) {
                    Video.SetImage(ref video, video.Path);
                    return;
                }
                if (NasPathHelper.IsNetworkPath(video.Path))
                    Video.SetImage(ref video, video.Path);
                return;
            }

            string cover = ResolveAlbumCoverPath(video, picPaths);
            if (!string.IsNullOrEmpty(cover) && (File.Exists(cover) || NasPathHelper.PathExists(cover)))
                Video.SetImage(ref video, cover);
            else
                Video.SetImage(ref video);
        }

        public static string ResolveAlbumCoverPath(Video video, string picPaths = null)
        {
            if (video == null || string.IsNullOrWhiteSpace(video.Path))
                return null;

            string folder = PicturePathHelper.NormalizeDir(video.Path);
            if (File.Exists(folder))
                return folder;

            picPaths = picPaths ?? string.Empty;
            if (string.IsNullOrWhiteSpace(picPaths))
                return TryFirstImageInFolder(folder);

            string first = picPaths.Split(new[] { ConstValues.SeparatorString }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(first))
                return TryFirstImageInFolder(folder);

            return Path.Combine(folder, first);
        }

        public static void HydrateSingleImageRow(Video video, System.Collections.Generic.Dictionary<string, object> row)
        {
            if (video == null || row == null)
                return;

            string scanRoot = row.TryGetValue("ScanRoot", out object sr) ? sr?.ToString() : null;
            string rel = row.TryGetValue("RelativePath", out object rp) ? rp?.ToString() : null;
            string fileName = row.TryGetValue("FileName", out object fn) ? fn?.ToString() : null;

            if (!string.IsNullOrWhiteSpace(scanRoot) && !string.IsNullOrWhiteSpace(rel))
                video.Path = PicturePathHelper.CombineFullPath(scanRoot, rel);
            if (!string.IsNullOrWhiteSpace(fileName))
                video.Title = fileName;
        }

        private static string TryFirstImageInFolder(string folder)
        {
            try {
                if (!Directory.Exists(folder))
                    return null;
                foreach (string file in Directory.GetFiles(folder)) {
                    string ext = Path.GetExtension(file).ToLower();
                    if (ScanExtensions.PICTURE_EXTENSIONS_LIST.Contains(ext))
                        return file;
                }
            } catch {
            }
            return null;
        }
    }
}
