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
        // Tab: Scan
        private void RunDiscoverPrecheck(object sender, RoutedEventArgs e)
        {
            if (vieModel.ScanPath == null || vieModel.ScanPath.Count == 0) {
                MessageCard.Warning("请先配置扫描路径");
                return;
            }

            LibraryContext library = LibraryContext.FromCurrent(vieModel.ScanPath.ToList());
            library.DataType = LibraryContext.Current.DataType;
            var options = new ScanOptions {
                Mode = ScanMode.Discover,
                DiscoveryMode = ConfigManager.ScanConfig.EnableDirIndexCache
                    ? DiscoveryMode.Incremental
                    : DiscoveryMode.Full,
            };
            DiscoveryResult result = ScanEngine.Discover(library, options);
            int count = result?.Timing?.FileCount ?? 0;
            long ms = result?.Timing?.EnumerateMs ?? 0;
            MessageCard.Info($"Discover 预检：{count} 个文件，耗时 {ms}ms，缓存={result?.FromCache == true}");
        }

        private void RunDiscoverOnlyScan(object sender, RoutedEventArgs e)
        {
            if (vieModel.ScanPath == null || vieModel.ScanPath.Count == 0) {
                MessageCard.Warning("请先配置扫描路径");
                return;
            }

            LibraryContext library = LibraryContext.FromCurrent(vieModel.ScanPath.ToList());
            library.DataType = LibraryContext.Current.DataType;
            var options = new ScanOptions {
                Mode = ScanMode.Discover,
                DiscoveryMode = ConfigManager.ScanConfig.EnableDirIndexCache
                    ? DiscoveryMode.Incremental
                    : DiscoveryMode.Full,
            };
            ScanJobBase job = ScanEngine.CreateJob(library, options);
            if (job == null) {
                MessageCard.Warning("无法创建 Discover 扫描任务");
                return;
            }

            job.Title = LangManager.GetValueByKey("Scanning");
            job.onError += (s, ev) => MessageCard.Error((ev as MessageCallBackEventArgs)?.Message);
            App.TaskHub.AddTask(job, TaskKind.Scan);
            MessageCard.Info("Discover 扫描已入队，请在任务列表查看进度");
        }

        public void SaveNFOParseValues()
        {
            vieModel.SaveNFOParseData();
        }
    }
}
