using Jvedio.Core.Enums;
using Jvedio.Core.UserControls;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using Jvedio.ViewModel;
using SuperControls.Style;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.Time;
using System;
using static Jvedio.App;

namespace Jvedio.Core.UI
{
    internal static class SideNavigations
    {
        public static ISideNavigation ForDataType(DataType dataType)
        {
            if (dataType == DataType.Picture)
                return PictureSideNavigation.Instance;
            return VideoSideNavigation.Instance;
        }

        private static void AddRecentWatch(VieModel_Main vm, SelectWrapper<Video> wrapper, string tabTitle)
        {
            DateTime date1 = DateTime.Now.AddDays(-1 * VieModel_Main.RECENT_DAY);
            DateTime date2 = DateTime.Now;
            wrapper.Between("ViewDate", DateHelper.ToLocalDate(date1), DateHelper.ToLocalDate(date2));
            vm.TabItemManager.Add(TabType.GeoRecentPlay, tabTitle, wrapper);
        }

        internal sealed class VideoSideNavigation : ISideNavigation
        {
            public static VideoSideNavigation Instance { get; } = new VideoSideNavigation();

            public void Handle(VieModel_Main vm, object command)
            {
                if (vm == null || command == null)
                    return;
                string param = command.ToString();
                SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
                switch (param) {
                    case "All":
                        vm.TabItemManager.Add(TabType.GeoVideo, LangManager.GetValueByKey("AllVideo"), wrapper);
                        break;
                    case "Favorite":
                        wrapper.Gt("metadata.Grade", 0);
                        vm.TabItemManager.Add(TabType.GeoStar, LangManager.GetValueByKey("Favorites"), wrapper);
                        break;
                    case "RecentWatch":
                        AddRecentWatch(vm, wrapper, LangManager.GetValueByKey("RecentPlay"));
                        break;
                    case "Label":
                        vm.TabItemManager.Add(TabType.GeoLabel, LangManager.GetValueByKey("Label"), LabelType.LabelName, vm.SearchText);
                        break;
                    case "Genre":
                    case "Studio":
                    case "Director":
                    case "Series":
                        if (Enum.TryParse(param, out LabelType type))
                            vm.TabItemManager.Add(TabType.GeoLabel, LangManager.GetValueByKey(param), type, vm.SearchText);
                        break;
                    case "Actor":
                        vm.TabItemManager.Add(TabType.GeoActor, LangManager.GetValueByKey(param), null, vm.SearchText);
                        break;
                }
            }
        }

        internal sealed class PictureSideNavigation : ISideNavigation
        {
            public static PictureSideNavigation Instance { get; } = new PictureSideNavigation();

            public void Handle(VieModel_Main vm, object command)
            {
                if (vm == null || command == null)
                    return;
                string param = command.ToString();
                SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
                switch (param) {
                    case "All":
                        PictureBrowseContext.ClearFolder();
                        vm.TabItemManager.Add(TabType.GeoPicture, LangManager.GetValueByKey("AllPicture"), wrapper);
                        break;
                    case "Favorite":
                        PictureBrowseContext.ClearFolder();
                        wrapper.Gt("metadata.Grade", 0);
                        vm.TabItemManager.Add(TabType.GeoStar, LangManager.GetValueByKey("Favorites"), wrapper);
                        break;
                    case "RecentWatch":
                        PictureBrowseContext.ClearFolder();
                        AddRecentWatch(vm, wrapper, LangManager.GetValueByKey("RecentView"));
                        break;
                    case "Label":
                        PictureBrowseContext.ClearFolder();
                        vm.TabItemManager.Add(TabType.GeoLabel, LangManager.GetValueByKey("Label"), LabelType.LabelName, vm.SearchText);
                        break;
                    case "Genre":
                    case "Series":
                        PictureBrowseContext.ClearFolder();
                        if (Enum.TryParse(param, out LabelType type))
                            vm.TabItemManager.Add(TabType.GeoLabel, LangManager.GetValueByKey(param), type, vm.SearchText);
                        break;
                    default:
                        if (PictureBrowseContext.TryParseFolderCommand(param, out string folderPath)) {
                            PictureBrowseContext.SetFolder(folderPath);
                            vm.TabItemManager.RefreshPrimaryPictureLists();
                        }
                        break;
                }
            }
        }
    }
}
