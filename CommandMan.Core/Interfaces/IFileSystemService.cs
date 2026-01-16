// <copyright file="IFileSystemService.cs" company="CommandMan">
// Copyright (c) CommandMan. All rights reserved.
// </copyright>

namespace CommandMan.Core.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CommandMan.Core.Entities;

    /// <summary>
    /// Provides methods for interacting with the file system.
    /// </summary>
    public interface IFileSystemService
    {
        /// <summary>
        /// Gets the contents of a directory.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <returns>A list of file system items.</returns>
        Task<List<FileSystemItem>> GetDirectoryContentAsync(string path);

        /// <summary>
        /// Gets the available logical drives on the system.
        /// </summary>
        /// <returns>A list of drive items.</returns>
        Task<List<FileSystemItem>> GetDrivesAsync();

        /// <summary>
        /// Checks if a file or directory exists at the specified path.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>True if it exists; otherwise, false.</returns>
        Task<bool> ExistsAsync(string path);

        /// <summary>
        /// Creates a new directory at the specified path.
        /// </summary>
        /// <param name="path">The path where the directory should be created.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CreateDirectoryAsync(string path);

        /// <summary>
        /// Creates a new empty file at the specified path.
        /// </summary>
        /// <param name="path">The path where the file should be created.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CreateFileAsync(string path);

        /// <summary>
        /// Deletes the item at the specified path.
        /// </summary>
        /// <param name="path">The path to the item to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteItemAsync(string path);

        /// <summary>
        /// Renames an item from the source path to the new path.
        /// </summary>
        /// <param name="sourcePath">The current path of the item.</param>
        /// <param name="newPath">The new path/name for the item.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RenameItemAsync(string sourcePath, string newPath);

        /// <summary>
        /// Copies an item from the source path to the destination path.
        /// </summary>
        /// <param name="sourcePath">The source item path.</param>
        /// <param name="destPath">The destination path.</param>
        /// <param name="onProgress">Optional callback for progress reporting.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CopyItemAsync(string sourcePath, string destPath, Action<double>? onProgress = null);

        /// <summary>
        /// Moves an item from the source path to the destination path.
        /// </summary>
        /// <param name="sourcePath">The source item path.</param>
        /// <param name="destPath">The destination path.</param>
        /// <param name="onProgress">Optional callback for progress reporting.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MoveItemAsync(string sourcePath, string destPath, Action<double>? onProgress = null);

        /// <summary>
        /// Zips the specified items into a zip file.
        /// </summary>
        /// <param name="sourcePaths">The list of paths to items to zip.</param>
        /// <param name="destinationZipPath">The path where the zip file should be created.</param>
        /// <param name="compressionLevel">The level of compression to use.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ZipItemsAsync(List<string> sourcePaths, string destinationZipPath, System.IO.Compression.CompressionLevel compressionLevel);

        /// <summary>
        /// Unzips a file to the specified destination directory.
        /// </summary>
        /// <param name="zipFilePath">The path to the zip file.</param>
        /// <param name="destinationPath">The destination directory path.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UnzipItemAsync(string zipFilePath, string destinationPath);

        /// <summary>
        /// Opens a file with the default associated application (or preferred editor).
        /// </summary>
        /// <param name="path">The path to the file to open.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OpenFileAsync(string path);

        /// <summary>
        /// Opens Windows Explorer and selects the specified item.
        /// </summary>
        /// <param name="path">The path to the item.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ShowInExplorerAsync(string path);

        /// <summary>
        /// Gets the parent path of the specified item.
        /// </summary>
        /// <param name="path">The path to the item.</param>
        /// <returns>The parent directory path, or an empty string if none.</returns>
        string GetParentPath(string path);
    }
}
