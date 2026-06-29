using Jvedio.Core.Scan.Discovery;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Jvedio.Test.ScanTest
{
    [TestClass]
    public class DiscoveryBackendTest
    {
        [TestMethod]
        public void SimpleDirDiscovery_EnumeratesFiles()
        {
            string root = CreateTempDir();
            try {
                CreateFile(root, "a.mp4");
                CreateFile(Path.Combine(root, "sub"), "b.mp4", createDir: true);

                var backend = new SimpleDirDiscovery();
                DiscoveryResult result = backend.Discover(new DiscoveryRequest {
                    RootPaths = new List<string> { root },
                }, null, null);

                Assert.AreEqual(2, result.FilePaths.Count);
                Assert.IsNotNull(result.Timing);
                Assert.IsTrue(result.Timing.EnumerateMs >= 0);
                Assert.AreEqual(2, result.Timing.FileCount);
            } finally {
                TryDeleteDir(root);
            }
        }

        [TestMethod]
        public void DiscoveryTiming_RecordsPhases()
        {
            DiscoveryTiming timing = DiscoveryTiming.Start();
            timing.EndEnumerate(10, 2);
            timing.EndFilter();

            Assert.AreEqual(10, timing.FileCount);
            Assert.AreEqual(2, timing.DirectoryCount);
            Assert.IsTrue(timing.TotalMs >= timing.EnumerateMs);
        }

        [TestMethod]
        public void ParallelDirDiscovery_MatchesSimpleResults()
        {
            string root = CreateTempDir();
            try {
                for (int i = 0; i < 5; i++)
                    CreateFile(root, $"file{i}.mp4");

                var request = new DiscoveryRequest { RootPaths = new List<string> { root } };
                List<string> simple = new SimpleDirDiscovery().Discover(request, null, null).FilePaths;
                List<string> parallel = new ParallelDirDiscovery().Discover(request, null, null).FilePaths;

                CollectionAssert.AreEquivalent(simple, parallel);
            } finally {
                TryDeleteDir(root);
            }
        }

        [TestMethod]
        public void CachedIncrementalDiscovery_SecondScanUsesCacheWhenUnchanged()
        {
            string root = CreateTempDir();
            var store = new MemoryScanDirIndexStore();
            try {
                CreateFile(root, "a.mp4");
                var cached = new CachedIncrementalDiscovery(new SimpleDirDiscovery(), store);
                var request = new DiscoveryRequest {
                    RootPaths = new List<string> { root },
                    Mode = DiscoveryMode.Full,
                    VolumeKey = "1",
                };

                DiscoveryResult first = cached.Discover(request, null, null);
                Assert.AreEqual(1, first.FilePaths.Count);
                Assert.IsFalse(first.FromCache);

                request.Mode = DiscoveryMode.Incremental;
                DiscoveryResult second = cached.Discover(request, null, null);
                Assert.IsTrue(second.FromCache);
                Assert.AreEqual(0, second.FilePaths.Count);
            } finally {
                TryDeleteDir(root);
            }
        }

        [TestMethod]
        public void CachedIncrementalDiscovery_RescansChangedSubdir()
        {
            string root = CreateTempDir();
            string sub = Path.Combine(root, "changed");
            Directory.CreateDirectory(sub);
            var store = new MemoryScanDirIndexStore();
            try {
                CreateFile(root, "root.mp4");
                var cached = new CachedIncrementalDiscovery(new SimpleDirDiscovery(), store);
                var request = new DiscoveryRequest {
                    RootPaths = new List<string> { root },
                    Mode = DiscoveryMode.Full,
                    VolumeKey = "2",
                };
                cached.Discover(request, null, null);

                CreateFile(sub, "new.mp4");
                request.Mode = DiscoveryMode.Incremental;
                DiscoveryResult second = cached.Discover(request, null, null);

                Assert.IsFalse(second.FromCache);
                Assert.IsTrue(second.FilePaths.Any(p => p.EndsWith("new.mp4", StringComparison.OrdinalIgnoreCase)));
            } finally {
                TryDeleteDir(root);
            }
        }

        private sealed class MemoryScanDirIndexStore : IScanDirIndexStore
        {
            private readonly Dictionary<string, DirIndexEntry> _entries = new Dictionary<string, DirIndexEntry>(StringComparer.OrdinalIgnoreCase);

            public DirIndexEntry Get(int dbId, string dirPath)
            {
                _entries.TryGetValue(Key(dbId, dirPath), out DirIndexEntry entry);
                return entry;
            }

            public void Upsert(DirIndexEntry entry)
            {
                _entries[Key(entry.DbId, entry.DirPath)] = entry;
            }

            public void Remove(int dbId, string dirPath)
            {
                _entries.Remove(Key(dbId, dirPath));
            }

            private static string Key(int dbId, string dirPath) => dbId + "|" + dirPath;
        }

        private static string CreateTempDir()
        {
            string dir = Path.Combine(Path.GetTempPath(), "jvedio_discover_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static string CreateFile(string dir, string name, bool createDir = false)
        {
            if (createDir)
                Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, name);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, name);
            return path;
        }

        private static void TryDeleteDir(string dir)
        {
            try {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            } catch {
            }
        }
    }
}
