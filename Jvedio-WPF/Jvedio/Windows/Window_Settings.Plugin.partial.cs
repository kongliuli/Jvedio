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
        // Tab: Plugin
        private void InitIndex()
        {
            // 初次启动后不给设置默认打开上一次库，否则会提示无此数据库
            if (ConfigManager.Settings.DefaultDBID <= 0)
                openDefaultCheckBox.IsEnabled = false;

            // 设置 crawlerIndex
            serverListBox.SelectedIndex = (int)ConfigManager.Settings.CrawlerSelectedIndex;

        }

        private void NewServer(object sender, RoutedEventArgs e)
        {
            string pluginID = GetPluginID();
            if (string.IsNullOrEmpty(pluginID))
                return;
            CrawlerServer server = new CrawlerServer() {
                PluginID = pluginID,
                Enabled = true,
                Url = DEFAULT_TEST_URL,
                Cookies = string.Empty,
                Available = 0,
                LastRefreshDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            };
            ObservableCollection<CrawlerServer> list = vieModel.CrawlerServers[pluginID];
            if (list == null)
                list = new ObservableCollection<CrawlerServer>();
            list.Add(server);
            vieModel.CrawlerServers[pluginID] = list;
            ServersDataGrid.ItemsSource = null;
            ServersDataGrid.ItemsSource = list;
        }

        private string GetPluginID()
        {
            int idx = serverListBox.SelectedIndex;
            if (idx < 0 || vieModel.CrawlerServers?.Count == 0)
                return null;
            return vieModel.CrawlerServers.Keys.ToList()[idx];
        }

        private void TestServer(object sender, RoutedEventArgs e)
        {
            int idx = CurrentRowIndex;
            string pluginID = GetPluginID();
            if (string.IsNullOrEmpty(pluginID))
                return;
            ObservableCollection<CrawlerServer> list = vieModel.CrawlerServers[pluginID];
            CrawlerServer server = list[idx];

            if (!server.IsHeaderProper()) {
                MessageNotify.Error(LangManager.GetValueByKey("HeaderNotProper"));
                return;
            }

            server.Available = 2;
            ServersDataGrid.IsEnabled = false;
            CheckUrl(server, (s) => {
                ServersDataGrid.IsEnabled = true;
                list[idx].LastRefreshDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            });
        }

        private void DeleteServer(object sender, RoutedEventArgs e)
        {
            string pluginID = GetPluginID();
            if (string.IsNullOrEmpty(pluginID))
                return;
            Console.WriteLine(CurrentRowIndex);
            ObservableCollection<CrawlerServer> list = vieModel.CrawlerServers[pluginID];
            list.RemoveAt(CurrentRowIndex);
            vieModel.CrawlerServers[pluginID] = list;
            ServersDataGrid.ItemsSource = null;
            ServersDataGrid.ItemsSource = list;
        }

        private void SetCurrentRowIndex(object sender, MouseButtonEventArgs e)
        {
            DataGridRow dgr = null;
            var visParent = VisualTreeHelper.GetParent(e.OriginalSource as FrameworkElement);
            while (dgr == null && visParent != null) {
                dgr = visParent as DataGridRow;
                visParent = VisualTreeHelper.GetParent(visParent);
            }

            if (dgr == null) {
                return;
            }

            CurrentRowIndex = dgr.GetIndex();
        }

        private async void CheckUrl(CrawlerServer server, Action<int> callback)
        {
            // library 需要保证 Cookies 和 UserAgent完全一致
            RequestHeader header = CrawlerServer.ParseHeader(server);
            try {
                string title = await HttpHelper.AsyncGetWebTitle(server.Url, header);
                if (string.IsNullOrEmpty(title)) {
                    server.Available = -1;
                } else {
                    server.Available = 1;
                }

                await Dispatcher.BeginInvoke((Action)delegate {
                    ServersDataGrid.Items.Refresh();
                    if (!string.IsNullOrEmpty(title))
                        MessageCard.Success(title);
                });
                callback.Invoke(0);
            } catch (WebException ex) {
                MessageCard.Error(ex.Message);
                server.Available = -1;
                await Dispatcher.BeginInvoke((Action)delegate {
                    ServersDataGrid.Items.Refresh();
                });
                callback.Invoke(0);
            }
        }

        public static T GetVisualChild<T>(Visual parent) where T : Visual

        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < numVisuals; i++) {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);

                child = v as T;

                if (child == null) {
                    child = GetVisualChild<T>

                    (v);
                }

                if (child != null) {
                    break;
                }
            }

            return child;
        }

        private void url_PreviewMouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            setHeaderPopup.IsOpen = true;
            SearchBox searchBox = sender as SearchBox;
            string headers = searchBox.Text;
            if (!string.IsNullOrEmpty(headers)) {
                try {
                    Dictionary<string, string> dict = JsonUtils.TryDeserializeObject<Dictionary<string, string>>(headers);
                    if (dict != null && dict.Count > 0) {
                        StringBuilder builder = new StringBuilder();
                        foreach (string key in dict.Keys) {
                            builder.Append($"{key}: {dict[key]}{Environment.NewLine}");
                        }

                        inputTextbox.Text = builder.ToString();
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            }

            string pluginID = GetPluginID();
            if (string.IsNullOrEmpty(pluginID))
                return;
            currentCrawlerServer = vieModel.CrawlerServers[pluginID][ServersDataGrid.SelectedIndex];
        }

        private void CancelHeader(object sender, RoutedEventArgs e)
        {
            setHeaderPopup.IsOpen = false;
        }

        private void ConfirmHeader(object sender, RoutedEventArgs e)
        {
            setHeaderPopup.IsOpen = false;
            if (currentCrawlerServer != null) {
                currentCrawlerServer.Headers = parsedTextbox.Text.Replace("{" + Environment.NewLine + "    ", "{")
                    .Replace(Environment.NewLine + "}", "}")
                    .Replace($"\",{Environment.NewLine}    \"", "\",\"");
                Dictionary<string, string> dict = JsonUtils.TryDeserializeObject<Dictionary<string, string>>(currentCrawlerServer.Headers);

                if (dict != null && dict.ContainsKey("cookie"))
                    currentCrawlerServer.Cookies = dict["cookie"];
            }
        }

        private void InputHeader_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (parsedTextbox != null)
                parsedTextbox.Text = Parse((sender as TextBox).Text);
        }

        private string Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            Dictionary<string, string> data = new Dictionary<string, string>();
            string[] array = text.Split(Environment.NewLine.ToCharArray());
            foreach (string item in array) {
                int idx = item.IndexOf(':');
                if (idx <= 0 || idx >= item.Length - 1)
                    continue;
                string key = item.Substring(0, idx).Trim().ToLower();
                string value = item.Substring(idx + 1).Trim();

                if (!data.ContainsKey(key))
                    data.Add(key, value);
            }

            // if (vieModel.AutoHandleHeader)
            // {
            data.Remove("content-encoding");
            data.Remove("accept-encoding");
            data.Remove("host");

            data = data.Where(arg => arg.Key.IndexOf(" ") < 0).ToDictionary(x => x.Key, y => y.Value);

            // }
            string json = JsonConvert.SerializeObject(data);
            if (json.Equals("{}"))
                return json;

            return json.Replace("{", "{" + Environment.NewLine + "    ")
                .Replace("}", Environment.NewLine + "}")
                .Replace("\",\"", $"\",{Environment.NewLine}    \"");
        }

        private void ShowHeaderHelp(object sender, RoutedEventArgs e)
        {
            setHeaderPopup.IsOpen = false;
            FileHelper.TryOpenUrl(UrlManager.HEADER_HELP);
        }

        private void PluginList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = (sender as ListBox).SelectedIndex;
            if (idx < 0)
                return;
            if (vieModel.CrawlerServers?.Count > 0) {
                string pluginID = PluginType.Crawler.ToString().ToLower() + "/" + vieModel.DisplayCrawlerServers[idx];
                PluginMetaData pluginMetaData = CrawlerManager.PluginMetaDatas.Where(arg => arg.PluginID.Equals(pluginID)).FirstOrDefault();
                if (vieModel.CrawlerServers.ContainsKey(pluginID)) {
                    ServersDataGrid.ItemsSource = null;
                    ServersDataGrid.ItemsSource = vieModel.CrawlerServers[pluginID];
                    vieModel.CurrentPlugin = pluginMetaData;
                    ConfigManager.Settings.CrawlerSelectedIndex = idx;
                    vieModel.ShowCurrentPlugin = true;
                }

            }
        }

        private void SavePluginSetting(object sender, RoutedEventArgs e)
        {
            // 保存插件启用情况
            vieModel.CurrentPlugin?.SaveConfig();
        }

        private void ShowCrawlerHelp(object sender, RoutedEventArgs e)
        {
            MessageCard.Info(LangManager.GetValueByKey("CrawlerServerHint"));
        }
    }
}
