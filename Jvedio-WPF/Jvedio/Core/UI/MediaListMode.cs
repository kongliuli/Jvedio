using Jvedio.Core.Enums;

namespace Jvedio.Core.UI
{
    public enum MediaListMode
    {
        Video,
        Picture,
    }

    public static class MediaListModeExtensions
    {
        public static MediaListMode FromDataType(DataType dataType)
        {
            if (dataType == DataType.Picture)
                return MediaListMode.Picture;
            return MediaListMode.Video;
        }
    }
}
