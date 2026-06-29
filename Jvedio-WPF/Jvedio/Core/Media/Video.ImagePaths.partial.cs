using Jvedio.Core.Enums;
using Jvedio.Core.Global;
using Jvedio.Core.Scan;
using SuperUtils.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Jvedio.App;

namespace Jvedio.Entity
{
    public partial class Video
    {        private string GetImagePath(ImageType imageType, string ext = null)
        {
            string result = string.Empty;
            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            string basePicPath = ConfigManager.Settings.PicPaths[pathType.ToString()].ToString();
            if (pathType != PathType.RelativeToData) {
                if (pathType == PathType.RelativeToApp)
                    basePicPath = System.IO.Path.Combine(PathManager.CurrentUserFolder, basePicPath);
                string saveDir = string.Empty;
                if (imageType == ImageType.Big)
                    saveDir = System.IO.Path.Combine(basePicPath, "BigPic");
                else if (imageType == ImageType.Small)
                    saveDir = System.IO.Path.Combine(basePicPath, "SmallPic");
                else if (imageType == ImageType.Preview)
                    saveDir = System.IO.Path.Combine(basePicPath, "ExtraPic");
                else if (imageType == ImageType.ScreenShot)
                    saveDir = System.IO.Path.Combine(basePicPath, "ScreenShot");
                else if (imageType == ImageType.Gif)
                    saveDir = System.IO.Path.Combine(basePicPath, "Gif");
                else if (imageType == ImageType.Actor) {
                    saveDir = System.IO.Path.Combine(basePicPath, "Actresses");
                    return System.IO.Path.GetFullPath(saveDir);
                }
                if (!Directory.Exists(saveDir))
                    FileHelper.TryCreateDir(saveDir);
                if (!string.IsNullOrEmpty(VID))
                    result = System.IO.Path.Combine(saveDir, $"{VID}{(string.IsNullOrEmpty(ext) ? string.Empty : ext)}");
                else
                    result = System.IO.Path.Combine(saveDir, $"{Hash}{(string.IsNullOrEmpty(ext) ? string.Empty : ext)}");
            } else {
                // todo 其它图片模式
            }

            if (!string.IsNullOrEmpty(result))
                return System.IO.Path.GetFullPath(result);
            return string.Empty;
        }

        private static string ParseRelativeImageFileName(string path)
        {
            string dirName = System.IO.Path.GetDirectoryName(path);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path).ToLower();

            string[] arr = FileHelper.TryGetAllFiles(dirName, "*.*");
            if (arr == null || arr.Length == 0)
                return "";
            List<string> list = arr.ToList();

            list = list.Where(arg => ScanExtensions.PICTURE_EXTENSIONS_LIST.Contains(System.IO.Path.GetExtension(arg).ToLower())).ToList();

            foreach (string item in list) {
                if (System.IO.Path.GetFileNameWithoutExtension(item).ToLower().IndexOf(fileName) >= 0)
                    return item;
            }

            return FileHelper.TryGetFullPath(path);
        }

        private static string ParseRelativePath(string path)
        {
            string rootDir = System.IO.Path.GetDirectoryName(path);
            List<string> list = DirHelper.TryGetDirList(rootDir).ToList();
            string dirName = System.IO.Path.GetFileName(path);
            foreach (string item in list) {
                if (System.IO.Path.GetFileName(item).ToLower().IndexOf(dirName.ToLower()) >= 0)
                    return item;
            }

            return FileHelper.TryGetFullPath(path);
        }

        public string GetSmallImage(string ext = ".jpg", bool searchExt = true)
        {
            string smallImagePath = GetImagePath(ImageType.Small, ext);
            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path)) {
                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)ConfigManager.Settings.PicPaths[pathType.ToString()];
                string smallPath = System.IO.Path.Combine(basePicPath, dict["SmallImagePath"]);
                if (string.IsNullOrEmpty(System.IO.Path.GetExtension(smallPath)))
                    smallPath += ext;
                smallImagePath = ParseRelativeImageFileName(smallPath);
            }

            // 替换成其他扩展名
            if (searchExt && !File.Exists(smallImagePath))
                smallImagePath = FileHelper.FindWithExt(smallImagePath, ScanExtensions.PICTURE_EXTENSIONS_LIST);
            return FileHelper.TryGetFullPath(smallImagePath);
        }

        public string GetBigImage(string ext = ".jpg", bool searchExt = true)
        {
            string bigImagePath = GetImagePath(ImageType.Big, ext);

            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path)) {
                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)ConfigManager.Settings.PicPaths[pathType.ToString()];
                string bigPath = FileHelper.TryGetFullPath(System.IO.Path.Combine(basePicPath, dict["BigImagePath"]));
                if (string.IsNullOrEmpty(System.IO.Path.GetExtension(bigPath)))
                    bigPath += ext;
                bigImagePath = ParseRelativeImageFileName(bigPath);
            }

            // 替换成其他扩展名
            if (searchExt && !File.Exists(bigImagePath))
                bigImagePath = FileHelper.FindWithExt(bigImagePath, ScanExtensions.PICTURE_EXTENSIONS_LIST);
            return FileHelper.TryGetFullPath(bigImagePath);
        }

        public string GetExtraImage()
        {
            string imagePath = GetImagePath(ImageType.Preview);

            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path)) {
                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)ConfigManager.Settings.PicPaths[pathType.ToString()];
                string path = FileHelper.TryGetFullPath(System.IO.Path.Combine(basePicPath, dict["PreviewImagePath"]));
                imagePath = ParseRelativePath(path);
            }

            return FileHelper.TryGetFullPath(imagePath);
        }
        public string GetActorPath()
        {
            string imagePath = GetImagePath(ImageType.Actor);

            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path)) {
                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)ConfigManager.Settings.PicPaths[pathType.ToString()];
                string path = FileHelper.TryGetFullPath(System.IO.Path.Combine(basePicPath, dict["ActorImagePath"]));
                imagePath = ParseRelativePath(path);
            }

            return FileHelper.TryGetFullPath(imagePath);
        }

        public string GetScreenShot()
        {
            string imagePath = GetImagePath(ImageType.ScreenShot);

            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path)) {
                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)ConfigManager.Settings.PicPaths[pathType.ToString()];
                string path = FileHelper.TryGetFullPath(System.IO.Path.Combine(basePicPath, dict["ScreenShotPath"]));
                imagePath = ParseRelativePath(path);
            }

            return imagePath;
        }

        /// <summary>
        /// 相对影片下，不支持 gif 截图，截图任务会提示给定关键字不在字典中
        /// </summary>
        /// <returns></returns>
        public string GetGifPath()
        {
            string imagePath = GetImagePath(ImageType.Gif, ".gif");

            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path)) {
                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)ConfigManager.Settings.PicPaths[pathType.ToString()];
                string path = FileHelper.TryGetFullPath(System.IO.Path.Combine(basePicPath, dict["Gif"]));
                imagePath = ParseRelativePath(path);
            }

            return FileHelper.TryGetFullPath(imagePath);
        }
    }
}

