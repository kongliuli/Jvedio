using Jvedio.Core.Config;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jvedio.Core.Tasks
{
    /// <summary>跨 Scan/Download/ScreenShot 的全局并发槽（TASK-001）。</summary>
    public static class UnifiedTaskWorkerPool
    {
        private static readonly object Gate = new object();
        private static SemaphoreSlim _slots = CreateSlots();

        static UnifiedTaskWorkerPool()
        {
            RefreshLimits();
        }

        public static int MaxWorkers { get; private set; } = 1;

        public static void RefreshLimits()
        {
            int max = TaskParallelismConfig.ResolveGenerateCount()
                + TaskParallelismConfig.ResolveDownloadCount()
                + 1;
            if (max < 1)
                max = 1;
            lock (Gate) {
                MaxWorkers = max;
                _slots = new SemaphoreSlim(max, max);
            }
        }

        public static void RunThrottled(Action work)
        {
            if (work == null)
                return;
            Task.Run(async () => {
                await _slots.WaitAsync().ConfigureAwait(false);
                try {
                    work();
                } finally {
                    _slots.Release();
                }
            });
        }

        private static SemaphoreSlim CreateSlots()
        {
            return new SemaphoreSlim(1, 1);
        }
    }
}
