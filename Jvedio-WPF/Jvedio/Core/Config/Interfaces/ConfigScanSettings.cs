namespace Jvedio.Core.Config.Interfaces
{
    public sealed class ConfigScanSettings : IScanSettings
    {
        public static ConfigScanSettings Instance { get; } = new ConfigScanSettings();

        public bool LoadDataAfterScan => ConfigManager.ScanConfig.LoadDataAfterScan;
        public bool ScrapeAfterScan => ConfigManager.ScanConfig.ScrapeAfterScan;
        public bool UseParallelDiscovery => ConfigManager.ScanConfig.UseParallelDiscovery;
        public bool EnableDirIndexCache => ConfigManager.ScanConfig.EnableDirIndexCache;
        public bool DownloadInfo => ConfigManager.DownloadConfig.DownloadInfo;
        public bool ScanNfo => ConfigManager.ScanConfig.ScanNfo;
    }
}
