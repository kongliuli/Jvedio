using Jvedio.Entity;
using Jvedio.Entity.Data;
using Jvedio.Mapper;
using SuperUtils.CustomEventArgs;
using SuperUtils.Time;
using System.Collections.Generic;
using System.Linq;
using static Jvedio.App;
using static Jvedio.MapperManager;

namespace Jvedio.Core.Scan
{
    public interface IGameScanStore
    {
        List<Game> LoadExisting();
        void Persist(ImportClassification<Game> classification, ScanResult scanResult);
    }

    internal sealed class MapperGameScanStore : IGameScanStore
    {
        public List<Game> LoadExisting()
        {
            string sql = GameMapper.BASE_SQL;
            sql = "select metadata.DataID,Hash,Path,GID " + sql;
            List<Dictionary<string, object>> list = gameMapper.Select(sql);
            return gameMapper.ToEntity<Game>(list, typeof(Game).GetProperties(), false);
        }

        public void Persist(ImportClassification<Game> classification, ScanResult scanResult)
        {
            foreach (var kv in classification.NotImport)
                scanResult.NotImport[kv.Key] = kv.Value;
            foreach (var kv in classification.UpdateReasons)
                scanResult.Update[kv.Key] = kv.Value;

            List<MetaData> toUpdateData = classification.ToUpdate.Select(arg => arg.toMetaData()).ToList();
            if (toUpdateData.Count > 0)
                metaDataMapper.UpdateBatch(toUpdateData, "Title", "Size", "Hash", "Path", "LastScanDate");

            List<Game> toInsert = classification.ToInsert;
            foreach (Game data in toInsert) {
                data.DBId = ConfigManager.Main.CurrentDBId;
                data.FirstScanDate = DateHelper.Now();
                data.LastScanDate = DateHelper.Now();
                scanResult.Import.Add(data.Path);
            }

            List<MetaData> toInsertData = toInsert.Select(arg => arg.toMetaData()).ToList();
            if (toInsertData.Count <= 0)
                return;
            long.TryParse(metaDataMapper.InsertAndGetID(toInsertData[0]).ToString(), out long before);
            toInsertData.RemoveAt(0);
            try {
                metaDataMapper.ExecuteNonQuery("BEGIN TRANSACTION;");
                metaDataMapper.InsertBatch(toInsertData);
            } catch (System.Exception ex) {
                Logger.Error(ex.Message);
            } finally {
                metaDataMapper.ExecuteNonQuery("END TRANSACTION;");
            }

            foreach (Game data in toInsert) {
                data.DataID = before;
                before++;
            }

            try {
                gameMapper.ExecuteNonQuery("BEGIN TRANSACTION;");
                gameMapper.InsertBatch(toInsert);
            } catch (System.Exception ex) {
                Logger.Error(ex.Message);
            } finally {
                gameMapper.ExecuteNonQuery("END TRANSACTION;");
            }
        }
    }
}
