using Jvedio.Core.Config.Interfaces;
using Jvedio.Core.Enums;
using Jvedio.Core.Scan.Discovery;
using Jvedio.Entity;
using SuperControls.Style;
using System;
using SuperUtils.CustomEventArgs;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Jvedio.App;

namespace Jvedio.Core.Scan
{
    internal sealed class VideoScanPipeline : IScanPipeline
    {
        public DataType DataType => DataType.Video;

        private readonly IVideoScanStore _store;
        private readonly IScanSettings _scanSettings;

        public VideoScanPipeline(IVideoScanStore store = null, IScanSettings scanSettings = null)
        {
            _store = store ?? new MapperVideoScanStore();
            _scanSettings = scanSettings ?? ConfigScanSettings.Instance;
        }

        public void Execute(ScanJobBase job, ScanContext context)
        {
            ScanTask scanTask = job as ScanTask;
            if (scanTask == null)
                return;

            scanTask.StartWatch();
            scanTask.NotifyScanPath();
            ScanDiscoveryHelper.RunDiscover(scanTask, ScanExtensions.VIDEO_EXTENSIONS_SET);

            if (ScanPipelineHelper.TryFinishDiscoverOnly(scanTask, context))
                return;

            try {
                scanTask.CheckStatus();
            } catch (TaskCanceledException) {
                scanTask.FinalizeWithCancel();
                return;
            }

            VideoParser videoParser = new VideoParser((msg) => {
                if (scanTask.Logs != null)
                    scanTask.Logs.Add(msg);
            });

            try {
                (List<Video> import, Dictionary<string, NotImportReason> notImport, List<string> failNFO)
                    parseResult = videoParser.ParseMovie(scanTask.FilePaths, scanTask.FileExt, scanTask.Token,
                        _scanSettings.ScanNfo, callBack: (msg) => Logger.Error(msg));

                scanTask.Progress = 60;
                scanTask.ScanResult.TotalCount = parseResult.import.Count + parseResult.notImport.Count + parseResult.failNFO.Count;

                try {
                    scanTask.CheckStatus();
                } catch (TaskCanceledException ex) {
                    Logger.Error(ex.Message);
                    scanTask.FinishCancelled();
                    return;
                }

                List<Video> importNFO = parseResult.import.Where(arg => arg.Path.ToLower().EndsWith(".nfo")).ToList();
                parseResult.import.RemoveAll(arg => arg.Path.ToLower().EndsWith(".nfo"));

                ExistVideoIndex existVideoIndex = new ExistVideoIndex(
                    _store.LoadExistingVideos((int)ConfigManager.Main.CurrentDBId));
                scanTask.SetExistVideoIndex(existVideoIndex);

                ClassifyAndPersistVideos(scanTask, parseResult.import, existVideoIndex);

                scanTask.Progress = 70;
                scanTask.HandleImportNFO(importNFO);
                scanTask.Progress = 80;
                HandleNotImport(scanTask, parseResult.notImport);
                scanTask.Progress = 90;
                HandleFailNFO(scanTask, parseResult.failNFO);
                scanTask.Progress = 100;
                scanTask.Success = true;
            } catch (Exception ex) {
                Logger.Error(ex.Message);
                scanTask.FinalizeWithCancel();
                return;
            }

            scanTask.FinishSuccess();
        }

        private void ClassifyAndPersistVideos(ScanTask scanTask, List<Video> import, ExistVideoIndex index)
        {
            ImportClassification<Video> classification = VideoImportClassifier.Classify(import, index);

            foreach (var kv in classification.NotImport)
                scanTask.ScanResult.NotImport[kv.Key] = kv.Value;
            foreach (var kv in classification.UpdateReasons)
                scanTask.ScanResult.Update[kv.Key] = kv.Value;

            _store.UpdateImportedVideos(classification.ToUpdate);
            _store.InsertVideos(classification.ToInsert, scanTask.ScanResult, scanTask.ReportInsertError);
        }

        private static void HandleNotImport(ScanTask scanTask, Dictionary<string, NotImportReason> notImport)
        {
            foreach (var key in notImport.Keys) {
                if (scanTask.ScanResult.NotImport.ContainsKey(key))
                    continue;
                NotImportReason reason = notImport[key];
                if (reason == NotImportReason.RepetitiveVID) {
                    string vid = JvedioLib.Security.Identify.GetVID(Path.GetFileNameWithoutExtension(key));
                    scanTask.ScanResult.NotImport.Add(key, new ScanDetailInfo($"{ScanReasonText.Get(reason)} => {vid}"));
                } else {
                    scanTask.ScanResult.NotImport.Add(key, new ScanDetailInfo(ScanReasonText.Get(reason)));
                }
            }
        }

        private static void HandleFailNFO(ScanTask scanTask, List<string> failNFO)
        {
            if (failNFO == null || failNFO.Count == 0)
                return;
            scanTask.ScanResult.FailNFO.AddRange(failNFO);
        }
    }
}
