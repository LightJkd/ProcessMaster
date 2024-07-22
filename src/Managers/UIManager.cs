using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ProcessManager.Managers
{
    public class UIManager
    {
        private readonly MainWindow _mainWindow;

        public UIManager(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void ResetUI()
        {
            _mainWindow.TextBoxInput.Clear();
            _mainWindow.TextBlockResult.Text = string.Empty;
            _mainWindow.ButtonPanel.Visibility = Visibility.Collapsed;
            _mainWindow.ProcessList.Visibility = Visibility.Collapsed;
        }

        public void ShowInputUI()
        {
            _mainWindow.TextBoxInput.Visibility = Visibility.Visible;
            _mainWindow.ButtonSearch.Visibility = Visibility.Visible;
        }

        public void ShowProcessListUI()
        {
            _mainWindow.ProcessList.Visibility = Visibility.Visible;
        }

        public void DisplayProcessInfo(Process process, TextBlock resultTextBlock, StackPanel buttonPanel)
        {
            if (process != null)
            {
                string mainWindowTitle = process.MainWindowTitle;
                resultTextBlock.Text = $"Process Name: {process.ProcessName}\nPID: {process.Id}\nWindow Title: {mainWindowTitle}";
                buttonPanel.Visibility = Visibility.Visible;
            }
            else
            {
                resultTextBlock.Text = "Process not found.";
            }
        }
    }
}
