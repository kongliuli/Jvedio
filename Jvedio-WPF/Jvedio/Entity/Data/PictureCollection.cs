using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.Framework.ORM.Enums;

namespace Jvedio.Entity.Data
{
    [Table(tableName: "picture_collection")]
    public class PictureCollection
    {
        [TableId(IdType.AUTO)]
        public long CollectionID { get; set; }

        public long DBId { get; set; }

        public string Name { get; set; }

        public int SortOrder { get; set; }

        public string CoverPath { get; set; }

        public string CreateDate { get; set; }
    }
}
