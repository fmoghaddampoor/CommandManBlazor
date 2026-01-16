// <copyright file="MainWindow.xaml.cs" company="CommandMan">
// Copyright (c) CommandMan. All rights reserved.
// </copyright>

namespace CommandMan.Tray
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Forms; // For NotifyIcon
    using System.Windows.Media;
    using Application = System.Windows.Application;
    using MessageBox = System.Windows.MessageBox;

    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon? trayIcon;
        private Process? serverProcess;
        private string startUrl;
        private bool isExplicitExit = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        /// <param name="url">The initial URL to open in the browser.</param>
        public MainWindow(string url)
        {
            this.InitializeComponent();
            this.startUrl = url;

            this.LoadPortSetting();
            this.InitializeTrayIcon();

            // Initial checks and starts
            this.Loaded += this.MainWindow_Loaded;
            this.Closing += this.MainWindow_Closing;
        }

        private void LoadPortSetting()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "port.txt");
                if (File.Exists(path))
                {
                    string savedPortText = File.ReadAllText(path).Trim();
                    if (int.TryParse(savedPortText, out int port))
                    {
                        this.TxtPort.Text = savedPortText;
                    }
                }
            }
            catch
            {
                // Ignore errors during port loading
            }
        }

        private void SavePortSetting(int port)
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "port.txt");
                File.WriteAllText(path, port.ToString());
            }
            catch
            {
                // Ignore errors during port saving
            }
        }

        private void InitializeTrayIcon()
        {
            this.trayIcon = new NotifyIcon();
            this.trayIcon.Text = "CommandMan Server";
            try
            {
                // Load from WPF Resource
                var resourceInfo = Application.GetResourceStream(new Uri("pack://application:,,,/App.ico"));
                if (resourceInfo != null)
                {
                    this.trayIcon.Icon = new System.Drawing.Icon(resourceInfo.Stream);
                }
                else
                {
                    this.trayIcon.Icon = System.Drawing.SystemIcons.Application;
                }
            }
            catch
            {
                this.trayIcon.Icon = System.Drawing.SystemIcons.Application;
            }

            this.trayIcon.Visible = true;
            this.trayIcon.DoubleClick += (s, e) => this.ShowWindow();

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Open CommandMan", null, (s, e) => this.OpenBrowser());
            contextMenu.Items.Add("Show Control Panel", null, (s, e) => this.ShowWindow());
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Close", null, (s, e) => this.ExitApp());
            this.trayIcon.ContextMenuStrip = contextMenu;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Start minimizing behavior? No, we might want to show it briefly or just hide it.
            // Requirement was "small ui... goes to tray bar".
            // Let's start minimized.
            this.Hide();

            await this.CheckAndStartServer();
            this.OpenBrowser();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!this.isExplicitExit)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                this.trayIcon?.Dispose();
            }
        }

        private void ShowWindow()
        {
            this.Show();
            this.Activate();
            this.WindowState = WindowState.Normal;
        }

        private void ExitApp()
        {
            this.isExplicitExit = true;
            this.StopServer();
            Application.Current.Shutdown();
        }

        // --- Server Logic ---
        private int GetPort()
        {
            if (int.TryParse(this.TxtPort.Text, out int port))
            {
                return port;
            }

            return 5000;
        }

        private async Task CheckAndStartServer()
        {
            int port = this.GetPort();
            this.UpdateStatus("Checking...", Brushes.Gray);
            this.FooterText.Text = $"Selected Port: {port}";

            if (this.IsPortListening(port))
            {
                this.UpdateStatus("Running (External)", Brushes.CornflowerBlue);
                this.BtnStart.IsEnabled = false;
                this.BtnStop.IsEnabled = false;
            }
            else
            {
                this.StartServer();
            }

            await Task.Yield();
        }

        private void StartServer()
        {
            int port = this.GetPort();
            try
            {
                string exeName = "CommandMan.Web.exe";
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, exeName);

                if (!File.Exists(path))
                {
                    this.UpdateStatus("Server EXE not found!", Brushes.Red);
                    return;
                }

                this.serverProcess = new Process();
                this.serverProcess.StartInfo.FileName = path;
                this.serverProcess.StartInfo.Arguments = $"--urls http://localhost:{port}";
                this.serverProcess.StartInfo.UseShellExecute = false;
                this.serverProcess.StartInfo.CreateNoWindow = true;
                this.serverProcess.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;

                this.serverProcess.Start();

                this.SavePortSetting(port);
                this.UpdateStatus("Running", Brushes.SpringGreen);
                this.FooterText.Text = $"Server running on port {port}";
                this.BtnStart.IsEnabled = false;
                this.BtnStop.IsEnabled = true;
                this.TxtPort.IsEnabled = false;
            }
            catch (Exception ex)
            {
                this.UpdateStatus("Error: " + ex.Message, Brushes.Red);
            }
        }

        private void StopServer()
        {
            if (this.serverProcess != null && !this.serverProcess.HasExited)
            {
                try
                {
                    this.serverProcess.Kill();
                    this.serverProcess = null;
                    this.UpdateStatus("Stopped", Brushes.Orange);
                    this.FooterText.Text = "Ready";
                    this.BtnStart.IsEnabled = true;
                    this.BtnStop.IsEnabled = false;
                    this.TxtPort.IsEnabled = true;
                }
                catch
                {
                    // Ignore errors during process termination
                }
            }
            else
            {
                this.UpdateStatus("Stopped", Brushes.Gray);
                this.FooterText.Text = "Ready";
                this.BtnStart.IsEnabled = true;
                this.BtnStop.IsEnabled = false;
                this.TxtPort.IsEnabled = true;
            }
        }

        private void OpenBrowser()
        {
            int port = this.GetPort();
            try
            {
                // Reconstruct URL with current port
                string url = this.startUrl;
                if (url.Contains("localhost:"))
                {
                    // Split and replace port if present
                    // This is a bit lazy, let's just use UriBuilder if we want to be clean
                    // But usually _startUrl is http://localhost:5000/...
                    url = url.Replace(":5000", ":" + port);

                    // To be safer, if the user passed arguments, we should handle them.
                    // Actually, App.xaml.cs constructs the URL with port 5000.
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true,
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open browser: " + ex.Message);
            }
        }

        private bool IsPortListening(int port)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect("localhost", port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));
                    if (success)
                    {
                        client.EndConnect(result);
                        return true;
                    }
                }
            }
            catch
            {
                // Ignore connection errors
            }

            return false;
        }

        private void UpdateStatus(string text, Brush color)
        {
            this.StatusText.Text = text;
            this.StatusDot.Fill = color;
        }

        // --- Event Handlers ---
        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            this.OpenBrowser();
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            this.StartServer();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            this.StopServer();
        }
    }
}
