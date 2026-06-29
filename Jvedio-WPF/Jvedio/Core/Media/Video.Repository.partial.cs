using Jvedio.Mapper;
using SuperUtils.Framework.ORM.Wrapper;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using static Jvedio.App;

namespace Jvedio.Entity
{
    public partial class Video
    {
        public static SelectWrapper<Video> InitWrapper()
        {
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            wrapper.Eq("metadata.DBId", ConfigManager.Main.CurrentDBId)
                .Eq("metadata.DataType", 0);
            return wrapper;
        }

        public static void SetAsso(ref Video video)
        {
            video.HasAssociation = false;
            video.AssociationList = new ObservableCollection<long>();

            HashSet<long> set = MapperManager.associationMapper.GetAssociationDatas(video.DataID);

            if (set != null) {
                video.HasAssociation = set.Count > 0;
                foreach (var item in set.ToArray()) {
                    video.AssociationList.Add(item);
                }
            }
        }

        public static Video GetById(long dataID)
        {
            Video video = MapperManager.videoMapper.SelectVideoByID(dataID);
            SetImage(ref video);
            SetTagStamps(ref video);
            SetTitleAndDate(ref video);
            SetAsso(ref video);
            return video;
        }

        public static List<Video> GetAllByDBID(long dbid)
        {
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            wrapper.Eq("DBId", dbid).Eq("DataType", "0");
            string[] SelectFields =
            {
                "DISTINCT metadata.DataID",
                "MVID",
                "VID",
                "metadata.Grade",
                "metadata.Title",
            };

            wrapper.Select(SelectFields);
            string sql = wrapper.ToSelect(false) + VideoMapper.SQL_BASE + wrapper.ToWhere(false);

            List<Dictionary<string, object>> list = MapperManager.metaDataMapper.Select(sql);
            List<Video> videos = MapperManager.metaDataMapper.ToEntity<Video>(list, typeof(Video).GetProperties(), false);

            return videos;
        }
    }
}
