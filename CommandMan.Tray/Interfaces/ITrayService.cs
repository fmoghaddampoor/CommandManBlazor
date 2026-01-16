// <copyright file="ITrayService.cs" company="CommandMan">
// Copyright (c) CommandMan. All rights reserved.
// </copyright>

namespace CommandMan.Tray.Interfaces
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// Service for managing the system tray icon.
    /// </summary>
    public interface ITrayService : IDisposable
    {
        /// <summary>
        /// Occurs when the tray icon is double-clicked.
        /// </summary>
        event EventHandler DoubleClick;

        /// <summary>
        /// Initializes the tray icon.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Adds a menu item to the tray icon's context menu.
        /// </summary>
        /// <param name="text">The text of the menu item.</param>
        /// <param name="command">The command to execute.</param>
        void AddMenuItem(string text, ICommand command);

        /// <summary>
        /// Adds a separator to the context menu.
        /// </summary>
        void AddSeparator();

        /// <summary>
        /// Shows a balloon tip.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        void ShowMessage(string title, string message);
    }
}
