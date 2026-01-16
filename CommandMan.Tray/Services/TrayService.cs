// <copyright file="TrayService.cs" company="CommandMan">
// Copyright (c) CommandMan. All rights reserved.
// </copyright>

namespace CommandMan.Tray.Services
{
    using System;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Input;
    using CommandMan.Tray.Interfaces;
    using Application = System.Windows.Application;

    /// <summary>
    /// Implementation of <see cref="ITrayService"/> using <see cref="NotifyIcon"/>.
    /// </summary>
    public class TrayService : ITrayService
    {
        private NotifyIcon? trayIcon;
        private ContextMenuStrip? contextMenu;

        /// <summary>
        /// Occurs when the tray icon is double-clicked.
        /// </summary>
        public event EventHandler? DoubleClick;

        /// <summary>
        /// Initializes the tray icon.
        /// </summary>
        public void Initialize()
        {
            this.trayIcon = new NotifyIcon();
            this.trayIcon.Text = "CommandMan Server";
            this.contextMenu = new ContextMenuStrip();

            try
            {
                var resourceInfo = Application.GetResourceStream(new Uri("pack://application:,,,/App.ico"));
                if (resourceInfo != null)
                {
                    this.trayIcon.Icon = new Icon(resourceInfo.Stream);
                }
                else
                {
                    this.trayIcon.Icon = SystemIcons.Application;
                }
            }
            catch
            {
                this.trayIcon.Icon = SystemIcons.Application;
            }

            this.trayIcon.Visible = true;
            this.trayIcon.DoubleClick += (s, e) => this.DoubleClick?.Invoke(this, EventArgs.Empty);
            this.trayIcon.ContextMenuStrip = this.contextMenu;
        }

        /// <summary>
        /// Adds a menu item to the tray icon's context menu.
        /// </summary>
        /// <param name="text">The text of the menu item.</param>
        /// <param name="command">The command to execute.</param>
        public void AddMenuItem(string text, ICommand command)
        {
            if (this.contextMenu == null)
            {
                return;
            }

            this.contextMenu.Items.Add(text, null, (s, e) =>
            {
                if (command.CanExecute(null))
                {
                    command.Execute(null);
                }
            });
        }

        /// <summary>
        /// Adds a separator to the context menu.
        /// </summary>
        public void AddSeparator()
        {
            this.contextMenu?.Items.Add("-");
        }

        /// <summary>
        /// Shows a balloon tip.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        public void ShowMessage(string title, string message)
        {
            this.trayIcon?.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
        }

        /// <summary>
        /// Disposes the tray icon.
        /// </summary>
        public void Dispose()
        {
            if (this.trayIcon != null)
            {
                this.trayIcon.Visible = false;
                this.trayIcon.Dispose();
                this.trayIcon = null;
            }

            this.contextMenu?.Dispose();
        }
    }
}
