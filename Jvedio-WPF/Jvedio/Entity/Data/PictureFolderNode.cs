using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.Framework.ORM.Enums;

namespace Jvedio.Entity.Data
{
    [Table(tableName: "picture_folder_node")]
    public class PictureFolderNode
    {
        [TableId(IdType.AUTO)]
        public long NodeID { get; set; }

        public long DBId { get; set; }

        public string ScanRoot { get; set; }

        public string FullPath { get; set; }

        public string ParentPath { get; set; }

        public string Name { get; set; }

        public int Depth { get; set; }

        public int HasImages { get; set; } = 1;
    }
}
