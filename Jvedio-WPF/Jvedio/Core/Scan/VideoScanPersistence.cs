using Jvedio.Core.DataBase;
using Jvedio.Entity;
using SuperControls.Style;
using SuperUtils.CustomEventArgs;
using SuperUtils.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using static Jvedio.App;
using static Jvedio.MapperManager;

namespace Jvedio.Core.Scan
{
    internal static class VideoScanPersistence
    {
        public static void UpdateImportedVideos(IList<Video> toUpdate)
        {
            if (toUpdate == null || toUpdate.Count == 0)
                return;
            videoMapper.UpdateBatch(toUpdate, "SubSection");
            List<MetaData> toUpdateData = toUpdate.Select(arg => arg.toMetaData()).ToList();
            metaDataMapper.UpdateBatch(toUpdateData, "Path", "LastScanDate", "PathExist");
            AddTags(toUpdate);
        }

        public static void InsertVideos(IList<Video> toInsert, ScanResult scanResult, Action<string> onError = null)
        {
            if (toInsert == null || toInsert.Count == 0)
                return;

            foreach (Video video in toInsert) {
                video.DBId = ConfigManager.Main.CurrentDBId;
                video.FirstScanDate = DateHelper.Now();
                video.LastScanDate = DateHelper.Now();
                video.PathExist = 1;
                scanResult.Import.Add(video.Path);
            }

            List<MetaData> toInsertData = toInsert.Select(arg => arg.toMetaData()).ToList();
            if (toInsertData.Count <= 0)
                return;
            long.TryParse(metaDataMapper.InsertAndGetID(toInsertData[0]).ToString(), out long before);
            toInsertData.RemoveAt(0);

            try {
                metaDataMapper.ExecuteNonQuery("BEGIN TRANSACTION;");
                metaDataMapper.InsertBatch(toInsertData);
                SqlManager.DataBaseBusy = true;
            } catch (Exception ex) {
                Logger.Error(ex.Message);
                onError?.Invoke(ex.Message);
            } finally {
                metaDataMapper.ExecuteNonQuery("END TRANSACTION;");
                SqlManager.DataBaseBusy = false;
            }

            foreach (Video video in toInsert) {
                video.DataID = before;
                before++;
            }

            try {
                videoMapper.ExecuteNonQuery("BEGIN TRANSACTION;");
                SqlManager.DataBaseBusy = true;
                videoMapper.InsertBatch(toInsert);
            } catch (Exception ex) {
                Logger.Error(ex.Message);
                onError?.Invoke(ex.Message);
            } finally {
                videoMapper.ExecuteNonQuery("END TRANSACTION;");
                SqlManager.DataBaseBusy = false;
            }

            AddTags(toInsert);
            List<ActorInfo> existActors = actorMapper.SelectList();
            foreach (Video video in toInsert)
                HandleActor(video, existActors);

            scanResult.InsertVideos.AddRange(toInsert);
        }

        public static void HandleActor(Video video, List<ActorInfo> existActors)
        {
            if (string.IsNullOrEmpty(video.ActorNames))
                return;
            List<string> list = video.ActorNames.Split(SuperUtils.Values.ConstValues.Separator).ToList();
            List<string> urls = video.ActorThumbs;
            for (int i = 0; i < list.Count; i++) {
                string name = list[i];
                string url = i < urls.Count ? urls[i] : string.Empty;
                ActorInfo actorInfo = existActors?.Where(arg => arg.ActorName.Equals(name)).FirstOrDefault();
                if (actorInfo == null || actorInfo.ActorID <= 0) {
                    actorInfo = new ActorInfo();
                    actorInfo.ActorName = name;
                    actorInfo.ImageUrl = url;
                    actorMapper.Insert(actorInfo);
                    existActors.Add(actorInfo);
                } else {
                    actorInfo.ImageUrl = url;
                    actorMapper.UpdateFieldById("ImageUrl", url, actorInfo.ActorID);
                }

                string sql = $"insert or ignore into metadata_to_actor (ActorID,DataID) values ({actorInfo.ActorID},{video.DataID})";
                metaDataMapper.ExecuteNonQuery(sql);
            }
        }

        private static void AddTags(ICollection<Video> videos)
        {
            List<string> list = new List<string>();
            foreach (Video video in videos) {
                if (video.IsHDV())
                    list.Add($"({video.DataID},1)");
                if (video.IsCHS())
                    list.Add($"({video.DataID},2)");
                list.Add($"({video.DataID},10000)");
            }

            if (list.Count > 0) {
                string sql = $"insert or ignore into metadata_to_tagstamp (DataID,TagID) values {string.Join(",", list)}";
                videoMapper.ExecuteNonQuery(sql);
            }
        }
    }
}
