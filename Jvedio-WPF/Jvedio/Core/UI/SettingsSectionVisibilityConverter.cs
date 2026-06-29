using Jvedio.Core.Enums;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Jvedio.Core.UI
{
    public sealed class SettingsSectionVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DataType dataType))
                return Visibility.Visible;
            if (parameter == null || !Enum.TryParse(parameter.ToString(), out SettingsSectionMask section))
                return Visibility.Visible;
            return MediaUIHost.GetSettingsSections(dataType).HasSection(section)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public sealed class NfoParseSectionVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            DataType dataType = values != null && values.Length > 0 && values[0] is DataType dt
                ? dt
                : DataType.Video;
            bool scanNfo = values != null && values.Length > 1 && values[1] is bool b && b;
            if (!MediaUIHost.GetSettingsSections(dataType).HasSection(SettingsSectionMask.NfoFfmpeg))
                return Visibility.Collapsed;
            return scanNfo ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public sealed class MediaFeatureVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DataType dataType))
                return Visibility.Visible;
            string feature = parameter?.ToString() ?? string.Empty;
            bool visible = MediaFeatureMaskExtensions.IsFeatureVisible(dataType, feature);
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
