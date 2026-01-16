// <copyright file="ServerService.cs" company="CommandMan">
// Copyright (c) CommandMan. All rights reserved.
// </copyright>

namespace CommandMan.Tray.Services
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using CommandMan.Tray.Interfaces;

    /// <summary>
    /// Implementation of <see cref="IServerService"/> that manages the CommandMan.Web process.
    /// </summary>
    public class ServerService : IServerService
    {
        private Process? serverProcess;
        private ServerStatus status = ServerStatus.Stopped;
        private string statusMessage = "Ready";

        /// <summary>
        /// Occurs when the server status changes.
        /// </summary>
        public event EventHandler? StatusChanged;

        /// <summary>
        /// Gets the current status of the server.
        /// </summary>
        public ServerStatus Status
        {
            get => this.status;
            private set
            {
                if (this.status != value)
                {
                    this.status = value;
                    this.NotifyStatusChanged();
                }
            }
        }

        /// <summary>
        /// Gets the message associated with the current status.
        /// </summary>
        public string StatusMessage
        {
            get => this.statusMessage;
            private set
            {
                if (this.statusMessage != value)
                {
                    this.statusMessage = value;
                    this.NotifyStatusChanged();
                }
            }
        }

        /// <summary>
        /// Starts the server on the specified port.
        /// </summary>
        /// <param name="port">The port to run the server on.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task StartAsync(int port)
        {
            this.Status = ServerStatus.Starting;
            this.StatusMessage = "Starting...";

            try
            {
                string exeName = "CommandMan.Web.exe";
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, exeName);

                if (!File.Exists(path))
                {
                    this.Status = ServerStatus.Error;
                    this.StatusMessage = "Server EXE not found!";
                    return;
                }

                this.serverProcess = new Process();
                this.serverProcess.StartInfo.FileName = path;
                this.serverProcess.StartInfo.Arguments = $"--urls http://localhost:{port}";
                this.serverProcess.StartInfo.UseShellExecute = false;
                this.serverProcess.StartInfo.CreateNoWindow = true;
                this.serverProcess.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;

                this.serverProcess.Start();

                // Small delay to allow port to open
                await Task.Delay(1000);

                if (await this.IsRunningAsync(port))
                {
                    this.Status = ServerStatus.Running;
                    this.StatusMessage = $"Server running on port {port}";
                }
                else
                {
                    this.Status = ServerStatus.Error;
                    this.StatusMessage = "Started but not responding.";
                }
            }
            catch (Exception ex)
            {
                this.Status = ServerStatus.Error;
                this.StatusMessage = "Error: " + ex.Message;
            }
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task StopAsync()
        {
            if (this.serverProcess != null && !this.serverProcess.HasExited)
            {
                try
                {
                    this.serverProcess.Kill();
                    this.serverProcess = null;
                }
                catch
                {
                    // Ignore errors during termination
                }
            }

            this.Status = ServerStatus.Stopped;
            this.StatusMessage = "Stopped";
            return Task.CompletedTask;
        }

        /// <summary>
        /// Checks if the server is running on the specified port.
        /// </summary>
        /// <param name="port">The port to check.</param>
        /// <returns>True if the server is running; otherwise, false.</returns>
        public async Task<bool> IsRunningAsync(int port)
        {
            try
            {
                using var client = new TcpClient();
                var result = client.BeginConnect("localhost", port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));
                if (success)
                {
                    client.EndConnect(result);

                    // If we didn't start it but it's running, update status
                    if (this.status != ServerStatus.Running && this.status != ServerStatus.Starting)
                    {
                        this.Status = ServerStatus.RunningExternal;
                        this.StatusMessage = "Running (External)";
                    }

                    return true;
                }
            }
            catch
            {
                // Port is not responding
            }

            return false;
        }

        private void NotifyStatusChanged()
        {
            this.StatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
