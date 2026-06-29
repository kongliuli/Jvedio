using Jvedio.Core.Enums;
using Jvedio.Entity.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jvedio.Core.Scan
{
    internal sealed class PictureScanPipeline : IScanPipeline
    {
        public DataType DataType => DataType.Picture;

        private readonly IPictureScanStore _store;

        public PictureScanPipeline(IPictureScanStore store = null)
        {
            _store = store ?? new MapperPictureScanStore();
        }

        public void Execute(ScanJobBase job, ScanContext context)
        {
            PictureScan pictureScan = job as PictureScan;
            if (pictureScan == null)
                return;

            pictureScan.TimeWatch.Start();
            long dbId = Jvedio.ConfigManager.Main.CurrentDBId;
            PictureScanIndexBuilder.BuildResult build = PictureScanIndexBuilder.Build(
                pictureScan.ScanPaths,
                pictureScan.FileExt,
                pictureScan.dataType,
                dbId);

            try {
                pictureScan.CheckStatus();
            } catch (TaskCanceledException ex) {
                System.Console.WriteLine(ex.Message);
                return;
            }

            List<Picture> import = build.Albums;
            if (import != null && import.Count > 0) {
                ExistPictureIndex existIndex = new ExistPictureIndex(_store.LoadExisting());
                ImportClassification<Picture> classification = HashPathImportClassifier.Classify(
                    import,
                    existIndex,
                    p => p.Path,
                    "同路径、同哈希",
                    "哈希相同，路径不同",
                    "哈希不同，路径相同",
                    (picture, exist) => {
                        picture.DataID = exist.DataID;
                        picture.PID = exist.PID;
                        picture.LastScanDate = SuperUtils.Time.DateHelper.Now();
                    });
                _store.Persist(classification, pictureScan.ScanResult, pictureScan.dataType, build.Sidecar);
            } else if (build.Sidecar.FolderNodes.Count > 0) {
                _store.Persist(
                    new ImportClassification<Picture>(),
                    pictureScan.ScanResult,
                    pictureScan.dataType,
                    build.Sidecar);
            }

            HandleNotImport(pictureScan, build.NotImport);

            pictureScan.Success = true;
            pictureScan.FinishSuccess();
        }

        private static void HandleNotImport(PictureScan pictureScan, List<string> notImport)
        {
            if (notImport == null)
                return;
            foreach (string path in notImport)
                pictureScan.ScanResult.NotImport.Add(path, new ScanDetailInfo("不导入"));
        }
    }
}
