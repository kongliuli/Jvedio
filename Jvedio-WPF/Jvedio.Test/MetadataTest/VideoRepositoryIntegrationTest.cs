using Jvedio.Core.Media;
using Jvedio.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jvedio.Test.MetadataTest
{
    [TestClass]
    public class VideoRepositoryIntegrationTest
    {
        [TestMethod]
        public void MapperVideoRepository_DelegatesToVideoStatic()
        {
            IVideoRepository repo = MapperVideoRepository.Instance;
            Assert.IsNotNull(repo);
            Video missing = repo.GetById(-1);
            Assert.IsTrue(missing == null || missing.DataID <= 0);
        }
    }
}
