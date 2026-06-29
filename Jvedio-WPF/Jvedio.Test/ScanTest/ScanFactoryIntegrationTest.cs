using Jvedio.Core.Enums;
using Jvedio.Core.Scan;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Jvedio.Test.ScanTest
{
    [TestClass]
    public class ScanFactoryIntegrationTest
    {
        [TestMethod]
        public void ProduceScanner_AllDataTypes_ReturnScanJobBase()
        {
            Assert.IsInstanceOfType(ScanFactory.ProduceScanner(DataType.Video, new List<string>(), new List<string>()), typeof(ScanTask));
            Assert.IsInstanceOfType(ScanFactory.ProduceScanner(DataType.Picture, new List<string>(), null), typeof(PictureScan));
        }

        [TestMethod]
        public void ScanResult_InsertedCount_MatchesImport()
        {
            ScanResult result = new ScanResult();
            result.Import.Add("a.mp4");
            result.Import.Add("b.mp4");
            Assert.AreEqual(2, result.InsertedCount);
            Assert.AreEqual(2, result.InsertedPaths.Count);
        }
    }
}
