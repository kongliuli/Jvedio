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
        // Lifecycle / theme / startup
        public Main()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            InitContext();
            BindingEvent();
            LoadNotifyIcon();
        }

        public void InitContext()
        {
            vieModel = new VieModel_Main(this);
            this.DataContext = vieModel;
        }

        public void Dispose()
        {
            ScanEngine.StopWatching();
            SaveConfigValue();
            App.TaskHub.CancelAll();
        }


        private void Window_ContentRendered(object sender, EventArgs e)
        {
            ConfigFirstRun();
            InitTheme();
            InitNotice();
            InitDataBases();
            RefreshLibraryWatch();
            BindingEventAfterRender();
            InitUpgrade();
            CheckServerStatus();
            InitAvalonEdit();
            InitSideMenu();
        }

        public void InitSideMenu()
        {
            if (LibraryContext.Current.DataType != DataType.Video) {
                sideMenuContainer.Child = null;
            }
            Core.UI.MediaUIHost.InitSideMenu(LibraryContext.Current.DataType, sideMenuContainer, vieModel);
            vieModel.Statistic();
        }



        public void InitAvalonEdit()
        {
            AvalonEditManager.Init();
        }

        private void BaseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAll();
        }

        public void LoadAll()
        {
            vieModel.LoadAll(); // 加载数据
        }

        public void LoadNotifyIcon()
        {
            SetNotiIconPopup(notiIconPopup);
            this.OnNotifyIconMouseLeftClick += (s, e) => {
                ShowMainWindow(s, new RoutedEventArgs());
            };
        }

        public async void CheckServerStatus()
        {
            vieModel.ServerStatus = await ServerManager.CheckStatus();
        }

        public void SetAllSelect()
        {
            List<VideoList> videoLists = vieModel.TabItemManager.GetAllVideoList();
            List<ActorList> actorLists = vieModel.TabItemManager.GetAllActorList();
            if (videoLists != null) {
                foreach (VideoList video in videoLists) {
                    video.SetSelected();
                }
            }
        }


        public void InitTheme()
        {
            foreach (var item in TransParentBackGround) {
                ThemeSelectorDefault.AddTransParentColor(item);
            }
            ThemeSelectorDefault.SetThemeConfig(ConfigManager.ThemeConfig.ThemeIndex,
                ConfigManager.ThemeConfig.ThemeID);
            ThemeSelectorDefault.onThemeChanged += (ThemeIdx, ThemeID) => {
                ConfigManager.ThemeConfig.ThemeIndex = ThemeIdx;
                ConfigManager.ThemeConfig.ThemeID = ThemeID;
                ConfigManager.ThemeConfig.Save();
                SetAllSelect();
            };
            ThemeSelectorDefault.onBackGroundImageChanged += (image) => {
                ImageBackground.Source = image;
                StyleManager.BackgroundImage = image;
            };
            ThemeSelectorDefault.onSetBgColorTransparent += () => {
                BorderTitle.Background = Brushes.Transparent;
            };

            ThemeSelectorDefault.onReSetBgColorBinding += () => {
                BorderTitle.SetResourceReference(Control.BackgroundProperty, "Window.Title.Background");
            };

            ThemeSelectorDefault.InitThemes();
        }


        public void InitUpgrade()
        {
            UpgradeHelper.Init(this);
            CheckUpgrade(); // 检查更新
        }
        private void ConfigFirstRun()
        {
            if (ConfigManager.Main.FirstRun) {
                vieModel.ShowFirstRun = Visibility.Visible;
                ConfigManager.Main.FirstRun = false;
            }
        }
    }
}
