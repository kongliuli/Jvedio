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
        // Tab: Delete
        public async void DeleteIDs(ObservableCollection<Video> originSource, List<Video> to_delete, bool fromDetailWindow = true)
        {
            if (originSource == null || to_delete == null || to_delete.Count == 0 || originSource.Count == 0)
                return;
            if (!fromDetailWindow) {
                originSource.RemoveMany(to_delete);
                vieModel.VideoList.RemoveMany(to_delete);
            } else {
                // 影片只有单个
                Video video = to_delete[0];
                int idx = -1;
                for (int i = 0; i < originSource.Count; i++) {
                    if (originSource[i].DataID == video.DataID) {
                        idx = i;
                        break;
                    }
                }

                if (idx >= 0) {
                    originSource.RemoveAt(idx);
                    vieModel.VideoList.RemoveAt(idx);
                }
            }

            videoMapper.deleteVideoByIds(to_delete.Select(arg => arg.DataID.ToString()).ToList());

            // 关闭详情窗口
            if (!fromDetailWindow && GetWindowByName("Window_Details", App.Current.Windows) is Window window) {
                Window_Details windowDetails = (Window_Details)window;
                foreach (var item in to_delete) {
                    if (windowDetails.DataID == item.DataID) {
                        windowDetails.Close();
                        break;
                    }
                }
            }

            to_delete.Clear();
            VideoListCommands.NotifyMetadataRefreshed();

            await Task.Delay(200);
            vieModel.EditMode = false;
            vieModel.SelectedVideo.Clear();
            SetSelected();
            vieModel.Refresh();
        }


        private void AddDataAssociation(object sender, RoutedEventArgs e)
        {
            HandleMenuSelected(sender, 1);

            if (vieModel.SelectedVideo.Count == 0) {
                MessageNotify.Error("无选择的资源");
                return;
            }

            Window_SearchAsso window_SearchAsso = new Window_SearchAsso(vieModel.SelectedVideo);
            window_SearchAsso.Owner = Application.Current.MainWindow as Main;
            window_SearchAsso.OnDataRefresh += (dataid) => {
                vieModel.RefreshData(dataid);
            };
            window_SearchAsso.OnSelectData += () => {
                SetSelected();
            };
            window_SearchAsso.ShowDialog();

        }


        public void DeleteID(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Video> videos = HandleMenuSelected(sender);
            if (vieModel.EditMode &&
                new MsgBox(SuperControls.Style.LangManager.GetValueByKey("IsToDelete")).ShowDialog() == false)
                return;
            List<Video> temp = vieModel.SelectedVideo.ToList();
            DeleteIDs(videos, vieModel.SelectedVideo, false);
            VideoListCommands.NotifyVideosDeleted(temp);
        }

        public void DeleteID(List<Video> list, bool fromDetail)
        {
            DeleteIDs(vieModel.CurrentVideoList, list, fromDetail);
        }

        private async Task<(int, int)> AsyncDeleteFile()
        {
            //return (0, 0);
            int num = 0;
            int totalCount = vieModel.SelectedVideo.Count;
            await Task.Run(() => {
                vieModel.SelectedVideo.ForEach((Action<Video>)(arg => {
                    if (arg.SubSectionList?.Count > 0) {
                        totalCount += arg.SubSectionList.Count - 1;

                        // 分段视频
                        foreach (var path in arg.SubSectionList.Select(t => t.Value)) {
                            if (File.Exists(path)) {
                                try {
                                    FileSystem.DeleteFile(path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                                    num++;
                                } catch (Exception ex) {
                                    Logger.Error(ex);
                                }
                            }
                        }
                    } else {
                        if (File.Exists(arg.Path)) {
                            try {
                                FileSystem.DeleteFile(arg.Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                                num++;
                            } catch (Exception ex) {
                                Logger.Error(ex);
                            }
                        }
                    }
                }));
            });
            return (num, totalCount);
        }

        public async void DeleteFile(object sender, RoutedEventArgs e)
        {
            HandleMenuSelected(sender);
            if (vieModel.EditMode && new MsgBox(SuperControls.Style.LangManager.GetValueByKey("IsToDelete")).ShowDialog() == false) {
                return;
            }
            VideoListCommands.NotifyWaiting("删除中", true);
            (int num, int totalCount) = await AsyncDeleteFile();
            //await Task.Delay(2000);
            if (ConfigManager.Settings.DelInfoAfterDelFile) {
                List<Video> temp = vieModel.SelectedVideo.ToList();
                DeleteIDs(GetVideosByMenu(sender as MenuItem, 0), vieModel.SelectedVideo, false);
                VideoListCommands.NotifyVideosDeleted(temp);
            }

            MessageNotify.Info($"{SuperControls.Style.LangManager.GetValueByKey("Message_DeleteToRecycleBin")} {num}/{totalCount}");
            VideoListCommands.NotifyWaiting("", false);
            if (!vieModel.EditMode)
                vieModel.SelectedVideo.Clear();
        }


    }
}
