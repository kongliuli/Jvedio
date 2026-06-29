using Jvedio.Core.Enums;
using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.Framework.ORM.Enums;

namespace Jvedio.Entity.Data
{
    [Table(tableName: "metadata_picture")]
    public class Picture : MetaData
    {
        [TableId(IdType.AUTO)]
        public long PID { get; set; }

#pragma warning disable CS0108 // “Picture.DataID”隐藏继承的成员“MetaData.DataID”。如果是有意隐藏，请使用关键字 new。
        public long DataID { get; set; }

#pragma warning restore CS0108 // “Picture.DataID”隐藏继承的成员“MetaData.DataID”。如果是有意隐藏，请使用关键字 new。
        public string Director { get; set; }

        public string Studio { get; set; }

        public string Publisher { get; set; }

        public string Plot { get; set; }

        public string Outline { get; set; }

        public int PicCount { get; set; }

        public string PicPaths { get; set; }

        public string VideoPaths { get; set; }

        public string ExtraInfo { get; set; }

        public MetaData toMetaData()
        {
            MetaData metaData = (MetaData)this;
            metaData.DataID = this.DataID;
            return metaData;
        }
    }
}
