using Jvedio.Core.Enums;
using System.Collections.Generic;
using System.Text;

namespace Jvedio.Core.Mapper
{
    internal static class MetaDataDeleteStrategy
    {
        internal static int DeleteTypeSpecificRows(List<string> idList, DataType dataType, int metadataDeletedCount)
        {
            if (idList == null || idList.Count == 0)
                return 0;

            int c2 = 0;
            if (dataType == DataType.Picture)
                c2 = MapperManager.pictureMapper.DeleteByIds(idList);
            else if (dataType == DataType.Comics)
                c2 = MapperManager.comicMapper.DeleteByIds(idList);
            else if (dataType == DataType.Game)
                c2 = MapperManager.gameMapper.DeleteByIds(idList);
            else
                c2 = metadataDeletedCount;

            ExecuteRelationCleanup(idList, dataType);
            return c2;
        }

        private static void ExecuteRelationCleanup(List<string> idList, DataType dataType)
        {
            if (dataType != DataType.Picture && dataType != DataType.Comics && dataType != DataType.Game)
                return;

            string ids = string.Join(",", idList);
            var builder = new StringBuilder();
            builder.Append("begin;");
            builder.Append($"delete from metadata_to_translation where DataID in ({ids});");
            builder.Append($"delete from metadata_to_tagstamp where DataID in ({ids});");
            builder.Append($"delete from metadata_to_actor where DataID in ({ids});");
            builder.Append($"delete from metadata_to_label where DataID in ({ids});");
            builder.Append("commit;");

            if (dataType == DataType.Picture)
                MapperManager.pictureMapper.ExecuteNonQuery(builder.ToString());
            else if (dataType == DataType.Comics)
                MapperManager.comicMapper.ExecuteNonQuery(builder.ToString());
            else if (dataType == DataType.Game)
                MapperManager.gameMapper.ExecuteNonQuery(builder.ToString());
        }
    }
}
