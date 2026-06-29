using Jvedio.Core.Enums;
using Jvedio.Core.Scan.Discovery;
using SuperUtils.Framework.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Jvedio.App;

namespace Jvedio.Core.Scan
{
    public static class ScanDiscoveryHelper
    {
        public const float DefaultDiscoverProgressWeight = 30f;

        public static DiscoveryResult RunDiscover(
            ScanJobBase job,
            ISet<string> extensionFilter,
            float progressWeight = DefaultDiscoverProgressWeight)
        {
            if (job == null)
                return new DiscoveryResult { FilePaths = new List<string>(), Timing = DiscoveryTiming.Start() };

            var request = new DiscoveryRequest {
                RootPaths = job.ScanPaths,
                FilePaths = job.FilePaths,
                ExtensionFilter = extensionFilter,
                Mode = job.DiscoveryMode,
                VolumeKey = ((int)ConfigManager.Main.CurrentDBId).ToString(),
                OnDirectoryScanning = job.RaiseScanning,
            };

            var progress = new Progress<DiscoveryProgress>(p => {
                job.Progress = p.Percent * progressWeight;
                if (!string.IsNullOrEmpty(p.CurrentPath))
                    job.Message = $"Discover {p.Phase} {Math.Min(100, (int)(p.Percent * 100))}%";
            });

            AbstractTask task = job as AbstractTask;
            DiscoveryResult result = DiscoveryBackendFactory.Discover(request, task?.TokenCTS, progress);
            if (result?.FilePaths != null && result.FilePaths.Count > 0)
                job.FilePaths.AddRange(result.FilePaths);
            job.Progress = progressWeight;
            RecordDiscoverResult(job, result);
            return result;
        }

        public static void RecordDiscoverResult(ScanJobBase job, DiscoveryResult result)
        {
            if (job?.ScanResult == null || result?.Timing == null)
                return;

            job.ScanResult.DiscoverFileCount = result.Timing.FileCount;
            job.ScanResult.DiscoverEnumerateMs = result.Timing.EnumerateMs;
            job.ScanResult.DiscoverFromCache = result.FromCache;

            string line = $"discover: {result.Timing.FileCount} files, {result.Timing.EnumerateMs}ms, cache={result.FromCache}";
            (job as AbstractTask)?.Logs?.Add(line);
            job.ScanResult.Logs?.Add(line);
        }

        public static Dictionary<string, List<string>> GroupByScanRoots(
            IReadOnlyList<string> scanPaths,
            IEnumerable<string> filePaths)
        {
            var dict = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            if (scanPaths == null || scanPaths.Count == 0)
                return dict;

            foreach (string root in scanPaths) {
                if (string.IsNullOrEmpty(root))
                    continue;
                dict[NormalizeDir(root)] = new List<string>();
            }

            if (filePaths == null)
                return dict;

            foreach (string file in filePaths) {
                string owner = FindOwningRoot(scanPaths, file);
                if (owner == null)
                    continue;
                string key = NormalizeDir(owner);
                if (dict.TryGetValue(key, out List<string> list))
                    list.Add(file);
            }

            return dict;
        }

        public static void RemoveEmptyPathGroups(Dictionary<string, List<string>> pathDict)
        {
            if (pathDict == null)
                return;
            foreach (string key in pathDict.Keys.ToList()) {
                if (pathDict[key] == null || pathDict[key].Count == 0)
                    pathDict.Remove(key);
            }
        }

        private static string FindOwningRoot(IReadOnlyList<string> scanPaths, string filePath)
        {
            string best = null;
            int bestLen = -1;
            string normalizedFile = NormalizeDir(filePath);

            foreach (string root in scanPaths) {
                if (string.IsNullOrEmpty(root))
                    continue;
                string normalizedRoot = NormalizeDir(root);
                if (normalizedFile.Length <= normalizedRoot.Length)
                    continue;
                if (!normalizedFile.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (normalizedRoot.Length > bestLen) {
                    best = root;
                    bestLen = normalizedRoot.Length;
                }
            }

            return best;
        }

        private static string NormalizeDir(string path)
        {
            return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        public static ISet<string> ExtensionFilterFor(DataType dataType, IEnumerable<string> fileExt)
        {
            switch (dataType) {
                case DataType.Video:
                    return ScanExtensions.VIDEO_EXTENSIONS_SET;
                case DataType.Picture:
                    return null;
                default:
                    return null;
            }
        }
    }
}
