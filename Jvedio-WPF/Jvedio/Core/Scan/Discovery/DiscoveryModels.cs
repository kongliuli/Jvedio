using System;
using System.Collections.Generic;

namespace Jvedio.Core.Scan.Discovery
{
    public enum DiscoveryMode
    {
        Full,
        Incremental,
    }

    public class DiscoveryRequest
    {
        public IReadOnlyList<string> RootPaths { get; set; } = new List<string>();
        public IReadOnlyList<string> FilePaths { get; set; } = new List<string>();
        public ISet<string> ExtensionFilter { get; set; }
        public DiscoveryMode Mode { get; set; } = DiscoveryMode.Full;
        public string VolumeKey { get; set; }
        public Action<string> OnDirectoryScanning { get; set; }
    }

    public class DiscoveryProgress
    {
        public string Phase { get; set; } = "Discover";
        public string CurrentPath { get; set; }
        public float Percent { get; set; }
        public int DirectoriesDone { get; set; }
        public int DirectoriesTotal { get; set; }
    }

    public class DiscoveryTiming
    {
        public long EnumerateMs { get; set; }
        public long FilterMs { get; set; }
        public long TotalMs { get; set; }
        public int DirectoryCount { get; set; }
        public int FileCount { get; set; }

        private DateTime _startUtc;
        private DateTime _enumerateStartUtc;

        public static DiscoveryTiming Start()
        {
            return new DiscoveryTiming {
                _startUtc = DateTime.UtcNow,
                _enumerateStartUtc = DateTime.UtcNow,
            };
        }

        public void EndEnumerate(int fileCount, int directoryCount)
        {
            FileCount = fileCount;
            DirectoryCount = directoryCount;
            EnumerateMs = (long)(DateTime.UtcNow - _enumerateStartUtc).TotalMilliseconds;
            TotalMs = (long)(DateTime.UtcNow - _startUtc).TotalMilliseconds;
        }

        public void EndFilter()
        {
            FilterMs = TotalMs - EnumerateMs;
            if (FilterMs < 0)
                FilterMs = 0;
            TotalMs = (long)(DateTime.UtcNow - _startUtc).TotalMilliseconds;
        }
    }

    public class DiscoveryResult
    {
        public List<string> FilePaths { get; set; } = new List<string>();
        public DiscoveryTiming Timing { get; set; }
        public bool FromCache { get; set; }
    }
}
