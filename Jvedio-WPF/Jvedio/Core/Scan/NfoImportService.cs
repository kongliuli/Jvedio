using Jvedio.Core.Enums;
using Jvedio.Entity;
using SuperControls.Style;
using SuperUtils.IO;
using SuperUtils.Time;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Jvedio.App;
using static Jvedio.MapperManager;

namespace Jvedio.Core.Scan
{
    public interface INfoImportService
    {
        void CopyImagesForImport(List<Video> import);
    }

    internal interface INfoImportBatchService : INfoImportService
    {
        void ImportNfoUpdates(List<Video> import, ExistVideoIndex existVideoIndex, ScanResult scanResult, Action<string> reportInsertError);
    }

    internal sealed class NfoImportService : INfoImportBatchService
    {
        private static readonly string[] NfoUpdateMetaProps = {
            "Title", "ReleaseYear", "ReleaseDate", "Country", "Genre", "Rating", "LastScanDate", "PathExist",
        };

        private static readonly string[] NfoUpdateVideoProps = {
            "Plot", "Director", "Duration", "Studio", "Series", "Outline",
        };

        public void CopyImagesForImport(List<Video> import)
        {
            if (import == null || import.Count == 0)
                return;
            if (!ConfigManager.ScanConfig.CopyNFOPicture &&
                !ConfigManager.ScanConfig.CopyNFOActorPicture &&
                !ConfigManager.ScanConfig.CopyNFOPreview &&
                !ConfigManager.ScanConfig.CopyNFOScreenShot)
                return;

            Dictionary<string, object> picPaths = ConfigManager.Settings.PicPaths;
            if (picPaths == null || !picPaths.ContainsKey(PathType.RelativeToData.ToString()))
                return;

            Dictionary<string, string> dict = null;
            try {
                dict = (Dictionary<string, string>)picPaths[PathType.RelativeToData.ToString()];
            } catch {
                return;
            }

            if (ConfigManager.ScanConfig.CopyNFOPicture) {
                CopyNfoImage(dict, import, ImageType.Big);
                CopyNfoImage(dict, import, ImageType.Small);
            }
            if (ConfigManager.ScanConfig.CopyNFOActorPicture)
                CopyNfoImage(dict, import, ImageType.Actor);
            if (ConfigManager.ScanConfig.CopyNFOPreview)
                CopyNfoImage(dict, import, ImageType.Preview);
            if (ConfigManager.ScanConfig.CopyNFOScreenShot)
                CopyNfoImage(dict, import, ImageType.ScreenShot);
        }

        public void ImportNfoUpdates(List<Video> import, ExistVideoIndex existVideoIndex, ScanResult scanResult, Action<string> reportInsertError)
        {
            if (import == null || import.Count <= 0 || existVideoIndex == null || scanResult == null)
                return;

            CopyImagesForImport(import);
            var existActors = actorMapper.SelectList();
            var toUpdate = new List<Video>();

            foreach (Video video in import) {
                Video existVideo = existVideoIndex.FindByVid(video.VID);
                if (existVideo == null)
                    continue;
                video.DataID = existVideo.DataID;
                video.MVID = existVideo.MVID;
                scanResult.Update.Add(video.Path, LangManager.GetValueByKey("UpdateNFO"));
                video.Path = null;
                video.PathExist = 1;
                video.LastScanDate = DateHelper.Now();
                toUpdate.Add(video);
                VideoScanPersistence.HandleActor(video, existActors);
            }

            import.RemoveAll(arg => existVideoIndex.HasAnyWithSameVid(arg));

            if (toUpdate.Count > 0)
                videoMapper.UpdateBatch(toUpdate, NfoUpdateVideoProps);
            List<MetaData> toUpdateData = toUpdate.Select(arg => arg.toMetaData()).ToList();
            if (toUpdateData.Count > 0)
                metaDataMapper.UpdateBatch(toUpdateData, NfoUpdateMetaProps);

            VideoScanPersistence.InsertVideos(import, scanResult, reportInsertError);
        }

        private static void CopyNfoImage(Dictionary<string, string> dict, List<Video> import, ImageType imageType)
        {
            if (dict == null)
                return;

            switch (imageType) {
                case ImageType.Big:
                    if (dict.ContainsKey("BigImagePath") && !string.IsNullOrEmpty(dict["BigImagePath"]))
                        CopyImage(import, dict["BigImagePath"].ToLower(), imageType);
                    break;
                case ImageType.Small:
                    if (dict.ContainsKey("SmallImagePath") && !string.IsNullOrEmpty(dict["SmallImagePath"]))
                        CopyImage(import, dict["SmallImagePath"].ToLower(), imageType);
                    break;
                case ImageType.Actor:
                case ImageType.Preview:
                case ImageType.ScreenShot:
                    CopyImages(import, imageType);
                    break;
            }
        }

        private static string GetImagePathByType(Video video, ImageType type)
        {
            switch (type) {
                case ImageType.Big: return video.GetBigImage();
                case ImageType.Small: return video.GetSmallImage();
                case ImageType.ScreenShot: return video.GetScreenShot();
                case ImageType.Preview: return video.GetExtraImage();
                case ImageType.Actor: return video.GetBigImage();
            }
            return "";
        }

        private static void CopyImage(List<Video> import, string dirName, ImageType imageType)
        {
            if (string.IsNullOrEmpty(dirName))
                return;

            foreach (Video item in import) {
                if (string.IsNullOrEmpty(item.Path))
                    continue;
                string dir = Path.GetDirectoryName(item.Path);
                if (!Directory.Exists(dir))
                    continue;
                string[] arr = FileHelper.TryGetAllFiles(dir, "*.*");
                if (arr == null || arr.Length == 0)
                    continue;
                List<string> list = arr.ToList();
                list = list.Where(arg => ScanExtensions.PICTURE_EXTENSIONS_SET
                    .Contains(Path.GetExtension(arg).ToLower())).ToList();
                string filename = Path.GetFileName(dirName);
                string originPath = list.Where(arg =>
                    Path.GetFileNameWithoutExtension(arg).ToLower().IndexOf(filename) >= 0).FirstOrDefault();
                if (!File.Exists(originPath))
                    continue;
                string targetImagePath = GetImagePathByType(item, imageType);
                if (!File.Exists(targetImagePath)) {
                    targetImagePath = Path.Combine(Path.GetDirectoryName(targetImagePath),
                        Path.GetFileNameWithoutExtension(targetImagePath) + Path.GetExtension(originPath));
                    FileHelper.TryCopyFile(originPath, targetImagePath, true);
                } else if (ConfigManager.ScanConfig.CopyNFOOverwriteImage) {
                    FileHelper.TryCopyFile(originPath, targetImagePath, true);
                }
            }
        }

        private static void CopyImages(List<Video> import, ImageType type)
        {
            foreach (Video item in import) {
                if (string.IsNullOrEmpty(item.Path))
                    continue;
                string dir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(item.Path), ConfigManager.ScanConfig.CopyNFOPreviewPath));
                if (type == ImageType.ScreenShot)
                    dir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(item.Path), ConfigManager.ScanConfig.CopyNFOScreenShotPath));
                else if (type == ImageType.Actor)
                    dir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(item.Path), ConfigManager.ScanConfig.CopyNFOActorPath));
                if (!Directory.Exists(dir))
                    continue;
                string[] arr = FileHelper.TryGetAllFiles(dir, "*.*");
                if (arr == null || arr.Length == 0)
                    continue;
                List<string> list = arr.ToList();
                list = list.Where(arg => ScanExtensions.PICTURE_EXTENSIONS_SET.Contains(Path.GetExtension(arg).ToLower())).ToList();
                string targetPath = item.GetExtraImage();
                if (type == ImageType.ScreenShot)
                    targetPath = item.GetScreenShot();
                else if (type == ImageType.Actor)
                    targetPath = item.GetActorPath();
                DirHelper.TryCreateDirectory(targetPath);
                foreach (string path in list) {
                    string targetFilePath = Path.Combine(targetPath, Path.GetFileName(path));
                    FileHelper.TryCopyFile(path, targetFilePath, ConfigManager.ScanConfig.CopyNFOOverwriteImage);
                }
            }
        }
    }
}
