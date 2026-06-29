using Jvedio.Core.Enums;
using System;

namespace Jvedio.Core.UI
{
    [Flags]
    public enum SettingsSectionMask
    {
        Scan = 1,
        NfoFfmpeg = 2,
        PlayerScreenshot = 4,
        Crawler = 8,
        PicturePaths = 16,
        Rename = 32,
        ThemeHotkey = 64,
    }

    public static class SettingsSectionMaskExtensions
    {
        public static SettingsSectionMask ForDataType(DataType dataType)
        {
            switch (dataType) {
                case DataType.Picture:
                case DataType.Comics:
                    return SettingsSectionMask.Scan
                        | SettingsSectionMask.PicturePaths
                        | SettingsSectionMask.Rename
                        | SettingsSectionMask.ThemeHotkey;
                case DataType.Game:
                    return SettingsSectionMask.Scan
                        | SettingsSectionMask.Rename
                        | SettingsSectionMask.ThemeHotkey;
                default:
                    return SettingsSectionMask.Scan
                        | SettingsSectionMask.NfoFfmpeg
                        | SettingsSectionMask.PlayerScreenshot
                        | SettingsSectionMask.Crawler
                        | SettingsSectionMask.PicturePaths
                        | SettingsSectionMask.Rename
                        | SettingsSectionMask.ThemeHotkey;
            }
        }

        public static bool HasSection(this SettingsSectionMask mask, SettingsSectionMask section)
        {
            return (mask & section) == section;
        }
    }
}
