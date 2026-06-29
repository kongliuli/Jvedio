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
        // Tag stamp / labels
private void onTagStampRemove(long dataID, long tagID)
{
    if (dataID != vieModel.CurrentVideo.DataID)
    return;
    Video video = vieModel.CurrentVideo;
    Video.RefreshTagStamp(ref video, tagID, true);
}

private void onDownloadPreview(long dataID, string path, byte[] fileByte)
{
    Dispatcher.Invoke(() => {
        lock (RefreshLock) {
            OnDownloadPreview(dataID, path, fileByte);
        }
});
}

private void onDownloadSuccess(DownLoadTask task)
{
    Dispatcher.Invoke(() => {
        lock (RefreshLock) {
            if (DataID == task.DataID)
            Refresh();
        }
});
}

private void onScreenShotCompleted(bool ok, long dataId)
{
    Dispatcher.Invoke(async () => {
        if (vieModel.ShowScreenShot && dataId.Equals(vieModel.CurrentVideo.DataID)) {
            // 加入到列表
            if (vieModel.CurrentVideo.PreviewImagePathList == null)
            vieModel.CurrentVideo.PreviewImagePathList = new ObservableCollection<string>();
            if (vieModel.CurrentVideo.PreviewImageList == null)
            vieModel.CurrentVideo.PreviewImageList = new ObservableCollection<BitmapSource>();
            await LoadImage(true);
            vieModel.ScreenShotCount = vieModel.CurrentVideo.PreviewImagePathList.Count;
        }
});
}

private void onTagStampChange(long id, long newTag, bool deleted)
{
    if (id != vieModel.CurrentVideo.DataID)
    return;

    Video video = vieModel.CurrentVideo;
    Video.RefreshTagStamp(ref video, newTag, deleted);
}

private void RefreshActor(long actorID)
{
    ShowActor();
}

///
/// 移除【新加入】标记
///
private void RemoveNewAddTag()
{
    if (vieModel.CurrentVideo != null && vieModel.CurrentVideo.TagStamp != null &&
    vieModel.CurrentVideo.TagStamp.Any(arg => arg.TagID == TagStamp.TAG_ID_NEW_ADD)) {
        string sql = $"delete from metadata_to_tagstamp where TagID='{TagStamp.TAG_ID_NEW_ADD}' and DataID='{DataID}'";
        tagStampMapper.ExecuteNonQuery(sql);
        LibraryEventBus.RaiseTagStampPanelRefresh();
        windowMain?.RefreshData(DataID);
    }
}

private void onRefreshTagStemp(long tagID)
{
    Video video = vieModel.CurrentVideo;
    ObservableCollection<TagStamp> tagStamp = video.TagStamp;
    if (tagStamp == null || !tagStamp.Any(arg => arg.TagID == tagID))
    return;
    Video.SetTagStamps(ref video);
}

public void RemoveTag(long tagID)
{
    if (vieModel.CurrentVideo != null && vieModel.CurrentVideo.TagStamp != null &&
        vieModel.CurrentVideo.TagStamp.Count > 0) {
        int idx = -1;
        for (int i = 0; i < vieModel.CurrentVideo.TagStamp.Count; i++) {
            if (vieModel.CurrentVideo.TagStamp[i].TagID == tagID) {
                idx = i;
                break;
            }
        }
        if (idx >= 0 && idx < vieModel.CurrentVideo.TagStamp.Count) {
            vieModel.CurrentVideo.TagStamp.RemoveAt(idx);
        }
    }
}

private void Border_MouseUp(object sender, MouseButtonEventArgs e)
{
    Border border = sender as Border;
    TextBlock textBlock = border.Child as TextBlock;
    string text = textBlock.Text;
    string value = text.Substring(0, text.IndexOf("("));
    ObservableString observableString = new ObservableString(value);
    if (vieModel.CurrentVideo.LabelList.Contains(observableString)) {
        searchLabelPopup.IsOpen = false;
        return;
    }
vieModel.CurrentVideo.LabelList.Add(observableString);
LabelChanged(null, null);
searchLabelPopup.IsOpen = false;
}
private void DeleteVideoTagStamp(object sender, RoutedEventArgs e)
{
    MenuItem menuItem = sender as MenuItem;
    Border border = (menuItem.Parent as ContextMenu).PlacementTarget as Border;
    if (border == null || border.Tag == null)
    return;
    long.TryParse(border.Tag.ToString(), out long tagID);

    ItemsControl itemsControl = border.FindParentOfType<ItemsControl>();
    if (itemsControl == null || itemsControl.Tag == null || itemsControl.ItemsSource == null)
    return;
    long.TryParse(itemsControl.Tag.ToString(), out long DataID);
    if (tagID <= 0 || DataID <= 0)
    return;
    ObservableCollection<TagStamp> tagStamps = itemsControl.ItemsSource as ObservableCollection<TagStamp>;
    if (tagStamps == null)
    return;
    TagStamp tagStamp = tagStamps.Where(arg => arg.TagID.Equals(tagID)).FirstOrDefault();
    if (tagStamp != null) {
        tagStamps.Remove(tagStamp);
        string sql = $"delete from metadata_to_tagstamp where TagID='{tagID}' and DataID='{DataID}'";
        tagStampMapper.ExecuteNonQuery(sql);
    }
}

private void StackPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
{
    CanRateChange = true;
}

private void CopyText(object sender, MouseButtonEventArgs e)
{
    TextBlock textBlock = sender as TextBlock;
    ClipBoard.TrySetDataObject(textBlock.Text);
}

private long getDataID(UIElement o)
{
    FrameworkElement element = o as FrameworkElement;
    if (element == null)
    return -1;
    Grid grid = element.FindParentOfType<Grid>("rootGrid");
    if (grid != null && grid.Tag != null) {
        long.TryParse(grid.Tag.ToString(), out long result);
        return result;
    }

return -1;
}

private void AssocDataRate_ValueChanged(object sender, EventArgs e)
{
    if (!CanRateChange)
    return;
    Rating rate = (Rating)sender;
    StackPanel stackPanel = rate.Parent as StackPanel;
    long id = getDataID(stackPanel);
    if (id <= 0)
    return;
    metaDataMapper.UpdateFieldById("Grade", rate.Value.ToString(), id);
    CanRateChange = false;
}

private void LabelChanged(object sender, RoutedEventArgs eventArgs)
{
    List<string> list = new List<string>();
    if (vieModel.CurrentVideo.LabelList != null)
    list = vieModel.CurrentVideo.LabelList.Select(arg => arg.Value).ToList();
    vieModel.CurrentVideo.Label = string.Join(SuperUtils.Values.ConstValues.SeparatorString, list);
    MapperManager.metaDataMapper.SaveLabel(vieModel.CurrentVideo.toMetaData());
    // 标签已改变
    vieModel.GetLabels();
}

private void AddToLabel(object sender, RoutedEventArgs e)
{
    searchLabelPopup.IsOpen = true;
}
    }
}
