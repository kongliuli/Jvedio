using Google.Protobuf.WellKnownTypes;
using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.Enums;
using Jvedio.Core.FFmpeg;
using Jvedio.Core.Global;
using Jvedio.Core.Library;
using Jvedio.Core.Media;
using Jvedio.Core.Metadata;
using Jvedio.Core.UI;
using Jvedio.Core.Net;
using Jvedio.Core.UserControls.ViewModels;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using Jvedio.Entity.CommonSQL;
using Microsoft.VisualBasic.FileIO;
using SuperControls.Style;
using SuperControls.Style.Windows;
using SuperUtils.Common;
using SuperUtils.Framework.ORM.Utils;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.Framework.Tasks;
using SuperUtils.IO;
using SuperUtils.Time;
using SuperUtils.WPF.VisualTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static Jvedio.App;
using static Jvedio.Core.UserControls.VideoItemEventArgs;
using static Jvedio.MapperManager;
using static SuperUtils.WPF.VisualTools.VisualHelper;
using static SuperUtils.WPF.VisualTools.WindowHelper;

namespace Jvedio.Core.UserControls
{
    public partial class VideoList
    {
        // Tab: Tags
        private void AddTagHandler(object sender, long tagID)
        {
            HandleMenuSelected(sender, 1);

            MenuItem menuItem = sender as MenuItem;
            bool deleted = false;
            if (menuItem != null)
                deleted = !menuItem.IsChecked;

            // 构造 sql 语句
            if (vieModel.SelectedVideo?.Count <= 0)
                return;

            if (deleted) {
                StringBuilder builder = new StringBuilder();
                foreach (var item in vieModel.SelectedVideo) {
                    builder.Append($"delete from metadata_to_tagstamp where DataID={item.DataID} and TagID={tagID};");
                }

                string sql = "begin;" + builder.ToString() + "commit;";
                tagStampMapper.ExecuteNonQuery(sql);
            } else {
                List<string> values = new List<string>();
                foreach (var item in vieModel.SelectedVideo) {
                    values.Add($"({item.DataID},{tagID})");
                }

                if (values.Count <= 0)
                    return;
                string sql = $"insert or replace into metadata_to_tagstamp (DataID,TagID)  values {string.Join(",", values)}";
                tagStampMapper.ExecuteNonQuery(sql);
            }

            onInitTagStamps?.Invoke();

            // 更新主界面
            ObservableCollection<Video> datas = GetVideosByMenu(menuItem, 1);

            if (datas != null) {
                foreach (var item in vieModel.SelectedVideo) {
                    long dataID = item.DataID;
                    if (dataID <= 0 || tagID <= 0 || datas == null || datas.Count == 0)
                        continue;
                    for (int i = 0; i < datas.Count; i++) {
                        if (datas[i].DataID == dataID) {
                            Video video = datas[i];
                            Video.RefreshTagStamp(ref video, tagID, deleted);
                            VideoListCommands.NotifyTagStampChange(dataID, tagID, deleted);
                            break;
                        }
                    }
                }
            }


            if (!vieModel.EditMode)
                vieModel.SelectedVideo.Clear();
        }



        public void RefreshTagStamps(long tagID)
        {
            List<long> toRefreshData = new List<long>();
            foreach (var video in vieModel.CurrentVideoList) {
                string tagIDs = video.TagIDs;
                List<string> list = tagIDs.Split(',').ToList();
                if (!list.Contains(tagID.ToString()))
                    continue;
                toRefreshData.Add(video.DataID);
            }
            foreach (var item in toRefreshData) {
                vieModel.RefreshTagStamp(item);
            }
        }




    }
}
