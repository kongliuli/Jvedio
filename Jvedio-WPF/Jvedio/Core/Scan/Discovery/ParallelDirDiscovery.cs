using SuperUtils.IO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Jvedio.App;

namespace Jvedio.Core.Scan.Discovery
{
    public sealed class ParallelDirDiscovery : IFileDiscoveryBackend
    {
        public DiscoveryResult Discover(DiscoveryRequest request, IProgress<DiscoveryProgress> progress, CancellationTokenSource ct)
        {
            DiscoveryTiming timing = DiscoveryTiming.Start();
            var throttler = new DiscoveryProgressThrottler(progress);
            var bag = new ConcurrentBag<string>();
            IReadOnlyList<string> roots = request.RootPaths ?? new List<string>();
            int total = roots.Count;
            int done = 0;

            int threadCount = 0;
            if (ConfigManager.ScanConfig != null)
                threadCount = ConfigManager.ScanConfig.DiscoveryThreadCount;
            if (threadCount <= 0)
                threadCount = Environment.ProcessorCount * 2;

            var options = new ParallelOptions {
                MaxDegreeOfParallelism = threadCount,
                CancellationToken = ct?.Token ?? CancellationToken.None,
            };

            Parallel.ForEach(roots, options, path => {
                request.OnDirectoryScanning?.Invoke(path);
                IEnumerable<string> paths = DirHelper.GetFileList(path, "*.*", (ex) => {
                    Logger.Error(ex.Message);
                }, (dir) => {
                    request.OnDirectoryScanning?.Invoke(dir);
                }, ct);
                foreach (string item in paths)
                    bag.Add(item);

                int current = Interlocked.Increment(ref done);
                throttler.Report(new DiscoveryProgress {
                    CurrentPath = path,
                    DirectoriesDone = current,
                    DirectoriesTotal = total,
                    Percent = total > 0 ? (float)current / total : 1f,
                });
            });

            List<string> files = bag.ToList();
            SimpleDirDiscovery.AppendExplicitFiles(request, files);
            files = SimpleDirDiscovery.ApplyExtensionFilter(files, request.ExtensionFilter);
            timing.EndEnumerate(files.Count, total);
            throttler.Report(new DiscoveryProgress { Percent = 1f, DirectoriesDone = total, DirectoriesTotal = total }, force: true);

            return new DiscoveryResult {
                FilePaths = files,
                Timing = timing,
            };
        }
    }
}
