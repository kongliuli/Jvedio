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
        // Drag import / scan orchestration
        private void OnDragFileDrop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            vieModel.DragInFile = false;
            AddScanTask(dragdropFiles);
        }
        private void AddScanTask(string[] scanFileList)
        {
            ScanOrchestrator.EnqueueScan(scanFileList, ScanComplete);
        }

        public void RebuildForDataType()
        {
            InitSideMenu();
            vieModel?.SyncLibraryContext(GetCurrentScanPaths());
            vieModel?.TabItemManager?.ClearMediaTabs();
            vieModel?.Statistic();
        }

        private List<string> GetCurrentScanPaths()
        {
            if (vieModel?.CurrentAppDataBase == null || string.IsNullOrEmpty(vieModel.CurrentAppDataBase.ScanPath))
                return new List<string>();
            List<string> paths = JsonUtils.TryDeserializeObject<List<string>>(vieModel.CurrentAppDataBase.ScanPath);
            return paths ?? new List<string>();
        }

        private void ScanComplete(object sender, EventArgs ev)
        {
            ScanOrchestrator.HandleScanCompleted(
                sender as ScanJobBase,
                vieModel,
                Dispatcher,
                LoadAll,
                ScreenShotAfterImport);
        }


        private void ScreenShotAfterImport(List<Video> import)
        {
            if (import != null &&
                import.Count > 0 &&
                File.Exists(ConfigManager.FFmpegConfig.Path)) {
                IMediaListTab mediaList = vieModel.TabItemManager.GetPrimaryMediaList();
                mediaList?.GenerateScreenShot(import);
            }
        }

        private void OnDragFileOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true; // 必须加
            vieModel.DragInFile = true;

        }

        private void OnDragFileLeave(object sender, DragEventArgs e)
        {
            vieModel.DragInFile = false;
        }
        public void RefreshLibraryWatch()
        {
            ScanEngine.StopWatching();
            if (vieModel?.CurrentAppDataBase == null || string.IsNullOrEmpty(vieModel.CurrentAppDataBase.ScanPath))
                return;

            List<string> paths = JsonUtils.TryDeserializeObject<List<string>>(vieModel.CurrentAppDataBase.ScanPath);
            if (paths == null || paths.Count == 0)
                return;

            LibraryContext library = LibraryContext.FromCurrent(paths);
            library.DataType = LibraryContext.Current.DataType;
            ScanEngine.StartWatching(library, EnqueueIncrementalScanJob);
        }

        private void EnqueueIncrementalScanJob(ScanJobBase scanJob)
        {
            if (scanJob == null)
                return;
            scanJob.Title = LangManager.GetValueByKey("Scanning");
            scanJob.onCanceled += (s, ev) => Logger.Warn("cancel incremental scan");
            scanJob.onError += (s, ev) => Logger.Error((ev as MessageCallBackEventArgs)?.Message);
            scanJob.onCompleted += ScanComplete;
            App.TaskHub.AddTask(scanJob, Core.Tasks.TaskKind.Scan);
        }
    }
}
