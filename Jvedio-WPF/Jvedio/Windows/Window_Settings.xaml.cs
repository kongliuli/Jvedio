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
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Window_Settings : SuperControls.Style.BaseWindow
    {

        private const string DEFAULT_TEST_URL = "https://www.example.com/";

        #region "事件"

        #endregion

        #region "静态属性"

        private List<string> RenameList { get; set; } = new List<string>();

        public static Video SampleVideo { get; set; }

        public static string SupportVideoFormat { get; set; }

        public static string SupportPictureFormat { get; set; } // bmp,gif,ico,jpe,jpeg,jpg,png

        #endregion

        #region "属性"
        public VieModel_Settings vieModel { get; set; }
        private Main MainWindow { get; set; }
        private int CurrentRowIndex { get; set; }

        private CrawlerServer currentCrawlerServer { get; set; }

        private bool IndexCanceled { get; set; } = false;

        #endregion

        static Window_Settings()
        {
            SampleVideo = new Video() {
                VID = "IRONMAN-01",
                Title = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Title"),
                VideoType = VideoType.Normal,
                ReleaseDate = "2020-01-01",
                Director = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Director"),
                Genre = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Genre"),
                Series = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Tag"),
                ActorNames = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Actor"),
                Studio = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Studio"),
                Rating = 9.0f,
                Label = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Label"),
                ReleaseYear = 2020,
                Duration = 126,
                Country = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Country"),
            };

            SupportVideoFormat =
                $"{SuperControls.Style.LangManager.GetValueByKey("NormalVideo")}(*.avi, *.mp4, *.mkv, *.mpg, *.rmvb)| *.avi; *.mp4; *.mkv; *.mpg; *.rmvb|{SuperControls.Style.LangManager.GetValueByKey("OtherVedio")}((*.rm, *.mov, *.mpeg, *.flv, *.wmv, *.m4v)| *.rm; *.mov; *.mpeg; *.flv; *.wmv; *.m4v|{SuperControls.Style.LangManager.GetValueByKey("AllFile")} (*.*)|*.*";
            SupportPictureFormat = $"图片(*.bmp, *.jpe, *.jpeg, *.jpg, *.png)|*.bmp;*.jpe;*.jpeg;*.jpg;*.png";
        }

        public Window_Settings()
        {
            InitializeComponent();

            vieModel = new VieModel_Settings();
            this.DataContext = vieModel;

            Init();

        }

        public void Init()
        {
            MainWindow = GetWindowByName("Main", App.Current.Windows) as Main;

            // 绑定事件
            foreach (var item in CheckedBoxWrapPanel.Children.OfType<ToggleButton>().ToList()) {
                item.Click += AddToRename;
            }
            vieModel.MainWindowVisible = MainWindow != null;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            InitIndex();
            InitScanDatabases();
            InitViewRename(ConfigManager.RenameConfig.FormatString);
            InitCheckedBoxChecked();
            InitRenameCombobox();
            InitProxy();
            InitLang();
        }

        /// <summary>
        /// 设置语言
        /// </summary>

        /// <summary>
        /// 设置代理选中
        /// </summary>

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            ApplySettings(null, null);
            this.Close();
        }

        private void ApplySettings(object sender, RoutedEventArgs e)
        {
            // 保存扫描库
            ConfigManager.DownloadConfig.Save();

            if (DatabaseComboBox.ItemsSource != null && DatabaseComboBox.SelectedItem != null) {
                AppDatabase db = DatabaseComboBox.SelectedItem as AppDatabase;
                List<string> list = new List<string>();
                if (vieModel.ScanPath != null)
                    list = vieModel.ScanPath.ToList();
                db.ScanPath = JsonConvert.SerializeObject(list);
                MapperManager.appDatabaseMapper.UpdateById(db);
                int idx = DatabaseComboBox.SelectedIndex;

                List<AppDatabase> appDatabases = MainWindow?.vieModel.DataBases.ToList();
                if (appDatabases != null && idx < MainWindow.vieModel.DataBases.Count) {
                    MainWindow.vieModel.DataBases[idx].ScanPath = db.ScanPath;
                }
                MainWindow?.RefreshLibraryWatch();
            }

            bool success = vieModel.SaveServers((msg) => {
                MessageCard.Error(msg);
            });
            if (success) {
                VideoParser.InitSearchPattern();
                SavePath();
                SaveSettings();

                SuperControls.Style.MessageNotify.Success(SuperControls.Style.LangManager.GetValueByKey("Message_Success"));
            }
            UtilsManager.OnUtilSettingChange();

            SaveNFOParseValues();
        }

        /// <summary>
        /// 保存 NFO 解析
        /// </summary>

        /// <summary>
        /// 设置当前数据库
        /// </summary>

        // 检视

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveSettings();
            ConfigManager.Settings.Save();
            ConfigManager.ProxyConfig.Save();
            ConfigManager.ScanConfig.Save();
            ConfigManager.FFmpegConfig.Save();
            ConfigManager.RenameConfig.Save();
        }

        private void SaveSettings()
        {
            ConfigManager.Main.ShowSearchHistory = vieModel.ShowSearchHistory;

            ConfigManager.Settings.TabControlSelectedIndex = vieModel.TabControlSelectedIndex;
            ConfigManager.Settings.OpenDataBaseDefault = vieModel.OpenDataBaseDefault;
            ConfigManager.Settings.AutoGenScreenShot = vieModel.AutoGenScreenShot;
            ConfigManager.Settings.CloseToTaskBar = vieModel.CloseToTaskBar;
            ConfigManager.Settings.CurrentLanguage = vieModel.CurrentLanguage;
            ConfigManager.Settings.SaveInfoToNFO = vieModel.SaveInfoToNFO;
            ConfigManager.Settings.NFOSavePath = vieModel.NFOSavePath;
            ConfigManager.Settings.OverwriteNFO = vieModel.OverwriteNFO;
            ConfigManager.Settings.AutoHandleHeader = vieModel.AutoHandleHeader;

            ConfigManager.Settings.PicPathMode = vieModel.PicPathMode;
            ConfigManager.Settings.SkipExistImage = vieModel.SkipExistImage;
            ConfigManager.Settings.DownloadWhenTitleNull = vieModel.DownloadWhenTitleNull;
            ConfigManager.Settings.IgnoreCertVal = vieModel.IgnoreCertVal;
            ConfigManager.Settings.AutoBackup = vieModel.AutoBackup;
            ConfigManager.Settings.AutoBackupPeriodIndex = vieModel.AutoBackupPeriodIndex;

            // 代理
            ConfigManager.ProxyConfig.Server = vieModel.ProxyServer;
            ConfigManager.ProxyConfig.Port = vieModel.ProxyPort;
            ConfigManager.ProxyConfig.UserName = vieModel.ProxyUserName;
            ConfigManager.ProxyConfig.Password = vieModel.ProxyPwd;
            ConfigManager.ProxyConfig.HttpTimeout = vieModel.HttpTimeout;

            // 扫描
            ConfigManager.ScanConfig.MinFileSize = vieModel.MinFileSize;
            ConfigManager.ScanConfig.FetchVID = vieModel.FetchVID;
            ConfigManager.ScanConfig.LoadDataAfterScan = vieModel.LoadDataAfterScan;
            ConfigManager.ScanConfig.DataExistsIndexAfterScan = vieModel.DataExistsIndexAfterScan;
            ConfigManager.ScanConfig.ImageExistsIndexAfterScan = vieModel.ImageExistsIndexAfterScan;
            ConfigManager.ScanConfig.ScrapeAfterScan = vieModel.ScrapeAfterScan;
            ConfigManager.ScanConfig.ScanOnStartUp = vieModel.ScanOnStartUp;
            ConfigManager.ScanConfig.UseParallelDiscovery = vieModel.UseParallelDiscovery;
            ConfigManager.ScanConfig.DiscoveryThreadCount = vieModel.DiscoveryThreadCount;
            ConfigManager.ScanConfig.EnableDirIndexCache = vieModel.EnableDirIndexCache;
            ConfigManager.ScanConfig.CopyNFOOverwriteImage = vieModel.CopyNFOOverwriteImage;
            ConfigManager.ScanConfig.CopyNFOPicture = vieModel.CopyNFOPicture;
            ConfigManager.ScanConfig.CopyNFOActorPicture = vieModel.CopyNFOActorPicture;
            ConfigManager.ScanConfig.CopyNFOPreview = vieModel.CopyNFOPreview;
            ConfigManager.ScanConfig.CopyNFOScreenShot = vieModel.CopyNFOScreenShot;
            ConfigManager.ScanConfig.CopyNFOActorPath = vieModel.CopyNFOActorPath;
            ConfigManager.ScanConfig.CopyNFOPreviewPath = vieModel.CopyNFOPreviewPath;
            ConfigManager.ScanConfig.CopyNFOScreenShotPath = vieModel.CopyNFOScreenShotPath;

            // ffmpeg
            ConfigManager.FFmpegConfig.Path = vieModel.FFMPEG_Path;
            ConfigManager.FFmpegConfig.ThreadNum = vieModel.ScreenShot_ThreadNum;
            ConfigManager.FFmpegConfig.TimeOut = vieModel.ScreenShot_TimeOut;
            ConfigManager.FFmpegConfig.ScreenShotNum = vieModel.ScreenShotNum;
            ConfigManager.FFmpegConfig.ScreenShotIgnoreStart = vieModel.ScreenShotIgnoreStart;
            ConfigManager.FFmpegConfig.ScreenShotIgnoreEnd = vieModel.ScreenShotIgnoreEnd;
            ConfigManager.FFmpegConfig.SkipExistGif = vieModel.SkipExistGif;
            ConfigManager.FFmpegConfig.SkipExistScreenShot = vieModel.SkipExistScreenShot;
            ConfigManager.FFmpegConfig.ScreenShotAfterImport = vieModel.ScreenShotAfterImport;
            ConfigManager.FFmpegConfig.GifAutoHeight = vieModel.GifAutoHeight;
            ConfigManager.FFmpegConfig.GifWidth = vieModel.GifWidth;
            ConfigManager.FFmpegConfig.GifHeight = vieModel.GifHeight;
            ConfigManager.FFmpegConfig.GifDuration = vieModel.GifDuration;

            // 重命名
            ConfigManager.RenameConfig.AddRenameTag = vieModel.AddRenameTag;
            ConfigManager.RenameConfig.RemoveTitleSpace = vieModel.RemoveTitleSpace;
            ConfigManager.RenameConfig.FormatString = vieModel.FormatString;

            // 监听
            ConfigManager.Settings.ListenEnabled = vieModel.ListenEnabled;
            ConfigManager.Settings.ListenPort = vieModel.ListenPort;
        }

        private void RestoreDefault(object sender, RoutedEventArgs e)
        {
            if ((bool)new MsgBox(LangManager.GetValueByKey("Restore") + "?").ShowDialog(this)) {

                ConfigManager.Restore();

                // 基本
                vieModel.OpenDataBaseDefault = false;
                vieModel.ScanOnStartUp = false;
                vieModel.CloseToTaskBar = false;

                ConfigManager.Settings.DelInfoAfterDelFile = true;
                ConfigManager.Settings.HotKeyEnable = false;
                ConfigManager.Settings.HotKeyString = "";
                langComboBox.SelectedIndex = 0;
                ConfigManager.Settings.VideoPlayerPath = "";

                // 图片
                vieModel.AutoGenScreenShot = true;

                ImageSelectComboBox.SelectedIndex = 0;
                vieModel.BasePicPath = Path.Combine(PathManager.CurrentUserFolder, "pic");

                // 扫描与导入
                vieModel.FetchVID = true;
                vieModel.LoadDataAfterScan = true;
                vieModel.ScrapeAfterScan = true;
                vieModel.MinFileSize = ScanConfig.DEFAULT_MIN_FILE_SIZE;
                vieModel.DataExistsIndexAfterScan = true;

                ConfigManager.ScanConfig.ScanNfo = false;
                ConfigManager.ScanConfig.Save();

                vieModel.CopyNFOOverwriteImage = false;
                vieModel.CopyNFOPicture = true;
                vieModel.CopyNFOActorPicture = true;
                vieModel.CopyNFOActorPath = ".actor";
                vieModel.CopyNFOPreview = true;
                vieModel.CopyNFOPreviewPath = ".preview";

                vieModel.CopyNFOScreenShot = true;
                vieModel.CopyNFOScreenShotPath = ".screenshot";

                // NFO 解析规则
                NfoParse.RestoreDefault();
                NfoParse.SaveData(NfoParse.CurrentNFOParse);
                vieModel.LoadNfoParseData();

                // 网络
                vieModel.IgnoreCertVal = true;
                vieModel.HttpTimeout = ProxyConfig.DEFAULT_TIMEOUT;
                vieModel.DownloadWhenTitleNull = true;
                vieModel.SkipExistImage = false;
                vieModel.SaveInfoToNFO = false;

                ConfigManager.DownloadConfig.DownloadThumbNail = true;
                ConfigManager.DownloadConfig.DownloadPoster = true;
                ConfigManager.DownloadConfig.DownloadPreviewImage = false;
                ConfigManager.DownloadConfig.DownloadActor = true;
                ConfigManager.DownloadConfig.OverrideInfo = false;
                ConfigManager.DownloadConfig.Save();

                ConfigManager.ProxyConfig.ProxyMode = (int)ProxyConfig.DEFAULT_PROXY_MODE;
                ConfigManager.ProxyConfig.ProxyType = (int)ProxyConfig.DEFAULT_PROXY_TYPE;
                ConfigManager.ProxyConfig.Save();

                // 显示
                ConfigManager.Main.DisplaySearchBox = true;
                ConfigManager.Main.DisplayPage = true;
                ConfigManager.Main.PaginationCombobox = true;
                ConfigManager.Main.DisplayStatusBar = true;
                ConfigManager.Main.DisplayFunBar = true;
                ConfigManager.Main.DisplayNavigation = true;
                ConfigManager.Main.DetailWindowShowAllMovie = true;
                ConfigManager.Main.ScrollSpeedFactor = 1.5;

                ConfigManager.VideoConfig.DisplayID = true;
                ConfigManager.VideoConfig.DisplayTitle = true;
                ConfigManager.VideoConfig.DisplayDate = true;
                ConfigManager.VideoConfig.DisplayStamp = true;
                ConfigManager.VideoConfig.DisplayFavorites = true;
                ConfigManager.VideoConfig.MainImageAutoMode = true;
                ConfigManager.VideoConfig.MovieOpacity = 1;

                ConfigManager.VideoConfig.ShowFileNameIfTitleEmpty = true;
                ConfigManager.VideoConfig.ShowCreateDateIfReleaseDateEmpty = true;

                // 视频处理
                vieModel.FFMPEG_Path = "";
                vieModel.ScreenShot_ThreadNum = FFmpegConfig.DEFAULT_THREAD_NUM;
                vieModel.SkipExistScreenShot = true;
                vieModel.ScreenShotAfterImport = true;
                vieModel.ScreenShotNum = FFmpegConfig.DEFAULT_SCREEN_SHOT_NUM;
                vieModel.ScreenShotIgnoreStart = FFmpegConfig.DEFAULT_SCREEN_SHOT_IGNORE_START;
                vieModel.ScreenShotIgnoreEnd = FFmpegConfig.DEFAULT_SCREEN_SHOT_IGNORE_END;
                vieModel.SkipExistGif = false;
                vieModel.GifWidth = FFmpegConfig.DEFAULT_GIF_WIDTH;
                vieModel.GifHeight = FFmpegConfig.DEFAULT_GIF_HEIGHT;
                vieModel.GifAutoHeight = true;
                vieModel.GifDuration = FFmpegConfig.DEFAULT_GIF_DURATION;

                // 重命名
                vieModel.RemoveTitleSpace = false;
                vieModel.AddRenameTag = false;
                ConfigManager.RenameConfig.OutSplit = RenameConfig.DEFAULT_OUT_SPLIT;
                ConfigManager.RenameConfig.InSplit = RenameConfig.DEFAULT_IN_SPLIT;
                vieModel.FormatString = "";
                ConfigManager.RenameConfig.Save();

                // 库
                vieModel.AutoBackup = true;
                vieModel.AutoBackupPeriodIndex = Jvedio.Core.WindowConfig.Settings.DEFAULT_BACKUP_PERIOD_INDEX;

                ConfigManager.Main.Save();
                ApplySettings(null, null);

            }
        }

    }
}
