using Jvedio.AvalonEdit;
using Jvedio.Core.Library;
using Jvedio.Core.Enums;
using Jvedio.Core.Global;
using Jvedio.Core.Media;
using Jvedio.Core.Metadata;
using Jvedio.Core.Plugins.Crawler;
using Jvedio.Core.Scan;
using Jvedio.Core.Server;
using Jvedio.Core.UI;
using Jvedio.Core.UserControls;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using Jvedio.Upgrade;
using Jvedio.ViewModel;
using Jvedio.ViewModels;
using SuperControls.Style;
using SuperControls.Style.CSFile.Interfaces;
using SuperControls.Style.Plugin;
using SuperControls.Style.Windows;
using SuperUtils.Common;
using SuperUtils.CustomEventArgs;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.IO;
using SuperUtils.Systems;
using SuperUtils.WPF.VisualTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using static Jvedio.App;
using static Jvedio.Core.Global.UrlManager;
using static Jvedio.MapperManager;
using static Jvedio.Window_Settings;
using static SuperUtils.WPF.VisualTools.WindowHelper;

namespace Jvedio
{
    public partial class Main
    {
        // List refresh / visual helper
        public childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            if (obj == null)
                return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++) {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is childItem)
                    return (childItem)child;
                childItem childOfChild = FindVisualChild<childItem>(child);
                if (childOfChild != null)
                    return childOfChild;
            }

            return null;
        }
        public void RefreshData(long dataID)
        {
            List<VideoList> videoLists = vieModel.TabItemManager.GetAllVideoList();
            foreach (var item in videoLists) {
                item.RefreshData(dataID);
            }
        }

        public void RefreshImage(Video video)
        {
            List<VideoList> videoLists = vieModel.TabItemManager.GetAllVideoList();
            foreach (var item in videoLists) {
                item.RefreshImage(video);
            }
        }

        public void RefreshGrade(long dataID, float grade)
        {
            List<VideoList> videoLists = vieModel.TabItemManager.GetAllVideoList();
            foreach (var item in videoLists) {
                item.RefreshGrade(dataID, grade);
            }
        }

        private void HideBeginScanGrid(object sender, RoutedEventArgs e)
        {
            vieModel.ShowFirstRun = Visibility.Hidden;
        }


        private void SideBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (SideGridColumn.ActualWidth <= 100 && !AnimatingSideGrid) {
                SideGridColumn.Width = new GridLength(0);
                if (ButtonSideTop != null)
                    ButtonSideTop.Visibility = Visibility.Visible;
            } else {
                if (ButtonSideTop != null)
                    ButtonSideTop.Visibility = Visibility.Collapsed;
            }
        }


        public void ShowSameActor(long actorID)
        {
            vieModel.ShowSameActor(actorID);
        }
    }
}
