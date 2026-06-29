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
    /// <summary>
    /// VideoList.xaml 的交互逻辑
    /// </summary>
    public partial class VideoList : UserControl, ITabItemControl, IMediaListTab
    {
        #region "事件"

        public static Action onStatistic {
            get => VideoListCommands.onStatistic;
            set => VideoListCommands.onStatistic = value;
        }
        public static Action<bool> onSearchingChange {
            get => VideoListCommands.onSearchingChange;
            set => VideoListCommands.onSearchingChange = value;
        }
        public static Action<long, long, bool> onTagStampChange {
            get => VideoListCommands.onTagStampChange;
            set => VideoListCommands.onTagStampChange = value;
        }
        public static Action<List<Video>> onDeleteID {
            get => VideoListCommands.onDeleteID;
            set => VideoListCommands.onDeleteID = value;
        }
        public static Action<string, bool> onWaiting {
            get => VideoListCommands.onWaiting;
            set => VideoListCommands.onWaiting = value;
        }

        public Action<long, float> onGradeChange { get; set; }
        public Action<WrapperEventArg<Video>> onRenderSql { get; set; }
        public Action onInitTagStamps;
        public Action<string> onRenameFile;

        public static readonly RoutedEvent OnItemClickEvent =
            EventManager.RegisterRoutedEvent("OnItemClick", RoutingStrategy.Bubble,
                typeof(VideoItemEventHandler), typeof(VideoList));

        public event VideoItemEventHandler OnItemClick {
            add => AddHandler(OnItemClickEvent, value);
            remove => RemoveHandler(OnItemClickEvent, value);
        }

        public static readonly RoutedEvent OnItemViewAssoEvent =
            EventManager.RegisterRoutedEvent("OnItemViewAsso", RoutingStrategy.Bubble,
                typeof(VideoItemEventHandler), typeof(VideoList));

        public event VideoItemEventHandler OnItemViewAsso {
            add => AddHandler(OnItemViewAssoEvent, value);
            remove => RemoveHandler(OnItemViewAssoEvent, value);
        }

        #endregion

        #region "属性"

        private Style SearchBoxListItemContainerStyle { get; set; }
        private VieModel_VideoList vieModel { get; set; }

        private DispatcherTimer ResizingTimer { get; set; }
        private ScrollViewer dataScrollViewer { get; set; }
        private bool Resizing { get; set; }


        private object RefreshLock { get; set; } = new object();



        public TabItemEx TabItemEx { get; set; }

        private int firstIdx { get; set; } = -1;
        private int secondIdx { get; set; } = -1;
        private int actorFirstIdx { get; set; } = -1;
        private int actorSecondIdx { get; set; } = -1;

        private bool canShowDetails { get; set; }
        public List<ImageSlide> ImageSlides { get; set; }

        private bool CanRateChange { get; set; }

        public MediaListMode ListMode { get; set; } = MediaListMode.Video;

        public void ApplyListModeColumns()
        {
            if (tableData == null || tableData.Columns.Count < MediaListColumnPolicy.ColumnCount)
                return;

            for (int i = 0; i < MediaListColumnPolicy.ColumnCount; i++) {
                bool visible = MediaListColumnPolicy.IsVisible(ListMode, (MediaListColumn)i);
                SetColumnVisible(i, visible);
            }
        }

        private void SetColumnVisible(int index, bool visible)
        {
            var col = tableData.Columns[index];
            col.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        public VideoList(SelectWrapper<Video> extraWrapper, TabItemEx tabItemEx)
        {
            InitializeComponent();

            vieModel = new VieModel_VideoList();
            this.DataContext = vieModel;

            vieModel.ExtraWrapper = extraWrapper;
            vieModel.UUID = tabItemEx.UUID;
            TabItemEx = tabItemEx;

            SetDataGrid();

            Init();
        }

        public void Init()
        {
            ResizingTimer = new DispatcherTimer();
            BindingEvent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            BindingEventAfterRender();
            Refresh(vieModel.CurrentPage);
        }

        public void Refresh(int page = -1)
        {
            if (page > 0 && vieModel.CurrentPage != page) {
                vieModel.CurrentPage = page;
                pagination.CurrentPage = page;
            } else {
                vieModel.Refresh();
            }
        }
        public void RefreshData(long dataID)
        {
            vieModel.RefreshData(dataID);
        }

        public void BindingEvent()
        {
            SetSortType();
            SetImageViewMode();
            ResizingTimer.Interval = TimeSpan.FromSeconds(0.5);
            ResizingTimer.Tick += new EventHandler(ResizingTimer_Tick);
            vieModel.PageChangedStarted += PageChangedStarted;
            vieModel.PageChangedCompleted += PageChangedCompleted;
            vieModel.RenderSqlChanged += (s, ev) => onRenderSql?.Invoke(ev as WrapperEventArg<Video>);
            vieModel.onPageChange += (totalCount) => pagination.Total = totalCount;
            this.onInitTagStamps += this.filter.InitTagStamp;
            VieModel_VideoList.onSearchingChange += onSearchingChange;
            LibraryEventBus.DownloadCompleted += (s, e) => onDownloadSuccess(e.Task);
            LibraryEventBus.ScreenShotCompleted += (s, e) => onScreenShotCompleted(e.Success, e.DataId);
            LibraryEventBus.MetadataRefreshed += (s, e) => {
                if (e.DataId > 0)
                    RefreshData(e.DataId);
            };
        }


        /// <summary>
        /// 设置排序类型
        /// </summary>
        private void SetSortType()
        {
            var menuItems = SortBorder.ContextMenu.Items.OfType<MenuItem>().ToList();
            for (int i = 0; i < menuItems.Count; i++) {
                menuItems[i].Click += SortMenu_Click;
                menuItems[i].IsCheckable = true;
                if (i == vieModel.SortType)
                    menuItems[i].IsChecked = true;
            }

        }

        /// <summary>
        /// 设置图片显示模式
        /// </summary>
        private void SetImageViewMode()
        {
            var rbs = ViewModeStackPanel.Children.OfType<PathRadioButton>().ToList();
            for (int i = 0; i < rbs.Count; i++) {
                rbs[i].Click += SetViewMode;
                if (i == vieModel.ShowImageMode)
                    rbs[i].IsChecked = true;
            }
        }

        private void PageChangedStarted()
        {
            GotoTop(null, null);
        }

        private void PageChangedCompleted(object sender, EventArgs ev)
        {
            if (vieModel.EditMode)
                SetSelected();
            if (ConfigManager.Settings.AutoGenScreenShot)
                AutoGenScreenShot(vieModel.CurrentVideoList);
            if (tableData.Visibility == Visibility.Visible && tableData.Items.Count > 0)
                tableData.ScrollIntoView(tableData.Items[0]);
        }

        public void SetViewMode(object sender, RoutedEventArgs e)
        {
            PathRadioButton radioButton = sender as PathRadioButton;
            if (radioButton == null)
                return;
            var rbs = ViewModeStackPanel.Children.OfType<PathRadioButton>().ToList();
            int idx = rbs.IndexOf(radioButton);
            ViewMode viewMode = (ViewMode)idx;

            vieModel.ShowImageMode = idx;

            if (idx == 0)
                vieModel.GlobalImageWidth = (int)ConfigManager.VideoConfig.SmallImage_Width;
            else if (idx == 1)
                vieModel.GlobalImageWidth = (int)ConfigManager.VideoConfig.BigImage_Width;
            else if (idx == 2) {
                vieModel.GlobalImageWidth = (int)ConfigManager.VideoConfig.GifImage_Width;
            } else if (idx == 3) {
                AsyncLoadGif();
            }
            SetDataGrid();
        }

        private void SetDataGrid()
        {
            if (vieModel.ShowImageMode == 2) {
                vieModel.ShowTable = true;
            } else {
                vieModel.ShowTable = false;
            }
        }

        /// <summary>
        /// todo 加载 gif
        /// </summary>
        public void AsyncLoadGif()
        {
            // if (vieModel.CurrentVideoList == null) return;
            // DisposeGif("", true);
            // Task.Run(async () =>
            // {
            //    for (int i = 0; i < vieModel.CurrentVideoList.Count; i++)
            //    {
            //        Video video = vieModel.CurrentVideoList[i];
            //        string gifPath = Video.parseImagePath(video.GifImagePath);
            //        if (video.GifUri != null && !string.IsNullOrEmpty(video.GifUri.OriginalString)
            //            && video.GifUri.OriginalString.IndexOf("/NoPrinting_G.gif") < 0) continue;
            //        if (File.Exists(gifPath))
            //            video.GifUri = new Uri(gifPath);
            //        else
            //            video.GifUri = new Uri("pack://application:,,,/Resources/Picture/NoPrinting_G.gif");
            //        await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate
            //        {
            //            vieModel.CurrentVideoList[i] = null;
            //            vieModel.CurrentVideoList[i] = video;
            //        });
            //    }
            // });
        }

        private void SortMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            for (int i = 0; i < contextMenu.Items.Count; i++) {
                MenuItem item = (MenuItem)contextMenu.Items[i];
                if (item == menuItem) {
                    item.IsChecked = true;
                    if (i == vieModel.SortType)
                        vieModel.SortDescending = !vieModel.SortDescending;
                    vieModel.SortType = i;
                } else {
                    item.IsChecked = false;
                }
            }

            vieModel.Refresh();
        }


        private void Pagination_CurrentPageChange(object sender, EventArgs e)
        {
            Pagination pagination = sender as Pagination;
            vieModel.CurrentPage = pagination.CurrentPage;
            vieModel.PageQueue.Enqueue(pagination.CurrentPage);
            vieModel.LoadData();
        }

        private void MovieItemsControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                if (e.Delta > 0) {
                    imageSizeSlider.Value += imageSizeSlider.LargeChange;
                } else {
                    imageSizeSlider.Value -= imageSizeSlider.LargeChange;
                }
                e.Handled = true;

            }
        }


        private void MovieItemsControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private ScrollViewer GetScrollViewer()
        {
            if (MovieItemsControl != null && VisualTreeHelper.GetChildrenCount(MovieItemsControl) > 0)
                return VisualTreeHelper.GetChild(MovieItemsControl, 0) as ScrollViewer;
            return null;
        }

        public void GotoTop(object sender, RoutedEventArgs e)
        {
            if (dataScrollViewer == null)
                dataScrollViewer = GetScrollViewer();
            dataScrollViewer?.ScrollToTop();
        }

        public void GotoBottom(object sender, RoutedEventArgs e)
        {
            if (dataScrollViewer == null)
                dataScrollViewer = GetScrollViewer();
            dataScrollViewer?.ScrollToBottom();
        }

        private MenuItem GetMenuItem(ContextMenu contextMenu, string header)
        {
            if (contextMenu == null || string.IsNullOrEmpty(header))
                return null;
            foreach (FrameworkElement element in contextMenu.Items) {
                if (element is MenuItem item && item.Header.ToString().Equals(header))
                    return item;
            }

            return null;
        }

        public void EditInfo(object sender, RoutedEventArgs e)
        {
            long dataID = 0;
            if (sender is Button button && button.Tag != null &&
                long.TryParse(button.Tag.ToString(), out dataID)) {

            } else {
                dataID = GetIDFromMenuItem(sender);
            }
            Window_Edit windowEdit = new Window_Edit(dataID);
            windowEdit.ShowDialog();
        }



        public ObservableCollection<Video> GetVideosByMenu(MenuItem menuItem, int depth)
        {
            if (menuItem == null)
                return null;

            MenuItem p1 = menuItem;
            if (depth == 1)
                p1 = p1.Parent as MenuItem;

            if (p1 == null)
                return null;


            ContextMenu contextMenu = p1.Parent as ContextMenu;
            if (contextMenu == null || contextMenu.PlacementTarget == null)
                return null;

            ViewVideo viewVideo = contextMenu.PlacementTarget as ViewVideo;
            if (viewVideo == null)
                return null;

            ItemsControl itemsControl = VisualHelper.FindParentOfType<ItemsControl>(viewVideo);
            if (itemsControl == null)
                return null;



            return itemsControl.ItemsSource as ObservableCollection<Video>;
        }

        public void RefreshGrade(long dataID, float grade)
        {
            if (dataID <= 0)
                return;
            for (int i = 0; i < vieModel.CurrentVideoList.Count; i++) {
                if (vieModel.CurrentVideoList[i].DataID == dataID) {
                    vieModel.CurrentVideoList[i].Grade = grade;
                    VideoListCommands.NotifyMetadataRefreshed();
                    break;
                }
            }
        }

        public void RefreshImage(Video newVideo)
        {
            if (newVideo == null || newVideo.DataID <= 0 || vieModel.CurrentVideoList?.Count <= 0)
                return;
            long dataId = newVideo.DataID;
            for (int i = 0; i < vieModel.CurrentVideoList.Count; i++) {
                if (vieModel.CurrentVideoList[i]?.DataID == dataId) {
                    Video video = videoMapper.SelectOne(new SelectWrapper<Video>().Eq("DataID", dataId));
                    if (video == null)
                        continue;
                    Video.SetImage(ref video);
                    vieModel.CurrentVideoList[i].SmallImage = null;
                    vieModel.CurrentVideoList[i].BigImage = null;
                    vieModel.CurrentVideoList[i].SmallImage = video.SmallImage;
                    vieModel.CurrentVideoList[i].BigImage = video.BigImage;
                    break;
                }
            }
        }


        /// <summary>
        /// 将点击的该项也加入到选中列表中
        /// </summary>
        /// <param name="dataID"></param>
        private ObservableCollection<Video> HandleMenuSelected(object sender, int depth = 0)
        {
            long dataID = GetIDFromMenuItem(sender, depth);
            if (!vieModel.EditMode)
                vieModel.SelectedVideo.Clear();

            ObservableCollection<Video> videos = GetVideosByMenu(sender as MenuItem, depth);
            if (videos == null)
                return null;

            Video currentVideo = videos.FirstOrDefault(arg => arg.DataID == dataID);
            if (currentVideo == null)
                return null;
            if (!vieModel.SelectedVideo.Where(arg => arg.DataID == dataID).Any())
                vieModel.SelectedVideo.Add(currentVideo);
            return videos;
        }

        /// <summary>
        /// 打开网址
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetSelectMode(object sender, RoutedEventArgs e)
        {
            SuperControls.Style.Switch s = sender as SuperControls.Style.Switch;
            vieModel.SelectedVideo.Clear();
            SetSelected();
        }

        public void SetSelected()
        {
            ItemsControl itemsControl = MovieItemsControl;
            if (itemsControl == null)
                return;

            for (int i = 0; i < itemsControl.Items.Count; i++) {
                ContentPresenter presenter = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                if (presenter == null)
                    continue;
                ViewVideo viewVideo = FindElementByName<ViewVideo>(presenter, "viewVideo");
                if (viewVideo == null)
                    continue;
                long dataID = GetDataID(viewVideo);

                viewVideo.SetEditMode(vieModel.EditMode);

                if (dataID > 0) {
                    viewVideo.SetBackground((SolidColorBrush)Application.Current.Resources["ListBoxItem.Background"]);
                    viewVideo.SetBorderBrush(Brushes.Transparent);
                    if (vieModel.EditMode && vieModel.SelectedVideo != null &&
                        vieModel.SelectedVideo.Where(arg => arg.DataID == dataID).Any()) {

                        viewVideo.SetBackground(StyleManager.Common.HighLight.Background);
                        viewVideo.SetBorderBrush(StyleManager.Common.HighLight.BorderBrush);
                    }
                }
            }
        }


        private long GetDataID(ViewVideo viewVideo)
        {
            if (viewVideo != null && viewVideo.Tag != null && viewVideo.Tag is Video video && video.DataID > 0) {
                return video.DataID;
            }
            return -1;
        }

        private void RandomDisplay(object sender, RoutedEventArgs e)
        {
            vieModel.RandomDisplay();
        }



        private void NavigationToLetter(object sender, RoutedEventArgs e)
        {
            // vieModel.SearchFirstLetter = true;
            // vieModel.Search = ((Button)sender).Content.ToString();
        }

        public void SelectAll(object sender, RoutedEventArgs e)
        {
            if (vieModel.CurrentVideoList == null || vieModel.SelectedVideo == null)
                return;
            vieModel.EditMode = true;
            bool allContain = true; // 检测是否取消选中
            foreach (var item in vieModel.CurrentVideoList) {
                if (!vieModel.SelectedVideo.Contains(item)) {
                    vieModel.SelectedVideo.Add(item);
                    allContain = false;
                }
            }

            if (allContain)
                vieModel.SelectedVideo.RemoveMany(vieModel.CurrentVideoList);
            SetSelected();
        }



        public void BindingEventAfterRender()
        {
            // 翻页完成
            pagination.PageSizeChange += (s, e) => {
                Pagination pagination = s as Pagination;
                vieModel.PageSize = pagination.PageSize;
                vieModel.LoadData();
            };


            // 搜索
            searchBox.TextChanged += RefreshCandidate;
            searchTabControl.SelectionChanged += (s, e) => {
                if (e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0].GetType() != typeof(string)) {
                    // 不知道为啥，点击 tabitem 会导致 SelectionChanged
                    RefreshCandidate(null, null);
                }
            };

            // 搜索的 style
            // 参考：https://social.msdn.microsoft.com/Forums/vstudio/en-US/cefcfaa5-cb86-426f-b57a-b31a3ea5fcdd/how-to-add-eventsetter-by-code?forum=wpf
            SearchBoxListItemContainerStyle = (System.Windows.Style)this.Resources["SearchBoxListItemContainerStyle"];
            EventSetter eventSetter = new EventSetter() {
                Event = ListBoxItem.MouseDoubleClickEvent,
                Handler = new MouseButtonEventHandler(ListBoxItem_MouseDoubleClick)
            };

            SearchBoxListItemContainerStyle.Setters.Add(eventSetter);

            actorInfoView.Close += () => vieModel.ShowActorGrid = false;

        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem listBoxItem &&
                listBoxItem.Content != null &&
                listBoxItem.Content.ToString() is string search) {
                BeginSearch(search);
                e.Handled = true;
            }

        }

        public Video GetVideoFromChildEle(FrameworkElement ele, long dataID)
        {
            if (ele == null)
                return null;
            ItemsControl itemsControl = VisualHelper.FindParentOfType<ItemsControl>(ele);
            if (itemsControl == null)
                return null;
            ObservableCollection<Video> videos = itemsControl.ItemsSource as ObservableCollection<Video>;
            if (videos == null)
                return null;
            return videos.FirstOrDefault(arg => arg.DataID == dataID);
        }

        public void SetActor(ActorInfo actorInfo)
        {
            vieModel.ShowActorGrid = true;
            vieModel.ShowActorToggle = true;
            actorInfoView.CurrentActorInfo = actorInfo;
        }

        public bool IsShowActor()
        {
            return vieModel.ShowActorGrid;
        }

        public ActorInfo GetCurrentActor()
        {
            return actorInfoView.CurrentActorInfo;
        }


        public void SetAsso(bool asso)
        {
            vieModel.ShowAsso = asso;
        }


        public void NextPage()
        {
            pagination.NextPage();
        }

        public void PreviousPage()
        {
            pagination.PrevPage();
        }

        public void GoToTop()
        {
            GotoTop(null, null);
        }

        public void GoToBottom()
        {
            GotoBottom(null, null);
        }

        public void FirstPage()
        {
            pagination.FirstPage();
        }

        public void LastPage()
        {
            pagination.LastPage();
        }

        private void Rate_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CanRateChange = true;
        }


        private void Rate_ValueChanged(object sender, FunctionEventArgs<double> e)
        {
            if (!CanRateChange)
                return;

            if (sender is Rate rate &&
                rate.Tag != null &&
                long.TryParse(rate.Tag.ToString(), out long dataID) && dataID > 0) {
                metaDataMapper.UpdateFieldById("Grade", rate.Value.ToString(), dataID);
                    VideoListCommands.NotifyMetadataRefreshed();
                onGradeChange?.Invoke(dataID, (float)rate.Value);
            }
            CanRateChange = false;
        }

        private void OpenVideoPath(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null &&
                long.TryParse(button.Tag.ToString(), out long dataID) &&
                dataID > 0) {
                Video video = videoMapper.SelectVideoByID(dataID);

                if (video == null || !File.Exists(video.Path))
                    MessageCard.Error(SuperControls.Style.LangManager.GetValueByKey("Message_FileNotExist") + ": " + video.Path);
                else
                    FileHelper.TryOpenSelectPath(video.Path);
            }
        }

        private void ShowDetail(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null &&
                long.TryParse(button.Tag.ToString(), out long id)) {
                RaiseEvent(new VideoItemEventArgs(id, OnItemClickEvent, sender));
            }
        }



        private void viewVideo_OnItemChange(object sender, ObjectEventArgs e)
        {
            if (sender is ViewVideo viewVideo &&
               GetDataID(viewVideo) is long dataID && dataID > 0 &&
               e.Data is float grade) {
                onGradeChange?.Invoke(dataID, grade);
            }
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            int idx = tableData.SelectedIndex;
            if (idx < 0 || idx >= vieModel.CurrentVideoList.Count)
                return;
            Video video = vieModel.CurrentVideoList[idx];
            if (video == null)
                return;
            RaiseEvent(new VideoItemEventArgs(video.DataID, OnItemClickEvent, sender));
        }

    }
}
