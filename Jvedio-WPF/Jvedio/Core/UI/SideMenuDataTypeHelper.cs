using Jvedio.Core.Enums;

namespace Jvedio.Core.UI
{
    internal static class SideMenuDataTypeHelper
    {
        public static int CurrentTypeInt => (int)Core.Library.LibraryContext.Current.DataType;

        public static string CurrentTypeSql => CurrentTypeInt.ToString();
    }
}
