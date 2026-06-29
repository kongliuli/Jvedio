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
        // Menu / plugin / settings / import UI
        private void OpenFeedBack(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenUrl(FeedBackUrl);
        }

        private void OpenHelp(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenUrl(WikiUrl);
        }

        private void OpenJvedioWebPage(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenUrl(WebPageUrl);
        }

        private async void CheckUpgrade()
        {
            // 启动后检查更新
            try {
                await Task.Delay(UpgradeHelper.AUTO_CHECK_UPGRADE_DELAY);
                (string LatestVersion, string ReleaseDate, string ReleaseNote) result = await UpgradeHelper.GetUpgradeInfo();
                string remote = result.LatestVersion;
                string ReleaseDate = result.ReleaseDate;
                if (!string.IsNullOrEmpty(remote)) {
                    string local = App.GetLocalVersion(false);
                    if (local.CompareTo(remote) < 0) {
                        bool opened = (bool)new MsgBox(
                            $"存在新版本\n版本：{remote}\n日期：{ReleaseDate}").ShowDialog();
                        if (opened)
                            UpgradeHelper.OpenWindow();
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }


        public void ShowSameString(string str, LabelType labelType)
        {
            vieModel.onLabelClick(str, labelType);
        }
        private void SaveConfigValue()
        {
            ConfigManager.Main.X = this.Left;
            ConfigManager.Main.Y = this.Top;
            ConfigManager.Main.Width = this.Width;
            ConfigManager.Main.Height = this.Height;
            ConfigManager.Main.SideGridWidth = SideGridColumn.ActualWidth;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Dispose();
        }
        private void OpenImageSavePath(object sender, RoutedEventArgs e)
        {
            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            if (!ConfigManager.Settings.PicPaths.ContainsKey(pathType.ToString()))
                return;
            string basePicPath = ConfigManager.Settings.PicPaths[pathType.ToString()].ToString();
            if (pathType == PathType.RelativeToApp)
                basePicPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePicPath);
            basePicPath = Path.GetFullPath(basePicPath);
            FileHelper.TryOpenPath(basePicPath);
        }

        private void OpenLogPath(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenSelectPath(PathManager.LogPath);
        }

        private void OpenApplicationPath(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenPath(AppDomain.CurrentDomain.BaseDirectory);
        }


        private void ImportVideo(object sender, RoutedEventArgs e)
        {
            string path = FileHelper.SelectPath(this);
            if (!string.IsNullOrEmpty(path)) {
                AddScanTask(new string[] { path });
                MessageNotify.Success($"{LangManager.GetValueByKey("AddScanTaskSuccess")} => " + path);
            }
        }

        private void ImportVideoByPaths(object sender, RoutedEventArgs e)
        {
            Window_SelectPaths window_SelectPaths = new Window_SelectPaths();
            if (window_SelectPaths.ShowDialog() == true) {
                List<string> folders = window_SelectPaths.Folders;
                if (folders.Count == 0) {
                    MessageNotify.Warning(LangManager.GetValueByKey("PathNotSelect"));
                } else {
                    AddScanTask(folders.ToArray());
                    // MessageCard.Success($"已添加 {folders.Count} 个文件夹到扫描任务!");
                }
            }
        }


        private void CopyTextBlock(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            TextBlock textBlock = contextMenu.PlacementTarget as TextBlock;
            if (textBlock != null) {
                string txt = textBlock.Text;
                if (!string.IsNullOrEmpty(txt))
                    ClipBoard.TrySetDataObject(txt);
            }
        }


        private void ShowUpgradeWindow(object sender, RoutedEventArgs e)
        {
            UpgradeHelper.OpenWindow();
        }

        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            App.ShowAbout();
        }

        private void ShowPluginWindow(object sender, RoutedEventArgs e)
        {
            if (window_Plugin == null || window_Plugin.IsClosed) {
                SuperControls.Style.Plugin.PluginConfig config = new SuperControls.Style.Plugin.PluginConfig();
                config.PluginBaseDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
                config.RemoteUrl = UrlManager.GetPluginUrl();
                window_Plugin = new SuperControls.Style.Plugin.Window_Plugin();
                window_Plugin.SetConfig(config);

                window_Plugin.OnEnabledChange += (data, enabled) => {
                    return true;
                };

                window_Plugin.OnBeginDelete += (PluginMetaData data) => {
                    PluginType pluginType = data.PluginType;

                    if (pluginType == PluginType.Crawler) {
                        List<string> list = JsonUtils.TryDeserializeObject<List<string>>(ConfigManager.PluginConfig.DeleteList);
                        if (list == null)
                            list = new List<string>();
                        if (!list.Contains(data.PluginID)) {
                            list.Add(data.PluginID);
                            ConfigManager.PluginConfig.DeleteList = JsonUtils.TrySerializeObject(list);
                            ConfigManager.PluginConfig.Save();
                            MessageNotify.Info("已加入待删除列表，重启后生效");
                        } else {
                            MessageNotify.Warning("已经在待删除列表里");
                        }
                    }
                    return false;
                };
                window_Plugin.OnDeleteCompleted += (data) => {
                    PluginType pluginType = data.PluginType;
                    if (pluginType == PluginType.Theme) {
                        ThemeSelectorDefault.InitThemes();
                    } else if (pluginType == PluginType.Crawler) {
                        CrawlerManager.Init(false);
                    }
                };

                window_Plugin.OnBeginDownload += (data) => {
                    return true;
                };
                window_Plugin.OnDownloadCompleted += (data) => {
                    // 根据类型，通知到对应模块
                    PluginType pluginType = data.PluginType;
                    if (pluginType == PluginType.Theme) {
                        ThemeSelectorDefault.InitThemes();
                    } else if (pluginType == PluginType.Crawler) {
                        // 如果是新下载，则可以直接使用
                        // 如果是更新，则需要重启
                        CrawlerManager.Init(false);
                    }
                };
            }
            window_Plugin.Show();
            window_Plugin.BringIntoView();
            window_Plugin.Focus();
            window_Plugin.Activate();
        }

        private void ShowSettingsWindow(object sender, RoutedEventArgs e)
        {
            Window_Settings window_Settings = new Window_Settings();
            window_Settings.Show();
            notiIconPopup.IsOpen = false;
        }



        private void ManageDataBase(object sender, RoutedEventArgs e)
        {
            Window_DataBase dataBase = new Window_DataBase();
            dataBase.Owner = this;

            dataBase.OnDataChanged += () => {
                vieModel.Statistic();
            };

            dataBase.ShowDialog();
        }

        public bool IsTaskRunning()
        {
            return App.TaskHub.HasRunningTasks;
        }


        private void OpenServer(object sender, RoutedEventArgs e)
        {
            MessageNotify.Info("开发中");
            return;

            //if (window_Server != null)
            //    window_Server.Close();
            //window_Server = new Window_Server();

            //window_Server.OnServerStatusChanged += (status) => {
            //    vieModel.ServerStatus = status;
            //};

            //window_Server.Owner = this;
            //window_Server.ShowDialog();

        }

        private void OpenServerWindow(object sender, MouseButtonEventArgs e)
        {
            OpenServer(null, null);
        }

        private void GoToStartUp(object sender, RoutedEventArgs e)
        {
            Main.ClickGoBackToStartUp = true;
            SetWindowVisualStatus(false); // 隐藏所有窗体
            WindowStartUp windowStartUp = GetWindowByName("WindowStartUp", App.Current.Windows) as WindowStartUp;
            if (windowStartUp == null)
                windowStartUp = new WindowStartUp();
            Application.Current.MainWindow = windowStartUp;
            windowStartUp.Show();
        }

        private void ClearCache(object sender, RoutedEventArgs e)
        {
            ImageCache.Clear();
        }
    }
}
