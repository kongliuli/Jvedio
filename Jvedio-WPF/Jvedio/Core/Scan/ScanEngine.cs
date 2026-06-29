using Jvedio.Core.Enums;
using Jvedio.Core.Library;
using Jvedio.Core.Metadata;
using Jvedio.Core.Scan.Discovery;
using Jvedio.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Jvedio.Core.Scan
{
    public static class ScanEngine
    {
        private static LibraryMonitor _monitor;

        public static ScanJobBase CreateJob(LibraryContext library, ScanOptions options = null)
        {
            if (library == null)
                return ScanFactory.ProduceScanner(DataType.Video, new List<string>(), new List<string>());

            ScanOptions opts = options ?? new ScanOptions();
            if (opts.Mode == ScanMode.RefreshMetadata)
                return null;

            return ScanFactory.ProduceScanner(
                library.DataType,
                library.RootPaths ?? new List<string>(),
                opts.FilePaths ?? new List<string>(),
                opts.FileExt,
                opts.DiscoveryMode,
                opts.Mode);
        }

        /// <summary>
        /// 仅刷新元数据（NFO/爬虫），不创建扫描任务。对应 <see cref="ScanMode.RefreshMetadata"/>。
        /// </summary>
        public static int RefreshMetadata(IEnumerable<Video> videos)
        {
            return MetadataRefreshService.Refresh(videos);
        }

        public static DiscoveryResult Discover(LibraryContext library, ScanOptions options = null, System.Threading.CancellationTokenSource ct = null)
        {
            if (library == null)
                return new DiscoveryResult { FilePaths = new List<string>(), Timing = DiscoveryTiming.Start() };

            ScanOptions opts = options ?? new ScanOptions();
            var request = new DiscoveryRequest {
                RootPaths = library.RootPaths ?? new List<string>(),
                FilePaths = opts.FilePaths ?? new List<string>(),
                ExtensionFilter = ScanDiscoveryHelper.ExtensionFilterFor(library.DataType, opts.FileExt),
                Mode = opts.DiscoveryMode,
                VolumeKey = library.DBId.ToString(),
            };
            return DiscoveryBackendFactory.Discover(request, ct);
        }

        public static ScanJobBase RunIncremental(LibraryContext library, IReadOnlyList<string> changedPaths)
        {
            if (library == null || changedPaths == null || changedPaths.Count == 0)
                return null;

            List<string> files = changedPaths.Where(System.IO.File.Exists).ToList();
            List<string> dirs = changedPaths.Where(System.IO.Directory.Exists).ToList();
            if (files.Count == 0 && dirs.Count == 0)
                return null;

            return ScanFactory.ProduceScanner(library.DataType, dirs, files, discoveryMode: DiscoveryMode.Incremental);
        }

        public static void StartWatching(LibraryContext library, System.Action<ScanJobBase> onIncrementalJob = null)
        {
            StopWatching();
            if (library == null || library.RootPaths == null || library.RootPaths.Count == 0)
                return;

            _monitor = new LibraryMonitor();
            _monitor.PathsChanged += paths => {
                ScanJobBase job = RunIncremental(library, paths);
                if (job != null)
                    onIncrementalJob?.Invoke(job);
            };
            _monitor.Watch(library.RootPaths);
        }

        public static void StopWatching()
        {
            _monitor?.Dispose();
            _monitor = null;
        }
    }
}
