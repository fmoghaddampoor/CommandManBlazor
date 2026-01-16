// <copyright file="ServerStatus.cs" company="CommandMan">
// Copyright (c) CommandMan. All rights reserved.
// </copyright>

namespace CommandMan.Tray.Interfaces
{
    /// <summary>
    /// Possible statuses for the server.
    /// </summary>
    public enum ServerStatus
    {
        /// <summary>
        /// The server is not running.
        /// </summary>
        Stopped,

        /// <summary>
        /// The server is currently starting.
        /// </summary>
        Starting,

        /// <summary>
        /// The server is running.
        /// </summary>
        Running,

        /// <summary>
        /// The server is running externally (not managed by this app).
        /// </summary>
        RunningExternal,

        /// <summary>
        /// An error occurred with the server.
        /// </summary>
        Error,
    }
}
