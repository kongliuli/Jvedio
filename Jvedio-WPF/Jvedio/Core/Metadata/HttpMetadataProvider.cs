using Jvedio.Core.Crawler;
using Jvedio.Entity;
using SuperUtils.Framework.Tasks;
using SuperUtils.NetWork.Entity;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jvedio.Core.Metadata
{
    /// <summary>HTTP 元数据 Provider（PLG-003）。</summary>
    public sealed class HttpMetadataProvider : IMetadataProvider
    {
        public string Name => "HttpMetadata";
        public int Priority => 50;

        public bool CanProvide(Video video)
        {
            return video != null && !string.IsNullOrEmpty(video.WebUrl);
        }

        public async Task<Dictionary<string, object>> GetMetadataAsync(
            Video video,
            CancellationToken cancellationToken,
            TaskLogger logger,
            RequestHeader header,
            System.Action<RequestHeader> headerCallback)
        {
            if (!CanProvide(video))
                return null;

            RequestHeader useHeader = header ?? CrawlerHeader.Default;
            headerCallback?.Invoke(useHeader);
            logger?.Info($"{Name}: fetch {video.WebUrl}");
            return await WebMetadataFetcher.FetchAsync(video.WebUrl, useHeader, logger, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
