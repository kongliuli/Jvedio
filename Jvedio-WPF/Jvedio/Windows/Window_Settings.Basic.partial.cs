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
        // Tab: Basic
        #region "热键"
        public const int HOTKEY_ID = 2415;
        public static uint VK { get; set; }
        public static IntPtr WindowHandle { get; set; }
        public static HwndSource HSource { get; set; }

        /// <summary>
        /// 功能键 [1,3] 个
        /// </summary>
        public static List<Key> FuncKeys { get; set; } = new List<Key>();

        /// <summary>
        /// 基础键 1 个
        /// </summary>
        public static Key BasicKey { get; set; } = Key.None;
        public static List<Key> _FuncKeys { get; set; } = new List<Key>();
        public static Key _BasicKey { get; set; } = Key.None;

        public enum Modifiers
        {
            None = 0x0000,
            Alt = 0x0001,
            Control = 0x0002,
            Shift = 0x0004,
            Win = 0x0008,
        }

        public static bool IsProperFuncKey(List<Key> keyList)
        {
            bool result = true;
            List<Key> keys = new List<Key>() { Key.LeftCtrl, Key.LeftAlt, Key.LeftShift };

            foreach (Key item in keyList) {
                if (!keys.Contains(item)) {
                    result = false;
                    break;
                }
            }

            return result;
        }

        private void hotkeyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Key currentKey = e.Key == Key.System ? e.SystemKey : e.Key;

            if (currentKey == Key.LeftCtrl | currentKey == Key.LeftAlt | currentKey == Key.LeftShift) {
                if (!FuncKeys.Contains(currentKey))
                    FuncKeys.Add(currentKey);
            } else if ((currentKey >= Key.A && currentKey <= Key.Z) || (currentKey >= Key.D0 && currentKey <= Key.D9) || (currentKey >= Key.NumPad0 && currentKey <= Key.NumPad9)) {
                BasicKey = currentKey;
            } else {
                // Console.WriteLine("不支持");
            }

            string singleKey = BasicKey.ToString();
            if (BasicKey.ToString().Length > 1) {
                singleKey = singleKey.ToString().Replace("D", string.Empty);
            }

            if (FuncKeys.Count > 0) {
                if (BasicKey == Key.None) {
                    hotkeyTextBox.Text = string.Join("+", FuncKeys);
                    _FuncKeys = new List<Key>();
                    _FuncKeys.AddRange(FuncKeys);
                    _BasicKey = Key.None;
                } else {
                    hotkeyTextBox.Text = string.Join("+", FuncKeys) + "+" + singleKey;
                    _FuncKeys = new List<Key>();
                    _FuncKeys.AddRange(FuncKeys);
                    _BasicKey = BasicKey;
                }
            } else {
                if (BasicKey != Key.None) {
                    hotkeyTextBox.Text = singleKey;
                    _FuncKeys = new List<Key>();
                    _BasicKey = BasicKey;
                }
            }
        }

        private void hotkeyTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            Key currentKey = e.Key == Key.System ? e.SystemKey : e.Key;

            if (currentKey == Key.LeftCtrl | currentKey == Key.LeftAlt | currentKey == Key.LeftShift) {
                if (FuncKeys.Contains(currentKey))
                    FuncKeys.Remove(currentKey);
            } else if ((currentKey >= Key.A && currentKey <= Key.Z) || (currentKey >= Key.D0 && currentKey <= Key.D9) || (currentKey >= Key.F1 && currentKey <= Key.F12)) {
                if (currentKey == BasicKey) {
                    BasicKey = Key.None;
                }
            }
        }

        private void ApplyHotKey(object sender, RoutedEventArgs e)
        {
            bool containsFunKey = _FuncKeys.Contains(Key.LeftAlt) | _FuncKeys.Contains(Key.LeftCtrl) | _FuncKeys.Contains(Key.LeftShift) | _FuncKeys.Contains(Key.CapsLock);

            if (!containsFunKey | _BasicKey == Key.None) {
                SuperControls.Style.MessageCard.Error(LangManager.GetValueByKey("HotKeyWarning"));
            } else {
                // 注册热键
                if (_BasicKey != Key.None & IsProperFuncKey(_FuncKeys)) {
                    uint fsModifiers = (uint)Modifiers.None;
                    foreach (Key key in _FuncKeys) {
                        if (key == Key.LeftCtrl)
                            fsModifiers = fsModifiers | (uint)Modifiers.Control;
                        if (key == Key.LeftAlt)
                            fsModifiers = fsModifiers | (uint)Modifiers.Alt;
                        if (key == Key.LeftShift)
                            fsModifiers = fsModifiers | (uint)Modifiers.Shift;
                    }

                    VK = (uint)KeyInterop.VirtualKeyFromKey(_BasicKey);

                    Win32Helper.UnregisterHotKey(WindowHandle, HOTKEY_ID); // 取消之前的热键
                    bool success = Win32Helper.RegisterHotKey(WindowHandle, HOTKEY_ID, fsModifiers, VK);
                    if (!success) {
                        new MsgBox(LangManager.GetValueByKey("HotKeyConflict")).ShowDialog(this);
                    }

                    {
                        // 保存设置
                        ConfigManager.Settings.HotKeyModifiers = fsModifiers;
                        ConfigManager.Settings.HotKeyVK = VK;
                        ConfigManager.Settings.HotKeyEnable = true;
                        ConfigManager.Settings.HotKeyString = hotkeyTextBox.Text;
                        ConfigManager.Settings.Save();
                        MessageNotify.Success(LangManager.GetValueByKey("HotKeySetSuccess"));
                    }
                }
            }
        }


        private void Unregister_HotKey(object sender, RoutedEventArgs e)
        {
            Win32Helper.UnregisterHotKey(WindowHandle, HOTKEY_ID); // 取消之前的热键
        }


        #endregion

        private void InitLang()
        {
            int langIdx = 0;
            if (!string.IsNullOrEmpty(ConfigManager.Settings.CurrentLanguage)) {
                for (int i = 0; i < langComboBox.Items.Count; i++) {
                    ComboBoxItem item = langComboBox.Items[i] as ComboBoxItem;
                    if (item.Tag.ToString().Equals(ConfigManager.Settings.CurrentLanguage)) {
                        langIdx = i;
                        break;
                    }
                }
            }
            langComboBox.SelectedIndex = langIdx;
            langComboBox.SelectionChanged += (s, ev) => {
                if (ev.AddedItems?.Count > 0) {
                    ComboBoxItem comboBoxItem = ev.AddedItems[0] as ComboBoxItem;
                    string lang = comboBoxItem.Tag.ToString();
                    SuperControls.Style.LangManager.SetLang(lang);
                    Jvedio.Core.Lang.LangManager.SetLang(lang);
                    vieModel.CurrentLanguage = lang;
                }
            };
        }

        private void SetVideoPlayerPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.Title = SuperControls.Style.LangManager.GetValueByKey("Choose");
            openFileDialog1.Filter = "exe|*.exe";
            openFileDialog1.FilterIndex = 1;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string exePath = openFileDialog1.FileName;
                if (File.Exists(exePath))
                    ConfigManager.Settings.VideoPlayerPath = exePath;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // 注册热键
            uint modifier = (uint)ConfigManager.Settings.HotKeyModifiers;
            uint vk = (uint)ConfigManager.Settings.HotKeyVK;

            if (modifier != 0 && vk != 0) {
                Win32Helper.UnregisterHotKey(WindowHandle, HOTKEY_ID); // 取消之前的热键
                bool success = Win32Helper.RegisterHotKey(WindowHandle, HOTKEY_ID, modifier, vk);
                if (!success) {
                    SuperControls.Style.MessageCard.Error(SuperControls.Style.LangManager.GetValueByKey("BossKeyError"));
                    ConfigManager.Settings.HotKeyEnable = false;
                }
            }
        }

        private void ViewSearchHistory(object sender, RoutedEventArgs e)
        {
        }

        private void ClearCache(object sender, RoutedEventArgs e)
        {
            ImageCache.Clear();
            MessageNotify.Success(LangManager.GetValueByKey("Message_Success"));
        }
    }
}
