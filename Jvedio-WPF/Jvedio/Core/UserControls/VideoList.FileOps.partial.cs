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
        // Tab: FileOps
        public void RenameFile(object sender, RoutedEventArgs e)
        {
            if (ConfigManager.RenameConfig.FormatString.IndexOf("{") < 0) {
                MessageNotify.Error(SuperControls.Style.LangManager.GetValueByKey("Message_SetRenameRule"));
                return;
            }

            HandleMenuSelected(sender, 1);

            ObservableCollection<Video> videos = GetVideosByMenu(sender as MenuItem, 1);
            if (videos == null)
                return;

            List<string> logs = new List<string>();
            TaskLogger logger = new TaskLogger(logs);
            List<Video> toRename = new List<Video>();
            foreach (Video video in vieModel.SelectedVideo) {
                if (File.Exists(video.Path)) {
                    toRename.Add(video);
                } else {
                    logger.Error(SuperControls.Style.LangManager.GetValueByKey("Message_FileNotExist") + $" => {video.Path}");
                }
            }

            int totalCount = toRename.Count;

            Dictionary<long, List<string>> dict = new Dictionary<long, List<string>>();

            // 重命名文件
            int successCount = RenameFile(toRename, logger, ref dict);

            // 更新
            if (dict.Count > 0) {

                UpdateVideo(dict, ref videos);
                MessageNotify.Success($"{SuperControls.Style.LangManager.GetValueByKey("Message_SuccessNum")} {successCount}/{totalCount} ");
            } else {
                MessageNotify.Info(LangManager.GetValueByKey("NoFileToRename"));
            }

            if (!vieModel.EditMode)
                vieModel.SelectedVideo.Clear();

            if (logs.Count > 0)
                //onRenameFile?.Invoke(string.Join(Environment.NewLine, logs));
                new Dialog_Logs(string.Join(Environment.NewLine, logs)).ShowDialog(App.Current.MainWindow);
        }

        public int RenameFile(List<Video> toRename, TaskLogger logger, ref Dictionary<long, List<string>> dict)
        {
            int successCount = 0;
            foreach (Video video in toRename) {
                long dataID = video.DataID;
                Video newVideo = videoMapper.SelectVideoByID(dataID);
                string[] newPath = null;
                try {
                    newPath = newVideo.ToFileName();
                } catch (Exception ex) {
                    logger.Error(ex.Message);
                    continue;
                }

                if (newPath == null || newPath.Length == 0)
                    continue;

                if (newVideo.HasSubSection) {
                    bool success = false;
                    bool changed = false;
                    string[] oldPaths = newVideo.SubSectionList.Select(arg => arg.Value).ToArray();

                    // 判断是否改变了文件名
                    for (int i = 0; i < newPath.Length; i++) {
                        if (!newPath[i].Equals(oldPaths[i])) {
                            changed = true;
                            break;
                        }
                    }

                    if (!changed) {
                        //logger.Info(LangManager.GetValueByKey("SameFileNameToOrigin"));
                        break;
                    }

                    for (int i = 0; i < newPath.Length; i++) {
                        if (File.Exists(newPath[i])) {
                            logger.Error($"{LangManager.GetValueByKey("SameFileNameExists")} => {newPath[i]}");
                            newPath[i] = oldPaths[i]; // 换回原来的
                            continue;
                        }

                        try {
                            File.Move(video.SubSectionList[i].ToString(), newPath[i]);
                            success = true;
                        } catch (Exception ex) {
                            logger.Error(ex.Message);
                            newPath[i] = oldPaths[i]; // 换回原来的
                            continue;
                        }
                    }

                    if (success)
                        successCount++;
                    if (!dict.ContainsKey(dataID))
                        dict.Add(dataID, newPath.ToList());
                } else {
                    string target = newPath[0];
                    string origin = newVideo.Path;
                    if (origin.Equals(target)) {
                        //logger.Info(LangManager.GetValueByKey("SameFileNameToOrigin") +
                        //    $"{Environment.NewLine}    origin: {origin}{Environment.NewLine}    target: {target}");
                        continue;
                    }

                    if (!File.Exists(target)) {
                        try {
                            File.Move(origin, target);
                            successCount++;
                        } catch (Exception ex) {
                            logger.Error(ex.Message);
                            continue;
                        }

                        // 显示
                        if (!dict.ContainsKey(dataID))
                            dict.Add(dataID, new List<string>() { target });
                    } else {
                        logger.Error($"{LangManager.GetValueByKey("SameFileNameExists")} => {target}");
                    }
                }
            }
            return successCount;
        }


        public void UpdateVideo(Dictionary<long, List<string>> dict, ref ObservableCollection<Video> videos)
        {
            if (videos == null || videos.Count == 0)
                return;
            for (int i = 0; i < videos.Count; i++) {
                Video video = videos[i];
                long dataID = video.DataID;
                if (dict.ContainsKey(dataID)) {
                    if (video.HasSubSection) {
                        List<string> list = dict[dataID];
                        string subSection = string.Join(SuperUtils.Values.ConstValues.SeparatorString, list);
                        videos[i].Path = list[0];
                        videos[i].SubSection = subSection;
                        metaDataMapper.UpdateFieldById("Path", list[0], dataID);
                        videoMapper.UpdateFieldById("SubSection", subSection, dataID);
                    } else {
                        string path = dict[dataID][0];
                        videos[i].Path = path;
                        metaDataMapper.UpdateFieldById("Path", path, dataID);
                    }
                }
            }


        }


        public static void UpdateImageIndex(long dataID, bool smallImageExists = false, bool bigImageExists = false)
        {
            long pathType = ConfigManager.Settings.PicPathMode;
            List<string> list = new List<string>();
            // 小图
            list.Add($"({dataID},{pathType},0,{(smallImageExists ? 1 : 0)})");
            // 大图
            list.Add($"({dataID},{pathType},1,{(bigImageExists ? 1 : 0)})");
            string insertSql = $"begin;insert or replace into common_picture_exist(DataID,PathType,ImageType,Exist) values {string.Join(",", list)};commit;";
            MapperManager.videoMapper.ExecuteNonQuery(insertSql);
        }

        public void ReMoveZero(object sender, RoutedEventArgs e)
        {
            HandleMenuSelected(sender, 1);

            ObservableCollection<Video> videos = GetVideosByMenu(sender as MenuItem, 1);
            if (videos == null)
                return;

            int successNum = 0;
            for (int i = 0; i < vieModel.SelectedVideo.Count; i++) {
                Video video = vieModel.SelectedVideo[i];
                string oldVID = video.VID.ToUpper();

                Logger.Info($"remove vid zero, old vid: {oldVID}");

                if (oldVID.IndexOf("-") <= 0) {
                    Logger.Warn($"vid[{oldVID}] not contain '-'");
                    continue;
                }

                string num = oldVID.Split('-').Last();
                string eng = oldVID.Remove(oldVID.Length - num.Length, num.Length);
                if (num.StartsWith("00")) {
                    string newVID = eng + num.Remove(0, 2);
                    video.VID = newVID;
                    Logger.Info($"update vid from {oldVID} to {newVID}");
                    if (videoMapper.UpdateFieldById("VID", newVID, video.DataID)) {
                        successNum++;
                        vieModel.RefreshData(video.DataID);
                    }
                } else {
                    Logger.Warn($"{num} not starts with 00");
                }

            }

            MessageCard.Info($"{SuperControls.Style.LangManager.GetValueByKey("Message_Success")} {successNum}/{vieModel.SelectedVideo.Count}");

            if (!vieModel.EditMode)
                vieModel.SelectedVideo.Clear();
        }

        public void CopyFile(object sender, RoutedEventArgs e)
        {
            HandleMenuSelected(sender);
            StringCollection paths = new StringCollection();
            int count = 0;
            int total = 0;
            foreach (var video in vieModel.SelectedVideo) {
                if (video == null)
                    continue;
                if (video.SubSectionList != null && video.SubSectionList.Count > 0) {
                    total += video.SubSectionList.Count;
                    foreach (var path in video.SubSectionList.Select(arg => arg.Value)) {
                        if (File.Exists(path)) {
                            paths.Add(path);
                            count++;
                        }
                    }
                } else {
                    total++;
                    if (File.Exists(video.Path)) {
                        paths.Add(video.Path);
                        count++;
                    }
                }
            }

            if (paths.Count <= 0) {
                MessageNotify.Warning(LangManager.GetValueByKey("CopyFileNameNull"));
                return;
            }

            bool success = ClipBoard.TrySetFileDropList(paths, (error) => { MessageCard.Error(error); });

            if (success)
                MessageNotify.Success($"{SuperControls.Style.LangManager.GetValueByKey("Message_Copied")} {count}/{total}");

            if (!vieModel.EditMode)
                vieModel.SelectedVideo.Clear();
        }

        public void CutFile(object sender, RoutedEventArgs e)
        {
            HandleMenuSelected(sender);
            StringCollection paths = new StringCollection();
            int count = 0;
            int total = 0;
            foreach (var video in vieModel.SelectedVideo) {
                if (video.SubSectionList?.Count > 0) {
                    total += video.SubSectionList.Count;
                    foreach (var path in video.SubSectionList.Select(arg => arg.Value)) {
                        if (File.Exists(path)) {
                            paths.Add(path);
                            count++;
                        }
                    }
                } else {
                    total++;
                    if (File.Exists(video.Path)) {
                        paths.Add(video.Path);
                        count++;
                    }
                }
            }

            if (paths.Count <= 0) {
                MessageNotify.Warning(LangManager.GetValueByKey("CutFileNameNull"));
                return;
            }

            bool success = ClipBoard.TryCutFileDropList(paths, (error) => { MessageCard.Error(error); });

            if (success)
                MessageNotify.Success($"{LangManager.GetValueByKey("Cut")} {count}/{total}");

            if (!vieModel.EditMode)
                vieModel.SelectedVideo.Clear();
        }


    }
}
