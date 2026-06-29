using Jvedio.Core.Enums;
using Jvedio.Entity;
using SuperUtils.Framework.Tasks;
using SuperUtils.NetWork.Entity;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jvedio.Core.Metadata
{
    public interface IMetadataProvider
    {
        string Name { get; }
        int Priority { get; }
        bool CanProvide(Video video);
        Task<Dictionary<string, object>> GetMetadataAsync(
            Video video,
            CancellationToken cancellationToken,
            TaskLogger logger,
            RequestHeader header,
            System.Action<RequestHeader> headerCallback);
    }
}
