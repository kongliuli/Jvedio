using Jvedio.Core.Enums;
using Jvedio.Core.Library;
using Jvedio.Core.Metadata;
using Jvedio.Core.Tasks;
using Jvedio.Core.UI;
using Jvedio.Entity;
using Jvedio.ViewModel;
using SuperControls.Style;
using SuperUtils.CustomEventArgs;
using SuperUtils.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using static Jvedio.App;

namespace Jvedio.Core.Scan
{
    /// <summary>扫描入队与完成处理（Wave 18 从 Window_Main 抽出）。</summary>
    public static class ScanOrchestrator
    {
        public static void EnqueueScan(IEnumerable<string> scanFileList, EventHandler onCompleted = null)
        {
            if (scanFileList == null)
                return;

            List<string> files = new List<string>();
            List<string> paths = new List<string>();

            foreach (string item in scanFileList) {
                if (FileHelper.IsFile(item))
                    files.Add(item);
                else
                    paths.Add(item);
            }

            ScanJobBase scanJob = ScanFactory.ProduceScanner(LibraryContext.Current.DataType, paths, files);
            scanJob.Title = "-";
            scanJob.onCanceled += (s, ev) => Logger.Warn("cancel scan task");
            scanJob.onError += (s, ev) => MessageCard.Error((ev as MessageCallBackEventArgs).Message);
            if (onCompleted != null)
                scanJob.onCompleted += onCompleted;
            Core.Tasks.TaskHub.Instance.AddTask(scanJob, TaskKind.Scan);
        }

        public static void HandleScanCompleted(
            ScanJobBase scanJob,
            VieModel_Main vieModel,
            Dispatcher dispatcher,
            Action reloadAll,
            Action<List<Video>> screenShotAfterImport)
        {
            if (scanJob == null || !scanJob.Success)
                return;

            dispatcher.Invoke(() => {
                DataType libraryType = vieModel.LibraryContext?.DataType ?? LibraryContext.Current.DataType;
                LibraryEventBus.RaiseScanCompleted(scanJob, libraryType);

                ScanCompletionPolicy policy = MediaUIHost.GetScanCompletionPolicy(libraryType);
                ScanResult scanResult = scanJob.ScanResult;
                if (scanResult != null) {
                    string discoverLine = scanResult.FormatDiscoverSummary();
                    string summary = $"总数    {scanResult.TotalCount.ToString().PadRight(8)}已导入    {scanResult.InsertedCount}{Environment.NewLine}" +
                        $"更新    {scanResult.Update.Count.ToString().PadRight(8)} 未导入    {scanResult.NotImport.Count}";
                    if (!string.IsNullOrEmpty(discoverLine))
                        summary += Environment.NewLine + discoverLine;
                    MessageCard.Info(summary);
                }

                if (policy.HasPolicy(ScanCompletionPolicy.RefreshStatistic))
                    vieModel.Statistic();

                if (ConfigManager.ScanConfig.LoadDataAfterScan && policy.HasPolicy(ScanCompletionPolicy.ReloadTabs))
                    reloadAll?.Invoke();

                if (policy.HasPolicy(ScanCompletionPolicy.ScreenShotAfterImport) &&
                    ConfigManager.FFmpegConfig.ScreenShotAfterImport &&
                    scanResult?.InsertVideos != null &&
                    scanResult.InsertVideos.Count > 0) {
                    screenShotAfterImport?.Invoke(scanResult.InsertVideos);
                }

                if (policy.HasPolicy(ScanCompletionPolicy.ScrapeAfterImport) &&
                    ConfigManager.ScanConfig.ScrapeAfterScan &&
                    scanResult?.InsertVideos != null &&
                    scanResult.InsertVideos.Count > 0) {
                    int scrapeAdded = MetadataScrapeService.EnqueueAfterScan(scanResult.InsertVideos);
                    if (scrapeAdded > 0) {
                        Logger.Info($"scan auto scrape enqueued {scrapeAdded}");
                        MessageCard.Info(string.Format(LangManager.GetValueByKey("ScrapeEnqueuedFormat"), scrapeAdded));
                    }
                }
            });
        }
    }
}
