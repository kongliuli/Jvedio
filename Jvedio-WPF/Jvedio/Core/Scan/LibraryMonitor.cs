using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Jvedio.Core.Scan
{
    public sealed class LibraryMonitor : IDisposable
    {
        private const int DebounceMs = 45000;

        private readonly ConcurrentDictionary<string, Timer> _debounceTimers =
            new ConcurrentDictionary<string, Timer>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> _pendingPaths =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly object _pendingLock = new object();
        private readonly List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();

        public event Action<IReadOnlyList<string>> PathsChanged;

        public void Watch(IEnumerable<string> rootPaths)
        {
            Stop();
            if (rootPaths == null)
                return;

            foreach (string path in rootPaths.Distinct(StringComparer.OrdinalIgnoreCase)) {
                if (!Directory.Exists(path))
                    continue;
                FileSystemWatcher watcher = new FileSystemWatcher(path) {
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName |
                                   NotifyFilters.LastWrite | NotifyFilters.Size,
                };
                watcher.Created += (_, e) => SchedulePath(e.FullPath);
                watcher.Deleted += (_, e) => SchedulePath(e.FullPath);
                watcher.Renamed += (_, e) => SchedulePath(e.FullPath);
                watcher.Changed += (_, e) => SchedulePath(e.FullPath);
                watcher.EnableRaisingEvents = true;
                _watchers.Add(watcher);
            }
        }

        private void SchedulePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            lock (_pendingLock)
                _pendingPaths.Add(path);

            _debounceTimers.AddOrUpdate(path,
                key => new Timer(_ => FlushPending(), null, DebounceMs, Timeout.Infinite),
                (_, timer) => {
                    timer.Change(DebounceMs, Timeout.Infinite);
                    return timer;
                });
        }

        private void FlushPending()
        {
            string[] batch;
            lock (_pendingLock) {
                batch = _pendingPaths.ToArray();
                _pendingPaths.Clear();
            }
            if (batch.Length == 0)
                return;
            PathsChanged?.Invoke(batch);
        }

        public void Stop()
        {
            foreach (FileSystemWatcher watcher in _watchers) {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            _watchers.Clear();

            foreach (Timer timer in _debounceTimers.Values)
                timer.Dispose();
            _debounceTimers.Clear();

            lock (_pendingLock)
                _pendingPaths.Clear();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
