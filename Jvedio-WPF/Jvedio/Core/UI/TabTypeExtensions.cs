using Jvedio.Core.Enums;
using Jvedio.Entity.Common;

namespace Jvedio.Core.UI
{
    public static class TabTypeExtensions
    {
        /// <summary>当前库类型的「全部」列表 Tab；GeoVideo 为 Video 别名。</summary>
        public static TabType PrimaryListTabType(DataType dataType)
        {
            switch (dataType) {
                case DataType.Picture:
                case DataType.Comics:
                    return TabType.GeoPicture;
                case DataType.Game:
                    return TabType.GeoGame;
                default:
                    return TabType.GeoVideo;
            }
        }

        /// <summary>GeoVideo 与 Profile 主 Tab 视为同一语义（旧 Tab 兼容）。</summary>
        public static bool IsPrimaryListTab(TabType tabType, DataType dataType)
        {
            TabType primary = PrimaryListTabType(dataType);
            if (tabType == primary)
                return true;
            // ponytail: 旧数据/侧栏仍可能打开 GeoVideo
            return tabType == TabType.GeoVideo && primary == TabType.GeoVideo;
        }
    }
}
