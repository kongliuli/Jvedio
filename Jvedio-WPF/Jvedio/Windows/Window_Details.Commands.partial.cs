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
        // Download / file / filter
public void DownLoad(object sender, RoutedEventArgs e)
{
    Video video = vieModel.CurrentVideo;
    if (video == null || video.DataID <= 0)
    return;
    DownLoadTask.DownloadVideo(video);
}

public void OnDownloadPreview(long dataID, string path, byte[] fileByte)
{
    if (dataID.Equals(vieModel.CurrentVideo.DataID) && !vieModel.ShowScreenShot &&
    File.Exists(path) && fileByte != null) {
        // 加入到列表
        if (vieModel.CurrentVideo.PreviewImagePathList == null)
        vieModel.CurrentVideo.PreviewImagePathList = new ObservableCollection<string>();
        if (vieModel.CurrentVideo.PreviewImageList == null)
        vieModel.CurrentVideo.PreviewImageList = new ObservableCollection<BitmapSource>();
        vieModel.CurrentVideo.PreviewImagePathList.Add(path);
        vieModel.CurrentVideo.PreviewImageList.Add(ImageHelper.BitmapImageFromByte(fileByte));

        vieModel.PreviewImageCount = vieModel.CurrentVideo.PreviewImagePathList.Count;
    }

}

public void GetScreenGif(object sender, RoutedEventArgs e)
{
    Video video = vieModel.CurrentVideo;
    ScreenShotTask.ScreenShotVideo(video, gif: true);
}

public void GetScreenShot(object sender, RoutedEventArgs e)
{
    ScreenShotTask.ScreenShotVideo(vieModel.CurrentVideo);
}

public void CloseWindow(object sender, RoutedEventArgs e) => this.Close();

// 显示类别
public void ShowSameGenre(object sender, MouseButtonEventArgs e)
{
    ShowSameString(sender, LabelType.Genre);
}

///
/// 显示演员
///
///
///
public void ShowSameActor(object sender, MouseButtonEventArgs e)
{
    Grid grid = sender as Grid;
    if (grid == null || grid.Tag == null)
    return;
    long.TryParse(grid.Tag.ToString(), out long actorID);
    if (actorID <= 0)
    return;
    windowMain.ShowSameActor(actorID);
    this.Close();
}

// 显示标签
public void ShowSameLabel(object sender, MouseButtonEventArgs e)
{
    ShowSameString(sender, LabelType.LabelName);
}

// 显示导演
public void ShowSameDirector(object sender, MouseButtonEventArgs e)
{
    ShowSameString(sender, LabelType.Director);
}

public void ShowSameSeries(object sender, MouseButtonEventArgs e)
{
    ShowSameString(sender, LabelType.Series);
}

// 显示发行商
public void ShowSameStudio(object sender, MouseButtonEventArgs e)
{
    ShowSameString(sender, LabelType.Studio);
}

// 显示系列
public void ShowSameString(object sender, LabelType type)
{
    Border border = sender as Border;
    string text = ((TextBlock)border.Child).Text;
    if (string.IsNullOrEmpty(text))
    return;
    windowMain.ShowSameString(text, type);
    this.Close();
}

public void EditInfo(object sender, RoutedEventArgs e)
{
    if (windowEdit != null)
    windowEdit.Close();
    windowEdit = new Window_Edit(vieModel.CurrentVideo.DataID);
    windowEdit.ShowDialog();
}

public void CopyFile(object sender, RoutedEventArgs e)
{
    string filepath = vieModel.CurrentVideo.Path;
    if (File.Exists(filepath)) {
        StringCollection paths = new StringCollection { filepath };
        bool success = ClipBoard.TrySetFileDropList(paths, (error) => { MessageCard.Error(error); });
        if (success)
        MessageNotify.Success(SuperControls.Style.LangManager.GetValueByKey("HasCopy"));
    } else {
    SuperControls.Style.MessageNotify.Error(SuperControls.Style.LangManager.GetValueByKey("Message_FileNotExist"));
}
}

public void DeleteFile(object sender, RoutedEventArgs e)
{

    if (new MsgBox(LangManager.GetValueByKey("ToolTip_DeleteFile")).ShowDialog() == false) {
        return;
    }

int num = 0;
Video video = vieModel.CurrentVideo;
if (video.SubSectionList?.Count > 0) {
    // 分段视频
    foreach (var path in video.SubSectionList.Select(arg => arg.Value)) {
        if (File.Exists(path)) {
            try {
                FileSystem.DeleteFile(path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                num++;
            } catch (Exception ex) {
            Logger.Error(ex);
        }
}
}
} else {
if (File.Exists(video.Path)) {
    try {
        FileSystem.DeleteFile(video.Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
        num++;
    } catch (Exception ex) {
    Logger.Error(ex);
}
}
}

DeleteID(null, null);
}

public void DeleteID(object sender, RoutedEventArgs e)
{
    if (new MsgBox(LangManager.GetValueByKey("IsToDeleteFromLibrary")).ShowDialog() == false) {
        return;
    }

windowMain?.DeleteID(new List<Video> { vieModel.CurrentVideo }, true);
int idx = DataIDs.IndexOf(vieModel.CurrentVideo.DataID);
DataIDs.RemoveAll(arg => arg == vieModel.CurrentVideo.DataID);
if (idx >= DataIDs.Count)
idx = 0;
if (idx >= 0 && idx < DataIDs.Count) {
    CancelLoadImage = false;
    vieModel.Load(DataIDs[idx]);
    vieModel.SelectImageIndex = 0;
} else {
this.Close();
}
}

private void OpenWeb(object sender, RoutedEventArgs e)
{
    vieModel.CurrentVideo.OpenWeb();
}

public void TranslateMovie(object sender, RoutedEventArgs e)
{
    // if (IsTranslating) return;

    // if (!Properties.Settings.Default.Enable_TL_BAIDU & !Properties.Settings.Default.Enable_TL_YOUDAO) { SuperControls.Style.MessageCard.Warning("请设置【有道翻译】并测试"); IsTranslating = false; return; }
    // string result = "";
    // MySqlite dataBase = new MySqlite("Translate");

    // CurrentVideo movie = vieModel.CurrentVideo;
    // IsTranslating = true;
    ////检查是否已经翻译过，如有提示
    // if (!string.IsNullOrEmpty(dataBase.SelectByField("translate_title", "youdao", movie.id)))
    // {
    // if (new MsgBox( SuperControls.Style.LangManager.GetValueByKey("AlreadyTranslate")).ShowDialog() == false)
    // {
    // IsTranslating = false;
    // return;
    // }

    // }

    // string title = DataBase.SelectInfoByID("title", "movie", movie.id);

    // if (title != "")
    // {

    // if (Properties.Settings.Default.Enable_TL_YOUDAO) result = await Translate.Youdao(title);
    // //保存
    // if (result != "")
    // {
    // dataBase.SaveYoudaoTranslateByID(movie.id, title, result, "title");
    // movie.title = result;
    // UpdateInfo(movie);
    // }
    // else
    // {
    // SuperControls.Style.MessageCard.Info(SuperControls.Style.LangManager.GetValueByKey("TranslateFail"));
    // }

    // }
    // string plot = DataBase.SelectInfoByID("plot", "movie", movie.id);
    // if (plot != "")
    // {
    // if (Properties.Settings.Default.Enable_TL_YOUDAO) result = await Translate.Youdao(plot);
    // //保存
    // if (result != "")
    // {
    // dataBase.SaveYoudaoTranslateByID(movie.id, plot, result, "plot");
    // movie.plot = result;
    // UpdateInfo(movie);
    // //SuperControls.Style.MessageCard.Info(SuperControls.Style.LangManager.GetValueByKey("TranslateSuccess"));
    // }
    // else
    // {
    // SuperControls.Style.MessageCard.Info(SuperControls.Style.LangManager.GetValueByKey("TranslateFail"));
    // }

    // }
    // dataBase.CloseDB();
    // IsTranslating = false;
}

private void Grid_KeyUp(object sender, KeyEventArgs e)
{
    if (e.Key == Key.Escape)
    this.Close();
    else if (e.Key == Key.Left)
    PreviousMovie(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
    else if (e.Key == Key.Right)
    NextMovie(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
    else if (e.Key == Key.Space || e.Key == Key.Enter || e.Key == Key.P)
    PlayVideo(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
}
    }
}
