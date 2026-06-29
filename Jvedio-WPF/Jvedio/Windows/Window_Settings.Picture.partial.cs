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
    public partial class Window_Settings
    {
        // Tab: Picture
        private void SavePath()
        {
            Dictionary<string, string> dict = (Dictionary<string, string>)vieModel.PicPaths[PathType.RelativeToData.ToString()];
            dict["BigImagePath"] = vieModel.BigImagePath;
            dict["SmallImagePath"] = vieModel.SmallImagePath;
            dict["PreviewImagePath"] = vieModel.PreviewImagePath;
            dict["ScreenShotPath"] = vieModel.ScreenShotPath;
            dict["ActorImagePath"] = vieModel.ActorImagePath;
            vieModel.PicPaths[PathType.RelativeToData.ToString()] = dict;
            ConfigManager.Settings.PicPathJson = JsonConvert.SerializeObject(vieModel.PicPaths);
        }

        private void ImageSelectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = (sender as ComboBox).SelectedIndex;
            if (idx >= 0 && vieModel != null && idx < VieModel_Settings.PIC_PATH_MODE_COUNT) {
                PathType type = (PathType)idx;
                if (type != PathType.RelativeToData)
                    vieModel.BasePicPath = vieModel.PicPaths[type.ToString()].ToString();
            }
        }

        private void SetBasePicPath(object sender, RoutedEventArgs e)
        {
            var path = FileHelper.SelectPath(this);
            if (Directory.Exists(path)) {
                if (!path.EndsWith("\\"))
                    path += "\\";
                vieModel.BasePicPath = path;
            }
        }
    }
}
