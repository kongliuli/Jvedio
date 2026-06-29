using Jvedio.Core.Enums;
using Jvedio.Core.Scan.Discovery;
using Jvedio.Core.Tasks;
using SuperUtils.CustomEventArgs;
using SuperUtils.Framework.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Jvedio.Core.Scan
{
    public class ScanContext
    {
        public List<string> ScanPaths { get; set; } = new List<string>();
        public List<string> FilePaths { get; set; } = new List<string>();
        public List<string> FileExt { get; set; }
        public DataType DataType { get; set; } = DataType.Video;
        public DiscoveryMode DiscoveryMode { get; set; } = DiscoveryMode.Full;
        public ScanMode Mode { get; set; } = ScanMode.FullImport;
    }

    public interface IScanPipeline
    {
        DataType DataType { get; }
        void Execute(ScanJobBase job, ScanContext context);
    }

    public abstract class ScanJobBase : AbstractTask, IBackgroundTask
    {
        public virtual IReadOnlyList<AbstractTask> Children { get; } = Array.Empty<AbstractTask>();
        public event EventHandler onScanning;

        public ScanResult ScanResult { get; set; }
        public List<string> ScanPaths { get; set; }
        public List<string> FilePaths { get; set; }
        public List<string> FileExt { get; set; }
        public DiscoveryMode DiscoveryMode { get; set; } = DiscoveryMode.Full;
        public ScanMode RunMode { get; set; } = ScanMode.FullImport;
        public DataType LibraryDataType { get; set; } = DataType.Video;

        public bool Success { get; set; }

        protected ScanJobBase()
        {
            ScanResult = new ScanResult();
            ScanPaths = new List<string>();
            FilePaths = new List<string>();
        }

        protected ScanJobBase(ScanContext context) : this()
        {
            if (context == null)
                return;
            if (context.ScanPaths != null && context.ScanPaths.Count > 0)
                ScanPaths = context.ScanPaths.Where(arg => Directory.Exists(arg)).ToList();
            if (context.FilePaths != null && context.FilePaths.Count > 0)
                FilePaths = context.FilePaths.Where(arg => File.Exists(arg)).ToList();
            FileExt = context.FileExt != null ? NormalizeExtensions(context.FileExt) : null;
            DiscoveryMode = context.DiscoveryMode;
            RunMode = context.Mode;
            LibraryDataType = context.DataType;
        }

        public static List<string> NormalizeExtensions(IEnumerable<string> fileExt)
        {
            List<string> result = new List<string>();
            foreach (var item in fileExt) {
                string ext = item.Trim();
                if (string.IsNullOrEmpty(ext))
                    continue;
                result.Add(ext.StartsWith(".") ? ext : "." + ext);
            }
            return result;
        }

        public void NotifyScanPath()
        {
            if (ScanPaths.Count == 0 && FilePaths.Count != 0) {
                string path = FilePaths[FilePaths.Count - 1];
                Message = path;
                onScanning?.Invoke(this, new MessageCallBackEventArgs(path));
            }
        }

        public void RaiseScanning(string path)
        {
            Message = path;
            onScanning?.Invoke(this, new MessageCallBackEventArgs(path));
        }

        public virtual void CheckStatus()
        {
            if (Status == TaskStatus.Canceled) {
                StopWatch();
                Running = false;
                throw new TaskCanceledException();
            }
        }

        public void FinishSuccess()
        {
            Running = false;
            StopWatch();
            if (ScanResult != null) {
                ScanResult.ElapsedMilliseconds = ElapsedMilliseconds;
                string discoverSummary = ScanResult.FormatDiscoverSummary();
                if (!string.IsNullOrEmpty(discoverSummary))
                    Message = discoverSummary;
            }
            Status = TaskStatus.RanToCompletion;
            OnCompleted(null);
        }

        public void FinishCancelled()
        {
            FinalizeWithCancel();
            OnCompleted(null);
        }

        public void ReportInsertError(string message)
        {
            OnError(new MessageCallBackEventArgs(message));
        }
    }
}
