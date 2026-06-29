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
    public partial class Window_Details : Window
    {
        // Ctor / refresh / lifecycle
    public Window_Details(long dataID, WrapperEventArg<Video> arg)
    {
        InitializeComponent();
        DataID = dataID;
        CurrentWrapperArg = arg;
        Init();
    }

public void Init()
{
    this.Height = SystemParameters.PrimaryScreenHeight * 0.8;
    this.Width = SystemParameters.PrimaryScreenHeight * 0.8 * 1230 / 720;
    DataIDs = new List<long>();
    InitProgressBar();

    vieModel = new VieModel_Details(this);
    vieModel.QueryCompleted += async delegate {
        await LoadImage(vieModel.ShowScreenShot);
        ShowMagnets();
        ShowActor();
        vieModel.GetLabels();
        vieModel.LoadingData = false;
    };
}

private void Window_ContentRendered(object sender, EventArgs e)
{
    SetShadow();
    SetSkin();
    vieModel.Load(DataID);
    this.DataContext = vieModel;

    rootGrid.Focus(); // 设置键盘左右可切换
    InitDataIDs(); // 设置切换的影片列表

    RemoveNewAddTag();
    BindEvent();
}

private void BindEvent()
{
    LibraryEventBus.DownloadPreview += (s, e) => onDownloadPreview(e.DataId, e.Path, e.FileByte);
    LibraryEventBus.DownloadCompleted += (s, e) => onDownloadSuccess(e.Task);
    LibraryEventBus.ScreenShotCompleted += (s, e) => onScreenShotCompleted(e.Success, e.DataId);
    LibraryEventBus.TagStampDeleted += (s, e) => RemoveTag(e.TagId);
    LibraryEventBus.TagStampFilterRefresh += (s, e) => onRefreshTagStemp(e.TagId);
    LibraryEventBus.MetadataRefreshed += (s, e) => {
        if (e.DataId <= 0 || e.DataId == DataID)
            Refresh();
    };
    LibraryEventBus.ActorInfoChanged += (s, e) => RefreshActor(e.ActorId);
    LibraryEventBus.TagStampChanged += (s, e) => onTagStampChange(e.DataId, e.TagId, e.Deleted);
}
private void SetShadow()
{
    SuperControls.Style.Utils.DwmDropShadow.DropShadowToWindow(this);
}
private void InitProgressBar()
{
    ProgressBar.Visibility = Visibility.Collapsed;
    if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported)
    TaskbarInstance = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
}
public void Refresh()
{
    vieModel.Load(DataID);
}

public void Refresh(long dataID)
{
    if (DataID == DataID)
    Refresh();
}

public void RefreshGrade(long dataID, float data)
{
    if (vieModel.CurrentVideo.DataID != dataID)
    return;
    vieModel.CurrentVideo.Grade = data;
}
public void RefreshLabel(long dataID, string data)
{
    if (vieModel.CurrentVideo.DataID != dataID)
    return;
    vieModel.CurrentVideo.Label = data;
}
public void RefreshGenre(long dataID, string data)
{
    if (vieModel.CurrentVideo.DataID != dataID)
    return;
    vieModel.CurrentVideo.Genre = data;
}
public void RefreshSeries(long dataID, string data)
{
    if (vieModel.CurrentVideo.DataID != dataID)
    return;
    vieModel.CurrentVideo.Series = data;
}

public void SetSkin()
{
    BgImage.Source = null;
    if (ConfigManager.Settings.DetailShowBg)
    BgImage.Source = StyleManager.BackgroundImage;
    ////设置字体
    // if (GlobalFont != null) this.FontFamily = GlobalFont;
}
private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
{
    ConfigManager.Detail.ShowScreenShot = vieModel.ShowScreenShot;
    ConfigManager.Detail.InfoSelectedIndex = vieModel.InfoSelectedIndex;
    ConfigManager.Detail.Save();
}
    }
}
