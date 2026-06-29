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
        // Actor panel
public async void ShowActor()
{
    vieModel.CurrentActorList = new ObservableCollection<ActorInfo>();

    // 加载演员
    if (vieModel.CurrentVideo.ActorInfos != null) {
        for (int i = 0; i < vieModel.CurrentVideo.ActorInfos.Count; i++) {
            if (CancelLoadImage)
            break;
            ActorInfo actorInfo = vieModel.CurrentVideo.ActorInfos[i];

            // 加载图片
            string imagePath = actorInfo.GetImagePath(vieModel.CurrentVideo.Path);
            BitmapImage smallimage = ImageCache.Get(imagePath);
            if (smallimage == null) {
                smallimage = MetaData.DefaultActorImage;
                //// 根据地址下载图片
                // if (!string.IsNullOrEmpty(actorInfo.ImageUrl))
                // {
                // string url = actorInfo.ImageUrl;
                // string ext = Path.GetExtension(url);
                // string dir = Path.GetDirectoryName(imagePath);
                // string name = Path.GetFileNameWithoutExtension(imagePath);
                // string saveFileName = Path.Combine(dir, name + ext);

                // Task.Run(() =>
                // {
                // StartDownLoadActorImage();
                // });

                // }
            }

        actorInfo.SmallImage = smallimage;
        await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadActorDelegate(LoadActor), actorInfo);
    }
}
}

private void StartDownLoadActorImage()
{
    // await HttpHelper.AsyncDownLoadFile(actorInfo.ImageUrl, CrawlerHeader.Default);
}
    }
}
