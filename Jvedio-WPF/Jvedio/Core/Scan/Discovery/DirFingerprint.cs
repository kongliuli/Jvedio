using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Jvedio.Core.Scan.Discovery
{
    public static class DirFingerprint
    {
        public static DirIndexEntry Snapshot(int dbId, string dirPath)
        {
            if (string.IsNullOrEmpty(dirPath) || !Directory.Exists(dirPath))
                return null;

            DirectoryInfo info = new DirectoryInfo(dirPath);
            string[] entries = Directory.GetFileSystemEntries(dirPath);
            return new DirIndexEntry {
                DbId = dbId,
                DirPath = dirPath,
                MtimeUtcTicks = info.LastWriteTimeUtc.Ticks,
                EntryCount = entries.Length,
                ChildrenChecksum = ComputeChildrenChecksum(entries.Select(Path.GetFileName)),
                LastScanUtc = DateTime.UtcNow.ToString("o"),
            };
        }

        public static bool Matches(DirIndexEntry cached, DirIndexEntry current)
        {
            if (cached == null || current == null)
                return false;
            return cached.MtimeUtcTicks == current.MtimeUtcTicks
                && cached.EntryCount == current.EntryCount
                && string.Equals(cached.ChildrenChecksum, current.ChildrenChecksum, StringComparison.Ordinal);
        }

        internal static string ComputeChildrenChecksum(IEnumerable<string> childNames)
        {
            unchecked {
                int hash = 17;
                foreach (string name in childNames.OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
                    hash = hash * 31 + StringComparer.OrdinalIgnoreCase.GetHashCode(name ?? string.Empty);
                return hash.ToString("x8");
            }
        }
    }
}
