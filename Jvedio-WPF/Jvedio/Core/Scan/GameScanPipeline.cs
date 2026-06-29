using Jvedio.Core.Enums;
using Jvedio.Entity;
using Jvedio.Entity.Data;
using SuperUtils.IO;
using SuperUtils.Security;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Jvedio.App;

namespace Jvedio.Core.Scan
{
    internal sealed class GameScanPipeline : IScanPipeline
    {
        public DataType DataType => DataType.Game;

        private readonly IGameScanStore _store;

        public GameScanPipeline(IGameScanStore store = null)
        {
            _store = store ?? new MapperGameScanStore();
        }

        public void Execute(ScanJobBase job, ScanContext context)
        {
            GameScan gameScan = job as GameScan;
            if (gameScan == null)
                return;

            gameScan.TimeWatch.Start();
            foreach (string path in gameScan.ScanPaths) {
                List<string> list = FileHelper.TryGetAllFiles(path, "*.exe").ToList();
                if (list != null && list.Count > 0)
                    gameScan.pathDict.Add(path, list);
            }

            try {
                gameScan.CheckStatus();
            } catch (TaskCanceledException ex) {
                Logger.Warn(ex.Message);
                return;
            }

            (List<Game> import, List<string> notImport) = ParseGame(gameScan.pathDict);

            try {
                gameScan.CheckStatus();
            } catch (TaskCanceledException ex) {
                Logger.Warn(ex.Message);
                return;
            }

            if (import != null && import.Count > 0) {
                ExistGameIndex existIndex = new ExistGameIndex(_store.LoadExisting());
                ImportClassification<Game> classification = HashPathImportClassifier.Classify(
                    import,
                    existIndex,
                    g => g.Path,
                    "同路径、同哈希",
                    "哈希相同，路径不同",
                    " 哈希不同，路径相同",
                    (game, exist) => {
                        game.DataID = exist.DataID;
                        game.GID = exist.GID;
                        game.LastScanDate = SuperUtils.Time.DateHelper.Now();
                    });
                _store.Persist(classification, gameScan.ScanResult);
            }

            HandleNotImport(gameScan, notImport);

            gameScan.Success = true;
            gameScan.FinishSuccess();
        }

        private static (List<Game>, List<string>) ParseGame(Dictionary<string, List<string>> pathDict)
        {
            List<Game> import = new List<Game>();
            List<string> notImport = new List<string>();
            if (pathDict == null || pathDict.Keys.Count == 0)
                return (null, null);

            foreach (string path in pathDict.Keys) {
                List<string> list = pathDict[path];
                Game game = new Game();
                game.DataType = DataType.Game;
                game.Title = Path.GetFileName(path);
                game.Path = list[0];
                game.Size = DirHelper.getDirSize(new DirectoryInfo(path));
                game.Hash = Encrypt.TryGetFileMD5(game.Path);
                import.Add(game);
            }

            return (import, notImport);
        }

        private static void HandleNotImport(GameScan gameScan, List<string> notImport)
        {
            if (notImport == null)
                return;
            foreach (string path in notImport)
                gameScan.ScanResult.NotImport.Add(path, new ScanDetailInfo("不导入"));
        }
    }
}
