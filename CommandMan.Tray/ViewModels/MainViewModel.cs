// <copyright file="MainViewModel.cs" company="CommandMan">
// Copyright (c) CommandMan. All rights reserved.
// </copyright>

namespace CommandMan.Tray.ViewModels
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Media;
    using CommandMan.Tray.Interfaces;
    using CommandMan.Tray.MVVM;

    /// <summary>
    /// ViewModel for the main window of the Tray application.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IServerService serverService;
        private readonly ISettingsService settingsService;
        private readonly string initialUrl;

        private string portText = "5000";
        private string statusText = "Checking...";
        private Brush statusColor = Brushes.Gray;
        private string footerText = "Ready";
        private bool isStartEnabled = true;
        private bool isStopEnabled = false;
        private bool isPortEnabled = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="serverService">The server service.</param>
        /// <param name="settingsService">The settings service.</param>
        /// <param name="initialUrl">The initial URL with arguments.</param>
        public MainViewModel(IServerService serverService, ISettingsService settingsService, string initialUrl)
        {
            this.serverService = serverService ?? throw new ArgumentNullException(nameof(serverService));
            this.settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            this.initialUrl = initialUrl;

            this.OpenCommand = new RelayCommand(_ => this.OpenBrowser());
            this.StartCommand = new RelayCommand(async _ => await this.StartServerAsync(), _ => this.IsStartEnabled);
            this.StopCommand = new RelayCommand(async _ => await this.StopServerAsync(), _ => this.IsStopEnabled);
            this.ExitCommand = new RelayCommand(_ => this.RequestExit?.Invoke(this, EventArgs.Empty));
            this.ShowCommand = new RelayCommand(_ => this.RequestShow?.Invoke(this, EventArgs.Empty));

            this.serverService.StatusChanged += this.OnServerStatusChanged;

            this.InitializeAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Occurs when the application exit is requested.
        /// </summary>
        public event EventHandler? RequestExit;

        /// <summary>
        /// Occurs when the main window should be shown.
        /// </summary>
        public event EventHandler? RequestShow;

        /// <summary>
        /// Gets the command to exit the application.
        /// </summary>
        public ICommand ExitCommand { get; }

        /// <summary>
        /// Gets the command to show the main window.
        /// </summary>
        public ICommand ShowCommand { get; }

        /// <summary>
        /// Gets the command to open the application in the browser.
        /// </summary>
        public ICommand OpenCommand { get; }

        /// <summary>
        /// Gets the command to start the server.
        /// </summary>
        public ICommand StartCommand { get; }

        /// <summary>
        /// Gets the command to stop the server.
        /// </summary>
        public ICommand StopCommand { get; }

        /// <summary>
        /// Gets or sets the port text.
        /// </summary>
        public string PortText
        {
            get => this.portText;
            set => this.SetProperty(ref this.portText, value);
        }

        /// <summary>
        /// Gets or sets the status text.
        /// </summary>
        public string StatusText
        {
            get => this.statusText;
            set => this.SetProperty(ref this.statusText, value);
        }

        /// <summary>
        /// Gets or sets the status dot color.
        /// </summary>
        public Brush StatusColor
        {
            get => this.statusColor;
            set => this.SetProperty(ref this.statusColor, value);
        }

        /// <summary>
        /// Gets or sets the footer text.
        /// </summary>
        public string FooterText
        {
            get => this.footerText;
            set => this.SetProperty(ref this.footerText, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the start button is enabled.
        /// </summary>
        public bool IsStartEnabled
        {
            get => this.isStartEnabled;
            set => this.SetProperty(ref this.isStartEnabled, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the stop button is enabled.
        /// </summary>
        public bool IsStopEnabled
        {
            get => this.isStopEnabled;
            set => this.SetProperty(ref this.isStopEnabled, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the port input is enabled.
        /// </summary>
        public bool IsPortEnabled
        {
            get => this.isPortEnabled;
            set => this.SetProperty(ref this.isPortEnabled, value);
        }

        private async Task InitializeAsync()
        {
            this.settingsService.Load();
            this.PortText = this.settingsService.Port.ToString();

            if (await this.serverService.IsRunningAsync(this.settingsService.Port))
            {
                // Status updated via event
            }
            else
            {
                await this.StartServerAsync();
            }

            this.OpenBrowser();
        }

        private async Task StartServerAsync()
        {
            if (int.TryParse(this.PortText, out int port))
            {
                this.settingsService.Port = port;
                this.settingsService.Save();
                await this.serverService.StartAsync(port);
            }
            else
            {
                this.StatusText = "Invalid Port";
                this.StatusColor = Brushes.Red;
            }
        }

        private async Task StopServerAsync()
        {
            await this.serverService.StopAsync();
        }

        private void OnServerStatusChanged(object? sender, EventArgs e)
        {
            this.StatusText = this.serverService.StatusMessage;
            this.FooterText = this.serverService.StatusMessage;

            switch (this.serverService.Status)
            {
                case ServerStatus.Running:
                    this.StatusColor = Brushes.SpringGreen;
                    this.IsStartEnabled = false;
                    this.IsStopEnabled = true;
                    this.IsPortEnabled = false;
                    break;
                case ServerStatus.RunningExternal:
                    this.StatusColor = Brushes.CornflowerBlue;
                    this.IsStartEnabled = false;
                    this.IsStopEnabled = false;
                    this.IsPortEnabled = false;
                    break;
                case ServerStatus.Starting:
                    this.StatusColor = Brushes.Yellow;
                    this.IsStartEnabled = false;
                    this.IsStopEnabled = false;
                    this.IsPortEnabled = false;
                    break;
                case ServerStatus.Error:
                    this.StatusColor = Brushes.Red;
                    this.IsStartEnabled = true;
                    this.IsStopEnabled = false;
                    this.IsPortEnabled = true;
                    break;
                case ServerStatus.Stopped:
                default:
                    this.StatusColor = Brushes.Gray;
                    this.IsStartEnabled = true;
                    this.IsStopEnabled = false;
                    this.IsPortEnabled = true;
                    this.FooterText = "Ready";
                    break;
            }

            CommandManager.InvalidateRequerySuggested();
        }

        private void OpenBrowser()
        {
            try
            {
                int port = this.settingsService.Port;
                string url = this.initialUrl;

                if (url.Contains("localhost:"))
                {
                    url = url.Replace(":5000", ":" + port);
                }

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
        }
    }
}
