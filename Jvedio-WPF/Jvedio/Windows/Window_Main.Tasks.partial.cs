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
        // Download progress / task tabs
        private void onDownloading()
        {
            double progress = App.DownloadManager.Progress;
            if (progress is double.NaN)
                progress = 0;
            vieModel.DownLoadProgress = progress;
            if (progress < 100)
                vieModel.DownLoadVisibility = Visibility.Visible;
            else
                vieModel.DownLoadVisibility = Visibility.Hidden;

            // 任务栏进度条
            Dispatcher.Invoke(() => {
                if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported && TaskbarInstance != null) {
                    TaskbarInstance.SetProgressValue((int)progress, 100, this);
                    if (progress >= 100 || progress <= 0)
                        TaskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress, this);
                    else
                        TaskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal, this);
                }
            });
        }
        public void ShowDownloadPopup(object sender, MouseButtonEventArgs e)
        {
            vieModel.TabItemManager
                .Add(Entity.Common.TabType.GeoTask, LangManager.GetValueByKey("Download"), TaskType.Download);
        }

        public void ShowScreenShotTab(object sender, MouseButtonEventArgs e)
        {
            vieModel.TabItemManager
                .Add(Entity.Common.TabType.GeoTask, LangManager.GetValueByKey("ScreenShotTask"), TaskType.ScreenShot);
        }

        private void ShowMsgScanPopup(object sender, MouseButtonEventArgs e)
        {
            vieModel.TabItemManager
                .Add(TabType.GeoTask, LangManager.GetValueByKey("Scan"), TaskType.Scan);
        }
    }
}
