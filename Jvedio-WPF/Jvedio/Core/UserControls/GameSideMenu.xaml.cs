using Jvedio.Core.Enums;
using Jvedio.Core.UI;
using Jvedio.Entity;
using SuperControls.Style;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.Time;
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
    public partial class GameSideMenu : UserControl, INotifyPropertyChanged, ISideMenuStatistics
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Action onStatistic { get; set; }
        public Action<object> onSideButtonCmd { get; set; }

        private long _AllItemCount;
        public long AllItemCount {
            get { return _AllItemCount; }
            set { _AllItemCount = value; RaisePropertyChanged(); }
        }

        private long _FavoriteCount;
        public long FavoriteCount {
            get { return _FavoriteCount; }
            set { _FavoriteCount = value; RaisePropertyChanged(); }
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

        public GameSideMenu()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
        }

        public void Statistic(string searchText)
        {
            int dataType = SideMenuDataTypeHelper.CurrentTypeInt;
            long dbid = ConfigManager.Main.CurrentDBId;
            AllItemCount = metaDataMapper.SelectCount(new SelectWrapper<MetaData>().Eq("DBId", dbid).Eq("DataType", dataType));
            appDatabaseMapper.UpdateFieldById("Count", AllItemCount.ToString(), dbid);
            FavoriteCount = metaDataMapper.SelectCount(new SelectWrapper<MetaData>().Eq("DBId", dbid).Eq("DataType", dataType).Gt("Grade", 0));

            string label_count_sql = "SELECT COUNT(DISTINCT LabelName) as Count from metadata_to_label " +
                "join metadata on metadata_to_label.DataID=metadata.DataID " +
                $"WHERE metadata.DBId={dbid} and metadata.DataType={dataType} ";
            AllLabelCount = metaDataMapper.SelectCount(label_count_sql);

            DateTime date1 = DateTime.Now.AddDays(-1 * ViewModel.VieModel_Main.RECENT_DAY);
            DateTime date2 = DateTime.Now;
            RecentWatchCount = metaDataMapper.SelectCount(new SelectWrapper<MetaData>().Eq("DBId", dbid).Eq("DataType", dataType)
                .Between("ViewDate", DateHelper.ToLocalDate(date1), DateHelper.ToLocalDate(date2)));
        }

        private void HandleSideClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag != null)
                onSideButtonCmd?.Invoke(element.Tag.ToString());
        }
    }
}
