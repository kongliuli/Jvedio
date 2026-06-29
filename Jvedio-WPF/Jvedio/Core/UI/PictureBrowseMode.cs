namespace Jvedio.Core.UI
{
    /// <summary>Picture 主列表浏览模式（Phase C）。</summary>
    public enum PictureBrowseMode
    {
        /// <summary>单图视图：展开 file 表行。</summary>
        SingleImage = 0,

        /// <summary>相册视图：每个含直接图片的子文件夹一条 metadata_picture。</summary>
        Album = 1,
    }
}
