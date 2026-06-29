using Jvedio.Core.Config.Base;

namespace Jvedio.Core.Config
{
    public class TaskParallelismConfig : AbstractConfig
    {
        private static TaskParallelismConfig _instance;

        private TaskParallelismConfig() : base("TaskParallelism")
        {
            GenerateTaskCount = 5;
            DownloadTaskCount = 2;
        }

        public static TaskParallelismConfig CreateInstance()
        {
            if (_instance == null)
                _instance = new TaskParallelismConfig();
            return _instance;
        }

        /// <summary>截图/GIF 等 Generate 任务并行度。</summary>
        public int GenerateTaskCount { get; set; }

        /// <summary>刮削/下载任务并行度。</summary>
        public int DownloadTaskCount { get; set; }

        public static int ResolveGenerateCount()
        {
            int n = Jvedio.ConfigManager.TaskParallelism?.GenerateTaskCount ?? 5;
            return n < 1 ? 1 : n;
        }

        public static int ResolveDownloadCount()
        {
            int n = Jvedio.ConfigManager.TaskParallelism?.DownloadTaskCount ?? 2;
            return n < 1 ? 1 : n;
        }
    }
}
