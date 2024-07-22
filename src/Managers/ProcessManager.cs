using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace ProcessManager.Managers
{
    public class ProcessManager
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool UpdateWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool EnumThreadWindows(int dwThreadId, EnumThreadWindowsProc lpEnumFunc, IntPtr lParam);

        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;

        public Process GetProcessFromInput(int selectedIndex, string input)
        {
            Process process = null;

            switch (selectedIndex)
            {
                case 0:
                    if (int.TryParse(input, out int pid))
                    {
                        process = Process.GetProcessById(pid);
                    }
                    break;
                case 1:
                    process = Process.GetProcessesByName(input).FirstOrDefault();
                    break;
                case 2:
                    IntPtr windowHandle = FindWindow(null, input);
                    if (windowHandle != IntPtr.Zero)
                    {
                        process = Process.GetProcesses().FirstOrDefault(p => p.MainWindowHandle == windowHandle);
                    }
                    break;
            }
            return process;
        }

        public void LoadProcessList(ListBox processList)
        {
            processList.Items.Clear();
            foreach (var process in Process.GetProcesses().Where(p => !string.IsNullOrEmpty(p.MainWindowTitle)))
            {
                processList.Items.Add($"{process.Id} - {process.MainWindowTitle} - {process.ProcessName}");
            }
        }

        public void ProcessAction(int action, string input, int selectedIndex, TextBlock resultTextBlock)
        {
            IntPtr hWnd = GetWindowHandleFromInput(selectedIndex, input);

            if (hWnd != IntPtr.Zero)
            {
                bool result = ShowWindow(hWnd, action);
                bool updateResult = UpdateWindow(hWnd);
                resultTextBlock.Text += $"\nAction {(action == SW_HIDE ? "hide" : "show")} was {(result && updateResult ? "successful" : "failed")}.";
            }
            else
            {
                resultTextBlock.Text += "\nWindow handle is invalid.";
            }
        }

        public IntPtr GetWindowHandleFromInput(int selectedIndex, string input)
        {
            IntPtr hWnd = IntPtr.Zero;

            switch (selectedIndex)
            {
                case 0:
                    if (int.TryParse(input, out int pid))
                    {
                        var process = Process.GetProcessById(pid);
                        hWnd = GetMainWindowHandle(process);
                    }
                    break;
                case 1:
                    var proc = Process.GetProcessesByName(input).FirstOrDefault();
                    if (proc != null)
                    {
                        hWnd = GetMainWindowHandle(proc);
                    }
                    break;
                case 2:
                    hWnd = FindWindow(null, input);
                    break;
            }

            return hWnd;
        }

        private IntPtr GetMainWindowHandle(Process process)
        {
            IntPtr windowHandle = IntPtr.Zero;

            foreach (ProcessThread thread in process.Threads)
            {
                EnumThreadWindows(thread.Id, (hWnd, lParam) =>
                {
                    windowHandle = hWnd;
                    return false;
                }, IntPtr.Zero);

                if (windowHandle != IntPtr.Zero)
                {
                    break;
                }
            }

            return windowHandle;
        }

        private delegate bool EnumThreadWindowsProc(IntPtr hWnd, IntPtr lParam);
    }
}
