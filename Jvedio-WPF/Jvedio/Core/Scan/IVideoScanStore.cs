using Jvedio.Entity;
using Jvedio.Mapper;
using System;
using System.Collections.Generic;
using static Jvedio.MapperManager;

namespace Jvedio.Core.Scan
{
    public interface IVideoScanStore
    {
        List<Video> LoadExistingVideos(int dbId);
        void UpdateImportedVideos(IList<Video> toUpdate);
        void InsertVideos(IList<Video> toInsert, ScanResult scanResult, Action<string> onError = null);
    }

    internal sealed class MapperVideoScanStore : IVideoScanStore
    {
        public List<Video> LoadExistingVideos(int dbId)
        {
            string sql = VideoMapper.SQL_BASE;
            sql = "select metadata.DataID,VID,Hash,Size,Path,MVID,SubSection " + sql + $" and metadata.DBId={dbId}";
            List<Dictionary<string, object>> list = videoMapper.Select(sql);
            return videoMapper.ToEntity<Video>(list, typeof(Video).GetProperties(), false);
        }

        public void UpdateImportedVideos(IList<Video> toUpdate)
        {
            VideoScanPersistence.UpdateImportedVideos(toUpdate);
        }

        public void InsertVideos(IList<Video> toInsert, ScanResult scanResult, Action<string> onError = null)
        {
            VideoScanPersistence.InsertVideos(toInsert, scanResult, onError);
        }
    }
}
