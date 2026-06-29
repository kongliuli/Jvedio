using Jvedio.Core.Enums;
using Jvedio.Core.Library;
using System.Collections.Generic;

namespace Jvedio.Core.UI
{
    public static class SideMenuLabelQueries
    {
        public static List<string> GetGenreList(string searchText)
        {
            return GetGenreList(LibraryContext.Current.DataType, searchText);
        }

        public static List<string> GetGenreList(DataType dataType, string searchText)
        {
            int typeInt = SideMenuStatisticsHelper.ResolveDataTypeInt(dataType);
            return SideMenuStatisticsHelper.GetGenreList(typeInt, searchText);
        }

        public static List<string> GetListByField(string field, string searchText)
        {
            return GetListByField(LibraryContext.Current.DataType, field, searchText);
        }

        public static List<string> GetListByField(DataType dataType, string field, string searchText)
        {
            int typeInt = SideMenuStatisticsHelper.ResolveDataTypeInt(dataType);
            return SideMenuStatisticsHelper.GetListByField(typeInt, field, searchText);
        }
    }
}
