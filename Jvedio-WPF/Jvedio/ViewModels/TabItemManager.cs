using Jvedio.Core.Tasks;
using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.Enums;
using Jvedio.Core.Library;
using Jvedio.Core.Scan;
using Jvedio.Core.UI;
using Jvedio.Core.UserControls;
using Jvedio.Core.UserControls.Tasks;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using Jvedio.ViewModel;
using SuperControls.Style;
using SuperUtils.Framework.ORM.Wrapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Jvedio.ViewModels
{

    public class TabItemManager
    {
        #region "事件"


        /// <summary>
        /// 滚动到该 Tab
        /// </summary>
        [Obsolete("Subscribe to LibraryEventBus.TabFocusChanged")]
        public static Action<TabItemEx> OnFocusItem { get; set; }
        #endregion


        #region "属性"

        private static TabItemManager instance { get; set; }

        private VieModel_Main vieModel { get; set; }
        private SimplePanel TabPanel { get; set; }

        private WrapperEventArg<Video> CurrentWrapperArg { get; set; }

        /// <summary>
        /// 只能打开一次详情页
        /// </summary>
        private Window detailsWindow { get; set; }

        #endregion

        public TabItemManager(VieModel_Main vieModel, SimplePanel tabPanel)
        {
            this.vieModel = vieModel;
            TabPanel = tabPanel;
            BindEvent();
        }

        [Obsolete("Use new TabItemManager(vieModel, tabPanel)")]
        public static TabItemManager CreateInstance(VieModel_Main vieModel, SimplePanel tabPanel)
        {
            return instance ?? (instance = new TabItemManager(vieModel, tabPanel));
        }

        private void BindEvent()
        {

        }

        public void Add(TabType type, string tabName, params object[] tabData)
        {
            if (vieModel == null) {
                return;
            }

            if (vieModel.TabItems == null) {
                vieModel.TabItems = new ObservableCollection<TabItemEx>();
                vieModel.TabItems.CollectionChanged += (s, ev) => {
                    if (vieModel.TabItems.Count == 0) {
                        vieModel.ShowSoft = true;
                    } else {
                        vieModel.ShowSoft = false;
                    }
                };
            }

            TabItemEx tabItem = vieModel.TabItems.FirstOrDefault(arg => arg.Name.Equals(tabName));

            if (tabItem != null) {

                SetTabSelected(vieModel.TabItems.IndexOf(tabItem));
                // 触发刷新
                RefreshVideoTab(type);

                OnFocusItem?.Invoke(tabItem);
                LibraryEventBus.RaiseTabFocusChanged(tabItem);
                return;
                //RemoveTabItem(vieModel.TabItems.IndexOf(tabItem));
            }


            tabItem = new TabItemEx(tabName, type);
            vieModel.TabItems.Add(tabItem);


            int idx = -1;
            for (int i = 0; i < vieModel.TabItems.Count; i++) {
                if (vieModel.TabItems[i].Name.Equals(tabName)) {
                    idx = i;
                    break;
                }
            }

            if (idx >= 0)
                SetTabSelected(idx);

            onAddData(tabItem, tabData);
        }

        private void OnViewAssoData(object sender, VideoItemEventArgs e)
        {
            OnViewAssoData(e.DataID);
        }


        public void OnActorGradeChange(long actorID, float grade)
        {
            List<ActorList> list = GetAllActorList();
            if (list != null) {
                foreach (ActorList data in list) {
                    data.RefreshGrade(actorID, grade);
                }
            }
            List<IMediaListTab> mediaLists = GetAllMediaLists();
            if (mediaLists != null) {

                ActorInfo actorInfo = ActorInfo.GetById(actorID);

                foreach (IMediaListTab mediaTab in mediaLists) {
                    if (!mediaTab.IsShowActor())
                        continue;

                    if (mediaTab.GetCurrentActor() is ActorInfo info &&
                        info.ActorID == actorID) {
                        mediaTab.SetActor(actorInfo);
                        break;
                    }
                }
            }
        }
        public void OnGradeChange(long dataID, float grade)
        {
            List<IMediaListTab> mediaLists = GetAllMediaLists();
            if (mediaLists != null) {
                foreach (IMediaListTab list in mediaLists) {
                    list.RefreshGrade(dataID, grade);
                }
            }
            (detailsWindow as Window_Details)?.RefreshGrade(dataID, grade);
        }
        public void OnViewAssoData(long dataID)
        {
            if (dataID <= 0)
                return;
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            wrapper.Eq("DataID", dataID);
            Video currentVideo = MapperManager.videoMapper.SelectById(wrapper);

            // 设置关联
            Video.SetAsso(ref currentVideo);

            if (currentVideo.AssociationList == null || currentVideo.AssociationList.Count <= 0)
                return;

            string tabName = currentVideo.VID;
            if (string.IsNullOrEmpty(tabName))
                tabName = currentVideo.Title;
            if (string.IsNullOrEmpty(tabName))
                tabName = System.IO.Path.GetFileNameWithoutExtension(currentVideo.Path);

            SelectWrapper<Video> extraWrapper = new SelectWrapper<Video>();

            currentVideo.AssociationList.Insert(0, dataID); // 自己也加入

            extraWrapper.In("metadata.DataID", currentVideo.AssociationList.Select(arg => arg.ToString()));
            Add(TabType.GeoAsso, $"关联：{tabName}", extraWrapper);
        }



        private void OnItemClick(object sender, VideoItemEventArgs e)
        {
            long dataID = e.DataID;
            onShowDetailData(dataID);
        }

        public void onShowDetailData(long dataID)
        {
            if (detailsWindow != null)
                detailsWindow.Close();
            Window next = MediaDetailsFactory.CreateDetailsWindow(dataID, CurrentWrapperArg, CurrentDataType());
            if (next is Window_Details details) {
                detailsWindow = details;
                details.onViewAssoData += (id) => {
                    OnViewAssoData(id);
                    details.Close();
                };
                details.Show();
            } else {
                detailsWindow = next;
                next?.Show();
            }
        }

        private void onAddData(TabItemEx tabItem, params object[] tabData)
        {
            switch (tabItem.TabType) {
                case TabType.GeoVideo:
                case TabType.GeoPicture:
                case TabType.GeoGame:
                case TabType.GeoStar:
                case TabType.GeoRecentPlay:
                case TabType.GeoAsso:

                    if (tabData == null || tabData.Length == 0)
                        return;
                    SelectWrapper<Video> ExtraWrapper = tabData[0] as SelectWrapper<Video>;

                    ITabContentFactory tabFactory = MediaUIHost.GetProfile(CurrentDataType()).CreateTabFactory();
                    MediaListMode listMode = MediaListModeExtensions.FromDataType(CurrentDataType());
                    IMediaListTab mediaList = tabFactory.CreateListTab(tabItem, ExtraWrapper, listMode);
                    if (mediaList == null)
                        return;

                    if (tabItem.TabType == TabType.GeoAsso)
                        mediaList.SetAsso(false);
                    mediaList.Uid = tabItem.UUID;
                    mediaList.OnItemClick += OnItemClick;
                    mediaList.OnItemViewAsso += OnViewAssoData;
                    mediaList.onGradeChange += OnGradeChange;
                    mediaList.onRenderSql += OnRenderSql;

                    if (tabData.Length > 1 && tabData[1] is ActorInfo actorInfo) {
                        mediaList.SetActor(actorInfo);
                    }

                    TabPanel.Children.Add(mediaList as UserControl);


                    break;
                case TabType.GeoActor:
                    ActorList actorList = new ActorList();
                    actorList.Uid = tabItem.UUID;

                    actorList.onShowSameActor += vieModel.ShowSameActor;
                    actorList.onGradeChange += OnActorGradeChange;

                    TabPanel.Children.Add(actorList);
                    break;

                case TabType.GeoTask:

                    if (tabData == null || tabData.Length == 0)
                        return;

                    if (tabData[0] is TaskType type) {
                        TaskList taskList = new TaskList(type);
                        taskList.Uid = tabItem.UUID;
                        SetTaskList(ref taskList, type);
                        TabPanel.Children.Add(taskList);
                    }
                    break;

                case TabType.GeoLabel:


                    if (tabData == null || tabData.Length == 0)
                        return;



                    if (tabData[0] is LabelType labelType && tabData[1] is string searchText) {
                        LabelView labelView = new LabelView(labelType);
                        labelView.Uid = tabItem.UUID;

                        labelView.SetLabel(GetLabelList(labelType, searchText));
                        labelView.onRefresh += (t) => {
                            labelView.SetLabel(GetLabelList(t, searchText));
                        };

                        labelView.onLabelClick += vieModel.onLabelClick;
                        TabPanel.Children.Add(labelView);
                    }
                    break;

                default:
                    break;
            }

            OnFocusItem?.Invoke(tabItem);
            LibraryEventBus.RaiseTabFocusChanged(tabItem);
        }

        private void OnRenderSql(WrapperEventArg<Video> arg)
        {
            if (arg == null || arg.Wrapper == null || string.IsNullOrEmpty(arg.SQL))
                return;
            CurrentWrapperArg = arg;
        }

        private List<string> GetLabelList(LabelType type,string searchText)
        {
            List<string> list = null;
            switch (type) {
                case LabelType.LabelName:
                    list = vieModel.GetLabelList();
                    break;
                case LabelType.Genre:
                    list = SideMenuLabelQueries.GetGenreList(searchText);
                    break;
                case LabelType.Series:
                case LabelType.Studio:
                case LabelType.Director:
                    list = SideMenuLabelQueries.GetListByField(type.ToString(), searchText);
                    break;
                default:
                    break;
            }

            return list;
        }

        private void SetTaskList(ref TaskList taskList, TaskType type)
        {
            switch (type) {
                case TaskType.ScreenShot:
                    TaskListWireHelper.WireLogDetail(taskList, App.ScreenShotManager);
                    App.ScreenShotManager.onRunning += () => {
                        TaskList list = GetTaskListByType(type);
                        if (list != null)
                            list.AllTaskProgress = App.ScreenShotManager.Progress;
                    };
                    break;
                case TaskType.Scan:
                    TaskListWireHelper.Wire(taskList, App.ScanManager,
                        onShowDetail: (tList, id) => onShowScanDetail(id));
                    App.ScanManager.onRunning += () => {
                        TaskList list = GetTaskListByType(type);
                        if (list != null)
                            list.AllTaskProgress = App.ScanManager.Progress;
                    };
                    break;
                case TaskType.Download:
                    TaskListWireHelper.Wire(taskList, App.DownloadManager,
                        onShowDetail: (tList, id) => {
                            tList.SetLogs(App.DownloadManager.GetTaskLogs(id));
                            tList.ShowLog = true;
                        },
                        onRestart: App.DownloadManager.Restart);
                    App.DownloadManager.onRunning += () => {
                        TaskList list = GetTaskListByType(type);
                        if (list != null)
                            list.AllTaskProgress = App.DownloadManager.Progress;
                    };
                    break;
                default:
                    break;
            }
        }



        public List<VideoList> GetAllVideoList()
            => TabPanel.Children.OfType<VideoList>().ToList();

        public List<IMediaListTab> GetAllMediaLists()
            => TabPanel.Children.OfType<IMediaListTab>().ToList();

        public List<ActorList> GetAllActorList()
        {
            return TabPanel.Children.OfType<ActorList>().ToList();
        }

        public FrameworkElement GetSelected()
        {
            int idx = GetSelectedIndex();
            if (idx >= 0 && idx < TabPanel.Children.Count) {
                return TabPanel.Children[idx] as FrameworkElement;
            }
            return null;
        }

        public IMediaListTab GetMediaListByType(TabType type)
        {
            if (TabPanel.Children == null || TabPanel.Children.Count == 0)
                return null;

            return GetAllMediaLists().FirstOrDefault(arg => arg.TabItemEx.TabType == type);
        }

        public VideoList GetVideoListByType(TabType type)
            => GetMediaListByType(type) as VideoList;

        public IMediaListTab GetPrimaryMediaList()
        {
            TabType primary = TabTypeExtensions.PrimaryListTabType(CurrentDataType());
            IMediaListTab list = GetMediaListByType(primary);
            if (list == null && primary != TabType.GeoVideo)
                list = GetMediaListByType(TabType.GeoVideo);
            return list;
        }

        public VideoList GetPrimaryVideoList()
            => GetPrimaryMediaList() as VideoList;

        public TaskList GetTaskListByType(TaskType type)
        {
            if (TabPanel.Children == null || TabPanel.Children.Count == 0)
                return null;

            List<TaskList> videoLists = TabPanel.Children.OfType<TaskList>().ToList();
            return videoLists.FirstOrDefault(arg => arg.TaskType == type);
        }

        public void RefreshVideoTab(TabType type) => RefreshMediaTab(type);

        public void RefreshMediaTab(TabType type)
        {
            if (type == TabType.GeoTask)
                return;
            IMediaListTab mediaList = GetMediaListByType(type);
            if (mediaList == null)
                return;
            if (mediaList is VideoList videoList) {
                videoList.ResetSearch();
                videoList.Refresh();
            }
        }

        public void RefreshPrimaryPictureLists()
        {
            foreach (IMediaListTab mediaList in GetAllMediaLists()) {
                if (mediaList is VideoList videoList && videoList.ListMode == MediaListMode.Picture)
                    videoList.Refresh();
            }
        }

        public void EnsurePictureListTab()
        {
            if (GetPrimaryMediaList() != null)
                return;
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            Add(TabType.GeoPicture, LangManager.GetValueByKey("AllPicture"), wrapper);
        }

        public void RefreshTab(int idx, int page)
        {
            if (idx < 0 || idx >= TabPanel.Children.Count)
                return;
            ITabItemControl ele = TabPanel.Children[idx] as ITabItemControl;
            ele?.Refresh(page);
        }

        public void ClearMediaTabs()
        {
            if (vieModel?.TabItems == null)
                return;
            while (vieModel.TabItems.Count > 0)
                RemoveTabItem(vieModel.TabItems.Count - 1);
        }

        public void RefreshAllTab(int page)
        {
            if (TabPanel.Children != null && TabPanel.Children.Count > 0) {
                for (int i = 0; i < TabPanel.Children.Count; i++) {
                    RefreshTab(i, page);
                }
            }
        }


        private void onShowScanDetail(string id)
        {
            ScanJobBase scanJob = App.ScanManager.CurrentTasks.FirstOrDefault(arg => arg.ID.Equals(id)) as ScanJobBase;
            if (scanJob == null)
                return;

            Window_ScanDetail scanDetail = new Window_ScanDetail(scanJob.ScanResult, scanJob.LibraryDataType);
            scanDetail.Show();
        }



        public void RemovePanel(TabItemEx tabItem)
        {
            int idx = -1;
            foreach (UIElement item in TabPanel.Children) {
                idx++;
                string uid = item.Uid;
                if (string.IsNullOrEmpty(uid))
                    continue;
                if (uid.Equals(tabItem.UUID)) {
                    break;
                }
            }
            if (idx >= 0 && idx < TabPanel.Children.Count) {
                TabPanel.Children.RemoveAt(idx);
            }
        }

        public void RemoveTabItem(int idx)
        {
            if (vieModel.TabItems == null)
                return;
            if (idx >= 0 && idx < vieModel.TabItems.Count) {

                // 移除对应的 panel
                RemovePanel(vieModel.TabItems[idx]);
                vieModel.TabItems[idx].Pinned = false;
                vieModel.TabItems.RemoveAt(idx);
            }
            // 默认选中左边的
            int selectIndex = idx - 1;
            if (selectIndex < 0)
                selectIndex = 0;

            if (vieModel.TabItems.Count > 0)
                vieModel.TabItemManager?.SetTabSelected(selectIndex);
        }

        public void SetTabSelected(int idx)
        {
            if (vieModel.TabItems == null || idx < 0 || idx >= vieModel.TabItems.Count)
                return;

            for (int i = 0; i < vieModel.TabItems.Count; i++) {
                vieModel.TabItems[i].Selected = false;
            }
            vieModel.TabItems[idx].Selected = true;
            List<FrameworkElement> allData = TabPanel.Children.OfType<FrameworkElement>().ToList();

            int target = -1;
            for (int i = 0; i < allData.Count; i++) {
                allData[i].Visibility = System.Windows.Visibility.Hidden;
                if (allData[i].Uid.Equals(vieModel.TabItems[idx].UUID)) {
                    target = i;
                }
            }
            if (target >= 0 && target < allData.Count)
                allData[target].Visibility = System.Windows.Visibility.Visible;
        }

        public int GetSelectedIndex()
        {
            if (vieModel.TabItems.Count > 0) {
                for (int i = 0; i < vieModel.TabItems.Count; i++) {
                    if (vieModel.TabItems[i].Selected)
                        return i;
                }
            }
            return -1;
        }

        public void PinByIndex(int idx)
        {

            if (idx < 0)
                return;

            if (vieModel == null || vieModel.TabItems == null || vieModel.TabItems.Count == 0 ||
                idx >= vieModel.TabItems.Count)
                return;
            TabItemEx tabItem = vieModel.TabItems[idx];
            if (tabItem.Pinned) {
                // 取消固定
                int targetIndex = vieModel.TabItems.Count;

                for (int i = vieModel.TabItems.Count - 1; i >= 0; i--) {
                    if (targetIndex == vieModel.TabItems.Count && vieModel.TabItems[i].Pinned)
                        targetIndex = i;

                    if (targetIndex < vieModel.TabItems.Count && idx >= 0)
                        break;
                }

                if (targetIndex == vieModel.TabItems.Count)
                    targetIndex = 0;
                tabItem.Pinned = false;
                // 移动到前面
                vieModel.TabItems.Move(idx, targetIndex);
            } else {
                // 固定
                int targetIndex = -1;
                for (int i = 0; i < vieModel.TabItems.Count; i++) {
                    if (targetIndex < 0 && !vieModel.TabItems[i].Pinned)
                        targetIndex = i;

                    if (targetIndex >= 0 && idx >= 0)
                        break;
                }
                if (targetIndex < 0)
                    return;
                tabItem.Pinned = true;
                // 移动到前面
                vieModel.TabItems.Move(idx, targetIndex);
            }
        }

        public void MoveToLast(int originIdx)
        {
            if (vieModel.TabItems[originIdx].Pinned) {
                int targetIndex = -1;
                for (int i = 0; i < vieModel.TabItems.Count; i++) {
                    if (vieModel.TabItems[i].Pinned)
                        targetIndex = i;
                }
                vieModel.TabItems.Move(originIdx, targetIndex);
            } else {
                vieModel.TabItems.Move(originIdx, vieModel.TabItems.Count - 1);
            }
        }
        public void MoveToFirst(int originIdx)
        {
            if (vieModel.TabItems[originIdx].Pinned) {
                // 如果已经固定，则移动到所有固定的前面
                vieModel.TabItems.Move(originIdx, 0);
            } else {
                // 如果没有固定，则找到最后一个固定的
                bool hasPinned = false;
                int targetIndex = -1;
                for (int i = 0; i < vieModel.TabItems.Count; i++) {
                    if (vieModel.TabItems[i].Pinned) {
                        hasPinned = true;
                        targetIndex = i;
                    }
                }

                if (targetIndex < 0 || targetIndex + 1 >= vieModel.TabItems.Count)
                    targetIndex = 0;
                if (hasPinned && targetIndex + 1 < vieModel.TabItems.Count)
                    vieModel.TabItems.Move(originIdx, targetIndex + 1);
                else
                    vieModel.TabItems.Move(originIdx, 0);
            }
        }

        public void RemoveRange(int start, int end)
        {
            if (vieModel.TabItems == null || vieModel.TabItems.Count == 0)
                return;

            int total = vieModel.TabItems.Count;

            if (start < 0 || start >= total || end < 0 || end >= total || start > end)
                return;

            for (int i = end; i >= start; i--) {
                if (vieModel.TabItems[i].Pinned)
                    continue;
                RemoveTabItem(i);
                //vieModel.TabItems.RemoveAt(i);
            }
        }

        private DataType CurrentDataType()
        {
            return vieModel?.LibraryContext?.DataType ?? LibraryContext.Current.DataType;
        }
    }
}
