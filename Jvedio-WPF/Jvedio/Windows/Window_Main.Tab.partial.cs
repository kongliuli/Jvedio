using Jvedio.AvalonEdit;
using Jvedio.Core.Library;
using Jvedio.Core.Enums;
using Jvedio.Core.Global;
using Jvedio.Core.Media;
using Jvedio.Core.Metadata;
using Jvedio.Core.Plugins.Crawler;
using Jvedio.Core.Scan;
using Jvedio.Core.Server;
using Jvedio.Core.UI;
using Jvedio.Core.UserControls;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using Jvedio.Upgrade;
using Jvedio.ViewModel;
using Jvedio.ViewModels;
using SuperControls.Style;
using SuperControls.Style.CSFile.Interfaces;
using SuperControls.Style.Plugin;
using SuperControls.Style.Windows;
using SuperUtils.Common;
using SuperUtils.CustomEventArgs;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.IO;
using SuperUtils.Systems;
using SuperUtils.WPF.VisualTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using static Jvedio.App;
using static Jvedio.Core.Global.UrlManager;
using static Jvedio.MapperManager;
using static Jvedio.Window_Settings;
using static SuperUtils.WPF.VisualTools.WindowHelper;

namespace Jvedio
{
    public partial class Main
    {
        // Tab keyboard / context menu / drag
        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.F) {
                SetTabItemStatus(TabActionType.Search);
                return;
            }

            if (vieModel.Searching) {
                return;
            }


            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Up) {
                SetTabItemStatus(TabActionType.GoToTop);
            } else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Down) {
                SetTabItemStatus(TabActionType.GoToBottom);
            } else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Left) {
                SetTabItemStatus(TabActionType.FirstPage);
            } else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Right) {
                SetTabItemStatus(TabActionType.LastPage);
            } else if (e.Key == Key.Right) {
                SetTabItemStatus(TabActionType.NextPage);
            } else if (e.Key == Key.Left) {
                SetTabItemStatus(TabActionType.PreviousPage);
            }
        }

        private void SetTabItemStatus(TabActionType type)
        {
            ITabItemControl ele = vieModel.TabItemManager.GetSelected() as ITabItemControl;
            switch (type) {
                case TabActionType.Search:
                    ele.SetSearchFocus();
                    break;
                case TabActionType.NextPage:
                    ele.NextPage();
                    break;
                case TabActionType.PreviousPage:
                    ele.PreviousPage();
                    break;
                case TabActionType.GoToTop:
                    ele.GoToTop();
                    break;
                case TabActionType.GoToBottom:
                    ele.GoToBottom();
                    break;
                case TabActionType.FirstPage:
                    ele.FirstPage();
                    break;
                case TabActionType.LastPage:
                    ele.LastPage();
                    break;

                default:
                    break;

            }

        }
        private void Tab_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
            e.Handled = true;
        }

        private int GetTabIndexByMenuItem(object sender)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            if (contextMenu != null &&
                contextMenu.PlacementTarget is Border border &&
                border.Tag != null &&
                int.TryParse(border.Tag.ToString(), out int index)) {
                return index;
            }
            return -1;
        }


        private void Border_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender is Border border &&
                border.Tag != null &&
                int.TryParse(border.Tag.ToString(), out int index)) {
                vieModel.TabItemManager.SetTabSelected(index);
            }
        }

        private void RefreshCurrentTab(object sender, RoutedEventArgs e)
        {
            int idx = GetTabIndexByMenuItem(sender);
            if (idx < 0)
                return;
            vieModel.TabItemManager.RefreshTab(idx, -1);
        }

        private void CloseCurrentTab(object sender, RoutedEventArgs e)
        {
            int idx = GetTabIndexByMenuItem(sender);
            if (idx < 0)
                return;
            vieModel.TabItemManager.RemoveTabItem(idx);
        }


        private void CloseOtherTab(object sender, RoutedEventArgs e)
        {
            if (vieModel == null ||
                vieModel.TabItems == null ||
                vieModel.TabItems.Count <= 1)
                return;

            int idx = GetTabIndexByMenuItem(sender);
            if (idx < 0)
                return;

            vieModel.TabItemManager?.RemoveRange(idx + 1, vieModel.TabItems.Count - 1);
            vieModel.TabItemManager?.RemoveRange(0, idx - 1);
        }

        private void CloseAllLeftTab(object sender, RoutedEventArgs e)
        {
            int idx = GetTabIndexByMenuItem(sender);
            if (idx <= 0)
                return;
            vieModel.TabItemManager?.RemoveRange(0, idx - 1);
        }

        private void CloseAllRightTab(object sender, RoutedEventArgs e)
        {
            if (vieModel.TabItems == null || vieModel.TabItems.Count == 0)
                return;

            int idx = GetTabIndexByMenuItem(sender);
            if (idx < 0)
                return;
            vieModel.TabItemManager?.RemoveRange(idx + 1, vieModel.TabItems.Count - 1);
        }



        private void CloseAllTabs(object sender, RoutedEventArgs e)
        {
            if (vieModel.TabItems == null || vieModel.TabItems.Count == 0)
                return;
            vieModel.TabItemManager?.RemoveRange(0, vieModel.TabItems.Count - 1);
        }

        private void MoveToFirst(object sender, RoutedEventArgs e)
        {
            if (vieModel.TabItems == null || vieModel.TabItems.Count == 0)
                return;

            int idx = GetTabIndexByMenuItem(sender);
            if (idx < 0 || idx >= vieModel.TabItems.Count)
                return;

            vieModel.TabItemManager.MoveToFirst(idx);

        }

        private void MoveToLast(object sender, RoutedEventArgs e)
        {
            if (vieModel.TabItems == null || vieModel.TabItems.Count == 0)
                return;

            int idx = GetTabIndexByMenuItem(sender);
            if (idx < 0 || idx >= vieModel.TabItems.Count)
                return;

            vieModel.TabItemManager?.MoveToLast(idx);
        }

        private void PinTab(object sender, RoutedEventArgs e)
        {
            if (vieModel.TabItems == null || vieModel.TabItems.Count == 0)
                return;

            int idx = GetTabIndexByMenuItem(sender);
            if (idx < 0 || idx >= vieModel.TabItems.Count)
                return;

            vieModel.TabItemManager?.PinByIndex(idx);
        }

        private void PinTab(object sender, MouseButtonEventArgs e)
        {
            if (vieModel.TabItems == null || vieModel.TabItems.Count == 0)
                return;

            if (sender is FrameworkElement ele && ele.Tag != null &&
                int.TryParse(ele.Tag.ToString(), out int index) &&
                index >= 0)
                vieModel.TabItemManager?.PinByIndex(index);
        }

        private void CloseTabItem(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement ele = sender as FrameworkElement;
            if (ele != null && ele.Tag != null && int.TryParse(ele.Tag.ToString(), out int index) &&
                index >= 0) {
                vieModel.TabItemManager.RemoveTabItem(index);
            }

        }

        private void BeginDragTabItem(object sender, MouseButtonEventArgs e)
        {

            FrameworkElement ele = (FrameworkElement)sender;
            if (ele == null || ele.Tag == null)
                return;
            string idxStr = ele.Tag.ToString();
            if (string.IsNullOrEmpty(idxStr) || vieModel.TabItems == null ||
                vieModel.TabItems.Count <= 0)
                return;
            int.TryParse(idxStr, out int index);
            if (index >= 0) {
                vieModel.TabItemManager?.SetTabSelected(index);
            }

            CanDragTabItem = true;
            CurrentDragElement = ele;
            Mouse.Capture(CurrentDragElement, CaptureMode.Element);

        }

        private void SetTabSelected(object sender, MouseButtonEventArgs e)
        {
            CanDragTabItem = false;
            if (CurrentDragElement != null)
                Mouse.Capture(CurrentDragElement, CaptureMode.None);
        }

        private void Border_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!CanDragTabItem)
                return;
        }
    }
}
