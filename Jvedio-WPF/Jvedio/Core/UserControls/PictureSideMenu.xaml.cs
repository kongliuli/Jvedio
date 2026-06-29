using Jvedio.Core.Enums;
using Jvedio.Core.UI;
using Jvedio.Core.UserControls;
using Jvedio.Entity;
using SuperControls.Style;
using SuperUtils.Framework.ORM.Wrapper;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using static Jvedio.App;
using static Jvedio.MapperManager;

namespace Jvedio.Core.UserControls
{
    public partial class PictureSideMenu : UserControl, INotifyPropertyChanged, ISideMenuStatistics
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Action onStatistic { get; set; }
        public Action<object> onSideButtonCmd { get; set; }

        private long _AllVideoCount;
        public long AllVideoCount {
            get { return _AllVideoCount; }
            set { _AllVideoCount = value; RaisePropertyChanged(); }
        }

        private double _FavoriteVideoCount;
        public double FavoriteVideoCount {
            get { return _FavoriteVideoCount; }
            set { _FavoriteVideoCount = value; RaisePropertyChanged(); }
        }

        private long _RecentWatchCount;
        public long RecentWatchCount {
            get { return _RecentWatchCount; }
            set { _RecentWatchCount = value; RaisePropertyChanged(); }
        }

        private long _AllLabelCount;
        public long AllLabelCount {
            get { return _AllLabelCount; }
            set { _AllLabelCount = value; RaisePropertyChanged(); }
        }

        private long _AllGenreCount;
        public long AllGenreCount {
            get { return _AllGenreCount; }
            set { _AllGenreCount = value; RaisePropertyChanged(); }
        }

        private long _AllSeriesCount;
        public long AllSeriesCount {
            get { return _AllSeriesCount; }
            set { _AllSeriesCount = value; RaisePropertyChanged(); }
        }

        public PictureSideMenu()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
        }

        private void ClearRecentWatched(object sender, RoutedEventArgs e)
        {
            SelectWrapper<MetaData> wrapper = new SelectWrapper<MetaData>();
            wrapper.Eq("DBId", ConfigManager.Main.CurrentDBId).Eq("DataType", SideMenuDataTypeHelper.CurrentTypeInt);
            metaDataMapper.UpdateField("ViewDate", string.Empty, wrapper);
            VideoListCommands.NotifyMetadataRefreshed();
            onStatistic?.Invoke();
        }

        public void Statistic(string searchText)
        {
            SideMenuMediaCounts counts = SideMenuStatisticsHelper.ComputeMedia(
                SideMenuDataTypeHelper.CurrentTypeInt, searchText, includeActorAndDirector: false);
            AllVideoCount = counts.AllVideoCount;
            FavoriteVideoCount = counts.FavoriteVideoCount;
            AllLabelCount = counts.AllLabelCount;
            RecentWatchCount = counts.RecentWatchCount;
            AllGenreCount = counts.AllGenreCount;
            AllSeriesCount = counts.AllSeriesCount;
        }

        private void HandleSideClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag != null)
                onSideButtonCmd?.Invoke(element.Tag.ToString());
        }

        public void SetSelected(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
            foreach (PathRadioButton item in firstStackPanel.Children.OfType<PathRadioButton>()) {
                if (item.Tag?.ToString() == text) {
                    item.IsChecked = true;
                    return;
                }
            }
            foreach (PathRadioButton item in secondStackPanel.Children.OfType<PathRadioButton>()) {
                if (item.Tag?.ToString() == text) {
                    item.IsChecked = true;
                    return;
                }
            }
        }
    }
}
