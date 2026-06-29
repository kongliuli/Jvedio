using Jvedio.Core.Global;
using SuperControls.Style;
using SuperControls.Style.Windows;
using SuperUtils.Systems;
using SuperUtils.WPF.VisualTools;
using System;
using System.Windows;
using System.Windows.Interop;
using static Jvedio.App;
using static Jvedio.Window_Settings;

namespace Jvedio
{
    public partial class Main
    {

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            Logger.Info("***************OnSourceInitialized***************");

            // 热键
            WindowHandle = new WindowInteropHelper(this).Handle;
            HSource = HwndSource.FromHwnd(WindowHandle);
            HSource.AddHook(HwndHook);

            // 注册热键
            uint modifier = (uint)ConfigManager.Settings.HotKeyModifiers;
            uint vk = (uint)ConfigManager.Settings.HotKeyVK;

            if (ConfigManager.Settings.HotKeyEnable && modifier != 0 && vk != 0) {
                Win32Helper.UnregisterHotKey(WindowHandle, HOTKEY_ID); // 取消之前的热键
                bool success = Win32Helper.RegisterHotKey(WindowHandle, HOTKEY_ID, modifier, vk);
                Logger.Info($"register hot key, modifier: {modifier}, vk: {vk}, ret: {success}");
                if (!success) {
                    new MsgBox(SuperControls.Style.LangManager.GetValueByKey("HotKeyConflict")).ShowDialog(this);
                }
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg) {
                case WM_HOTKEY:
                    switch (wParam.ToInt32()) {
                        case HOTKEY_ID:
                            int key = ((int)lParam >> 16) & 0xFFFF;
                            if (key == (uint)ConfigManager.Settings.HotKeyVK) {
                                if (TaskIconVisible) {
                                    SetWindowVisualStatus(false, false);
                                } else {
                                    SetWindowVisualStatus(!WindowsVisible, !WindowsVisible);
                                }
                            }

                            handled = true;
                            break;
                    }

                    break;
            }

            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            HSource.RemoveHook(HwndHook);
            Win32Helper.UnregisterHotKey(WindowHandle, HOTKEY_ID); // 取消热键
            base.OnClosed(e);
        }


    }
}
