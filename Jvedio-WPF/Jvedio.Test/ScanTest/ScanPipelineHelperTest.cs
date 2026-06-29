using Jvedio.Core.Enums;
using Jvedio.Core.Scan;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Jvedio.Test.ScanTest
{
    [TestClass]
    public class ScanPipelineHelperTest
    {
        [TestMethod]
        public void TryFinishDiscoverOnly_WhenDiscoverMode_FinishesJob()
        {
            ScanJobBase job = ScanFactory.ProduceScanner(
                DataType.Video,
                new List<string>(),
                new List<string>(),
                scanMode: ScanMode.Discover);
            job.FilePaths.Clear();
            job.FilePaths.Add(@"C:\fake\a.mp4");
            var context = new ScanContext { Mode = ScanMode.Discover };

            Assert.IsTrue(ScanPipelineHelper.TryFinishDiscoverOnly(job, context));
            Assert.IsTrue(job.Success);
            Assert.AreEqual(1, job.ScanResult.TotalCount);
        }

        [TestMethod]
        public void TryFinishDiscoverOnly_WhenFullImport_ReturnsFalse()
        {
            ScanJobBase job = ScanFactory.ProduceScanner(DataType.Video, new List<string>(), new List<string>());
            var context = new ScanContext { Mode = ScanMode.FullImport };
            Assert.IsFalse(ScanPipelineHelper.TryFinishDiscoverOnly(job, context));
        }
    }
}
