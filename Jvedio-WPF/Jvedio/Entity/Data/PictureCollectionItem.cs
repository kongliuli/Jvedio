using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.Framework.ORM.Enums;

namespace Jvedio.Entity.Data
{
    [Table(tableName: "picture_collection_item")]
    public class PictureCollectionItem
    {
        [TableId(IdType.AUTO)]
        public long id { get; set; }

        public long CollectionID { get; set; }

        /// <summary>folder | file | album</summary>
        public string ItemType { get; set; }

        public string RefPath { get; set; }

        public long RefDataID { get; set; }

        public long RefFID { get; set; }

        public int SortOrder { get; set; }
    }
}
