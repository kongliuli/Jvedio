using System;
using System.IO;

namespace Jvedio.Core.Scan
{
    internal static class PicturePathHelper
    {
        public static string NormalizeDir(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;
            return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        public static string ToRelativePath(string scanRoot, string fullPath)
        {
            string root = NormalizeDir(Path.GetFullPath(scanRoot));
            string full = Path.GetFullPath(fullPath);
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                return full.Replace('\\', '/');
            string rel = full.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return rel.Replace('\\', '/');
        }

        public static int DepthFromRoot(string scanRoot, string folderPath)
        {
            string rel = ToRelativePath(scanRoot, folderPath);
            if (string.IsNullOrEmpty(rel))
                return 0;
            return rel.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static string ParentDir(string folderPath)
        {
            folderPath = NormalizeDir(folderPath);
            string parent = Path.GetDirectoryName(folderPath);
            return string.IsNullOrEmpty(parent) ? null : NormalizeDir(parent);
        }
    }
}
