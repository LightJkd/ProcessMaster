using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using ProcessManager.Managers;

namespace ProcessManager
{
    public partial class MainWindow : Window
    {
        private readonly ProcessManager.Managers.ProcessManager _processManager;
        private readonly UIManager _uiManager;
        private bool _isSettingHotkey;
        private DispatcherTimer _dispatcherTimer;

        public MainWindow()
        {
            InitializeComponent();
            _processManager = new ProcessManager.Managers.ProcessManager();
            _uiManager = new UIManager(this);

            HotkeyManager.Initialize(ToggleWindowVisibility);
            this.Closed += (s, e) => HotkeyManager.Uninitialize();
            
            this.MouseMove += Window_MouseMove;
            this.MouseLeftButtonUp += Window_MouseLeftButtonUp;

            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Interval = TimeSpan.FromSeconds(2);
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (ComboBoxSelection.SelectedIndex == 3)
            {
                _processManager.LoadProcessList(ProcessList);
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                var currentPosition = e.GetPosition(null);
                var offsetX = currentPosition.X - _startPoint.X;
                var offsetY = currentPosition.Y - _startPoint.Y;

                this.Left += offsetX;
                this.Top += offsetY;

                _startPoint = currentPosition;
            }
        }

        private Point _startPoint;
        private bool _isDragging;
        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                Mouse.Capture(null);
                _isDragging = false;

                var currentLeft = this.Left;
                var currentTop = this.Top;

                var offsetX = currentLeft % 10;
                var offsetY = currentTop % 10;

                var newLeft = currentLeft - offsetX;
                var newTop = currentTop - offsetY;

                AnimateWindowPosition(currentLeft, newLeft, currentTop, newTop);
            }
        }

        private void AnimateWindowPosition(double fromLeft, double toLeft, double fromTop, double toTop)
        {
            var leftAnimation = new DoubleAnimation
            {
                From = fromLeft,
                To = toLeft,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };

            var topAnimation = new DoubleAnimation
            {
                From = fromTop,
                To = toTop,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };

            this.BeginAnimation(Window.LeftProperty, leftAnimation);
            this.BeginAnimation(Window.TopProperty, topAnimation);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void ComboBoxSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _uiManager.ResetUI();
            if (ComboBoxSelection.SelectedIndex == 3)
            {
                _uiManager.ShowProcessListUI();
                _processManager.LoadProcessList(ProcessList);
            }
            else
            {
                _uiManager.ShowInputUI();
            }
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            var process = _processManager.GetProcessFromInput(ComboBoxSelection.SelectedIndex, TextBoxInput.Text);
            _uiManager.DisplayProcessInfo(process, TextBlockResult, ButtonPanel);
        }

        private void HideButton_Click(object sender, RoutedEventArgs e)
        {
            _processManager.ProcessAction(ProcessManager.Managers.ProcessManager.SW_HIDE, TextBoxInput.Text, ComboBoxSelection.SelectedIndex, TextBlockResult);
        }

        private void ShowButton_Click(object sender, RoutedEventArgs e)
        {
            IntPtr hWnd = _processManager.GetWindowHandleFromInput(ComboBoxSelection.SelectedIndex, TextBoxInput.Text);
            if (hWnd != IntPtr.Zero)
            {
                bool result = ShowWindow(hWnd, ProcessManager.Managers.ProcessManager.SW_SHOW);
                bool updateResult = UpdateWindow(hWnd);
                
                this.ShowInTaskbar = true;
                
                TextBlockResult.Text += $"\nAction show was {(result && updateResult ? "successful" : "failed")}. Window handle: {hWnd}. ShowInTaskbar: {this.ShowInTaskbar}";
            }
            else
            {
                TextBlockResult.Text += "\nWindow handle is invalid.";
            }
        }

        private void ProcessList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProcessList.SelectedItem != null)
            {
                var selectedProcess = ProcessList.SelectedItem.ToString();
                var parts = selectedProcess.Split(" - ");
                TextBoxInput.Text = parts[0]; // PID
                ButtonSearch_Click(this, null);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _processManager.LoadProcessList(ProcessList);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (_isSettingHotkey)
            {
                HotkeyManager.ChangeToggleKey(e.Key);
                TextBlockHotkey.Text = e.Key.ToString().ToUpper();
                _isSettingHotkey = false;
                e.Handled = true;
            }
        }

        private void TextBlockHotkey_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isSettingHotkey = true;
            TextBlockHotkey.Text = "...";
        }

        private void TextBoxInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ButtonSearch_Click(this, null);
            }
        }

        private void ToggleWindowVisibility()
        {
            this.Visibility = this.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
        
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        [DllImport("user32.dll")]
        private static extern bool UpdateWindow(IntPtr hWnd);
    }
}
