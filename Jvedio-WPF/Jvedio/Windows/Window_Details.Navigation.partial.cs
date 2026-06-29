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
        // Prev/next / move
public void InitDataIDs()
{
    DataIDs = new List<long>();

    if (CurrentWrapperArg != null &&
    CurrentWrapperArg.Wrapper is SelectWrapper<Video> wrapper &&
    wrapper != null &&
    !string.IsNullOrEmpty(CurrentWrapperArg.SQL)) {
        string sql = "select metadata.DataID" + CurrentWrapperArg.SQL + wrapper.ToWhere(false) + wrapper.ToOrder();
        if (!ConfigManager.Main.DetailWindowShowAllMovie)
        sql += wrapper.ToLimit();
        List<Dictionary<string, object>> list = videoMapper.Select(sql);
        if (list != null && list.Count > 0)
        DataIDs = list.Select(arg => long.Parse(arg["DataID"].ToString())).ToList();
    }
}
private void MoveWindow(object sender, MouseEventArgs e)
{
    if (e.LeftButton == MouseButtonState.Pressed)
    this.DragMove();
}

private void scrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
{
    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
    e.Handled = true;
}

public void PreviousMovie(object sender, MouseButtonEventArgs e)
{
    if (DataIDs.Count == 0)
    return;
    CancelLoadImage = true;
    long nextID = 0L;
    for (int i = 0; i < DataIDs.Count; i++) {
        long id = DataIDs[i];
        int idx = i;
        if (id == vieModel.CurrentVideo.DataID) {
            idx--;
            if (idx < 0)
            idx = DataIDs.Count - 1;
            nextID = DataIDs[idx];
            break;
        }
}

if (nextID > 0) {
    CancelLoadImage = false;
    vieModel.Load(nextID);
    vieModel.SelectImageIndex = 0;
    RemoveNewAddTag();
}
}

public void NextMovie(object sender, MouseButtonEventArgs e)
{
    if (DataIDs.Count == 0)
    return;
    CancelLoadImage = true;

    long nextID = 0L;
    for (int i = 0; i < DataIDs.Count; i++) {
        long id = DataIDs[i];
        int idx = i;
        if (id == vieModel.CurrentVideo.DataID) {
            idx++;
            if (idx >= DataIDs.Count)
            idx = 0;
            nextID = DataIDs[idx];
            break;
        }
}

if (nextID > 0) {
    CancelLoadImage = false;
    vieModel.Load(nextID);
    vieModel.SelectImageIndex = 0;
    RemoveNewAddTag();
}
}
    }
}
