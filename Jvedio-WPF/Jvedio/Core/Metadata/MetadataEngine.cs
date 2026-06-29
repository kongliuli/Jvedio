using Jvedio.Entity;
using SuperUtils.Framework.Tasks;
using SuperUtils.NetWork.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jvedio.Core.Metadata
{
    public static class MetadataEngine
    {
        private static readonly List<IMetadataProvider> Providers = new List<IMetadataProvider>();

        static MetadataEngine()
        {
            Register(new NfoMetadataProvider());
            Register(new CachedMetadataProvider(new CrawlerMetadataProvider()));
            Register(new HttpMetadataProvider());
        }

        public static void Register(IMetadataProvider provider)
        {
            if (provider == null)
                return;
            Providers.Add(provider);
            Providers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }

        public static async Task<Dictionary<string, object>> RefreshAsync(
            Video video,
            CancellationToken cancellationToken,
            TaskLogger logger,
            RequestHeader header = null,
            System.Action<RequestHeader> headerCallback = null)
        {
            if (video == null)
                return null;

            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
            foreach (IMetadataProvider provider in Providers) {
                if (!provider.CanProvide(video))
                    continue;
                Dictionary<string, object> data = await provider.GetMetadataAsync(
                    video, cancellationToken, logger, header, headerCallback);
                if (data != null && data.Count > 0)
                    results.Add(data);
            }

            if (results.Count == 0)
                return null;
            Dictionary<string, object> merged = MetadataMerger.Merge(results);
            Jvedio.Core.Plugins.PluginHookRegistry.InvokePostMetadata(video, merged);
            return merged;
        }
    }
}
