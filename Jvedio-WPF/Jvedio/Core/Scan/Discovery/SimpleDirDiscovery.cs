using SuperUtils.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using static Jvedio.App;

namespace Jvedio.Core.Scan.Discovery
{
    public sealed class SimpleDirDiscovery : IFileDiscoveryBackend
    {
        public DiscoveryResult Discover(DiscoveryRequest request, IProgress<DiscoveryProgress> progress, CancellationTokenSource ct)
        {
            DiscoveryTiming timing = DiscoveryTiming.Start();
            var throttler = new DiscoveryProgressThrottler(progress);
            List<string> files = new List<string>();
            IReadOnlyList<string> roots = request.RootPaths ?? new List<string>();
            int total = roots.Count;
            int done = 0;

            foreach (string path in roots) {
                if (ct != null)
                    ct.Token.ThrowIfCancellationRequested();
                request.OnDirectoryScanning?.Invoke(path);
                IEnumerable<string> paths = DirHelper.GetFileList(path, "*.*", (ex) => {
                    Logger.Error(ex.Message);
                }, (dir) => {
                    request.OnDirectoryScanning?.Invoke(dir);
                }, ct);
                files.AddRange(paths);
                done++;
                throttler.Report(new DiscoveryProgress {
                    CurrentPath = path,
                    DirectoriesDone = done,
                    DirectoriesTotal = total,
                    Percent = total > 0 ? (float)done / total : 1f,
                });
            }

            AppendExplicitFiles(request, files);
            files = ApplyExtensionFilter(files, request.ExtensionFilter);
            timing.EndEnumerate(files.Count, total);
            throttler.Report(new DiscoveryProgress { Percent = 1f, DirectoriesDone = total, DirectoriesTotal = total }, force: true);

            return new DiscoveryResult {
                FilePaths = files,
                Timing = timing,
            };
        }

        internal static void AppendExplicitFiles(DiscoveryRequest request, List<string> files)
        {
            if (request.FilePaths == null)
                return;
            foreach (string file in request.FilePaths) {
                if (!string.IsNullOrEmpty(file) && File.Exists(file))
                    files.Add(file);
            }
        }

        internal static List<string> ApplyExtensionFilter(List<string> files, ISet<string> extensionFilter)
        {
            if (extensionFilter == null || extensionFilter.Count == 0)
                return files;
            return files.Where(f => extensionFilter.Contains(Path.GetExtension(f))).ToList();
        }
    }
}
