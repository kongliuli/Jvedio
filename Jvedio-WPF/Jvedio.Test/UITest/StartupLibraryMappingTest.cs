using Jvedio.Core.Enums;
using Jvedio.Core.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jvedio.Test.UITest
{
    [TestClass]
    public class StartupLibraryMappingTest
    {
        [TestMethod]
        public void ResolveDataType_UsesStoredDataTypeWhenValid()
        {
            Assert.AreEqual(DataType.Game, StartupLibraryMapping.ResolveDataType(0, (int)DataType.Game));
        }

        [TestMethod]
        public void ResolveDataType_FallsBackToSideIndex()
        {
            Assert.AreEqual(DataType.Picture, StartupLibraryMapping.ResolveDataType(1, -1));
        }
    }
}
