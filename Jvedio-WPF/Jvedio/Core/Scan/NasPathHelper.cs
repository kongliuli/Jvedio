using System;
using System.Collections.Generic;
using System.IO;

namespace Jvedio.Core.Scan
{
    /// <summary>NAS / 极空间等网络路径识别与扫描建议（Phase C+）。</summary>
    public static class NasPathHelper
    {
        private static readonly string[] ZSpaceHints = {
            "zspace", "zspool", "极空间", "zenithspace",
        };

        public static bool IsUncPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;
            return path.StartsWith(@"\\", StringComparison.Ordinal);
        }

        public static bool IsNetworkPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;
            if (IsUncPath(path))
                return true;

            try {
                if (!Path.IsPathRooted(path))
                    return false;
                string root = Path.GetPathRoot(path);
                if (string.IsNullOrEmpty(root) || root.Length < 2)
                    return false;
                if (root.StartsWith(@"\\", StringComparison.Ordinal))
                    return true;
                DriveInfo drive = new DriveInfo(root);
                return drive.DriveType == DriveType.Network;
            } catch {
                return false;
            }
        }

        public static bool IsLikelyZSpacePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;
            string lower = path.ToLowerInvariant();
            foreach (string hint in ZSpaceHints) {
                if (lower.Contains(hint))
                    return true;
            }
            return false;
        }

        /// <summary>ponytail: 规则建议，非设备探测；极空间无公开 SDK 时用路径/UNC 启发式。</summary>
        public static IReadOnlyList<string> GetScanRecommendations(string path)
        {
            var tips = new List<string>();
            if (!IsNetworkPath(path))
                return tips;

            tips.Add("网络路径建议开启「目录指纹增量缓存」，减少 NAS 全量遍历。");
            tips.Add("Picture 缩略图将优先读本地 ImageCache；首次浏览大图可能较慢。");

            if (IsLikelyZSpacePath(path)) {
                tips.Add("极空间：推荐 SMB/UNC 挂载（如 \\\\NAS\\share），WebDAV 需确保客户端已映射为盘符或 UNC。");
                tips.Add("极空间相册/远程下载链接可写入 metadata ExtraInfo，后续由 PictureRemoteImageService 拉取到本地缓存。");
            }

            return tips;
        }

        public static bool PathExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;
            try {
                if (File.Exists(path))
                    return true;
                return Directory.Exists(path);
            } catch {
                return false;
            }
        }
    }
}
