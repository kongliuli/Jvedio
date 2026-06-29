using Jvedio.Core.Enums;

namespace Jvedio.Core.UI
{
    public enum MediaListMode
    {
        Video,
        Picture,
        Game,
        Comics,
    }

    public static class MediaListModeExtensions
    {
        public static MediaListMode FromDataType(DataType dataType)
        {
            switch (dataType) {
                case DataType.Picture:
                    return MediaListMode.Picture;
                case DataType.Game:
                    return MediaListMode.Game;
                case DataType.Comics:
                    return MediaListMode.Comics;
                default:
                    return MediaListMode.Video;
            }
        }
    }
}
