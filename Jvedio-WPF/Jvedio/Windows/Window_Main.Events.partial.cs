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
        // Binding / callbacks / tag stamp
        private void BindingEventAfterRender()
        {
            SetComboboxID();
            App.DownloadManager.onRunning += onDownloading;
            App.DownloadManager.onLongDelay += onLoadDelay;
            Main.OnRecvWinMsg += onRecvWinMsg;
            LibraryEventBus.MetadataRefreshed += (s, e) => vieModel.Statistic();
            LibraryEventBus.VideosDeleted += (s, e) => {
                if (e.Videos != null && e.Videos.Count > 0)
                    DeleteID(new List<Video>(e.Videos), false);
            };
            LibraryEventBus.ActorInfoChanged += (s, e) => onActorInfoChanged(e.ActorId);
            LibraryEventBus.TabFocusChanged += (s, e) => OnFocusItem(e.TabItem);
        }

        private void OnFocusItem(TabItemEx tabItem)
        {
            var container = tabItemsControl.ItemContainerGenerator.ContainerFromItem(tabItem) as FrameworkElement;
            if (container != null)
                container.BringIntoView();
        }

        private void onRecvWinMsg(string str)
        {
            Logger.Info($"recv win msg: {str}");
            switch (str) {
                case Win32Helper.WIN_CUSTOM_MSG_OPEN_WINDOW:
                    ShowMainWindow(null, null);
                    break;
                default:
                    break;
            }
        }

        private void onActorInfoChanged(long id)
        {
            vieModel.TabItemManager.GetAllActorList().ForEach(arg => {
                arg.RefreshActor(id);
            });

            SelectWrapper<ActorInfo> wrapper = new SelectWrapper<ActorInfo>();
            wrapper.Eq("ActorID", id);
            ActorInfo actorInfo = actorMapper.SelectById(wrapper);
            if (actorInfo != null) {
                ActorInfo.SetImage(ref actorInfo);

                vieModel.TabItemManager.GetAllVideoList().ForEach(arg => {
                    if (arg.actorInfoView.CurrentActorInfo != null) {
                        arg.actorInfoView.CurrentActorInfo = null;
                        arg.actorInfoView.CurrentActorInfo = actorInfo;
                        arg.actorInfoView.CurrentActorInfo.SmallImage = actorInfo.SmallImage;
                    }

                });
            }
        }

        private void onLoadDelay(object sender, EventArgs e)
        {
            string message = (e as MessageCallBackEventArgs).Message;
            int.TryParse(message, out int value);
        }
        /// <summary>
        /// 绑定事件
        /// </summary>
        private void BindingEvent()
        {
            // 初始化任务栏的进度条
            if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported)
                TaskbarInstance = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;

            this.OnSideTrigger += onSideTrigger;
            LibraryEventBus.TagStampDeleted += (s, e) => onTagStampDelete(e.TagId);
            LibraryEventBus.TagStampFilterRefresh += (s, e) => RefreshTagStamp(e.TagId);
        }


        /// <summary>
        /// 侧边栏动画
        /// </summary>
        private async void onSideTrigger()
        {
            AnimatingSideGrid = true;
            ButtonSideTop.Visibility = Visibility.Collapsed;
            await Task.Run(async () => {
                for (int i = 0; i <= 200; i += 10) {
                    await App.Current.Dispatcher.InvokeAsync(() => {
                        SideGridColumn.Width = new GridLength(i);
                    });
                    await Task.Delay(5);
                }
            });
            AnimatingSideGrid = false;
        }

        public void RefreshTagStamp(long id)
        {
            List<VideoList> videoLists = vieModel.TabItemManager.GetAllVideoList();
            if (videoLists != null) {
                foreach (VideoList video in videoLists) {
                    video.RefreshTagStamps(id);
                }
            }
        }


        public void DeleteID(List<Video> list, bool fromDetail)
        {
            List<VideoList> videoLists = vieModel.TabItemManager.GetAllVideoList();
            if (videoLists != null) {
                foreach (VideoList video in videoLists) {
                    video.DeleteID(list.ToList(), fromDetail);
                }
            }
        }

        private void onTagStampDelete(long id)
        {
            // 删除主窗体所有标签戳
            VideoList videoList = vieModel.TabItemManager.GetPrimaryVideoList();
            videoList?.RefreshTagStamps(id);

            // 删除详情窗口的标签戳
            Window window = GetWindowByName("Window_Details", App.Current.Windows);
            if (window != null && window is Window_Details window_Details)
                window_Details?.RemoveTag(id);
        }
    }
}
