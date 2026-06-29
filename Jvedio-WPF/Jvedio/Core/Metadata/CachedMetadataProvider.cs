using Jvedio.Core.Exceptions;
using Jvedio.Entity;
using SuperUtils.Framework.Tasks;
using SuperUtils.NetWork.Entity;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jvedio.Core.Metadata
{
    internal sealed class CachedMetadataProvider : IMetadataProvider
    {
        private readonly IMetadataProvider _inner;

        public CachedMetadataProvider(IMetadataProvider inner)
        {
            _inner = inner;
        }

        public string Name => _inner.Name;

        public int Priority => _inner.Priority;

        public bool CanProvide(Video video) => _inner.CanProvide(video);

        public async Task<Dictionary<string, object>> GetMetadataAsync(
            Video video,
            CancellationToken cancellationToken,
            TaskLogger logger,
            RequestHeader header,
            System.Action<RequestHeader> headerCallback)
        {
            if (!MetadataLookupCache.ShouldBypassCache()) {
                if (MetadataLookupCache.TryGetHit(Name, video, out Dictionary<string, object> cached)) {
                    logger?.Info($"metadata cache hit [{Name}] {video?.VID}");
                    return cached;
                }
                if (MetadataLookupCache.IsCachedNotFound(Name, video)) {
                    logger?.Info($"metadata cache miss(404) [{Name}] {video?.VID}");
                    return null;
                }
            }

            try {
                Dictionary<string, object> data = await _inner.GetMetadataAsync(
                    video, cancellationToken, logger, header, headerCallback);
                if (MetadataLookupCache.IsSuccessResult(data)) {
                    MetadataLookupCache.SetHit(Name, video, data);
                    return data;
                }

                MetadataLookupCache.SetNotFound(Name, video);
                return data;
            } catch (CrawlerNotFoundException) {
                MetadataLookupCache.SetNotFound(Name, video);
                throw;
            }
        }
    }
}
