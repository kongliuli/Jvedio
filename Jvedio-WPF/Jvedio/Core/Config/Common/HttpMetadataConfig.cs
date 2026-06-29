using Jvedio.Core.Config.Base;
using Jvedio.Core.Metadata;
using System.Collections.Generic;

namespace Jvedio.Core.Config
{
    public class HttpMetadataConfig : AbstractConfig
    {
        private static HttpMetadataConfig _instance;

        private HttpMetadataConfig() : base("HttpMetadata")
        {
            EnableSiteRules = true;
            SiteRules = new List<HttpMetadataSiteRule>();
        }

        public static HttpMetadataConfig CreateInstance()
        {
            if (_instance == null)
                _instance = new HttpMetadataConfig();
            return _instance;
        }

        public bool EnableSiteRules { get; set; }

        public List<HttpMetadataSiteRule> SiteRules { get; set; }
    }
}
