using System;
using System.Threading;
using Jvedio.Core.Config;

namespace Jvedio.Core.Scan.Discovery
{
    public static class DiscoveryBackendFactory
    {
        public static IFileDiscoveryBackend Create()
        {
            ScanConfig config = ConfigManager.ScanConfig;
            IFileDiscoveryBackend inner = config != null && config.UseParallelDiscovery
                ? (IFileDiscoveryBackend)new ParallelDirDiscovery()
                : new SimpleDirDiscovery();

            if (config != null && config.EnableDirIndexCache)
                return new CachedIncrementalDiscovery(inner);

            return inner;
        }

        public static DiscoveryResult Discover(DiscoveryRequest request, CancellationTokenSource ct, IProgress<DiscoveryProgress> progress = null)
        {
            return Create().Discover(request, progress, ct);
        }
    }
}
