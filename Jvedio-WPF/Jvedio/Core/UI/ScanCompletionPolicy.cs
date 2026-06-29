using Jvedio.Core.Enums;
using System;

namespace Jvedio.Core.UI
{
    [Flags]
    public enum ScanCompletionPolicy
    {
        None = 0,
        RefreshStatistic = 1,
        ReloadTabs = 2,
        ScreenShotAfterImport = 4,
        ScrapeAfterImport = 8,
    }

    public static class ScanCompletionPolicyExtensions
    {
        public static ScanCompletionPolicy ForDataType(DataType dataType)
        {
            ScanCompletionPolicy policy = ScanCompletionPolicy.RefreshStatistic | ScanCompletionPolicy.ReloadTabs;
            if (dataType == DataType.Video)
                policy |= ScanCompletionPolicy.ScreenShotAfterImport | ScanCompletionPolicy.ScrapeAfterImport;
            return policy;
        }

        public static bool HasPolicy(this ScanCompletionPolicy mask, ScanCompletionPolicy policy)
        {
            return (mask & policy) == policy;
        }
    }
}
