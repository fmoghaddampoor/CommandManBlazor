// <copyright file="App.xaml.cs" company="CommandMan">
// Copyright (c) CommandMan. All rights reserved.
// </copyright>

namespace CommandMan.Tray
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using CommandMan.Tray.Interfaces;
    using CommandMan.Tray.Services;
    using CommandMan.Tray.ViewModels;

    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private const string MutexName = "{COMMANDMAN-B9A3-4122-8320-TRRAY-INSTANCE}";
        private static Mutex? mutex = null;

        private IServerService? serverService;
        private ISettingsService? settingsService;
        private ITrayService? trayService;
        private MainViewModel? viewModel;
        private MainWindow? mainWindow;
        private bool isExplicitExit = false;

        /// <summary>
        /// Handles the application startup event.
        /// </summary>
        /// <param name="e">The startup event arguments.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Parse arguments
            string left = string.Empty;
            string right = string.Empty;
            for (int i = 0; i < e.Args.Length; i++)
            {
                if (e.Args[i] == "-left" && i + 1 < e.Args.Length)
                {
                    left = e.Args[i + 1];
                }

                if (e.Args[i] == "-right" && i + 1 < e.Args.Length)
                {
                    right = e.Args[i + 1];
                }
            }

            // Construct URL
            int port = 5000;
            try
            {
                string portPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "port.txt");
                if (System.IO.File.Exists(portPath))
                {
                    if (int.TryParse(System.IO.File.ReadAllText(portPath).Trim(), out int savedPort))
                    {
                        port = savedPort;
                    }
                }
            }
            catch
            {
                // Fallback to default port
            }

            string url = $"http://localhost:{port}";
            if (!string.IsNullOrEmpty(left) || !string.IsNullOrEmpty(right))
            {
                url += "?";
                if (!string.IsNullOrEmpty(left))
                {
                    url += $"left={Uri.EscapeDataString(left)}";
                }

                if (!string.IsNullOrEmpty(left) && !string.IsNullOrEmpty(right))
                {
                    url += "&";
                }

                if (!string.IsNullOrEmpty(right))
                {
                    url += $"right={Uri.EscapeDataString(right)}";
                }
            }

            // Check for existing instance
            bool createdNew;
            mutex = new Mutex(true, MutexName, out createdNew);

            if (!createdNew)
            {
                // Instance exists. Just launch browser.
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true,
                    });
                }
                catch
                {
                    // Silent fail
                }

                // Exit immediately
                this.Shutdown();
                return;
            }

            // Continue startup
            base.OnStartup(e);

            // Initialize Services
            this.serverService = new ServerService();
            this.settingsService = new SettingsService();
            this.trayService = new TrayService();

            // Initialize ViewModel
            this.viewModel = new MainViewModel(this.serverService, this.settingsService, url);
            this.viewModel.RequestShow += (s, ev) => this.ShowWindow();
            this.viewModel.RequestExit += (s, ev) => this.ExitApp();

            // Initialize Tray
            this.trayService.Initialize();
            this.trayService.DoubleClick += (s, ev) => this.ShowWindow();
            this.trayService.AddMenuItem("Open CommandMan", this.viewModel.OpenCommand);
            this.trayService.AddMenuItem("Show Control Panel", this.viewModel.ShowCommand);
            this.trayService.AddSeparator();
            this.trayService.AddMenuItem("Close", this.viewModel.ExitCommand);

            // Initialize MainWindow
            this.mainWindow = new MainWindow();
            this.mainWindow.DataContext = this.viewModel;
            this.mainWindow.Closing += this.OnMainWindowClosing;

            // Set shutdown mode
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Initially hidden (minimizes to tray)
        }

        private void ShowWindow()
        {
            if (this.mainWindow != null)
            {
                this.mainWindow.Show();
                this.mainWindow.Activate();
                this.mainWindow.WindowState = WindowState.Normal;
            }
        }

        private void OnMainWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!this.isExplicitExit)
            {
                e.Cancel = true;
                this.mainWindow?.Hide();
            }
        }

        private async void ExitApp()
        {
            this.isExplicitExit = true;
            if (this.serverService != null)
            {
                await this.serverService.StopAsync();
            }

            this.trayService?.Dispose();
            this.Shutdown();
        }
    }
}
