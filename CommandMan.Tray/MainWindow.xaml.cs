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
using Brushes = System.Windows.Media.Brushes;
using Brush = System.Windows.Media.Brush;

namespace CommandMan.Tray
{
    public partial class MainWindow : Window
    {
        private NotifyIcon? _trayIcon;
        private Process? _serverProcess;
        private string _startUrl;
        private bool _isExplicitExit = false;

        public MainWindow(string url)
        {
            InitializeComponent();
            _startUrl = url;
            
            LoadPortSetting();
            InitializeTrayIcon();
            
            // Initial checks and starts
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void LoadPortSetting()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "port.txt");
                if (File.Exists(path))
                {
                    string savedPort = File.ReadAllText(path).Trim();
                    if (int.TryParse(savedPort, out int port))
                    {
                        TxtPort.Text = savedPort;
                    }
                }
            }
            catch { }
        }

        private void SavePortSetting(int port)
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "port.txt");
                File.WriteAllText(path, port.ToString());
            }
            catch { }
        }

        private void InitializeTrayIcon()
        {
            _trayIcon = new NotifyIcon();
            _trayIcon.Text = "CommandMan Server";
            try {
                // Load from WPF Resource
                var resourceInfo = Application.GetResourceStream(new Uri("pack://application:,,,/App.ico"));
                if (resourceInfo != null)
                {
                    _trayIcon.Icon = new System.Drawing.Icon(resourceInfo.Stream);
                }
                else
                {
                    _trayIcon.Icon = System.Drawing.SystemIcons.Application;
                }
            } catch { 
                _trayIcon.Icon = System.Drawing.SystemIcons.Application; 
            }
            _trayIcon.Visible = true;
            _trayIcon.DoubleClick += (s, e) => ShowWindow();

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Open CommandMan", null, (s, e) => OpenBrowser());
            contextMenu.Items.Add("Show Control Panel", null, (s, e) => ShowWindow());
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Close", null, (s, e) => ExitApp());
            _trayIcon.ContextMenuStrip = contextMenu;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Start minimizing behavior? No, we might want to show it briefly or just hide it.
            // Requirement was "small ui... goes to tray bar".
            // Let's start minimized.
            Hide();
            
            await CheckAndStartServer();
            OpenBrowser();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isExplicitExit)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                _trayIcon.Dispose();
            }
        }

        private void ShowWindow()
        {
            Show();
            Activate();
            WindowState = WindowState.Normal;
        }

        private void ExitApp()
        {
            _isExplicitExit = true;
            StopServer();
            Application.Current.Shutdown();
        }

        // --- Server Logic ---

        private int GetPort()
        {
            if (int.TryParse(TxtPort.Text, out int port))
            {
                return port;
            }
            return 5000;
        }

        private async Task CheckAndStartServer()
        {
            int port = GetPort();
            UpdateStatus("Checking...", Brushes.Gray);
            FooterText.Text = $"Selected Port: {port}";
            
            if (IsPortListening(port))
            {
                UpdateStatus("Running (External)", Brushes.CornflowerBlue);
                BtnStart.IsEnabled = false;
                BtnStop.IsEnabled = false;
            }
            else
            {
                StartServer();
            }
        }

        private void StartServer()
        {
            int port = GetPort();
            try
            {
                string exeName = "CommandMan.Web.exe";
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, exeName);
                
                if (!File.Exists(path))
                {
                    UpdateStatus("Server EXE not found!", Brushes.Red);
                    return;
                }

                _serverProcess = new Process();
                _serverProcess.StartInfo.FileName = path;
                _serverProcess.StartInfo.Arguments = $"--urls http://localhost:{port}";
                _serverProcess.StartInfo.UseShellExecute = false;
                _serverProcess.StartInfo.CreateNoWindow = true;
                _serverProcess.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                
                _serverProcess.Start();
                
                SavePortSetting(port);
                UpdateStatus("Running", Brushes.SpringGreen);
                FooterText.Text = $"Server running on port {port}";
                BtnStart.IsEnabled = false;
                BtnStop.IsEnabled = true;
                TxtPort.IsEnabled = false;
            }
            catch (Exception ex)
            {
                UpdateStatus("Error: " + ex.Message, Brushes.Red);
            }
        }

        private void StopServer()
        {
            if (_serverProcess != null && !_serverProcess.HasExited)
            {
                try
                {
                    _serverProcess.Kill();
                    _serverProcess = null;
                    UpdateStatus("Stopped", Brushes.Orange);
                    FooterText.Text = "Ready";
                    BtnStart.IsEnabled = true;
                    BtnStop.IsEnabled = false;
                    TxtPort.IsEnabled = true;
                }
                catch { }
            }
            else
            {
                UpdateStatus("Stopped", Brushes.Gray);
                FooterText.Text = "Ready";
                BtnStart.IsEnabled = true;
                BtnStop.IsEnabled = false;
                TxtPort.IsEnabled = true;
            }
        }

        private void OpenBrowser()
        {
            int port = GetPort();
            try
            {
                // Reconstruct URL with current port
                string url = _startUrl;
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
                    UseShellExecute = true
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
            catch { }
            return false;
        }

        private void UpdateStatus(string text, Brush color)
        {
            StatusText.Text = text;
            StatusDot.Fill = color;
        }

        // --- Event Handlers ---

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenBrowser();
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            StartServer();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            StopServer();
        }
    }
}