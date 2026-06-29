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
        // Tab: Tasks
        private void onDownloadSuccess(DownLoadTask task)
        {
            Dispatcher.Invoke(() => {
                lock (RefreshLock) {
                    vieModel.RefreshData(task.DataID);
                    // 更新图片存在
                    if (task.Success)
                        UpdateImageIndex(task.DataID, true, true);
                }
            });
        }

        private void onScreenShotCompleted(bool ok, long dataID)
        {
            Dispatcher.Invoke(() => {
                if (ok) {
                    LoadImageAfterScreenShort(dataID);
                }
            });
        }

        private void ResizingTimer_Tick(object sender, EventArgs e)
        {
            Resizing = false;
            ResizingTimer.Stop();
        }

        private void AutoGenScreenShot(ObservableCollection<Video> data)
        {
            for (int i = 0; i < data.Count; i++) {
                if (data[i].BigImage == MetaData.DefaultBigImage ||
                    data[i].BigImage == MetaData.DefaultSmallImage) {
                    // 检查有无截图
                    Video video = data[i];
                    string path = video.GetScreenShot();
                    if (Directory.Exists(path)) {
                        string[] array = FileHelper.TryScanDIr(path, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                        if (array.Length > 0) {
                            Video.SetImage(ref video, array[array.Length / 2]);
                            data[i].BigImage = null;
                            data[i].BigImage = video.ViewImage;
                        }
                    }
                }
            }
        }

        private void DownLoadSelectMovie(object sender, RoutedEventArgs e)
        {
            HandleMenuSelected(sender);
            DownLoadVideo(vieModel.SelectedVideo);
        }

        public void DownLoadVideo(List<Video> videoList)
        {
            foreach (Video video in videoList) {
                DownLoadTask.DownloadVideo(video);
            }
            App.DownloadManager.Start();
        }

        public void GenerateAllScreenShot(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(ConfigManager.FFmpegConfig.Path)) {
                MessageNotify.Error(SuperControls.Style.LangManager.GetValueByKey("Message_SetFFmpeg"));
                return;
            }

            List<Video> videos = VideoRepository.GetAllByDbId(ConfigManager.Main.CurrentDBId);
            if (videos == null || videos.Count == 0) {
                MessageNotify.Error("数目为空");
                return;
            }

            if (new MsgBox($"即将开始截图 {videos.Count} 个资源，是否继续？").ShowDialog() == false) {
                return;
            }

            foreach (Video video in videos) {
                ScreenShotTask.ScreenShotVideo(video);
            }
        }


        public void DownloadAllVideo(object sender, RoutedEventArgs e)
        {
            List<Video> videos = VideoRepository.GetAllByDbId(ConfigManager.Main.CurrentDBId);

            if (videos == null || videos.Count == 0) {
                MessageNotify.Error("数目为空");
                return;
            }

            if (new MsgBox($"即将开始同步 {videos.Count} 个资源，是否继续？").ShowDialog() == false) {
                return;
            }

            MessageCard.Warning(LangManager.GetValueByKey("CrawlAllWarning"));
            DownLoadVideo(videos);
        }



        private void LoadImageAfterScreenShort(long dataID)
        {
            if (dataID <= 0)
                return;
            for (int i = 0; i < vieModel.CurrentVideoList.Count; i++) {
                if (vieModel.CurrentVideoList[i] == null)
                    continue;
                try {
                    if (!dataID.Equals(vieModel.CurrentVideoList[i].DataID))
                        continue;
                    if (vieModel.CurrentVideoList[i].BigImage == MetaData.DefaultBigImage) {
                        // 检查有无截图
                        Video currentVideo = vieModel.CurrentVideoList[i];
                        string path = currentVideo.GetScreenShot();
                        if (Directory.Exists(path)) {
                            string[] array = FileHelper.TryScanDIr(path, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                            if (array.Length > 0) {
                                Video.SetImage(ref currentVideo, array[array.Length / 2]);
                                vieModel.CurrentVideoList[i].BigImage = null;
                                vieModel.CurrentVideoList[i].BigImage = currentVideo.ViewImage;
                                // 更新索引
                                UpdateImageIndex(currentVideo.DataID, false, true);
                            }
                        }
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                    continue;
                }
                break;
            }
        }


        public void GenerateGif(object sender, RoutedEventArgs e)
        {
            HandleMenuSelected(sender, 1);
            GenerateScreenShot(vieModel.SelectedVideo, true);
        }

        public void GenerateScreenShot(object sender, RoutedEventArgs e)
        {
            HandleMenuSelected(sender, 1);
            GenerateScreenShot(vieModel.SelectedVideo);
        }

        public void GenerateScreenShot(List<Video> Videos, bool gif = false)
        {
            if (!File.Exists(ConfigManager.FFmpegConfig.Path)) {
                MessageNotify.Error(SuperControls.Style.LangManager.GetValueByKey("Message_SetFFmpeg"));
                return;
            }

            foreach (Video video in Videos) {
                ScreenShotTask.ScreenShotVideo(video, gif);
            }
        }



    }
}
