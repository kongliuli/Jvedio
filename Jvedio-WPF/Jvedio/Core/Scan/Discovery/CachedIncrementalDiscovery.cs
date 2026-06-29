using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Jvedio.Core.Scan.Discovery
{
    public sealed class CachedIncrementalDiscovery : IFileDiscoveryBackend
    {
        private readonly IFileDiscoveryBackend _inner;
        private readonly IScanDirIndexStore _indexStore;

        public CachedIncrementalDiscovery(IFileDiscoveryBackend inner, IScanDirIndexStore indexStore = null)
        {
            _inner = inner ?? new SimpleDirDiscovery();
            _indexStore = indexStore ?? new ScanDirIndexStore();
        }

        public DiscoveryResult Discover(DiscoveryRequest request, IProgress<DiscoveryProgress> progress, CancellationTokenSource ct)
        {
            if (request == null)
                return new DiscoveryResult { FilePaths = new List<string>(), Timing = DiscoveryTiming.Start() };

            if (request.Mode != DiscoveryMode.Incremental)
                return DiscoverFull(request, progress, ct);

            return DiscoverIncremental(request, progress, ct);
        }

        private DiscoveryResult DiscoverFull(DiscoveryRequest request, IProgress<DiscoveryProgress> progress, CancellationTokenSource ct)
        {
            DiscoveryResult result = _inner.Discover(request, progress, ct);
            UpdateIndexForRoots(request, result.FilePaths);
            result.FromCache = false;
            return result;
        }

        private DiscoveryResult DiscoverIncremental(DiscoveryRequest request, IProgress<DiscoveryProgress> progress, CancellationTokenSource ct)
        {
            int dbId = ParseDbId(request.VolumeKey);
            List<string> dirsToScan = new List<string>();
            bool anyChange = false;

            foreach (string root in request.RootPaths ?? new List<string>()) {
                if (ct != null)
                    ct.Token.ThrowIfCancellationRequested();
                if (!Directory.Exists(root))
                    continue;

                DirIndexEntry rootCached = _indexStore.Get(dbId, root);
                DirIndexEntry rootCurrent = DirFingerprint.Snapshot(dbId, root);
                if (!DirFingerprint.Matches(rootCached, rootCurrent))
                    anyChange = true;

                dirsToScan.Add(root);

                foreach (string child in Directory.GetDirectories(root)) {
                    DirIndexEntry cached = _indexStore.Get(dbId, child);
                    DirIndexEntry current = DirFingerprint.Snapshot(dbId, child);
                    if (cached == null || !DirFingerprint.Matches(cached, current)) {
                        dirsToScan.Add(child);
                        anyChange = true;
                    }
                }
            }

            if (!anyChange && (request.FilePaths == null || request.FilePaths.Count == 0)) {
                DiscoveryTiming timing = DiscoveryTiming.Start();
                timing.EndEnumerate(0, request.RootPaths?.Count ?? 0);
                return new DiscoveryResult {
                    FilePaths = new List<string>(),
                    Timing = timing,
                    FromCache = true,
                };
            }

            var reduced = new DiscoveryRequest {
                RootPaths = dirsToScan.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                FilePaths = request.FilePaths,
                ExtensionFilter = request.ExtensionFilter,
                Mode = DiscoveryMode.Full,
                VolumeKey = request.VolumeKey,
                OnDirectoryScanning = request.OnDirectoryScanning,
            };

            DiscoveryResult result = _inner.Discover(reduced, progress, ct);
            UpdateIndexForRoots(request, result.FilePaths);
            result.FromCache = false;
            return result;
        }

        private void UpdateIndexForRoots(DiscoveryRequest request, List<string> discoveredFiles)
        {
            int dbId = ParseDbId(request.VolumeKey);
            if (dbId <= 0)
                return;

            HashSet<string> updated = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string root in request.RootPaths ?? new List<string>()) {
                if (!Directory.Exists(root))
                    continue;
                UpsertDir(dbId, root, updated);
                try {
                    foreach (string child in Directory.GetDirectories(root))
                        UpsertDir(dbId, child, updated);
                } catch {
                    // ponytail: skip unreadable child dirs; next full scan will retry
                }
            }
        }

        private void UpsertDir(int dbId, string dirPath, HashSet<string> updated)
        {
            if (!updated.Add(dirPath))
                return;
            DirIndexEntry entry = DirFingerprint.Snapshot(dbId, dirPath);
            if (entry != null)
                _indexStore.Upsert(entry);
        }

        private static int ParseDbId(string volumeKey)
        {
            if (string.IsNullOrEmpty(volumeKey))
                return (int)ConfigManager.Main.CurrentDBId;
            return int.TryParse(volumeKey, out int dbId) ? dbId : (int)ConfigManager.Main.CurrentDBId;
        }
    }
}
