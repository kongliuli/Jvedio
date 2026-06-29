using Jvedio.Core.Enums;
using System;

namespace Jvedio.Core.UI
{
    [Flags]
    public enum MediaFeatureMask
    {
        None = 0,
        AddMovie = 1,
        AddMediaPath = 2,
    }

    public static class MediaFeatureMaskExtensions
    {
        public static MediaFeatureMask ForDataType(DataType dataType)
        {
            switch (dataType) {
                case DataType.Video:
                    return MediaFeatureMask.AddMovie;
                case DataType.Picture:
                    return MediaFeatureMask.AddMediaPath;
                default:
                    return MediaFeatureMask.None;
            }
        }

        public static bool HasFeature(this MediaFeatureMask mask, string feature)
        {
            if (string.IsNullOrEmpty(feature))
                return true;
            switch (feature) {
                case "AddMovie":
                    return mask.HasFlag(MediaFeatureMask.AddMovie);
                case "AddMediaPath":
                case "AddPicturePath":
                    return mask.HasFlag(MediaFeatureMask.AddMediaPath);
                default:
                    return true;
            }
        }

        public static bool IsFeatureVisible(DataType dataType, string feature)
        {
            if (feature == "FetchVID" || feature == "VideoImport" || feature == "ScrapeAfterScan")
                return dataType == DataType.Video;
            if (feature == "ImportVideoHello")
                return dataType == DataType.Video;
            if (feature == "AddGamePathOnly")
                return false;
            if (feature == "AddPicturePathOnly")
                return dataType == DataType.Picture;
            return MediaFeatureMaskExtensions.ForDataType(dataType).HasFeature(feature);
        }
    }
}
