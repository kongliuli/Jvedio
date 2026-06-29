using Jvedio.Core.Library;
using Jvedio.Core.Scan;
using System;

namespace Jvedio.Core.UI
{
    /// <summary>Picture 侧栏目录 + 工具栏浏览状态（全局，作用于当前 Picture 库 Tab）。</summary>
    public static class PictureBrowseContext
    {
        public const string FolderCommandPrefix = "Folder:";

        public static string SelectedFolderPath { get; private set; }

        public static PictureBrowseMode BrowseMode =>
            (PictureBrowseMode)ConfigManager.VideoConfig.PictureBrowseMode;

        public static bool IncludeSubfolders => ConfigManager.VideoConfig.PictureIncludeSubfolders;

        public static void SetFolder(string fullPath)
        {
            SelectedFolderPath = string.IsNullOrWhiteSpace(fullPath)
                ? null
                : PicturePathHelper.NormalizeDir(fullPath);
            LibraryEventBus.RaisePictureBrowseChanged();
        }

        public static void ClearFolder()
        {
            if (string.IsNullOrEmpty(SelectedFolderPath))
                return;
            SelectedFolderPath = null;
            LibraryEventBus.RaisePictureBrowseChanged();
        }

        public static void SetBrowseMode(PictureBrowseMode mode)
        {
            if (ConfigManager.VideoConfig.PictureBrowseMode == (long)mode)
                return;
            ConfigManager.VideoConfig.PictureBrowseMode = (long)mode;
            ConfigManager.VideoConfig.Save();
            LibraryEventBus.RaisePictureBrowseChanged();
        }

        public static void SetIncludeSubfolders(bool include)
        {
            if (ConfigManager.VideoConfig.PictureIncludeSubfolders == include)
                return;
            ConfigManager.VideoConfig.PictureIncludeSubfolders = include;
            ConfigManager.VideoConfig.Save();
            LibraryEventBus.RaisePictureBrowseChanged();
        }

        public static bool TryParseFolderCommand(object command, out string folderPath)
        {
            folderPath = null;
            if (command == null)
                return false;
            string text = command.ToString();
            if (string.IsNullOrEmpty(text) || !text.StartsWith(FolderCommandPrefix, StringComparison.Ordinal))
                return false;
            folderPath = text.Substring(FolderCommandPrefix.Length);
            return !string.IsNullOrWhiteSpace(folderPath);
        }
    }
}
