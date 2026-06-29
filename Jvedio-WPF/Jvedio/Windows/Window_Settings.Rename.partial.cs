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
        // Tab: Rename
        private void InitRenameCombobox()
        {
            foreach (ComboBoxItem item in OutComboBox.Items) {
                if (item.Content.ToString().Equals(ConfigManager.RenameConfig.OutSplit)) {
                    OutComboBox.SelectedIndex = OutComboBox.Items.IndexOf(item);
                    break;
                }
            }

            if (OutComboBox.SelectedIndex < 0)
                OutComboBox.SelectedIndex = 0;

            foreach (ComboBoxItem item in InComboBox.Items) {
                if (item.Content.ToString().Equals(ConfigManager.RenameConfig.InSplit)) {
                    InComboBox.SelectedIndex = InComboBox.Items.IndexOf(item);
                    break;
                }
            }

            if (InComboBox.SelectedIndex < 0)
                OutComboBox.SelectedIndex = 0;
        }

        private void InitCheckedBoxChecked()
        {
            List<ToggleButton> toggleButtons = CheckedBoxWrapPanel.Children.OfType<ToggleButton>().ToList();
            List<string> list = toggleButtons.Select(arg => Video.ToSqlField(arg.Content.ToString())).ToList();

            // 按照顺序
            string formatString = ConfigManager.RenameConfig.FormatString;
            if (!string.IsNullOrEmpty(formatString)) {
                int left = formatString.IndexOf("{"), right = formatString.IndexOf("}");
                while (right > 0 && right < formatString.Length) {
                    string name = formatString.Substring(left + 1, right - left - 1);
                    if (list.Contains(name)) {
                        RenameList.Add(name);
                        toggleButtons[list.IndexOf(name)].IsChecked = true;
                    }
                    left = formatString.IndexOf("{", left + 1);
                    right = formatString.IndexOf("}", right + 1);
                    Console.WriteLine();
                }
            }
        }

        private void ReplaceWithValue(string property)
        {
            string inSplit = ConfigManager.RenameConfig.InSplit.Equals("[null]") ? string.Empty : ConfigManager.RenameConfig.InSplit;
            PropertyInfo[] propertyList = SampleVideo.GetType().GetProperties();
            foreach (PropertyInfo item in propertyList) {
                string name = item.Name;
                if (name == property) {
                    object o = item.GetValue(SampleVideo);
                    if (o != null) {
                        string value = o.ToString();

                        if (property == "ActorNames" || property == "Genre" || property == "Label")
                            value = value.Replace(" ", inSplit).Replace("/", inSplit);

                        if (vieModel.RemoveTitleSpace && property.Equals("Title"))
                            value = value.Trim();

                        if (property == "VideoType") {
                            int v = 0;
                            int.TryParse(value, out v);
                            if (v == 1)
                                value = SuperControls.Style.LangManager.GetValueByKey("Uncensored");
                            else if (v == 2)
                                value = SuperControls.Style.LangManager.GetValueByKey("Censored");
                            else if (v == 3)
                                value = SuperControls.Style.LangManager.GetValueByKey("Europe");
                        }

                        vieModel.ViewRenameFormat = vieModel.ViewRenameFormat.Replace("{" + property + "}", value);
                    }

                    break;
                }
            }
        }

        private void SetRenameFormat()
        {
            string format = vieModel.FormatString;
            if (RenameList.Count > 0) {

                StringBuilder builder = new StringBuilder();
                string sep = ConfigManager.RenameConfig.OutSplit.Equals("[null]") ? string.Empty : ConfigManager.RenameConfig.OutSplit;
                List<string> formatNames = new List<string>();
                foreach (string name in RenameList) {
                    formatNames.Add($"{{{name}}}");
                }
                vieModel.FormatString = string.Join(sep, formatNames);
            } else
                vieModel.FormatString = string.Empty;
        }

        private void AddToRename(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            if (toggleButton != null) {
                string sep = ConfigManager.RenameConfig.OutSplit.Equals("[null]") ? string.Empty : ConfigManager.RenameConfig.OutSplit;
                string format = vieModel.FormatString;
                string value = Video.ToSqlField(toggleButton.Content.ToString());
                if ((bool)toggleButton.IsChecked) {

                    if (format.IndexOf($"{{{value}}}") < 0) {
                        // 加到最后
                        if (format.Length > 0 && !string.IsNullOrEmpty(sep) && !format[format.Length - 1].Equals(sep.ToCharArray()[0]))
                            format += sep;
                        format += $"{{{value}}}";
                        RenameList.Add(value);
                    }
                } else {
                    // 移除所有
                    format = format.Replace($"{sep}{{{value}}}", "");
                    format = format.Replace($"{{{value}}}", "");
                    RenameList.Remove(value);
                }
                vieModel.FormatString = format;
            }
            SetRenameFormat();
            InitViewRename(vieModel.FormatString);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vieModel == null)
                return;
            TextBox textBox = (TextBox)sender;
            string txt = textBox.Text;
            InitViewRename(txt);
        }

        private void InitViewRename(string txt)
        {
            if (string.IsNullOrEmpty(txt)) {
                vieModel.ViewRenameFormat = string.Empty;
                return;
            }

            MatchCollection matches = Regex.Matches(txt, "\\{[a-zA-Z]+\\}");
            if (matches != null && matches.Count > 0) {
                vieModel.ViewRenameFormat = txt;
                foreach (Match match in matches) {
                    string property = match.Value.Replace("{", string.Empty).Replace("}", string.Empty);
                    ReplaceWithValue(property);
                }
            } else {
                vieModel.ViewRenameFormat = string.Empty;
            }
        }

        private void OutComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            ConfigManager.RenameConfig.OutSplit = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
            SetRenameFormat();
        }

        private void InComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            ConfigManager.RenameConfig.InSplit = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
            SetRenameFormat();
            InitViewRename(vieModel.FormatString);
        }
    }
}
