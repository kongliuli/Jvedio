using Jvedio.Core.Scan;
using SuperUtils.Framework.Tasks;

namespace Jvedio.Core.Tasks
{
    public enum TaskKind
    {
        Scan,
        Scrape,
        Download,
        Generate,
        Rename,
        Migrate,
    }

    public sealed class TaskHub
    {
        public static TaskHub Instance { get; } = new TaskHub();

        public ScanManager Scan => ScanManager.Instance;
        public DownloadManager Download => DownloadManager.Instance;
        public ScreenShotManager ScreenShot => ScreenShotManager.Instance;

        public int TotalRunningCount =>
            Scan.RunningCount + Download.RunningCount + ScreenShot.RunningCount;

        public bool HasRunningTasks => TotalRunningCount > 0;

        public void CancelAllIfRunning()
        {
            if (!HasRunningTasks)
                return;
            CancelAll();
        }

        public void AddTask(AbstractTask task, TaskKind kind)
        {
            EnqueueUnified(task, kind);
        }

        /// <summary>统一入队；Scan 走全局槽，Download/Generate 走各自 Dispatcher（Wave 15 去双重限流）。</summary>
        public void EnqueueUnified(AbstractTask task, TaskKind kind)
        {
            if (task == null)
                return;

            BaseManager manager = ResolveManager(kind);
            manager.RegisterTask(task);
            if (kind == TaskKind.Scan) {
                UnifiedTaskWorkerPool.RunThrottled(() => ExecuteTask(task, kind, manager));
            } else {
                ExecuteTask(task, kind, manager);
            }
        }

        private static BaseManager ResolveManager(TaskKind kind)
        {
            switch (kind) {
                case TaskKind.Scan:
                    return ScanManager.Instance;
                case TaskKind.Scrape:
                case TaskKind.Download:
                    return DownloadManager.Instance;
                case TaskKind.Generate:
                    return ScreenShotManager.Instance;
                default:
                    return DownloadManager.Instance;
            }
        }

        private static void ExecuteTask(AbstractTask task, TaskKind kind, BaseManager manager)
        {
            switch (kind) {
                case TaskKind.Scan:
                    (task as ScanJobBase)?.Start();
                    break;
                default:
                    manager.EnqueueToDispatcher(task);
                    break;
            }
        }

        public void CancelAll()
        {
            Scan.CancelAll();
            Download.CancelAll();
            ScreenShot.CancelAll();
        }
    }
}
