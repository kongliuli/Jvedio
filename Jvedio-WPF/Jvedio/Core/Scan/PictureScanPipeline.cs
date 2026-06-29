using Jvedio.Core.Enums;
using Jvedio.Entity;
using Jvedio.Entity.Data;
using SuperUtils.IO;
using SuperUtils.Security;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            foreach (string path in pictureScan.ScanPaths) {
                List<string> list = FileHelper.TryGetAllFiles(path, "*.*").ToList();
                if (list != null && list.Count > 0)
                    pictureScan.pathDict.Add(path, list);
            }

            try {
                pictureScan.CheckStatus();
            } catch (TaskCanceledException ex) {
                System.Console.WriteLine(ex.Message);
                return;
            }

            (List<Picture> import, List<string> notImport) = ParsePicture(
                pictureScan.pathDict, pictureScan.FileExt, pictureScan.dataType);

            try {
                pictureScan.CheckStatus();
            } catch (TaskCanceledException ex) {
                System.Console.WriteLine(ex.Message);
                return;
            }

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
                _store.Persist(classification, pictureScan.ScanResult, pictureScan.dataType);
            }

            HandleNotImport(pictureScan, notImport);

            pictureScan.Success = true;
            pictureScan.FinishSuccess();
        }

        private static (List<Picture>, List<string>) ParsePicture(
            Dictionary<string, List<string>> pathDict, List<string> fileExt, DataType dataType)
        {
            List<Picture> import = new List<Picture>();
            List<string> notImport = new List<string>();
            if (pathDict == null || pathDict.Keys.Count == 0)
                return (null, null);

            foreach (string path in pathDict.Keys) {
                List<string> list = pathDict[path];
                List<string> videoPaths = list.Where(arg =>
                    ScanExtensions.VIDEO_EXTENSIONS_LIST.Contains(Path.GetExtension(arg).ToLower())).ToList();
                List<string> imgPaths = list.Where(arg =>
                    fileExt.Contains(Path.GetExtension(arg).ToLower())).ToList();

                Picture picture = new Picture();
                picture.DataType = dataType;
                picture.Title = Path.GetFileName(path);
                picture.PicCount = imgPaths.Count;
                picture.VideoPaths = string.Join(SuperUtils.Values.ConstValues.SeparatorString, videoPaths);
                picture.PicPaths = string.Join(SuperUtils.Values.ConstValues.SeparatorString,
                    imgPaths.Select(arg => Path.GetFileName(arg)));
                picture.Path = path;
                long totalSize = 0;
                foreach (string imgPath in imgPaths)
                    totalSize += new FileInfo(imgPath).Length;
                picture.Size = totalSize;

                List<string> hashPaths = new List<string>(imgPaths);
                hashPaths.AddRange(videoPaths);
                notImport.AddRange(list.Except(hashPaths));
                picture.Hash = Encrypt.FasterDirMD5(hashPaths);
                import.Add(picture);
            }

            return (import, notImport);
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
