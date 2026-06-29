using Jvedio.Core.Enums;
using Jvedio.Core.UserControls;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using SuperUtils.Framework.ORM.Utils;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using static Jvedio.App;
using static Jvedio.MapperManager;

namespace Jvedio.Core.UI
{
    public sealed class SideMenuMediaCounts
    {
        public long AllVideoCount { get; set; }
        public double FavoriteVideoCount { get; set; }
        public long AllActorCount { get; set; }
        public long AllLabelCount { get; set; }
        public long RecentWatchCount { get; set; }
        public long AllGenreCount { get; set; }
        public long AllSeriesCount { get; set; }
        public long AllStudioCount { get; set; }
        public long AllDirectorCount { get; set; }
    }

    /// <summary>SideMenu 统计 SQL 共享（Wave 16）。</summary>
    public static class SideMenuStatisticsHelper
    {
        public static SideMenuMediaCounts ComputeMedia(int dataType, string searchText, bool includeActorAndDirector)
        {
            long dbid = ConfigManager.Main.CurrentDBId;
            var counts = new SideMenuMediaCounts();
            counts.AllVideoCount = metaDataMapper.SelectCount(new SelectWrapper<MetaData>().Eq("DBId", dbid).Eq("DataType", dataType));
            appDatabaseMapper.UpdateFieldById("Count", counts.AllVideoCount.ToString(), dbid);
            counts.FavoriteVideoCount = metaDataMapper.SelectCount(new SelectWrapper<MetaData>().Eq("DBId", dbid).Eq("DataType", dataType).Gt("Grade", 0));

            if (includeActorAndDirector) {
                string actor_count_sql = "SELECT count(*) as Count from (SELECT actor_info.ActorID FROM actor_info join metadata_to_actor " +
                    "on metadata_to_actor.ActorID=actor_info.ActorID join metadata on metadata_to_actor.DataID=metadata.DataID " +
                    $"WHERE metadata.DBId={dbid} and metadata.DataType={dataType} GROUP BY actor_info.ActorID UNION " +
                    "select actor_info.ActorID FROM actor_info WHERE NOT EXISTS " +
                    "(SELECT 1 from metadata_to_actor where metadata_to_actor.ActorID=actor_info.ActorID ) GROUP BY actor_info.ActorID)";
                counts.AllActorCount = actorMapper.SelectCount(actor_count_sql);
            }

            string label_count_sql = "SELECT COUNT(DISTINCT LabelName) as Count from metadata_to_label " +
                "join metadata on metadata_to_label.DataID=metadata.DataID " +
                $"WHERE metadata.DBId={dbid} and metadata.DataType={dataType} ";
            counts.AllLabelCount = metaDataMapper.SelectCount(label_count_sql);

            DateTime date1 = DateTime.Now.AddDays(-1 * ViewModel.VieModel_Main.RECENT_DAY);
            DateTime date2 = DateTime.Now;
            counts.RecentWatchCount = metaDataMapper.SelectCount(new SelectWrapper<MetaData>().Eq("DBId", dbid).Eq("DataType", dataType)
                .Between("ViewDate", DateHelper.ToLocalDate(date1), DateHelper.ToLocalDate(date2)));

            counts.AllGenreCount = GetGenreDict(dataType, searchText).Count;
            counts.AllSeriesCount = GetListByField(dataType, LabelType.Series.ToString(), searchText).Count;
            counts.AllStudioCount = GetListByField(dataType, LabelType.Studio.ToString(), searchText).Count;
            if (includeActorAndDirector)
                counts.AllDirectorCount = GetListByField(dataType, LabelType.Director.ToString(), searchText).Count;
            return counts;
        }

        public static Dictionary<string, long> GetGenreDict(int dataType, string searchText)
        {
            Dictionary<string, long> genreDict = new Dictionary<string, long>();
            string sql = $"SELECT Genre from metadata where metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={dataType} AND Genre !=''";
            List<Dictionary<string, object>> lists = metaDataMapper.Select(sql);
            if (lists == null)
                return genreDict;

            string text = string.IsNullOrEmpty(searchText) ? string.Empty : searchText;
            bool search = !string.IsNullOrEmpty(text);
            foreach (Dictionary<string, object> item in lists) {
                if (!item.ContainsKey("Genre"))
                    continue;
                string genre = item["Genre"].ToString();
                if (string.IsNullOrEmpty(genre))
                    continue;
                foreach (string g in genre.Split(new[] { SuperUtils.Values.ConstValues.Separator }, StringSplitOptions.RemoveEmptyEntries)) {
                    if (search && g.IndexOf(text, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                    if (genreDict.ContainsKey(g))
                        genreDict[g]++;
                    else
                        genreDict[g] = 1;
                }
            }
            return genreDict;
        }

        public static List<string> GetGenreList(int dataType, string searchText)
        {
            Dictionary<string, long> ordered = GetGenreDict(dataType, searchText)
                .OrderByDescending(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value);
            return ordered.Keys.Select(k => $"{k}({ordered[k]})").ToList();
        }

        public static List<string> GetListByField(int dataType, string field, string searchText)
        {
            string like_sql = string.IsNullOrEmpty(searchText) ? string.Empty : $" and {field} like '%{searchText.ToProperSql()}%' ";
            string joinVideo = dataType == (int)DataType.Video ? "JOIN metadata_video on metadata.DataID=metadata_video.DataID " : string.Empty;
            string sql = $"SELECT {field},Count({field}) as Count from metadata {joinVideo}" +
                $"where metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={dataType} AND {field} !='' {like_sql}" +
                $"GROUP BY {field} ORDER BY Count DESC";
            List<Dictionary<string, object>> list = metaDataMapper.Select(sql);
            List<string> result = new List<string>();
            if (list == null)
                return result;
            foreach (Dictionary<string, object> item in list) {
                if (!item.ContainsKey(field))
                    continue;
                string name = item[field].ToString();
                long.TryParse(item["Count"].ToString(), out long count);
                if (string.IsNullOrEmpty(name))
                    continue;
                result.Add($"{name}({count})");
            }
            return result;
        }

        public static int ResolveDataTypeInt(DataType dataType) => (int)dataType;
    }
}
