using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.FFmpeg;
using Jvedio.Core.Library;
using Jvedio.Core.Media;
using Jvedio.Core.Net;
using Jvedio.Core.Scan;
using Jvedio.Core.UserControls;
using Jvedio.Entity;
using Jvedio.Entity.CommonSQL;
using Jvedio.ViewModel;
using Microsoft.VisualBasic.FileIO;
using SuperControls.Style;
using SuperControls.Style.Windows;
using SuperUtils.Common;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.IO;
using SuperUtils.Media;
using SuperUtils.WPF.Entity;
using SuperUtils.WPF.VisualTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static Jvedio.App;
using static Jvedio.MapperManager;
using static SuperUtils.Media.ImageHelper;
using static SuperUtils.WPF.VisualTools.VisualHelper;
using static SuperUtils.WPF.VisualTools.WindowHelper;

namespace Jvedio
{
    /// <summary>
    /// Window_Details.xaml 的交互逻辑
    /// </summary>
    public partial class Window_Details : Window
    {

        #region "静态属性"

        private delegate void LoadActorDelegate(ActorInfo actor);

        private void LoadActor(ActorInfo actor) => vieModel.CurrentActorList.Add(actor);

        private delegate void LoadExtraImageDelegate(BitmapSource bitmapSource);
        private delegate void LoadExtraPathDelegate(string path);

        public Action<long> onViewAssoData;

        [Obsolete("Subscribe to LibraryEventBus.TagStampPanelRefresh")]
        public static Action onRemoveTagStamp;

        private static Main windowMain { get; set; }

        #endregion

        #region "属性"

        /// <summary>
        /// 用于传递当前主窗体的列表 Wrapper
        /// </summary>
        private WrapperEventArg<Video> CurrentWrapperArg { get; set; }

        private VieModel_Details vieModel { get; set; }
        private Window_Edit windowEdit { get; set; }

        private List<long> DataIDs { get; set; } = new List<long>();

        private object RefreshLock { get; set; } = new object();

        public long DataID { get; set; }

        private bool CancelLoadImage { get; set; }// 切换到下一个影片时停止加载图片

        /// <summary>
        /// 进度条
        /// </summary>
        private Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager TaskbarInstance { get; set; }

        private bool CanRateChange { get; set; }

        #endregion

        static Window_Details()
        {
            windowMain = GetWindowByName("Main", App.Current.Windows) as Main;
        }

    }
}
