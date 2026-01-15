using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CommandMan.Core.Entities;
using CommandMan.Core.Interfaces;

namespace CommandMan.Infrastructure.Services
{
    public class LocalFileSystemService : IFileSystemService
    {
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
                            LastModified = DateTime.Now
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

        public Task<List<FileSystemItem>> GetDirectoryContentAsync(string path)
        {
            return Task.Run(async () =>
            {
                var items = new List<FileSystemItem>();

                try
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        return await GetDrivesAsync();
                    }

                    var dirInfo = new DirectoryInfo(path);
                    
                    if (!dirInfo.Exists)
                    {
                        // Fallback to drives if path invalid?
                        return await GetDrivesAsync();
                    }

                    try 
                    {
                        foreach (var dir in dirInfo.GetDirectories())
                        {
                            items.Add(new DirectoryItem
                            {
                                Name = dir.Name,
                                FullPath = dir.FullName,
                                LastModified = dir.LastWriteTime
                            });
                        }
                    }
                    catch (UnauthorizedAccessException) { /* Skip inaccessible dirs */ }
                    catch (Exception ex) { Console.WriteLine($"Error reading dirs in {path}: {ex.Message}"); }

                    try
                    {
                        foreach (var file in dirInfo.GetFiles())
                        {
                            items.Add(new FileItem
                            {
                                Name = file.Name,
                                FullPath = file.FullName,
                                LastModified = file.LastWriteTime,
                                FileSize = file.Length,
                                FileVersion = FileVersionInfo.GetVersionInfo(file.FullName).FileVersion ?? string.Empty
                            });
                        }
                    }
                    catch (UnauthorizedAccessException) { /* Skip inaccessible files */ }
                    catch (Exception ex) { Console.WriteLine($"Error reading files in {path}: {ex.Message}"); }
                }
                catch (Exception ex)
                {
                    // Handle general permission errors etc.
                    Console.WriteLine($"Error accessing {path}: {ex.Message}");
                }

                return items;
            });
        }

        public Task<bool> ExistsAsync(string path)
        {
            return Task.FromResult(File.Exists(path) || Directory.Exists(path));
        }

        public Task CreateDirectoryAsync(string path)
        {
            return Task.Run(() => Directory.CreateDirectory(path));
        }

        public Task CreateFileAsync(string path)
        {
            return Task.Run(() => File.WriteAllText(path, string.Empty));
        }

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

        public async Task CopyItemAsync(string sourcePath, string destPath, Action<double>? onProgress = null)
        {
            await Task.Run(() =>
            {
                if (Directory.Exists(sourcePath))
                {
                    CopyDirectory(sourcePath, destPath, onProgress);
                }
                else
                {
                    File.Copy(sourcePath, destPath, true);
                    onProgress?.Invoke(100);
                }
            });
        }

        private void CopyDirectory(string sourceDir, string destinationDir, Action<double>? onProgress)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists) return;

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
                CopyDirectory(subdir.FullName, targetPath, null); // Nested progress is complex, skip for now
                processedItems++;
                onProgress?.Invoke((double)processedItems / totalItems * 100);
            }
        }

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
                    catch (Exception ex) // catch any error and fallback
                    {
                        Console.WriteLine($"Move failed: {ex.Message}. Falling back to Copy+Delete.");
                        await CopyItemAsync(sourcePath, destPath, onProgress);
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
                onProgress?.Invoke(100);
            });
        }

        public Task OpenFileAsync(string path)
        {
            return Task.Run(() =>
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

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

        public string GetParentPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            var parent = Directory.GetParent(path);
            return parent?.FullName ?? string.Empty;
        }
    }
}
