using Jvedio.Core.Config;
using Jvedio.Core.Crawler;
using Jvedio.Core.Enums;
using Jvedio.Core.Library;
using Jvedio.Core.Scan;
using Jvedio.Core.Scan.Discovery;
using Jvedio.Core.Tasks;
using SuperUtils.CustomEventArgs;
using Jvedio.Core.UI;
using Jvedio.Core.Global;
using Jvedio.Core.Media;
using Jvedio.Core.Plugins.Crawler;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using Jvedio.Mapper;
using Jvedio.ViewModel;
using Newtonsoft.Json;
using SuperControls.Style;
using SuperControls.Style.Plugin;
using SuperControls.Style.Windows;
using SuperUtils.Common;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.IO;
using SuperUtils.NetWork;
using SuperUtils.NetWork.Entity;
using SuperUtils.Systems;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using static Jvedio.App;
using static Jvedio.Core.Global.UrlManager;
using static SuperUtils.WPF.VisualTools.WindowHelper;

namespace Jvedio
{
    public partial class Window_Settings
    {
        // Tab: Library
        public void AddPath(object sender, RoutedEventArgs e)
        {
            var path = FileHelper.SelectPath(this);
            if (Directory.Exists(path)) {
                if (vieModel.ScanPath == null)
                    vieModel.ScanPath = new ObservableCollection<string>();
                if (!vieModel.ScanPath.Contains(path) && !vieModel.ScanPath.IsIntersectWith(path))
                    vieModel.ScanPath.Add(path);
                else
                    MessageCard.Error(SuperControls.Style.LangManager.GetValueByKey("FilePathIntersection"));
            }
        }

        public void DelPath(object sender, RoutedEventArgs e)
        {
            if (PathListBox.SelectedIndex >= 0) {
                for (int i = PathListBox.SelectedItems.Count - 1; i >= 0; i--) {
                    vieModel.ScanPath.Remove(PathListBox.SelectedItems[i].ToString());
                }
            }
        }

        public void ClearPath(object sender, RoutedEventArgs e)
        {
            vieModel.ScanPath?.Clear();
        }

        #region "文件监听"


        private FileSystemWatcher[] watchers { get; set; }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public bool TestListen()
        {
            string[] drives = Environment.GetLogicalDrives();
            watchers = new FileSystemWatcher[drives.Count()];
            for (int i = 0; i < drives.Count(); i++) {
                try {
                    if (drives[i] == @"C:\") {
                        continue;
                    }

                    FileSystemWatcher watcher = new FileSystemWatcher();
                    watcher.Path = drives[i];
                    watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                    watcher.Filter = "*.*";
                    watcher.EnableRaisingEvents = true;
                    watchers[i] = watcher;
                    watcher.Dispose();
                } catch {
                    SuperControls.Style.MessageNotify.Error($"{SuperControls.Style.LangManager.GetValueByKey("NoPermissionToListen")} {drives[i]}");
                    return false;
                }
            }

            return true;
        }
        #endregion

        private void InitScanDatabases()
        {
            List<AppDatabase> appDatabases = MainWindow?.vieModel.DataBases.ToList();
            AppDatabase db = MainWindow?.vieModel.CurrentAppDataBase;
            if (appDatabases != null) {
                DatabaseComboBox.ItemsSource = appDatabases;
                for (int i = 0; i < appDatabases.Count; i++) {
                    if (appDatabases[i].Equals(db)) {
                        DatabaseComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            AppDatabase db = e.AddedItems[0] as AppDatabase;
            vieModel.LoadScanPath(db);
        }

        private void PathListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true; // 必须加
        }

        private void PathListBox_Drop(object sender, DragEventArgs e)
        {
            if (vieModel.ScanPath == null)
                vieModel.ScanPath = new ObservableCollection<string>();
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var item in dragdropFiles) {
                if (!FileHelper.IsFile(item)) {
                    if (!vieModel.ScanPath.Contains(item) && !vieModel.ScanPath.IsIntersectWith(item))
                        vieModel.ScanPath.Add(item);
                    else
                        MessageCard.Error(SuperControls.Style.LangManager.GetValueByKey("FilePathIntersection"));
                }
            }
        }

        private async void CreatePlayableIndex(object sender, RoutedEventArgs e)
        {
            vieModel.IndexCreating = true;
            IndexCanceled = false;
            long total = 0;
            bool result = await Task.Run(() => {
                List<MetaData> metaDatas = MapperManager.metaDataMapper.SelectList();
                total = metaDatas.Count;
                if (total <= 0)
                    return false;
                StringBuilder builder = new StringBuilder();
                List<string> list = new List<string>();
                for (int i = 0; i < total; i++) {
                    MetaData metaData = metaDatas[i];
                    if (!File.Exists(metaData.Path))
                        builder.Append($"update metadata set PathExist=0 where DataID='{metaData.DataID}';");
                    if (IndexCanceled)
                        return false;
                    App.Current.Dispatcher.Invoke(() => {
                        indexCreatingProgressBar.Value = Math.Round(((double)i + 1) / total * 100, 2);
                    });
                }

                string sql = $"begin;update metadata set PathExist=1;{builder};commit;"; // 因为大多数资源都是存在的，默认先设为1
                MapperManager.videoMapper.ExecuteNonQuery(sql);
                return true;
            });
            ConfigManager.Settings.PlayableIndexCreated = true;
            vieModel.IndexCreating = false;
            if (result)
                MessageCard.Success($"{LangManager.GetValueByKey("CreateSuccess")} {total} {LangManager.GetValueByKey("DataIndex")}");
        }

        private async void CreatePictureIndex(object sender, RoutedEventArgs e)
        {
            if (new MsgBox($"{LangManager.GetValueByKey("CurrentImageType")} {((PathType)ConfigManager.Settings.PicPathMode).ToString()}，{LangManager.GetValueByKey("TakeEffectToCurrent")}")
                .ShowDialog() == false) {
                return;
            }

            vieModel.IndexCreating = true;
            IndexCanceled = false;
            long total = 0;
            bool result = await Task.Run(() => {
                string sql = VideoMapper.SQL_BASE;
                IWrapper<Video> wrapper = new SelectWrapper<Video>();
                wrapper.Select("metadata.DataID", "Path", "VID", "Hash");
                sql = wrapper.ToSelect(false) + sql;
                List<Dictionary<string, object>> temp = MapperManager.metaDataMapper.Select(sql);
                List<Video> videos = MapperManager.metaDataMapper.ToEntity<Video>(temp, typeof(Video).GetProperties(), true);
                total = videos.Count;
                if (total <= 0)
                    return false;
                List<string> list = new List<string>();
                long pathType = ConfigManager.Settings.PicPathMode;
                for (int i = 0; i < total; i++) {
                    Video video = videos[i];

                    // 小图
                    list.Add($"({video.DataID},{pathType},0,{(File.Exists(video.GetSmallImage()) ? 1 : 0)})");

                    // 大图
                    list.Add($"({video.DataID},{pathType},1,{(File.Exists(video.GetBigImage()) ? 1 : 0)})");
                    if (IndexCanceled)
                        return false;

                    // todo 预览图的图片索引地址
                    //list.Add($"({video.DataID},{pathType},1,{(File.Exists(video.GetExtraImage()) ? 1 : 0)})");
                    //if (IndexCanceled)
                    //    return false;

                    // todo 影片截图的图片索引地址

                    App.Current.Dispatcher.Invoke(() => {
                        indexCreatingProgressBar.Value = Math.Round(((double)i + 1) / total * 100, 2);
                    });
                }

                string insertSql = $"begin;insert or replace into common_picture_exist(DataID,PathType,ImageType,Exist) values {string.Join(",", list)};commit;";
                MapperManager.videoMapper.ExecuteNonQuery(insertSql);
                return true;
            });
            if (result)
                MessageCard.Success($"{LangManager.GetValueByKey("CreateSuccess")} {total} {LangManager.GetValueByKey("DataIndex")}");
            ConfigManager.Settings.PictureIndexCreated = true;
            vieModel.IndexCreating = false;
        }

        private void CancelCreateIndex(object sender, RoutedEventArgs e)
        {
            IndexCanceled = true;
        }
    }
}
