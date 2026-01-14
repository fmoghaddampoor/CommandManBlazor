using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace CommandMan.Tray
{
    public partial class App : System.Windows.Application
    {
        private static Mutex _mutex = null;
        private const string MutexName = "{COMMANDMAN-B9A3-4122-8320-TRRAY-INSTANCE}";

        protected override void OnStartup(StartupEventArgs e)
        {
            // Parse arguments
            string left = "";
            string right = "";
            for (int i = 0; i < e.Args.Length; i++)
            {
                if (e.Args[i] == "-left" && i + 1 < e.Args.Length) left = e.Args[i + 1];
                if (e.Args[i] == "-right" && i + 1 < e.Args.Length) right = e.Args[i + 1];
            }

            // Construct URL
            string url = "http://localhost:5000";
            if (!string.IsNullOrEmpty(left) || !string.IsNullOrEmpty(right))
            {
                url += "?";
                if (!string.IsNullOrEmpty(left)) url += $"left={Uri.EscapeDataString(left)}";
                if (!string.IsNullOrEmpty(left) && !string.IsNullOrEmpty(right)) url += "&";
                if (!string.IsNullOrEmpty(right)) url += $"right={Uri.EscapeDataString(right)}";
            }

            // Check for existing instance
            bool createdNew;
            _mutex = new Mutex(true, MutexName, out createdNew);

            if (!createdNew)
            {
                // Instance exists. Just launch browser.
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch { }
                
                // Exit immediately
                Shutdown();
                return;
            }

            base.OnStartup(e);

            // Important: Don't shutdown when main window closes (because we minimize to tray)
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Initialize Main Window with URL
            var window = new MainWindow(url);
            // We don't show the window initially if it's just a startup. 
            // BUT, if it's the *first* launch, maybe we want to open the browser? Yes.
            // window.Show(); // Don't show UI, just start tray logic.
        }
    }
}
