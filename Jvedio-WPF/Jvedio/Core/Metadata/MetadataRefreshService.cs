using Jvedio.Entity;
using System.Collections.Generic;

namespace Jvedio.Core.Metadata
{
    /// <summary>
    /// 仅刷新元数据（刮削/NFO），不触发扫描 Pipeline。
    /// 对应 <see cref="Jvedio.Core.Scan.ScanMode.RefreshMetadata"/>。
    /// </summary>
    public static class MetadataRefreshService
    {
        public static int Refresh(IEnumerable<Video> videos)
        {
            return MetadataScrapeService.Enqueue(videos, scrapeOnly: true);
        }

        public static int Refresh(Video video)
        {
            return Refresh(new[] { video });
        }
    }
}
