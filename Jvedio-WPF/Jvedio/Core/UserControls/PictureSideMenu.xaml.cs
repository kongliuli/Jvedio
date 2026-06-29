using Jvedio.Core.Enums;
using Jvedio.Core.Library;
using Jvedio.Core.UI;
using Jvedio.Core.UserControls;
using Jvedio.Entity;
using SuperControls.Style;
using SuperControls.Style.Windows;
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
            ReloadFolderTree();
            ReloadCollections();
            LibraryEventBus.ScanCompleted += OnScanCompleted;
            LibraryEventBus.PictureBrowseChanged += OnPictureBrowseChanged;
            LibraryEventBus.PictureCollectionChanged += OnPictureCollectionChanged;
        }

        private void OnScanCompleted(object sender, ScanCompletedEventArgs e)
        {
            if (e?.DataType == DataType.Picture) {
                ReloadFolderTree();
                ReloadCollections();
            }
        }

        private void OnPictureBrowseChanged(object sender, PictureBrowseChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(PictureBrowseContext.SelectedFolderPath))
                folderTreeView?.SetValue(TreeView.SelectedItemProperty, null);
        }

        private void OnPictureCollectionChanged(object sender, EventArgs e) => ReloadCollections();

        public void ReloadFolderTree()
        {
            if (folderTreeView == null)
                return;
            var nodes = PictureFolderTreeService.LoadNodes(ConfigManager.Main.CurrentDBId);
            PictureFolderTreeService.PopulateTreeView(
                folderTreeView,
                PictureFolderTreeService.BuildForest(nodes));
        }

        public void ReloadCollections()
        {
            if (collectionStackPanel == null)
                return;
            collectionStackPanel.Children.Clear();
            foreach (PictureCollectionSummary summary in PictureCollectionService.ListCollections(ConfigManager.Main.CurrentDBId)) {
                var btn = new PathRadioButton {
                    GroupName = "videoRadioButton",
                    MainText = summary.Name,
                    SubText = summary.ItemCount.ToString(),
                    Tag = PictureBrowseContext.CollectionCommandPrefix + summary.CollectionID,
                    Style = (Style)FindResource("BasePathRadioButton"),
                    Path = (System.Windows.Media.Geometry)FindResource("GeoLabel"),
                };
                btn.Click += HandleSideClick;
                collectionStackPanel.Children.Add(btn);
            }
        }

        private void CreateCollection_Click(object sender, RoutedEventArgs e)
        {
            var input = new DialogInput(LangManager.GetValueByKey("NewPictureCollection"));
            if (input.ShowDialog(App.Current.MainWindow) != true)
                return;
            string name = input.Text?.Trim();
            if (string.IsNullOrEmpty(name))
                return;
            var created = PictureCollectionService.Create(ConfigManager.Main.CurrentDBId, name);
            if (created == null)
                return;
            ReloadCollections();
            onSideButtonCmd?.Invoke(PictureBrowseContext.CollectionCommandPrefix + created.CollectionID);
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

        private void FolderTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null)
                return;
            if (e.NewValue is TreeViewItem item && item.Tag is string path && !string.IsNullOrWhiteSpace(path))
                onSideButtonCmd?.Invoke(PictureBrowseContext.FolderCommandPrefix + path);
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
            foreach (PathRadioButton item in collectionStackPanel.Children.OfType<PathRadioButton>()) {
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
