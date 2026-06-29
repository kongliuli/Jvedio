namespace Jvedio.Core.Scan
{
    public static class ScanPipelineHelper
    {
        public static bool TryFinishDiscoverOnly(ScanJobBase job, ScanContext context)
        {
            if (job == null || context == null || context.Mode != ScanMode.Discover)
                return false;

            job.ScanResult.TotalCount = job.FilePaths?.Count ?? 0;
            job.Progress = 100;
            job.Success = true;
            job.FinishSuccess();
            return true;
        }
    }
}
