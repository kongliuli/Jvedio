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
        // Context menu / assoc / misc
private void Rate_ValueChanged(object sender, EventArgs e)
{
    if (vieModel.CurrentVideo != null) {
        vieModel.SaveLove();

        // 更新主界面
        windowMain?.RefreshGrade(vieModel.CurrentVideo.DataID, vieModel.CurrentVideo.Grade);
    }
}

private void ContextMenu_PreviewKeyUp(object sender, KeyEventArgs e)
{
    ContextMenu contextMenu = sender as ContextMenu;
    if (e.Key == Key.D) {
        MenuItem menuItem = GetMenuItem(contextMenu, SuperControls.Style.LangManager.GetValueByKey("Menu_DeleteInfo"));
        if (menuItem != null)
        DeleteID(menuItem, new RoutedEventArgs());
    } else if (e.Key == Key.T) {
    MenuItem menuItem = GetMenuItem(contextMenu, SuperControls.Style.LangManager.GetValueByKey("Menu_DeleteFile"));
    if (menuItem != null)
    DeleteFile(menuItem, new RoutedEventArgs());
} else if (e.Key == Key.S) {
MenuItem menuItem = GetMenuItem(contextMenu, SuperControls.Style.LangManager.GetValueByKey("Menu_SyncInfo"));
if (menuItem != null)
DownLoad(menuItem, new RoutedEventArgs());
} else if (e.Key == Key.E) {
MenuItem menuItem = GetMenuItem(contextMenu, SuperControls.Style.LangManager.GetValueByKey("Menu_EditInfo"));
if (menuItem != null)
EditInfo(menuItem, new RoutedEventArgs());
} else if (e.Key == Key.W) {
MenuItem menuItem = GetMenuItem(contextMenu, SuperControls.Style.LangManager.GetValueByKey("Menu_OpenWebSite"));
if (menuItem != null)
OpenWeb(menuItem, new RoutedEventArgs());
} else if (e.Key == Key.C) {
MenuItem menuItem = GetMenuItem(contextMenu, SuperControls.Style.LangManager.GetValueByKey("Menu_CopyFile"));
if (menuItem != null)
CopyFile(menuItem, new RoutedEventArgs());
}

contextMenu.IsOpen = false;
}

private void ShowMagnets()
{
    if (ConfigManager.Settings.TeenMode
    || vieModel.CurrentVideo.Magnets == null
    || vieModel.CurrentVideo.Magnets.Count == 0)
    return;

    CopyMagnetsMenuItem.Items.Clear();
    foreach (var magnet in vieModel.CurrentVideo.Magnets) {
        if (magnet.Tags == null)
        continue;
        MenuItem menuItem = new MenuItem();
        string tag = string.Empty;
        if (magnet.Tags.Count > 0)
        tag = "（" + string.Join(" ", magnet.Tags) + "）";

        menuItem.Header = $"{magnet.Releasedate} {tag} {magnet.Size.ToProperFileSize()} {magnet.Title}";
        menuItem.ToolTip = menuItem.Header;
        menuItem.Click += (s, ev) => {
            ClipBoard.TrySetDataObject(magnet.MagnetLink);
            MessageNotify.Success(LangManager.GetValueByKey("Message_Copied"));
        };
    CopyMagnetsMenuItem.Items.Add(menuItem);
}
}
private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
{
    if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported && TaskbarInstance != null) {
        TaskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal, this);
        TaskbarInstance.SetProgressValue((int)e.NewValue, 100, this);
        if (e.NewValue >= 100 || e.NewValue <= 0)
        TaskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress, this);
    }
}

// todo 刮削
private void ProgressBar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
{
    //if (ProgressBar.Visibility == Visibility.Collapsed && Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported && TaskbarInstance != null) {
    // TaskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress, this);
    //}
}

private void TextBox_MouseEnter(object sender, MouseEventArgs e)
{
    TextBlock textBlock = (TextBlock)sender;
    textBlock.TextDecorations = System.Windows.TextDecorations.Underline;
}

private void TextBox_MouseLeave(object sender, MouseEventArgs e)
{
    TextBlock textBlock = (TextBlock)sender;
    textBlock.TextDecorations = null;
}

private void OpenFilePath(object sender, MouseButtonEventArgs e)
{
    FileHelper.TryOpenSelectPath(vieModel.CurrentVideo.Path);
}

private void GetPlot(object sender, RoutedEventArgs e)
{

}

public void ShowSubsection(object sender)
{
    if (sender is Grid grid && grid.ContextMenu is ContextMenu contextMenu) {
        contextMenu.Items.Clear();
        for (int i = 0; i < vieModel.CurrentVideo.SubSectionList?.Count; i++) {
            string filepath = vieModel.CurrentVideo.SubSectionList[i].Value; // 这样可以，放在 PlayVideoWithPlayer 就超出索引
            MenuItem menuItem = new MenuItem();
            menuItem.Header = i + 1;
            menuItem.Click += (s, _) => Video.PlayVideoWithPlayer(filepath, DataID);
            contextMenu.Items.Add(menuItem);
        }

    contextMenu.IsOpen = true;
    contextMenu.Visibility = Visibility.Visible;
}
}

private void PlayVideo(object sender, MouseButtonEventArgs e)
{
    if (vieModel == null || vieModel.CurrentVideo == null)
    return;

    if (vieModel.CurrentVideo.HasSubSection)
    ShowSubsection(sender);
    else
    Video.PlayVideoWithPlayer(vieModel.CurrentVideo.Path, vieModel.CurrentVideo.DataID);
}

private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    ComboBox comboBox = sender as ComboBox;
    videoMapper.UpdateField("VideoType", comboBox.SelectedIndex.ToString(), new SelectWrapper<Video>().Eq("DataID", DataID));
}

private void DownLoadInfo(object sender, MouseButtonEventArgs e)
{
    DownLoad(null, null);
}

private void Image_MouseEnter(object sender, MouseEventArgs e)
{
    FrameworkElement element = sender as FrameworkElement;
    Grid grid = element.FindParentOfType<Grid>("rootGrid");
    Border border = grid.Children[0] as Border;
    border.Background = StyleManager.Common.HighLight.Background;
}

private void Image_MouseLeave(object sender, MouseEventArgs e)
{
    FrameworkElement element = sender as FrameworkElement;
    Grid grid = element.FindParentOfType<Grid>("rootGrid");
    Border border = grid.Children[0] as Border;
    border.Background = (SolidColorBrush)Application.Current.Resources["ListBoxItem.Background"];
}

private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
{
    ScrollViewer scrollViewer = sender as ScrollViewer;
    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
    e.Handled = true;
}

private void OpenPath(object sender, RoutedEventArgs e)
{
    MenuItem menu = sender as MenuItem;
    string header = menu.Header.ToString();
    vieModel.CurrentVideo.OpenPath(Video.StringToImageType(header));
}

private void ViewAssocDatas(object sender, RoutedEventArgs e)
{
    onViewAssoData?.Invoke(vieModel.CurrentVideo.DataID);
}

private void ShowAssocSubSection(object sender, RoutedEventArgs e)
{
    if (vieModel.ViewAssociationDatas == null)
        return;

    Button button = sender as Button;
    long dataID = getDataID(button);
    if (dataID <= 0)
        return;

    ContextMenu contextMenu = button.ContextMenu;
    contextMenu.Items.Clear();

    Video video = vieModel.ViewAssociationDatas.Where(arg => arg.DataID == dataID).FirstOrDefault();
    if (video != null) {
        for (int i = 0; i < video.SubSectionList?.Count; i++) {
            string filepath = video.SubSectionList[i].Value;
            MenuItem menuItem = new MenuItem();
            menuItem.Header = i + 1;
            menuItem.Click += (s, _) => Video.PlayVideoWithPlayer(filepath, dataID);
            contextMenu.Items.Add(menuItem);
        }

        contextMenu.IsOpen = true;
    }
}

private void CopyVideoInfo(object sender, RoutedEventArgs e)
{
    StringBuilder builder = new StringBuilder();
    foreach (var item in infoStackPanel.Children) {
        if (item is StackPanel stackPanel) {
            foreach (FrameworkElement element in stackPanel.Children) {
                if (element is TextBlock textBlock)
                    builder.Append(textBlock.Text);
                else if (element is TextBox textBox)
                    builder.Append(textBox.Text);
            }

            builder.Append(Environment.NewLine);
        }
    }

    if (builder.Length > 0)
        ClipBoard.TrySetDataObject(builder.ToString());
    else
        MessageNotify.Error("无信息！");
}
    }
}
