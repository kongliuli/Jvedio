using System.Collections.Generic;

namespace Jvedio.Core.Metadata
{
    /// <summary>按站点 Host 定制的 HTTP 元数据字段映射。</summary>
    public sealed class HttpMetadataSiteRule
    {
        public string HostContains { get; set; }

        /// <summary>og:property 名 → 元数据字段（如 og:title → Title）。</summary>
        public Dictionary<string, string> OgPropertyMap { get; set; } = new Dictionary<string, string>();

        /// <summary>meta name → 元数据字段。</summary>
        public Dictionary<string, string> MetaNameMap { get; set; } = new Dictionary<string, string>();
    }
}
