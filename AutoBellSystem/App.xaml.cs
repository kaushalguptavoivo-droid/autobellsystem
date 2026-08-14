using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AutoBellSystem.Models;
using AutoBellSystem.Services;
using Hardcodet.Wpf.TaskbarNotification;

namespace AutoBellSystem
{
    public partial class App : Application
    {
        private TaskbarIcon? _notifyIcon;
        private MenuItem? _pauseMenuItem;
        private readonly StartupService _startupService = new StartupService();
        private MainWindow? _mainWindow;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            // Catch crashes anywhere in the app so it never silently dies -
            // instead the error is written to a log file and shown to the user.
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                Logger.LogError("AppDomain (fatal)", args.ExceptionObject as Exception ?? new Exception("Unknown fatal error"));
            };
            DispatcherUnhandledException += (s, args) =>
            {
                Logger.LogError("UI thread", args.Exception);
                MessageBox.Show(
                    $"Something went wrong:\n\n{args.Exception.Message}\n\nDetails were saved to:\n{Logger.LogFile}\n\nThe app will keep running.",
                    "School Auto Bell System - Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                args.Handled = true; // keep the app (and bell schedule) alive
            };
            TaskScheduler.UnobservedTaskException += (s, args) =>
            {
                Logger.LogError("Background task", args.Exception);
                args.SetObserved();
            };

            Logger.LogInfo("Application starting.");

            try
            {
                // Initialize Scheduler - bells keep ringing on time even with no
                // internet connection, since everything runs locally.
                await AppServices.Scheduler.InitializeAsync();
                await AppServices.Scheduler.ReloadSchedulesAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError("Scheduler startup", ex);
                MessageBox.Show(
                    $"The bell scheduler failed to start:\n\n{ex.Message}\n\nDetails were saved to:\n{Logger.LogFile}",
                    "School Auto Bell System - Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Create Taskbar Icon
            _notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            if (_notifyIcon.ContextMenu != null)
            {
                foreach (var item in _notifyIcon.ContextMenu.Items)
                {
                    if (item is MenuItem mi && mi.Name == "PauseMenuItem")
                    {
                        _pauseMenuItem = mi;
                        break;
                    }
                }
            }

            Settings? settings = null;
            try
            {
                settings = await AppServices.Database.GetSettingsAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError("Loading settings", ex);
            }

            if (settings != null)
            {
                _startupService.SetAutoStart(settings.AutoStartWithWindows);
                UpdatePauseMenuText(settings.SchedulePaused);
            }

            bool startHidden = false;
            foreach (var arg in e.Args)
            {
                if (arg.Equals("-hidden", StringComparison.OrdinalIgnoreCase))
                    startHidden = true;
            }

            if (!startHidden)
            {
                ShowDashboard();
            }

            Logger.LogInfo("Application started successfully.");
        }

        private void NotifyIcon_DoubleClick(object sender, RoutedEventArgs e) => ShowDashboard();

        private void MenuItem_Dashboard_Click(object sender, RoutedEventArgs e) => ShowDashboard();

        private void ShowDashboard()
        {
            if (_mainWindow == null)
            {
                _mainWindow = new MainWindow();
                // Closing the dashboard window only hides it - the app keeps
                // running quietly in the tray so bells keep firing on schedule.
                _mainWindow.Closing += (s, args) =>
                {
                    args.Cancel = true;
                    _mainWindow?.Hide();
                };
            }
            _mainWindow.Show();
            if (_mainWindow.WindowState == WindowState.Minimized)
                _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
        }

        private async void MenuItem_Pause_Click(object sender, RoutedEventArgs e)
        {
            var s = await AppServices.Database.GetSettingsAsync();
            if (s == null) return;

            s.SchedulePaused = !s.SchedulePaused;
            await AppServices.Database.SaveSettingsAsync(s);

            if (s.SchedulePaused)
                await AppServices.Scheduler.PauseAllAsync();
            else
                await AppServices.Scheduler.ResumeAllAsync();

            UpdatePauseMenuText(s.SchedulePaused);
        }

        private void UpdatePauseMenuText(bool isPaused)
        {
            if (_pauseMenuItem != null)
                _pauseMenuItem.Header = isPaused ? "Resume Schedule" : "Pause Schedule";
        }

        private async void MenuItem_TestBell_Click(object sender, RoutedEventArgs e)
        {
            var settings = await AppServices.Database.GetSettingsAsync();
            AppServices.Audio.PlaySound("", 1, settings?.VolumeLevel ?? 100, settings?.PreferredDeviceId ?? "");
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            _notifyIcon?.Dispose();
        }
    }
}
