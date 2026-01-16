// <copyright file="ISettingsService.cs" company="CommandMan">
// Copyright (c) CommandMan. All rights reserved.
// </copyright>

namespace CommandMan.Tray.Interfaces
{
    /// <summary>
    /// Service for managing application settings.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Gets or sets the application port.
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// Loads settings from the persistent store.
        /// </summary>
        void Load();

        /// <summary>
        /// Saves settings to the persistent store.
        /// </summary>
        void Save();
    }
}
