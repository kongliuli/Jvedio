using Jvedio.Core.Scan.Discovery;
using System.Collections.Generic;

namespace Jvedio.Core.Scan
{
    public enum ScanMode
    {
        /// <summary>仅枚举文件，不入库。</summary>
        Discover,
        /// <summary>完整扫描 Pipeline（默认）。</summary>
        FullImport,
        /// <summary>仅刷新元数据，不入库扫描。请用 <see cref="ScanEngine.RefreshMetadata"/>。</summary>
        RefreshMetadata,
    }

    public class ScanOptions
    {
        public ScanMode Mode { get; set; } = ScanMode.FullImport;
        public List<string> FilePaths { get; set; }
        public List<string> FileExt { get; set; }
        public DiscoveryMode DiscoveryMode { get; set; } = DiscoveryMode.Full;
    }
}
