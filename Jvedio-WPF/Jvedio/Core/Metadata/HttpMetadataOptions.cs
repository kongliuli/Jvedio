using Jvedio.Core.Config;
using System.Collections.Generic;

namespace Jvedio.Core.Metadata
{
    /// <summary>HTTP 元数据运行时选项（由 App 启动时从 ConfigManager 注入）。</summary>
    public static class HttpMetadataOptions
    {
        public static bool EnableSiteRules { get; set; } = true;

        public static IList<HttpMetadataSiteRule> SiteRules { get; set; } = new List<HttpMetadataSiteRule>();

        public static void SyncFromConfig(HttpMetadataConfig config)
        {
            if (config == null) {
                EnableSiteRules = true;
                SiteRules = new List<HttpMetadataSiteRule>();
                return;
            }
            EnableSiteRules = config.EnableSiteRules;
            SiteRules = config.SiteRules ?? new List<HttpMetadataSiteRule>();
        }
    }
}
