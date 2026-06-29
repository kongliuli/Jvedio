using Jvedio.Core.Net;
using Jvedio.Entity;
using SuperUtils.Framework.Tasks;
using SuperUtils.NetWork.Entity;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jvedio.Core.Metadata
{
    internal sealed class CrawlerMetadataProvider : IMetadataProvider
    {
        public string Name => "Crawler";
        public int Priority => 100;

        public bool CanProvide(Video video)
        {
            return video != null && !string.IsNullOrEmpty(video.VID);
        }

        public async Task<Dictionary<string, object>> GetMetadataAsync(
            Video video,
            CancellationToken cancellationToken,
            TaskLogger logger,
            RequestHeader header,
            System.Action<RequestHeader> headerCallback)
        {
            VideoDownLoader loader = new VideoDownLoader(video, cancellationToken, logger);
            return await loader.GetInfo(headerCallback);
        }
    }
}
