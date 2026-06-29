using Jvedio.Mapper;
using Jvedio.Entity;
using Jvedio.Entity.CommonSQL;
using SuperUtils.Framework.ORM.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using static Jvedio.MapperManager;

namespace Jvedio.Core.UI
{
    public static class FilterQueryBuilder
    {
        public static void ApplyTagStampFilter(
            SelectWrapper<Video> wrapper,
            ref string joinSql,
            IEnumerable<TagStamp> tagStamps)
        {
            if (tagStamps == null)
                return;

            List<TagStamp> stamps = tagStamps.ToList();
            if (stamps.Count == 0)
                return;

            bool allFalse = stamps.All(item => item.Selected == false);
            if (allFalse) {
                wrapper.IsNull("TagID");
                joinSql += VideoMapper.SQL_LEFT_JOIN_TAGSTAMP;
                return;
            }

            bool allTrue = stamps.All(item => item.Selected == true);
            if (!allTrue) {
                wrapper.In("metadata_to_tagstamp.TagID", stamps.Where(item => item.Selected).Select(item => item.TagID.ToString()));
                joinSql += VideoMapper.SQL_JOIN_TAGSTAMP;
            }
        }

        public static void ApplyPlayableFilter(SelectWrapper<Video> wrapper, IEnumerable<RadioButton> radioButtons)
        {
            if (!ConfigManager.Settings.PlayableIndexCreated)
                return;

            List<RadioButton> plays = radioButtons?.ToList();
            if (plays == null || plays.Count == 0)
                return;

            int idx = 0;
            for (int i = 0; i < plays.Count; i++) {
                if ((bool)plays[i].IsChecked) {
                    idx = i;
                    break;
                }
            }
            if (idx > 0)
                wrapper.Eq("metadata.PathExist", idx - 1);
        }

        public static void ApplyVideoTypeFilter(SelectWrapper<Video> wrapper, IEnumerable<ToggleButton> allTypeButtons)
        {
            List<ToggleButton> allMenus = allTypeButtons?.ToList();
            if (allMenus == null || allMenus.Count == 0)
                return;

            List<ToggleButton> checkedMenus = allMenus.Where(t => (bool)t.IsChecked).ToList();
            int checkedCount = checkedMenus.Count;
            if (checkedCount <= 0 || checkedCount >= 4)
                return;

            const string field = "VideoType";
            if (checkedCount == 1) {
                int idx = allMenus.IndexOf(checkedMenus[0]) - 1;
                if (idx >= 0)
                    wrapper.Eq(field, idx);
            } else if (checkedCount == 2) {
                int idx1 = allMenus.IndexOf(checkedMenus[0]) - 1;
                int idx2 = allMenus.IndexOf(checkedMenus[1]) - 1;
                if (idx1 >= 0 && idx2 >= 0)
                    wrapper.Eq(field, idx1).LeftBracket().Or().Eq(field, idx2).RightBracket();
            } else if (checkedCount == 3) {
                int idx1 = allMenus.IndexOf(checkedMenus[0]) - 1;
                int idx2 = allMenus.IndexOf(checkedMenus[1]) - 1;
                int idx3 = allMenus.IndexOf(checkedMenus[2]) - 1;
                if (idx1 >= 0 && idx2 >= 0 && idx3 >= 0)
                    wrapper.Eq(field, idx1).LeftBracket().Or().Eq(field, idx2).Or().Eq(field, idx3).RightBracket();
            }
        }

        public static void ApplyPictureModeFilter(
            SelectWrapper<Video> wrapper,
            ref string joinSql,
            IEnumerable<RadioButton> pictureRadios,
            bool pictureReverse)
        {
            if (!ConfigManager.Settings.PictureIndexCreated)
                return;

            List<RadioButton> plays = pictureRadios?.ToList();
            if (plays == null || plays.Count == 0)
                return;

            int idx = 0;
            for (int i = 0; i < plays.Count; i++) {
                if ((bool)plays[i].IsChecked) {
                    idx = i;
                    break;
                }
            }
            if (idx <= 0)
                return;

            int exists = pictureReverse ? 0 : 1;
            joinSql += VideoMapper.SQL_JOIN_COMMON_PICTURE_EXIST;
            wrapper.Eq("common_picture_exist.PathType", ConfigManager.Settings.PicPathMode)
                .Eq("common_picture_exist.ImageType", idx - 1)
                .Eq("common_picture_exist.Exist", exists);
        }

        public static void ApplyDurationFilter(
            SelectWrapper<Video> wrapper,
            IEnumerable<ToggleButton> timeButtons,
            IList<int> timeThresholds)
        {
            List<ToggleButton> timeList = timeButtons?.ToList();
            if (timeList == null || timeThresholds == null)
                return;

            ToggleButton timeButton = timeList.FirstOrDefault(item => (bool)item.IsChecked);
            if (timeButton == null)
                return;
            int timeIndex = timeList.IndexOf(timeButton);
            if (timeIndex <= 0)
                return;

            const string field = "Duration";
            if (timeIndex < 5)
                wrapper.Ge(field, timeThresholds[timeIndex - 1]).Le(field, timeThresholds[timeIndex]);
            else if (timeIndex == 5)
                wrapper.Ge(field, timeThresholds[timeIndex]);
        }

        public static void ApplySizeFilter(
            SelectWrapper<Video> wrapper,
            IEnumerable<RadioButton> sizeRadios,
            IList<long> sizeThresholds)
        {
            List<RadioButton> sizeList = sizeRadios?.ToList();
            if (sizeList == null || sizeThresholds == null)
                return;

            RadioButton sizeButton = sizeList.FirstOrDefault(item => (bool)item.IsChecked);
            if (sizeButton == null)
                return;
            int sizeIndex = sizeList.IndexOf(sizeButton);
            if (sizeIndex <= 0)
                return;

            const string field = "Size";
            if (sizeIndex < 5)
                wrapper.Ge(field, sizeThresholds[sizeIndex - 1]).Le(field, sizeThresholds[sizeIndex]);
            else if (sizeIndex == 5)
                wrapper.Ge(field, sizeThresholds[sizeIndex - 1]);
        }

        public static void ApplyGradeFilter(SelectWrapper<Video> wrapper, double minRate, double maxRate, double minimum, double maximum)
        {
            if (minRate == minimum && maxRate == maximum)
                return;

            const string field = "Grade";
            if (minRate == maxRate)
                wrapper.Eq(field, minRate);
            else
                wrapper.Ge(field, minRate).Le(field, maxRate);
        }

        public static void ApplyYearFilter(SelectWrapper<Video> wrapper, IEnumerable<ToggleButton> yearButtons, int totalYearCount)
        {
            List<ToggleButton> yearList = yearButtons?.Where(item => (bool)item.IsChecked).ToList();
            if (yearList == null || yearList.Count == 0 || yearList.Count == totalYearCount)
                return;

            const string field = "ReleaseYear";
            int count = yearList.Count;
            List<int> list = yearList.Select(item => int.Parse(item.Content.ToString())).ToList();
            wrapper.Eq(field, list[0]).LeftBracket().Or();
            for (int i = 1; i < count - 1; i++)
                wrapper.Eq(field, list[i]).Or();
            wrapper.Eq(field, list[count - 1]).RightBracket();
        }

        public static void ApplyToggleFieldFilter(
            SelectWrapper<Video> wrapper,
            string field,
            IEnumerable<ToggleButton> checkedButtons)
        {
            List<ToggleButton> list = checkedButtons?.ToList();
            if (list == null || list.Count == 0)
                return;

            int count = list.Count;
            List<string> values = list.Select(item => item.Content.ToString()).ToList();
            wrapper.Like(field, values[0]).LeftBracket().Or();
            for (int i = 1; i < count - 1; i++)
                wrapper.Like(field, values[i]).Or();
            wrapper.Like(field, values[count - 1]).RightBracket();
        }

        public static void ApplyMultiToggleFieldFilter(
            SelectWrapper<Video> wrapper,
            string field,
            IEnumerable<ToggleButton> panelButtons,
            int panelChildCount)
        {
            List<ToggleButton> selected = panelButtons?.Where(item => (bool)item.IsChecked).ToList();
            if (selected == null || selected.Count == 0 || selected.Count == panelChildCount)
                return;
            ApplyToggleFieldFilter(wrapper, field, selected);
        }
    }
}
