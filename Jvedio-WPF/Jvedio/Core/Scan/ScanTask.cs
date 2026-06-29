using Jvedio.Core.DataBase;
using Jvedio.Core.Enums;
using Jvedio.Entity;
using Jvedio.Mapper;
using SuperControls.Style;
using SuperUtils.CustomEventArgs;
using SuperUtils.IO;
using SuperUtils.Time;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Jvedio.App;
using static Jvedio.MapperManager;

namespace Jvedio.Core.Scan
{
    public class ScanTask : ScanJobBase
    {
        private ExistVideoIndex existVideoIndex { get; set; }
        private readonly VideoScanPipeline _pipeline;

        static ScanTask()
        {
            STATUS_TO_TEXT_DICT[TaskStatus.Running] = $"{LangManager.GetValueByKey("Scanning")}...";
        }

        public ScanTask(List<string> scanPaths, List<string> filePaths, IEnumerable<string> fileExt = null)
            : base(CreateContext(scanPaths, filePaths, fileExt))
        {
            if (FileExt == null)
                FileExt = ScanExtensions.VIDEO_EXTENSIONS_LIST;
            _pipeline = new VideoScanPipeline();
            Logs.Add($"scan task init, paths count: {FilePaths.Count}, dir count: {ScanPaths.Count}");
        }

        internal ScanTask(ScanContext context, IVideoScanStore store = null) : base(context)
        {
            if (FileExt == null)
                FileExt = ScanExtensions.VIDEO_EXTENSIONS_LIST;
            _pipeline = new VideoScanPipeline(store);
        }

        private static ScanContext CreateContext(List<string> scanPaths, List<string> filePaths, IEnumerable<string> fileExt)
        {
            return new ScanContext {
                ScanPaths = scanPaths,
                FilePaths = filePaths,
                FileExt = fileExt != null ? NormalizeExtensions(fileExt) : null,
                DataType = DataType.Video,
            };
        }

        internal void SetExistVideoIndex(ExistVideoIndex index)
        {
            existVideoIndex = index;
        }

        public override void DoWork()
        {
            Logger.Info(LangManager.GetValueByKey("BeginScan"));
            Task.Run(() => _pipeline.Execute(this, null));
        }

        private readonly INfoImportBatchService _nfoImportService = new NfoImportService();

        internal void HandleImportNFO(List<Video> import)
        {
            if (import == null || import.Count <= 0)
                return;

            if (existVideoIndex == null)
                existVideoIndex = new ExistVideoIndex(new MapperVideoScanStore().LoadExistingVideos((int)ConfigManager.Main.CurrentDBId));

            _nfoImportService.ImportNfoUpdates(import, existVideoIndex, ScanResult, ReportInsertError);
        }
    }
}
