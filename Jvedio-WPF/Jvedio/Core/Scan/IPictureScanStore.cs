using Jvedio.Core.Enums;
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
    public interface IPictureScanStore
    {
        List<Picture> LoadExisting();
        void Persist(ImportClassification<Picture> classification, ScanResult scanResult, DataType dataType);
    }

    internal sealed class MapperPictureScanStore : IPictureScanStore
    {
        public List<Picture> LoadExisting()
        {
            string sql = PictureMapper.BASE_SQL;
            sql = "select metadata.DataID,Hash,Size,Path,PID " + sql;
            List<Dictionary<string, object>> list = pictureMapper.Select(sql);
            return pictureMapper.ToEntity<Picture>(list, typeof(Picture).GetProperties(), false);
        }

        public void Persist(ImportClassification<Picture> classification, ScanResult scanResult, DataType dataType)
        {
            foreach (var kv in classification.NotImport)
                scanResult.NotImport[kv.Key] = kv.Value;
            foreach (var kv in classification.UpdateReasons)
                scanResult.Update[kv.Key] = kv.Value;

            List<MetaData> toUpdateData = classification.ToUpdate.Select(arg => arg.toMetaData()).ToList();
            if (toUpdateData.Count > 0)
                metaDataMapper.UpdateBatch(toUpdateData, "Title", "Size", "Hash", "Path", "LastScanDate");

            if (classification.ToUpdate.Count > 0)
                pictureMapper.UpdateBatch(classification.ToUpdate, "PicCount", "PicPaths", "VideoPaths");

            List<Picture> toInsert = classification.ToInsert;
            foreach (Picture data in toInsert) {
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

            foreach (Picture data in toInsert) {
                data.DataID = before;
                before++;
            }

            try {
                pictureMapper.ExecuteNonQuery("BEGIN TRANSACTION;");
                pictureMapper.InsertBatch(toInsert);
            } catch (System.Exception ex) {
                Logger.Error(ex.Message);
            } finally {
                pictureMapper.ExecuteNonQuery("END TRANSACTION;");
            }
        }
    }
}
