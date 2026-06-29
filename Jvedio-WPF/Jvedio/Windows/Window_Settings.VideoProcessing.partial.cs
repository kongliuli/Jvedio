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
        // Tab: VideoProcessing
        private void SetFFMPEGPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.Title = SuperControls.Style.LangManager.GetValueByKey("ChooseFFmpeg");
            openFileDialog1.FileName = "ffmpeg.exe";
            openFileDialog1.Filter = "ffmpeg.exe|*.exe";
            openFileDialog1.FilterIndex = 1;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string exePath = openFileDialog1.FileName;
                if (File.Exists(exePath)) {
                    if (new FileInfo(exePath).Name.ToLower() == "ffmpeg.exe")
                        vieModel.FFMPEG_Path = exePath;
                }
            }
        }

        private void CopyFFmpegUrl(object sender, MouseButtonEventArgs e)
        {
            FileHelper.TryOpenUrl(FFMPEG_URL);
        }
    }
}
