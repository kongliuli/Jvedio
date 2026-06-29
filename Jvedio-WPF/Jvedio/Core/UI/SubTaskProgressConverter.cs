using Jvedio.Core.Tasks;
using SuperUtils.Framework.Tasks;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Jvedio.Core.UI
{
    public sealed class SubTaskProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TaskDisplayHelper.GetSubTaskProgressText(value as AbstractTask) ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
