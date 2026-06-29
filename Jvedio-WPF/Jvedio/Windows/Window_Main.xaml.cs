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
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : BaseWindow, IBaseWindow
    {

        #region "静态属性"

        public static List<string> ClickFilterDict { get; set; }

        /// <summary>
        /// 如果包含以下文本，则显示对应的标签戳
        /// </summary>
        public static string[] TagStringHD { get; set; }

        public static string[] TagStringTranslated { get; set; }


        public static DataBaseType CurrentDataBaseType { get; set; }

        public static bool ClickGoBackToStartUp { get; set; }// 是否是点击了返回去到 Startup

        public static DataType CurrentDataType {
            get => Core.Library.LibraryContext.Current.DataType;
            set => Core.Library.LibraryContext.Apply(value, null, (int?)ConfigManager.Main?.CurrentDBId);
        }

        public static string[] TransParentBackGround { get; set; } = new string[] {
            "TabItem.Background",
            "ListBoxItem.Background",
            "Window.Side.Background",
            "Window.Side.Hover.Background",
            "DataGrid.Row.Even.Background",
            "DataGrid.Row.Odd.Background",
            "DataGrid.Row.Hover.Background",
            "DataGrid.Header.Background",
        };


        #endregion

        #region "属性"

        private Window_Server window_Server { get; set; }
        private bool CanDragTabItem { get; set; } = false;

        private FrameworkElement CurrentDragElement { get; set; }

        private Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager TaskbarInstance { get; set; }

        private SuperControls.Style.Plugin.Window_Plugin window_Plugin { get; set; }


        private Window_Details windowDetails { get; set; }

        private bool AnimatingSideGrid { get; set; } = false;

        public VieModel_Main vieModel { get; set; }


        #endregion

        static Main()
        {
            StaticInit();
        }

        static void StaticInit()
        {
            ClickFilterDict = new List<string>() { "Genre", "Series", "Studio", "Director", };
            TagStringHD = new string[] { "hd", "高清" };
            TagStringTranslated = new string[] { "中文", "日本語", "Translated", "English" };
            CurrentDataBaseType = DataBaseType.SQLite;
            ClickGoBackToStartUp = false;
            CurrentDataType = DataType.Video;
        }

    }
}
