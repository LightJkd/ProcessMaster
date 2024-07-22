using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace ProcessManager.Managers
{
    public static class HotkeyManager
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        private const int HOTKEY_ID = 9000;
        private static Key _toggleKey = Key.Insert;
        private static Action _toggleWindowVisibility;

        public static void Initialize(Action toggleWindowVisibility)
        {
            _toggleWindowVisibility = toggleWindowVisibility;
            RegisterHotKey(GetConsoleWindow(), HOTKEY_ID, 0, (uint)KeyInterop.VirtualKeyFromKey(_toggleKey));
            ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessageMethod;
        }

        public static void Uninitialize()
        {
            UnregisterHotKey(GetConsoleWindow(), HOTKEY_ID);
        }

        private static void ThreadPreprocessMessageMethod(ref MSG msg, ref bool handled)
        {
            if (msg.message == 0x0312 && (int)msg.wParam == HOTKEY_ID)
            {
                _toggleWindowVisibility();
                handled = true;
            }
        }

        public static void ChangeToggleKey(Key newKey)
        {
            UnregisterHotKey(GetConsoleWindow(), HOTKEY_ID);
            _toggleKey = newKey;
            RegisterHotKey(GetConsoleWindow(), HOTKEY_ID, 0, (uint)KeyInterop.VirtualKeyFromKey(_toggleKey));
        }
    }
}
