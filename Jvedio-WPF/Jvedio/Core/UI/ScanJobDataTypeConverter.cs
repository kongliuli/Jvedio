using Jvedio.Core.Scan;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Jvedio.Core.UI
{
    public sealed class ScanJobDataTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ScanJobBase job)
                return job.LibraryDataType.ToString();
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
