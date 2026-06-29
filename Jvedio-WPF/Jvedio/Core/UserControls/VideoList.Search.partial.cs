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
        // Tab: Search
        private async void RefreshCandidate(object sender, TextChangedEventArgs e)
        {
            Logger.Info("refresh candidate");
            List<string> list = await vieModel.GetSearchCandidate();
            int idx = vieModel.SearchSelectedIndex;
            if (idx < 0 || idx >= searchTabControl.Items.Count)
                return;
            TabItem tabItem = searchTabControl.Items[idx] as TabItem;
            AddOrRefreshItem(tabItem, list);
        }

        private void AddOrRefreshItem(TabItem tabItem, List<string> list)
        {
            ListBox listBox;
            if (tabItem.Content == null) {
                listBox = new ListBox();
                tabItem.Content = listBox;
                listBox.Margin = new Thickness(0, 0, 0, 5);
                listBox.Style = (System.Windows.Style)App.Current.Resources["NormalListBox"];
                listBox.ItemContainerStyle = SearchBoxListItemContainerStyle;
                listBox.Background = Brushes.Transparent;
                listBox.PreviewKeyUp += searchTabItem_PreviewKeyUp;
            } else {
                listBox = tabItem.Content as ListBox;
            }
            listBox.ItemsSource = list;
            if (!string.IsNullOrEmpty(vieModel.SearchText))
                vieModel.Searching = true;
        }



        private void doSearch(object sender, RoutedEventArgs e)
        {
            int idx = vieModel.SearchSelectedIndex;
            if (idx < 0)
                idx = 0;
            //SearchMode mode = (SearchMode)vieModel.TabSelectedIndex;
            vieModel.Query((SearchField)idx);
            //SaveSearchHistory(mode, (SearchField)idx);
        }

        private void SaveSearchHistory(SearchMode mode, SearchField field)
        {
            string searchValue = vieModel.SearchText.ToProperSql();
            if (string.IsNullOrEmpty(searchValue))
                return;
            SearchHistory history = new SearchHistory() {
                SearchMode = mode,
                SearchValue = searchValue,
                CreateDate = DateHelper.Now(),
                SearchField = field,
                CreateYear = DateTime.Now.Year,
                CreateMonth = DateTime.Now.Month,
                CreateDay = DateTime.Now.Day,
            };
            searchHistoryMapper.Insert(history);
        }



        private void SearchBar_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                vieModel.Searching = false;
            } else if (e.Key == Key.Down || e.Key == Key.Up) {
                if (searchTabControl.SelectedItem is TabItem tabItem &&
                    tabItem != null && tabItem.Content is ListBox listbox && listbox.Items.Count > 0) {
                    listbox.Focus();
                    //listbox.SelectedIndex = 0;
                }
            } else if (e.Key == Key.Escape) {
                vieModel.Searching = false;
            } else if (e.Key == Key.Delete) {
                searchBox.ClearText();
            } else if (e.Key == Key.Tab) {

            }
        }

        private void searchTabItem_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            App.Logger.Info(e.Key.ToString());
            if (e.Key == Key.Left) {
                int idx = searchTabControl.SelectedIndex - 1;
                if (idx < 0)
                    idx = searchTabControl.Items.Count - 1;
                searchTabControl.SelectedIndex = idx;
                e.Handled = true;
            } else if (e.Key == Key.Right) {
                int idx = searchTabControl.SelectedIndex + 1;
                if (idx >= searchTabControl.Items.Count - 1) {
                    idx = 0;
                }
                searchTabControl.SelectedIndex = idx;
                e.Handled = true;
            } else if (e.Key == Key.Enter) {
                if (sender is ListBox listBox && listBox.Items.Count > 0 && listBox.SelectedItem is string search
                    && !string.IsNullOrEmpty(search)) {
                    BeginSearch(search);
                    e.Handled = true;
                }
            }
        }

        private void BeginSearch(string search)
        {
            searchBox.TextChanged -= RefreshCandidate;
            vieModel.SearchText = search;
            doSearch(null, null);
            vieModel.Searching = false;
            searchBox.TextChanged += RefreshCandidate;
        }

        private void searchTabControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) {
                vieModel.Searching = false;
                searchBox.SetFocus();
            }
        }

        public void SetSearchFocus()
        {
            searchBox.SetFocus();
        }

        public void ResetSearch()
        {
            vieModel.SearchText = "";
            vieModel.SearchWrapper = null;
            vieModel.Searching = false;
        }


    }
}
