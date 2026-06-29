using Google.Protobuf.WellKnownTypes;
using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.Enums;
using Jvedio.Core.FFmpeg;
using Jvedio.Core.Global;
using Jvedio.Core.Library;
using Jvedio.Core.Media;
using Jvedio.Core.Metadata;
using Jvedio.Core.UI;
using Jvedio.Core.Net;
using Jvedio.Core.UserControls.ViewModels;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using Jvedio.Entity.CommonSQL;
using Microsoft.VisualBasic.FileIO;
using SuperControls.Style;
using SuperControls.Style.Windows;
using SuperUtils.Common;
using SuperUtils.Framework.ORM.Utils;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.Framework.Tasks;
using SuperUtils.IO;
using SuperUtils.Time;
using SuperUtils.WPF.VisualTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static Jvedio.App;
using static Jvedio.Core.UserControls.VideoItemEventArgs;
using static Jvedio.MapperManager;
using static SuperUtils.WPF.VisualTools.VisualHelper;
using static SuperUtils.WPF.VisualTools.WindowHelper;

namespace Jvedio.Core.UserControls
{
    public partial class VideoList
    {
        // Tab: ContextMenu
        private void OpenWeb(object sender, RoutedEventArgs e)
        {
            HandleMenuSelected(sender);

            // 超过 3 个网页，询问是否继续
            if (vieModel.SelectedVideo.Count >= 3 && new MsgBox(
                $"{LangManager.GetValueByKey("ReadyToOpenReadyToOpen")} {vieModel.SelectedVideo.Count} {LangManager.GetValueByKey("SomeWebSite")}").ShowDialog() == false)
                return;

            foreach (Video video in vieModel.SelectedVideo) {
                video.OpenWeb();
            }
        }

        private long GetIDFromMenuItem(object sender, int depth = 0)
        {
            MenuItem mnu = sender as MenuItem;
            ContextMenu contextMenu = null;
            if (depth == 0) {
                contextMenu = mnu.Parent as ContextMenu;
            } else {
                MenuItem _mnu = mnu.Parent as MenuItem;
                contextMenu = _mnu.Parent as ContextMenu;
            }

            FrameworkElement ele = contextMenu.PlacementTarget as FrameworkElement;
            if (ele.Tag is Video video)
                return video.DataID;
            return -1;
        }

        public void ContextMenu_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            ContextMenu contextMenu = sender as ContextMenu;
            if (e.Key == Key.D) {
                MenuItem menuItem = GetMenuItem(contextMenu, SuperControls.Style.LangManager.GetValueByKey("Menu_DeleteInfo"));
                if (menuItem != null)
                    DeleteID(menuItem, new RoutedEventArgs());
            } else if (e.Key == Key.T) {
                MenuItem menuItem = GetMenuItem(contextMenu, SuperControls.Style.LangManager.GetValueByKey("Menu_DeleteFile"));
                if (menuItem != null)
                    DeleteFile(menuItem, new RoutedEventArgs());
            } else if (e.Key == Key.S) {
                MenuItem menuItem = GetMenuItem(contextMenu, SuperControls.Style.LangManager.GetValueByKey("Menu_SyncInfo"));
                if (menuItem != null)
                    DownLoadSelectMovie(menuItem, new RoutedEventArgs());
            } else if (e.Key == Key.E) {
                MenuItem menuItem = GetMenuItem(contextMenu, SuperControls.Style.LangManager.GetValueByKey("Menu_EditInfo"));
                if (menuItem != null)
                    EditInfo(menuItem, new RoutedEventArgs());
            } else if (e.Key == Key.W) {
                MenuItem menuItem = GetMenuItem(contextMenu, SuperControls.Style.LangManager.GetValueByKey("Menu_OpenWebSite"));
                if (menuItem != null)
                    OpenWeb(menuItem, new RoutedEventArgs());
            } else if (e.Key == Key.C) {
                MenuItem menuItem = GetMenuItem(contextMenu, SuperControls.Style.LangManager.GetValueByKey("Menu_CopyFile"));
                if (menuItem != null)
                    CopyFile(menuItem, new RoutedEventArgs());
            } else if (e.Key == Key.X) {
                MenuItem menuItem = GetMenuItem(contextMenu, SuperControls.Style.LangManager.GetValueByKey("Menu_CopyFile"));
                if (menuItem != null)
                    CutFile(menuItem, new RoutedEventArgs());
            }

            contextMenu.IsOpen = false;
        }


        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (vieModel.Rendering) {
                e.Handled = true;
                return;
            }

            // 标记
            FrameworkElement element = sender as FrameworkElement;
            if (element == null || element.Tag == null)
                return;
            Video video = element.Tag as Video;
            if (video == null)
                return;

            long dataID = video.DataID;

            if (dataID <= 0)
                return;

            ContextMenu contextMenu = element.ContextMenu;
            if (contextMenu == null)
                return;

            ItemsControl itemsControl = VisualHelper.FindParentOfType<ItemsControl>(element);
            if (itemsControl == null)
                return;

            ObservableCollection<Video> videos = itemsControl.ItemsSource as ObservableCollection<Video>;
            if (videos == null)
                return;

            List<string> tagIDs = new List<string>();
            if (!string.IsNullOrEmpty(video.TagIDs))
                tagIDs = video.TagIDs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (FrameworkElement item in contextMenu.Items) {
                if ("TagMenuItems".Equals(item.Name) && item is MenuItem menuItem) {
                    menuItem.Items.Clear();
                    TagStamp.TagStamps.ForEach(arg => {
                        string tagID = arg.TagID.ToString();
                        MenuItem menu = new MenuItem() {
                            Header = arg.TagName,
                            IsCheckable = true,
                            IsChecked = tagIDs.Contains(tagID),
                        };
                        menu.Click += (s, ev) => {
                            long TagID = arg.TagID;
                            AddTagHandler(menu, TagID);
                        };
                        menuItem.Items.Add(menu);
                    });
                }
            }

            if (ListMode == MediaListMode.Picture)
                RefreshPictureCollectionMenus(contextMenu, video);
        }

        private void OpenPath(object sender, RoutedEventArgs e)
        {
            MenuItem menu = sender as MenuItem;
            if (menu == null)
                return;

            ObservableCollection<Video> datas = GetVideosByMenu(menu, 1);
            if (datas == null)
                return;


            string header = menu.Header.ToString();

            OpenPathType openPathType = Video.StringToImageType(header);

            long dataID = GetIDFromMenuItem(sender, 1);
            if (dataID <= 0)
                return;
            Video video = datas.Where(arg => arg.DataID == dataID).FirstOrDefault();
            video?.OpenPath(openPathType);
        }


        private void AddToPlayerList(object sender, RoutedEventArgs e)
        {
            string playerPath = ConfigManager.Settings.VideoPlayerPath;
            bool success = false;

            if (!File.Exists(playerPath)) {
                MessageNotify.Error(LangManager.GetValueByKey("VideoPlayerPathNotSet"));
                return;
            }

            HandleMenuSelected(sender);
            if (Path.GetFileName(playerPath).ToLower().Equals("PotPlayerMini64.exe".ToLower())) {
                List<string> list = vieModel.SelectedVideo
                    .Where(arg => File.Exists(arg.Path)).Select(arg => arg.Path).ToList();
                if (list.Count > 0) {
                    // potplayer 添加清单
                    string processParameters = $"\"{playerPath}\" \"{string.Join("\" \"", list)}\" /add";
                    using (Process process = new Process()) {
                        process.StartInfo.FileName = "cmd.exe";

                        // process.StartInfo.Arguments = arguments;
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.RedirectStandardInput = true; // 接受来自调用程序的输入信息
                        process.Start();
                        process.StandardInput.WriteLine(processParameters);
                        process.StandardInput.AutoFlush = true;
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        // if (process.ExitCode != 0)
                        //    MessageCard.Error("添加失败");
                    }
                }

                success = true;
            }

            if (!success)
                MessageNotify.Error(LangManager.GetValueByKey("SupportPotPlayerOnly"));
        }

        private void onItemShowDetail(object sender, RoutedEventArgs e)
        {
            FrameworkElement ele = sender as FrameworkElement;
            if (ele != null && ele.Tag != null && ele.Tag is Video video) {
                if (vieModel.EditMode) {
                    if (vieModel.CurrentVideoList == null)
                        return;
                    // 多选
                    int selectIdx = vieModel.CurrentVideoList.IndexOf(video);

                    // 多选
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) {
                        if (firstIdx == -1)
                            firstIdx = selectIdx;
                        else
                            secondIdx = selectIdx;
                    }

                    if (firstIdx >= 0 && secondIdx >= 0) {
                        if (firstIdx > secondIdx) {
                            // 交换一下顺序
                            int temp = firstIdx;
                            firstIdx = secondIdx - 1;
                            secondIdx = temp - 1;
                        }

                        for (int i = firstIdx + 1; i <= secondIdx; i++) {
                            Video m = vieModel.CurrentVideoList[i];
                            if (vieModel.SelectedVideo.Contains(m))
                                vieModel.SelectedVideo.Remove(m);
                            else
                                vieModel.SelectedVideo.Add(m);
                        }

                        firstIdx = -1;
                        secondIdx = -1;
                    } else {
                        if (vieModel.SelectedVideo.Contains(video))
                            vieModel.SelectedVideo.Remove(video);
                        else
                            vieModel.SelectedVideo.Add(video);
                    }

                    SetSelected();
                } else {
                    RaiseEvent(new VideoItemEventArgs(video.DataID, OnItemClickEvent, sender));
                }
            }
        }

        private void viewVideo_ImageMouseEnter(object sender, RoutedEventArgs e)
        {
            if (vieModel.EditMode && sender is ViewVideo viewVideo) {
                viewVideo.SetBorderBrush(StyleManager.Common.HighLight.BorderBrush);
            }
        }

        private void viewVideo_ImageMouseLeave(object sender, RoutedEventArgs e)
        {
            if (vieModel.EditMode &&
                sender is ViewVideo viewVideo &&
                GetDataID(viewVideo) is long dataID && dataID > 0) {
                if (vieModel.SelectedVideo.Where(arg => arg.DataID == dataID).Any()) {
                    viewVideo.SetBorderBrush(StyleManager.Common.HighLight.BorderBrush);
                } else {
                    viewVideo.SetBorderBrush(Brushes.Transparent);
                }
            }
        }

        private void viewVideo_OnPlayVideo(object sender, RoutedEventArgs e)
        {
            if (!vieModel.EditMode &&
                sender is ViewVideo viewVideo &&
                GetDataID(viewVideo) is long dataId && dataId > 0) {

                Video video = GetVideoFromChildEle(viewVideo, dataId);
                PlayVideo(video);
            }
        }

        private void PlayVideo(Video video)
        {
            if (video == null) {
                MessageNotify.Error(LangManager.GetValueByKey("CanNotPlay"));
                return;
            }
            long dataId = video.DataID;
            string sql = $"delete from metadata_to_tagstamp where TagID='{TagStamp.TAG_ID_NEW_ADD}' and DataID='{dataId}'";
            tagStampMapper.ExecuteNonQuery(sql);
            onInitTagStamps?.Invoke();
            vieModel.RefreshData(dataId);
            Video.PlayVideoWithPlayer(video.Path, dataId);
        }

        private void OnPlayVideo(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null &&
                long.TryParse(button.Tag.ToString(), out long dataID) &&
                dataID > 0) {
                Video video = videoMapper.SelectVideoByID(dataID);
                PlayVideo(video);
            }
        }

        private void DownLoadWithUrl(object sender, RoutedEventArgs e)
        {

        }

        private void TranslateMovie(object sender, RoutedEventArgs e)
        {

        }

        private void GenerateSmallImage(object sender, RoutedEventArgs e)
        {

        }

        private void GenerateActor(object sender, RoutedEventArgs e)
        {

        }

        private void FilterClose()
        {
            vieModel.ShowFilter = false;
        }

        private void Filter_OnApplyWrapper(object sender, EventArgs ev)
        {
            if (ev is WrapperEventArg<Video> e &&
                e.Wrapper != null &&
                e.Wrapper is SelectWrapper<Video> wrapper) {
                vieModel.FilterWrapper = wrapper;
                vieModel.FilterSQL = e.SQL;
                vieModel.LoadData();
            }
        }


        private void viewVideo_OnTagStampRemove(object sender, RoutedEventArgs e)
        {
            this.filter.InitTagStamp();
        }

        private void viewVideo_OnViewAssoData(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement ele &&
                ele.Tag is Video video)
                RaiseEvent(new VideoItemEventArgs(video.DataID, OnItemViewAssoEvent, sender));
        }


    }
}
