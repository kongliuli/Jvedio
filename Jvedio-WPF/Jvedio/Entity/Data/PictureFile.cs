using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.Framework.ORM.Enums;

namespace Jvedio.Entity.Data
{
    [Table(tableName: "metadata_picture_file")]
    public class PictureFile
    {
        [TableId(IdType.AUTO)]
        public long FID { get; set; }

        public long DataID { get; set; }

        public string RelativePath { get; set; }

        public string FileName { get; set; }

        public long Size { get; set; }

        public string Hash { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string ExtraInfo { get; set; }
    }
}
