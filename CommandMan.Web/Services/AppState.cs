// <copyright file="AppState.cs" company="CommandMan">
// Copyright (c) CommandMan. All rights reserved.
// </copyright>

namespace CommandMan.Web.Services
{
    using System;

    /// <summary>
    /// Represents the global state of the application.
    /// </summary>
    public class AppState : IAppState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppState"/> class.
        /// </summary>
        public AppState()
        {
            this.ActivePanel = this.LeftPanel;
        }

        /// <summary>
        /// Occurs when the application state changes.
        /// </summary>
        public event Action? OnChange;

        /// <summary>
        /// Gets the state of the left panel.
        /// </summary>
        public PanelState LeftPanel { get; } = new ();

        /// <summary>
        /// Gets the state of the right panel.
        /// </summary>
        public PanelState RightPanel { get; } = new ();

        /// <summary>
        /// Gets the currently active panel.
        /// </summary>
        public PanelState ActivePanel { get; private set; }

        /// <summary>
        /// Sets the active panel.
        /// </summary>
        /// <param name="panel">The panel to set as active.</param>
        public void SetActivePanel(PanelState panel)
        {
            if (this.ActivePanel != panel)
            {
                this.ActivePanel = panel;
                this.NotifyStateChanged();
            }
        }

        private void NotifyStateChanged() => this.OnChange?.Invoke();
    }
}
