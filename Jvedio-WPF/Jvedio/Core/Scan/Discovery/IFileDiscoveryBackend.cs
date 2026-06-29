using System;
using System.Threading;

namespace Jvedio.Core.Scan.Discovery
{
    public interface IFileDiscoveryBackend
    {
        DiscoveryResult Discover(DiscoveryRequest request, IProgress<DiscoveryProgress> progress, CancellationTokenSource ct);
    }
}
