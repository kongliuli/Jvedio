using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Jvedio.Core.Tasks
{
    /// <summary>TaskHub 聚合状态，供 XAML 绑定（Wave 15）。</summary>
    public sealed class TaskHubBinding : INotifyPropertyChanged
    {
        public static TaskHubBinding Instance { get; } = new TaskHubBinding();

        static TaskHubBinding()
        {
            void Refresh(object s, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(BaseManager.RunningCount)
                    || e.PropertyName == nameof(BaseManager.Running))
                    Instance.Notify();
            }

            ScanManager.Instance.PropertyChanged += Refresh;
            DownloadManager.Instance.PropertyChanged += Refresh;
            ScreenShotManager.Instance.PropertyChanged += Refresh;
        }

        public int TotalRunningCount => TaskHub.Instance.TotalRunningCount;

        public bool HasRunningTasks => TaskHub.Instance.HasRunningTasks;

        public bool? ScanRunning => ScanManager.Instance.Running;

        public int DownloadRunningCount => DownloadManager.Instance.RunningCount;

        public int ScreenShotRunningCount => ScreenShotManager.Instance.RunningCount;

        public event PropertyChangedEventHandler PropertyChanged;

        private void Notify([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            if (name != nameof(TotalRunningCount))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalRunningCount)));
            if (name != nameof(HasRunningTasks))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasRunningTasks)));
        }
    }
}
