using Jvedio.Core.Scan;
using SuperUtils.Framework.Tasks;
using System.Linq;
using System.Threading.Tasks;

namespace Jvedio.Core.Tasks
{
    public static class TaskDisplayHelper
    {
        public static string GetSubTaskProgressText(AbstractTask task)
        {
            if (task is ScanJobBase scan && scan.ScanResult != null && scan.ScanResult.TotalCount > 0) {
                long done = (scan.ScanResult.Import?.Count ?? 0)
                    + (scan.ScanResult.NotImport?.Count ?? 0)
                    + (scan.ScanResult.FailNFO?.Count ?? 0);
                return $"{done}/{scan.ScanResult.TotalCount}";
            }

            if (task is IBackgroundTask composite
                && composite.Children != null
                && composite.Children.Count > 0) {
                int done = composite.Children.Count(c => c.Status == TaskStatus.RanToCompletion);
                return $"{done}/{composite.Children.Count}";
            }

            return null;
        }
    }
}
