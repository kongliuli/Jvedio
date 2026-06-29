using Jvedio.Core.Enums;
using Jvedio.Core.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jvedio.Test.UITest
{
    [TestClass]
    public class StartupLibraryMappingTest
    {
        [TestMethod]
        public void ResolveDataType_IgnoresLegacyStoredTypes()
        {
            Assert.AreEqual(DataType.Video, StartupLibraryMapping.ResolveDataType(0, StartupLibraryMapping.LegacyGameDataType));
            Assert.AreEqual(DataType.Picture, StartupLibraryMapping.ResolveDataType(1, StartupLibraryMapping.LegacyComicsDataType));
        }

        [TestMethod]
        public void ResolveDataType_FallsBackToSideIndex()
        {
            Assert.AreEqual(DataType.Picture, StartupLibraryMapping.ResolveDataType(1, -1));
        }
    }
}
