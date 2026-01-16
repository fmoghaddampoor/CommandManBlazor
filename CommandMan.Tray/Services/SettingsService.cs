// <copyright file="SettingsService.cs" company="CommandMan">
// Copyright (c) CommandMan. All rights reserved.
// </copyright>

namespace CommandMan.Tray.Services
{
    using System;
    using System.IO;
    using CommandMan.Tray.Interfaces;

    /// <summary>
    /// Implementation of <see cref="ISettingsService"/> that uses a port.txt file.
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private const int DefaultPort = 5000;
        private readonly string settingsPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsService"/> class.
        /// </summary>
        public SettingsService()
        {
            this.settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "port.txt");
            this.Port = DefaultPort;
        }

        /// <summary>
        /// Gets or sets the application port.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Loads settings from the persistent store.
        /// </summary>
        public void Load()
        {
            try
            {
                if (File.Exists(this.settingsPath))
                {
                    string savedPortText = File.ReadAllText(this.settingsPath).Trim();
                    if (int.TryParse(savedPortText, out int port))
                    {
                        this.Port = port;
                    }
                }
            }
            catch
            {
                // Silently fallback to default
            }
        }

        /// <summary>
        /// Saves settings to the persistent store.
        /// </summary>
        public void Save()
        {
            try
            {
                File.WriteAllText(this.settingsPath, this.Port.ToString());
            }
            catch
            {
                // Silently fail if cannot save
            }
        }
    }
}
