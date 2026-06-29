using Jvedio.Core.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SuperUtils.Framework.Tasks;

namespace Jvedio.Test.MetadataTest
{
    [TestClass]
    public class Wave15TaskHubTest
    {
        [TestMethod]
        [TestCategory("QA-001")]
        public void EnqueueUnified_ScanUsesPoolOnly()
        {
            var hub = TaskHub.Instance;
            Assert.IsNotNull(hub);
            Assert.AreEqual(
                ScanManager.Instance.RunningCount + DownloadManager.Instance.RunningCount + ScreenShotManager.Instance.RunningCount,
                hub.TotalRunningCount);
        }

        [TestMethod]
        public void TaskHubBinding_ExposesAggregateCounts()
        {
            Assert.IsNotNull(TaskHubBinding.Instance);
            Assert.AreEqual(TaskHub.Instance.TotalRunningCount, TaskHubBinding.Instance.TotalRunningCount);
        }
    }
}
