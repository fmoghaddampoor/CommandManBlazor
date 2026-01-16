// <copyright file="App.xaml.cs" company="CommandMan">
// Copyright (c) CommandMan. All rights reserved.
// </copyright>

namespace CommandMan.Tray
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private const string MutexName = "{COMMANDMAN-B9A3-4122-8320-TRRAY-INSTANCE}";
        private static Mutex? mutex = null;

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

            base.OnStartup(e);

            // Important: Don't shutdown when main window closes (because we minimize to tray)
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Initialize Main Window with URL
            var window = new MainWindow(url);

            // We don't show the window initially if it's just a startup.
            // BUT, if it's the *first* launch, maybe we want to open the browser? Yes.
            // window.Show(); // Don't show UI, just start tray logic.
        }
    }
}
