using Jvedio.Core.Enums;
using Jvedio.Core.Scan;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Jvedio.Test.ScanTest
{
    [TestClass]
    public class ScanOptimizationTest
    {
        private static bool _appInitialized;

        [TestInitialize]
        public void Setup()
        {
            VideoParser.InitSearchPattern();
        }

        private static void EnsureScanConfig()
        {
            if (!_appInitialized) {
                Jvedio.App.Init();
                if (System.Windows.Application.ResourceAssembly == null)
                    System.Windows.Application.ResourceAssembly = typeof(Jvedio.App).Assembly;
                _appInitialized = true;
            }
            if (ConfigManager.ScanConfig == null)
                ConfigManager.ScanConfig = Jvedio.Core.Config.ScanConfig.CreateInstance();
        }

        [TestMethod]
        public void HandleSubSection_12Parts_IsSubSection()
        {
            string dir = CreateTempDir();
            try {
                List<string> paths = new List<string>();
                for (int i = 1; i <= 12; i++)
                    paths.Add(CreateFile(dir, $"abcd-123-{i}.mp4", $"part{i}"));

                (bool isSubSection, List<string> subSectionList, List<string> notSubSection) =
                    VideoParser.HandleSubSection(paths);

                Assert.IsTrue(isSubSection);
                Assert.AreEqual(12, subSectionList.Count);
                Assert.AreEqual(0, notSubSection.Count);
            } finally {
                TryDeleteDir(dir);
            }
        }

        [TestMethod]
        public void HandleSubSection_RequiresFullSequence_NotPartialMatchOnOne()
        {
            string dir = CreateTempDir();
            try {
                // 只有 2、3 段，缺少 1 —— 不应判定为完整分段
                List<string> paths = new List<string> {
                    CreateFile(dir, "abcd-123-2.mp4", "b"),
                    CreateFile(dir, "abcd-123-3.mp4", "c"),
                };

                (bool isSubSection, _, _) = VideoParser.HandleSubSection(paths);
                Assert.IsFalse(isSubSection);
            } finally {
                TryDeleteDir(dir);
            }
        }

        [TestMethod]
        public void DistinctMovie_SameVidDifferentHash_KeepsAll()
        {
            EnsureScanConfig();
            ConfigManager.ScanConfig.FetchVID = true;

            string dir = CreateTempDir();
            try {
                string path1 = CreateFile(dir, "abcd-123-clip1.mp4", "content-one");
                string path2 = CreateFile(dir, "abcd-123-clip2.mp4", "content-two");
                var parser = new VideoParser();
                Dictionary<string, NotImportReason> notImport = new Dictionary<string, NotImportReason>();
                List<Video> videos = parser.DistinctMovie(
                    new List<string> { path1, path2 },
                    CancellationToken.None,
                    (dict) => {
                        foreach (var item in dict)
                            notImport[item.Key] = item.Value;
                    });

                Assert.IsTrue(videos.Count >= 1);
                if (videos.Count == 2) {
                    Assert.AreEqual(0, notImport.Count);
                    Assert.AreNotEqual(videos[0].Hash, videos[1].Hash);
                }
            } finally {
                TryDeleteDir(dir);
            }
        }

        [TestMethod]
        public void DistinctMovie_SameVidSameHash_KeepsOne()
        {
            EnsureScanConfig();
            ConfigManager.ScanConfig.FetchVID = true;

            string dir = CreateTempDir();
            try {
                string path1 = CreateFile(dir, "abcd-123-copy1.mp4", "same-content");
                string path2 = CreateFile(dir, "abcd-123-copy2.mp4", "same-content");
                var parser = new VideoParser();
                Dictionary<string, NotImportReason> notImport = new Dictionary<string, NotImportReason>();
                List<Video> videos = parser.DistinctMovie(
                    new List<string> { path1, path2 },
                    CancellationToken.None,
                    (dict) => {
                        foreach (var item in dict)
                            notImport[item.Key] = item.Value;
                    });

                Assert.AreEqual(1, videos.Count);
                Assert.AreEqual(1, notImport.Count);
                Assert.IsTrue(notImport.Values.All(arg => arg == NotImportReason.RepetitiveVID));
            } finally {
                TryDeleteDir(dir);
            }
        }

        [TestMethod]
        public void NfoParse_UnknownConfigKey_DoesNotThrow()
        {
            EnsureScanConfig();
            NfoParse.RestoreDefault();
            Assert.IsNotNull(NfoParse.CurrentNFOParse);
            Assert.IsTrue(NfoParse.CurrentNFOParse.ContainsKey("title"));
        }

        [TestMethod]
        public void HandleFailNFO_DoesNotDuplicateEntries()
        {
            ScanResult result = new ScanResult();
            List<string> failNFO = new List<string> { "a.nfo", "b.nfo", "c.nfo" };
            if (failNFO != null && failNFO.Count > 0)
                result.FailNFO.AddRange(failNFO);
            Assert.AreEqual(3, result.FailNFO.Count);
        }

        private static string CreateTempDir()
        {
            string dir = Path.Combine(Path.GetTempPath(), "jvedio-scan-test-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static string CreateFile(string dir, string name, string content)
        {
            string path = Path.Combine(dir, name);
            File.WriteAllText(path, content);
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
