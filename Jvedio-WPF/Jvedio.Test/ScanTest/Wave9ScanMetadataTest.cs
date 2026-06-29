using Jvedio.Core.Config;
using Jvedio.Core.Library;
using Jvedio.Core.Metadata;
using Jvedio.Core.Scan;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace Jvedio.Test.ScanTest
{
    [TestClass]
    public class Wave9TestAssemblyInit
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Jvedio.App.Init();
            if (System.Windows.Application.ResourceAssembly == null)
                System.Windows.Application.ResourceAssembly = typeof(Jvedio.App).Assembly;
            if (ConfigManager.ScanConfig == null)
                ConfigManager.ScanConfig = ScanConfig.CreateInstance();
        }
    }

    [TestClass]
    public class ScanEngineRefreshMetadataTest
    {
        [TestMethod]
        public void CreateJob_RefreshMetadataMode_ReturnsNull()
        {
            var library = new LibraryContext {
                DataType = Core.Enums.DataType.Video,
                RootPaths = new List<string> { @"C:\fake" },
            };
            ScanJobBase job = ScanEngine.CreateJob(library, new ScanOptions { Mode = ScanMode.RefreshMetadata });
            Assert.IsNull(job);
        }

        [TestMethod]
        public void CreateJob_FullImport_ReturnsScanner()
        {
            var library = new LibraryContext {
                DataType = Core.Enums.DataType.Video,
                RootPaths = new List<string>(),
            };
            ScanJobBase job = ScanEngine.CreateJob(library, new ScanOptions { Mode = ScanMode.FullImport });
            Assert.IsNotNull(job);
        }

        [TestMethod]
        public void RefreshMetadata_EmptyList_ReturnsZero()
        {
            Assert.AreEqual(0, ScanEngine.RefreshMetadata(null));
            Assert.AreEqual(0, ScanEngine.RefreshMetadata(new List<Video>()));
        }
    }

    [TestClass]
    public class NfoMetadataReaderTest
    {
        [TestInitialize]
        public void InitNfoParse()
        {
            NfoParse.RestoreDefault();
        }

        [TestMethod]
        public void TryReadMovie_ValidNfo_ReturnsMovie()
        {
            string dir = Path.Combine(Path.GetTempPath(), "jvedio-nfo-test-" + Path.GetRandomFileName());
            Directory.CreateDirectory(dir);
            string nfoPath = Path.Combine(dir, "ABC-123.nfo");
            try {
                File.WriteAllText(nfoPath,
                    "<?xml version=\"1.0\"?><movie><id>ABC-123</id><title>Test Title</title></movie>");
                Movie movie = NfoMetadataReader.TryReadMovie(nfoPath);
                Assert.IsNotNull(movie);
                Assert.AreEqual("ABC-123", movie.id);
                Assert.AreEqual("Test Title", movie.title);
            } finally {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
        }

        [TestMethod]
        public void TryReadMovie_NoId_ReturnsNull()
        {
            string dir = Path.Combine(Path.GetTempPath(), "jvedio-nfo-test-" + Path.GetRandomFileName());
            Directory.CreateDirectory(dir);
            string nfoPath = Path.Combine(dir, "empty.nfo");
            try {
                File.WriteAllText(nfoPath, "<?xml version=\"1.0\"?><movie><title>No Id</title></movie>");
                Assert.IsNull(NfoMetadataReader.TryReadMovie(nfoPath));
            } finally {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
        }

        [TestMethod]
        public void Movie_GetInfoFromNfo_DelegatesToReader()
        {
            string dir = Path.Combine(Path.GetTempPath(), "jvedio-nfo-test-" + Path.GetRandomFileName());
            Directory.CreateDirectory(dir);
            string nfoPath = Path.Combine(dir, "XYZ-999.nfo");
            try {
                File.WriteAllText(nfoPath,
                    "<?xml version=\"1.0\"?><movie><id>XYZ-999</id><title>Delegate</title></movie>");
                Movie fromMovie = Movie.GetInfoFromNfo(nfoPath);
                Movie fromReader = NfoMetadataReader.TryReadMovie(nfoPath);
                Assert.AreEqual(fromReader.id, fromMovie.id);
                Assert.AreEqual(fromReader.title, fromMovie.title);
            } finally {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
        }
    }
}
