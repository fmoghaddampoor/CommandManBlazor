// <copyright file="IServerService.cs" company="CommandMan">
// Copyright (c) CommandMan. All rights reserved.
// </copyright>

namespace CommandMan.Tray.Interfaces
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Service for managing the CommandMan backend server.
    /// </summary>
    public interface IServerService
    {
        /// <summary>
        /// Occurs when the server status changes.
        /// </summary>
        event EventHandler StatusChanged;

        /// <summary>
        /// Gets the current status of the server.
        /// </summary>
        ServerStatus Status { get; }

        /// <summary>
        /// Gets the message associated with the current status.
        /// </summary>
        string StatusMessage { get; }

        /// <summary>
        /// Starts the server on the specified port.
        /// </summary>
        /// <param name="port">The port to run the server on.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task StartAsync(int port);

        /// <summary>
        /// Stops the server.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task StopAsync();

        /// <summary>
        /// Checks if the server is running on the specified port.
        /// </summary>
        /// <param name="port">The port to check.</param>
        /// <returns>True if the server is running; otherwise, false.</returns>
        Task<bool> IsRunningAsync(int port);
    }
}
