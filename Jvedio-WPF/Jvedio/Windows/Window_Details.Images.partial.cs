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
        // Preview images
public void OpenExtraImagePath(object sender, RoutedEventArgs e)
{
    string path = GetExtraImagePath(sender as FrameworkElement);
    FileHelper.TryOpenSelectPath(path);
}

private string GetExtraImagePath(FrameworkElement element, int depth = 0)
{
    if (element == null || depth < 0)
    return string.Empty;
    MenuItem menuItem = element as MenuItem;
    ContextMenu contextMenu = menuItem.Parent as ContextMenu;
    if (depth == 1)
    contextMenu = (menuItem.Parent as MenuItem).Parent as ContextMenu;

    if (contextMenu != null && contextMenu.Tag != null &&
    int.TryParse(contextMenu.Tag.ToString(), out int idx) &&
    idx >= 0 && idx < vieModel.CurrentVideo.PreviewImagePathList.Count) {
        return vieModel.CurrentVideo.PreviewImagePathList[idx];
    }
return string.Empty;
}

public async void DeleteImage(object sender, RoutedEventArgs e)
{
    string path = GetExtraImagePath(sender as FrameworkElement);
    if (string.IsNullOrEmpty(path))
    return;
    int idx = vieModel.CurrentVideo.PreviewImagePathList.IndexOf(path);

    if (idx >= 0) {
        FileHelper.TryMoveToRecycleBin(path, 0);
        bool deleteBigImage = vieModel.CurrentVideo.PreviewImageList[0] == vieModel.CurrentVideo.BigImage;
        vieModel.CurrentVideo.PreviewImagePathList.RemoveAt(idx);
        vieModel.CurrentVideo.PreviewImageList.RemoveAt(idx);
        if (deleteBigImage && idx == 0) {
            await Task.Delay(300);
            Refresh();
            windowMain?.RefreshImage(vieModel.CurrentVideo);
        } else if (vieModel.CurrentVideo.PreviewImageList.Count > 0) {
        SetImage(0);
    }
}
}

public async void DeleteAllImage(object sender, RoutedEventArgs e)
{
    for (int i = 0; i < vieModel.CurrentVideo.PreviewImagePathList.Count; i++) {
        string path = vieModel.CurrentVideo.PreviewImagePathList[i];
        FileHelper.TryMoveToRecycleBin(path, 0);
        ImageCache.Remove(path); // 清除缓存
    }

vieModel.CurrentVideo.PreviewImageList.Clear();
vieModel.CurrentVideo.PreviewImagePathList.Clear();
await Task.Delay(300);
Refresh();
windowMain?.RefreshImage(vieModel.CurrentVideo);
}

private void SetToBigPic(object sender, RoutedEventArgs e)
{
    SetToPic(vieModel.CurrentVideo.GetBigImage(searchExt: false), GetExtraImagePath(sender as FrameworkElement, 1));
}

private void SetToSmallPic(object sender, RoutedEventArgs e)
{
    SetToPic(vieModel.CurrentVideo.GetSmallImage(searchExt: false), GetExtraImagePath(sender as FrameworkElement, 1));
}

private void SetToBigAndSmallPic(object sender, RoutedEventArgs e)
{
    string path = GetExtraImagePath(sender as FrameworkElement, 1);
    SetToPic(vieModel.CurrentVideo.GetSmallImage(searchExt: false), path);
    SetToPic(vieModel.CurrentVideo.GetBigImage(searchExt: false), path);
}

private void SetToPic(string target, string origin)
{
    if (File.Exists(target) && new MsgBox("图片已存在，是否覆盖？").ShowDialog() == false) {
        return;
    }

if (File.Exists(origin) && FileHelper.TryCopyFile(origin, target, overwrite: true)) {
    // 清除缓存
    ImageCache.Remove(target);
    Refresh();
    windowMain?.RefreshImage(vieModel.CurrentVideo);
} else {
MessageNotify.Error("设置失败");
}
}
private void SetImage(int idx)
{
    if (CancelLoadImage)
    return;
    if (vieModel.CurrentVideo.PreviewImageList.Count == 0) {
        // 设置为默认图片
        BigImage.Source = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_B.png", UriKind.Relative));
    } else {
    if (idx < vieModel.CurrentVideo.PreviewImageList?.Count)
    BigImage.Source = vieModel.CurrentVideo.PreviewImageList[idx];

    // 设置遮罩
    for (int i = 0; i < imageItemsControl.Items.Count; i++) {
        ContentPresenter c = (ContentPresenter)imageItemsControl.ItemContainerGenerator.ContainerFromItem(imageItemsControl.Items[i]);
        StackPanel stackPanel = VisualHelper.FindElementByName<StackPanel>(c, "ImageStackPanel");
        if (stackPanel != null) {
            Grid grid = stackPanel.Children[0] as Grid;
            Border border = grid.Children[0] as Border;
            if (border != null) {
                if (int.Parse(border.Tag.ToString()) == idx)
                border.Opacity = 0;
                else
                border.Opacity = 0.5;
            }
    }
}
}
}

private void ShowExtraImage(object sender, MouseButtonEventArgs e)
{
    Border border = sender as Border;
    if (border == null || border.Tag == null)
    return;
    int idx = int.Parse(border.Tag.ToString());
    vieModel.SelectImageIndex = idx;
    SetImage(vieModel.SelectImageIndex);
}

private void BigImage_DragOver(object sender, DragEventArgs e)
{
    e.Effects = DragDropEffects.Link;
    e.Handled = true; // 必须加
}

private void BigImage_Drop(object sender, DragEventArgs e)
{
    string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
    string file = dragdropFiles[0];

    if (!FileHelper.IsFile(file))
    return;
    SetToPic(vieModel.CurrentVideo.GetBigImage(searchExt: false), file);
}

private void Border_DragOver(object sender, DragEventArgs e)
{
    e.Effects = DragDropEffects.Link;
    e.Handled = true; // 必须加
}

private void Border_Drop(object sender, DragEventArgs e)
{
    // 分为文件夹和文件
    string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
    List<string> files = new List<string>();
    StringCollection stringCollection = new StringCollection();
    foreach (var item in dragdropFiles) {
        if (FileHelper.IsFile(item))
        files.Add(item);
        else
        stringCollection.Add(item);
    }
List<string> filepaths = new List<string>();
//扫描导入
foreach (var item in stringCollection) {
    try {
        filepaths.AddRange(Directory.GetFiles(item, "*.jpg").ToList ());
    } catch (Exception ex) {
    Console.WriteLine(ex.Message);
    continue;
}
}
if (files.Count > 0)
filepaths.AddRange(files);

//复制文件
string path;
if ((bool)ExtraImageRadioButton.IsChecked) {
    path = vieModel.CurrentVideo.GetExtraImage();
} else
path = vieModel.CurrentVideo.GetScreenShot();

DirHelper.TryCreateDirectory(path);

bool success = false;
foreach (var item in filepaths) {
    try {
        string target = Path.Combine(path, Path.GetFileName(item));
        File.Copy(item, target);
        success = true;
    } catch (Exception ex) {
    Console.WriteLine(ex.Message);
    continue;
}

}
if (success) {
    Refresh();
    MessageNotify.Success($"{SuperControls.Style.LangManager.GetValueByKey("ImportNumber")} {filepaths.Count}");
}
}

private void Border_MouseEnter(object sender, MouseEventArgs e)
{
    Border border = sender as Border;
    border.Opacity = 0;
}

private void Border_MouseLeave(object sender, MouseEventArgs e)
{
    Border border = sender as Border;
    int idx = int.Parse(border.Tag.ToString());
    if (idx != vieModel.SelectImageIndex)
    border.Opacity = 0.5;
    else
    border.Opacity = 0;
}

private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
{
    if (vieModel.CurrentVideo.PreviewImagePathList?.Count == 0)
    return;
    vieModel.SelectImageIndex += e.Delta > 0 ? -1 : 1;

    if (vieModel.SelectImageIndex < 0) {
        vieModel.SelectImageIndex = 0;
    } else if (vieModel.SelectImageIndex >= imageItemsControl.Items.Count) {
    vieModel.SelectImageIndex = imageItemsControl.Items.Count - 1;
}

SetImage(vieModel.SelectImageIndex);

// 滚动到指定的
ContentPresenter presenter = (ContentPresenter)imageItemsControl.ItemContainerGenerator
.ContainerFromItem(imageItemsControl.Items[vieModel.SelectImageIndex]);
presenter?.BringIntoView();
}

private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
{
    if (e.ClickCount == 2) {
        Window_ImageViewer window_ImageViewer = new Window_ImageViewer(this, BigImage.Source);
        window_ImageViewer.ShowDialog();
    }
}

private void Border_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
{
    if (e.Delta < 0)
    NextMovie(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
    else
    PreviousMovie(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
}

private MenuItem GetMenuItem(ContextMenu contextMenu, string header)
{
    if (contextMenu == null || string.IsNullOrEmpty(header))
    return null;
    foreach (MenuItem item in contextMenu.Items) {
        if (item.Header.ToString() == header) {
            return item;
        }
}

return null;
}

private void DisposeImage()
{
    CancelLoadImage = true;
    if (vieModel.CurrentVideo.PreviewImageList != null) {
        for (int i = 0; i < vieModel.CurrentVideo.PreviewImageList.Count; i++) {
            vieModel.CurrentVideo.PreviewImageList[i] = null;
        }
}

GC.Collect();
CancelLoadImage = false;
}

private void AddBigImageToPreviewList()
{
    Video video = vieModel.CurrentVideo;
    BitmapSource bigImage = video.BigImage;
    if (bigImage == null || bigImage == MetaData.DefaultBigImage)
    return;

    string path = video.BigImagePath;
    if (File.Exists(path)) {
        video.PreviewImageList.Add(bigImage);
        video.PreviewImagePathList.Add(path);
    }
}

private async Task<bool> LoadImage(bool isScreenShot = false)
{
    scrollViewer.ScrollToHorizontalOffset(0);

    // 加载大图到预览图
    DisposeImage();

    Video video = vieModel.CurrentVideo;

    video.PreviewImageList = new ObservableCollection<BitmapSource>();
    video.PreviewImagePathList = new ObservableCollection<string>();
    if (!isScreenShot)
    AddBigImageToPreviewList();

    await Task.Run(async () => {
        await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate {
            imageItemsControl.ItemsSource = video.PreviewImageList;
            SetImage(0);
        });
});

// 扫描预览图目录
List<string> screenShotList = await GetImageList(video.GetScreenShot());
List<string> imageList = await GetImageList(video.GetExtraImage());
vieModel.PreviewImageCount = imageList.Count;
vieModel.ScreenShotCount = screenShotList.Count;

List<string> imagePathList = new List<string>();

if (isScreenShot) {
    imagePathList = screenShotList;
} else {
imagePathList = imageList;
}

// 加载预览图/截图
foreach (var path in imagePathList) {
    await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadExtraImageDelegate(LoadExtraImage), BitmapImageFromFile(path));
    await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadExtraPathDelegate(LoadExtraPath), path);
    if (CancelLoadImage)
    break;
}

SetImage(0);
return true;
}

private async Task<List<string>> GetImageList(string imagePath)
{
    return await Task.Run(() => {
        List<string> imagePathList = new List<string>();
        if (Directory.Exists(imagePath)) {
            foreach (var path in FileHelper.TryScanDIr(imagePath, "*.*", System.IO.SearchOption.AllDirectories))
            imagePathList.Add(path);

            if (imagePathList.Count > 0)
            imagePathList = imagePathList.Where(arg => ScanExtensions.PICTURE_EXTENSIONS_LIST.Contains(Path.GetExtension(arg).ToLower())).CustomSort().ToList();
        }
    return imagePathList;
});
}

private void LoadExtraImage(BitmapSource bitmapSource)
{
    vieModel.CurrentVideo.PreviewImageList.Add(bitmapSource);
}

private void LoadExtraPath(string path)
{
    if (vieModel.CurrentVideo.PreviewImagePathList != null)
    vieModel.CurrentVideo.PreviewImagePathList.Add(path);
}

private async void ExtraImageRadioButton_Click(object sender, RoutedEventArgs e)
{
    // 切换为预览图
    await LoadImage();
    scrollViewer.ScrollToHorizontalOffset(0);
}

private async void ScreenShotRadioButton_Click(object sender, RoutedEventArgs e)
{
    // 切换为截图
    await LoadImage(true);
    scrollViewer.ScrollToHorizontalOffset(0);
}
    }
}
