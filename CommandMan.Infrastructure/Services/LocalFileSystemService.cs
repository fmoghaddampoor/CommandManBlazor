// <copyright file="LocalFileSystemService.cs" company="CommandMan">
// Copyright (c) CommandMan. All rights reserved.
// </copyright>

namespace CommandMan.Infrastructure.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using CommandMan.Core.Entities;
    using CommandMan.Core.Interfaces;

    /// <summary>
    /// Implementation of <see cref="IFileSystemService"/> for the local file system.
    /// </summary>
    public class LocalFileSystemService : IFileSystemService
    {
        /// <summary>
        /// Gets the available logical drives on the local system.
        /// </summary>
        /// <returns>A list of drive items.</returns>
        public Task<List<FileSystemItem>> GetDrivesAsync()
        {
            return Task.Run(() =>
            {
                var items = new List<FileSystemItem>();
                try
                {
                    foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
                    {
                        items.Add(new DirectoryItem
                        {
                            Name = drive.Name, // e.g. "C:\"
                            FullPath = drive.Name,
                            LastModified = DateTime.Now,
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting drives: {ex.Message}");
                }

                return items;
            });
        }

        /// <summary>
        /// Gets the contents of a directory on the local system.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <returns>A list of file system items.</returns>
        public Task<List<FileSystemItem>> GetDirectoryContentAsync(string path)
        {
            return Task.Run(async () =>
            {
                var items = new List<FileSystemItem>();

                try
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        return await this.GetDrivesAsync();
                    }

                    var dirInfo = new DirectoryInfo(path);

                    if (!dirInfo.Exists)
                    {
                        return await this.GetDrivesAsync();
                    }

                    // Directories
                    try
                    {
                        foreach (var dir in dirInfo.GetDirectories())
                        {
                            try
                            {
                                items.Add(new DirectoryItem
                                {
                                    Name = dir.Name,
                                    FullPath = dir.FullName,
                                    LastModified = dir.LastWriteTime,
                                });
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error processing directory {dir.FullName}: {ex.Message}");
                            }
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        /* Skip inaccessible dirs */
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading dirs in {path}: {ex.Message}");
                    }

                    // Files
                    try
                    {
                        foreach (var file in dirInfo.GetFiles())
                        {
                            try
                            {
                                var fileItem = new FileItem
                                {
                                    Name = file.Name,
                                    FullPath = file.FullName,
                                    LastModified = file.LastWriteTime,
                                    FileSize = file.Length,
                                };

                                try
                                {
                                    fileItem.FileVersion = FileVersionInfo.GetVersionInfo(file.FullName).FileVersion ?? string.Empty;
                                }
                                catch
                                {
                                    fileItem.FileVersion = string.Empty;
                                }

                                items.Add(fileItem);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error processing file {file.FullName}: {ex.Message}");
                            }
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        /* Skip inaccessible files */
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading files in {path}: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accessing {path}: {ex.Message}");
                }

                Console.WriteLine($"GetDirectoryContentAsync for {path} returning {items.Count} items");
                return items;
            });
        }

        /// <summary>
        /// Checks if a file or directory exists at the specified path.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>True if it exists; otherwise, false.</returns>
        public Task<bool> ExistsAsync(string path)
        {
            return Task.FromResult(File.Exists(path) || Directory.Exists(path));
        }

        /// <summary>
        /// Creates a new directory at the specified path.
        /// </summary>
        /// <param name="path">The path where the directory should be created.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task CreateDirectoryAsync(string path)
        {
            return Task.Run(() => Directory.CreateDirectory(path));
        }

        /// <summary>
        /// Creates a new empty file at the specified path.
        /// </summary>
        /// <param name="path">The path where the file should be created.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task CreateFileAsync(string path)
        {
            return Task.Run(() => File.WriteAllText(path, string.Empty));
        }

        /// <summary>
        /// Deletes the item at the specified path.
        /// </summary>
        /// <param name="path">The path to the item to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task DeleteItemAsync(string path)
        {
            return Task.Run(() =>
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                else if (File.Exists(path))
                {
                    File.Delete(path);
                }
            });
        }

        /// <summary>
        /// Renames an item from the source path to the new path.
        /// </summary>
        /// <param name="sourcePath">The current path of the item.</param>
        /// <param name="newPath">The new path/name for the item.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task RenameItemAsync(string sourcePath, string newPath)
        {
            return Task.Run(() =>
            {
                if (Directory.Exists(sourcePath))
                {
                    Directory.Move(sourcePath, newPath);
                }
                else if (File.Exists(sourcePath))
                {
                    File.Move(sourcePath, newPath, true);
                }
            });
        }

        /// <summary>
        /// Copies an item from the source path to the destination path.
        /// </summary>
        /// <param name="sourcePath">The source item path.</param>
        /// <param name="destPath">The destination path.</param>
        /// <param name="onProgress">Optional callback for progress reporting.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CopyItemAsync(string sourcePath, string destPath, Action<double>? onProgress = null)
        {
            await Task.Run(() =>
            {
                if (Directory.Exists(sourcePath))
                {
                    this.CopyDirectory(sourcePath, destPath, onProgress);
                }
                else
                {
                    File.Copy(sourcePath, destPath, true);
                    onProgress?.Invoke(100);
                }
            });
        }

        /// <summary>
        /// Moves an item from the source path to the destination path.
        /// </summary>
        /// <param name="sourcePath">The source item path.</param>
        /// <param name="destPath">The destination path.</param>
        /// <param name="onProgress">Optional callback for progress reporting.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task MoveItemAsync(string sourcePath, string destPath, Action<double>? onProgress = null)
        {
            await Task.Run(async () =>
            {
                if (Directory.Exists(sourcePath))
                {
                    Console.WriteLine($"Moving directory: {sourcePath} -> {destPath}");
                    try
                    {
                        Directory.Move(sourcePath, destPath);
                    }
                    catch (Exception ex)
                    {
                        // catch any error and fallback
                        Console.WriteLine($"Move failed: {ex.Message}. Falling back to Copy+Delete.");
                        await this.CopyItemAsync(sourcePath, destPath, onProgress);
                        Directory.Delete(sourcePath, true);
                    }
                }
                else
                {
                    Console.WriteLine($"Moving file: {sourcePath} -> {destPath}");
                    try
                    {
                        File.Move(sourcePath, destPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"File move failed: {ex.Message}. Falling back to Copy+Delete.");
                        File.Copy(sourcePath, destPath, true);
                        File.Delete(sourcePath);
                    }
                }
            });
        }

        /// <summary>
        /// Zips the specified items into a zip file.
        /// </summary>
        /// <param name="sourcePaths">The list of paths to items to zip.</param>
        /// <param name="destinationZipPath">The path where the zip file should be created.</param>
        /// <param name="compressionLevel">The level of compression to use.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ZipItemsAsync(List<string> sourcePaths, string destinationZipPath, System.IO.Compression.CompressionLevel compressionLevel)
        {
            await Task.Run(() =>
            {
                // Create a temporary directory to collect items
                string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);

                try
                {
                    foreach (var sourcePath in sourcePaths)
                    {
                        if (string.IsNullOrEmpty(sourcePath))
                        {
                            continue;
                        }

                        string name = Path.GetFileName(sourcePath);
                        if (string.IsNullOrEmpty(name))
                        {
                            // If it's a drive root or something similar
                            name = new DirectoryInfo(sourcePath).Name;
                        }

                        string destPath = Path.Combine(tempDir, name);

                        if (Directory.Exists(sourcePath))
                        {
                            this.CopyDirectory(sourcePath, destPath, null);
                        }
                        else if (File.Exists(sourcePath))
                        {
                            File.Copy(sourcePath, destPath, true);
                        }
                    }

                    // Create Zip from temp directory
                    if (File.Exists(destinationZipPath))
                    {
                        File.Delete(destinationZipPath);
                    }

                    System.IO.Compression.ZipFile.CreateFromDirectory(tempDir, destinationZipPath, compressionLevel, false);
                }
                finally
                {
                    // Cleanup temp directory
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                }
            });
        }

        /// <summary>
        /// Unzips a file to the specified destination directory.
        /// </summary>
        /// <param name="zipFilePath">The path to the zip file.</param>
        /// <param name="destinationPath">The destination directory path.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UnzipItemAsync(string zipFilePath, string destinationPath)
        {
            await Task.Run(() =>
            {
                if (!Directory.Exists(destinationPath))
                {
                    Directory.CreateDirectory(destinationPath);
                }

                // Requires System.IO.Compression.ZipFile
                System.IO.Compression.ZipFile.ExtractToDirectory(zipFilePath, destinationPath, true);
            });
        }

        /// <summary>
        /// Opens a file with the default associated application (or preferred editor).
        /// </summary>
        /// <param name="path">The path to the file to open.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task OpenFileAsync(string path)
        {
            return Task.Run(() =>
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return;
                }

                string nppPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Notepad++", "notepad++.exe");
                if (!File.Exists(nppPath))
                {
                    nppPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Notepad++", "notepad++.exe");
                }

                if (File.Exists(nppPath))
                {
                    Process.Start(nppPath, $"\"{path}\"");
                }
                else
                {
                    Process.Start("notepad.exe", $"\"{path}\"");
                }
            });
        }

        /// <summary>
        /// Opens Windows Explorer and selects the specified item.
        /// </summary>
        /// <param name="path">The path to the item.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task ShowInExplorerAsync(string path)
        {
            return Task.Run(() =>
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return;
                }

                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                try
                {
                    if (File.Exists(path) || Directory.Exists(path))
                    {
                        Process.Start("explorer.exe", $"/select,\"{path}\"");
                    }
                    else
                    {
                        // If path doesn't exist, maybe it's a drive or parent path
                        Process.Start("explorer.exe", $"\"{path}\"");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error showing in explorer: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Gets the parent path of the specified item.
        /// </summary>
        /// <param name="path">The path to the item.</param>
        /// <returns>The parent directory path, or an empty string if none.</returns>
        public string GetParentPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            var parent = Directory.GetParent(path);
            return parent?.FullName ?? string.Empty;
        }

        private void CopyDirectory(string sourceDir, string destinationDir, Action<double>? onProgress)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
            {
                return;
            }

            Directory.CreateDirectory(destinationDir);

            var files = dir.GetFiles();
            var subdirs = dir.GetDirectories();

            int totalItems = files.Length + subdirs.Length;
            int processedItems = 0;

            foreach (var file in files)
            {
                string targetPath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetPath, true);
                processedItems++;
                onProgress?.Invoke((double)processedItems / totalItems * 100);
            }

            foreach (var subdir in subdirs)
            {
                string targetPath = Path.Combine(destinationDir, subdir.Name);

                // Nested progress is complex, skip for now
                this.CopyDirectory(subdir.FullName, targetPath, null);
                processedItems++;
                onProgress?.Invoke((double)processedItems / totalItems * 100);
            }
        }
    }
}
