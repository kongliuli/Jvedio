namespace Jvedio.Core.Config.Interfaces
{
    public interface IScanSettings
    {
        bool LoadDataAfterScan { get; }
        bool ScrapeAfterScan { get; }
        bool UseParallelDiscovery { get; }
        bool EnableDirIndexCache { get; }
        bool DownloadInfo { get; }
        bool ScanNfo { get; }
    }
}
