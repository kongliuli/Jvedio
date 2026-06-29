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
        // Tab: Network
        private void InitProxy()
        {
            List<RadioButton> proxies = proxyStackPanel.Children.OfType<RadioButton>().ToList();
            for (int i = 0; i < proxies.Count; i++) {
                if (i == ConfigManager.ProxyConfig.ProxyMode)
                    proxies[i].IsChecked = true;
                int idx = i;
                proxies[i].Click += (s, ev) => {
                    ConfigManager.ProxyConfig.ProxyMode = idx;
                    MessageCard.Info(LangManager.GetValueByKey("RebootToTakeEffect"));
                };
            }

            List<RadioButton> proxyTypes = proxyTypesStackPanel.Children.OfType<RadioButton>().ToList();
            for (int i = 0; i < proxyTypes.Count; i++) {
                if (i == ConfigManager.ProxyConfig.ProxyType)
                    proxyTypes[i].IsChecked = true;
                int idx = i;
                proxyTypes[i].Click += (s, ev) => {
                    ConfigManager.ProxyConfig.ProxyType = idx;
                };
            }

            // 设置代理密码
            passwordBox.Password = vieModel.ProxyPwd;

            passwordBox.PasswordChanged += (s, ev) => {
                if (!string.IsNullOrEmpty(passwordBox.Password))
                    vieModel.ProxyPwd = JvedioLib.Security.Encrypt.AesEncrypt(passwordBox.Password, 0);
            };
        }

        private void SelectNfoPath(object sender, RoutedEventArgs e)
        {
            // 选择NFO存放位置
            var path = FileHelper.SelectPath(this);
            if (Directory.Exists(path)) {
                if (!path.EndsWith("\\"))
                    path = path + "\\";
                vieModel.NFOSavePath = path;
            } else {
                MessageNotify.Error(SuperControls.Style.LangManager.GetValueByKey("Message_CanNotBeNull"));
            }
        }

        private async void TestProxy(object sender, RoutedEventArgs e)
        {
            vieModel.TestProxyStatus = TaskStatus.Running;
            SaveSettings();
            Button button = sender as Button;
            button.IsEnabled = false;
            string url = textProxyUrl.Text;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            RequestHeader header = new RequestHeader();
            IWebProxy proxy = ConfigManager.ProxyConfig.GetWebProxy();
            header.TimeOut = ConfigManager.ProxyConfig.HttpTimeout * 1000; // 转为 ms
            header.WebProxy = proxy;
            string error = LangManager.GetValueByKey("Error");
            HttpResult httpResult = null;
            try {
                httpResult = await HttpClient.Get(url, header, SuperUtils.NetWork.Enums.HttpMode.String);
            } catch (TimeoutException ex) { error = ex.Message; } catch (Exception ex) { error = ex.Message; }
            if (httpResult != null) {
                if (httpResult.StatusCode == HttpStatusCode.OK) {
                    MessageCard.Success($"{LangManager.GetValueByKey("Success")} {LangManager.GetValueByKey("Delay")} {stopwatch.ElapsedMilliseconds} ms");
                    vieModel.TestProxyStatus = TaskStatus.RanToCompletion;
                    MessageCard.Info(LangManager.GetValueByKey("RebootToTakeEffect"));
                } else {
                    MessageCard.Error(httpResult.Error);
                    vieModel.TestProxyStatus = TaskStatus.Canceled;
                }
            } else {
                MessageCard.Error(error);
                vieModel.TestProxyStatus = TaskStatus.Canceled;
            }

            stopwatch.Stop();
            button.IsEnabled = true;
        }
    }
}
