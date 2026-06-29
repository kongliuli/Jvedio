using System;

namespace Jvedio.Core.Scan.Discovery
{
    internal sealed class DiscoveryProgressThrottler
    {
        private readonly IProgress<DiscoveryProgress> _inner;
        private DateTime _lastReportUtc = DateTime.MinValue;
        private const int MinIntervalMs = 50;

        public DiscoveryProgressThrottler(IProgress<DiscoveryProgress> inner)
        {
            _inner = inner;
        }

        public void Report(DiscoveryProgress progress, bool force = false)
        {
            if (_inner == null)
                return;
            DateTime now = DateTime.UtcNow;
            if (!force && (now - _lastReportUtc).TotalMilliseconds < MinIntervalMs)
                return;
            _lastReportUtc = now;
            _inner.Report(progress);
        }
    }
}
