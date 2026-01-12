using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
                                FileSize = file.Length
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

        public Task CopyItemAsync(string sourcePath, string destPath)
        {
            // Simplified copy. Real implementation needs recursion for folders.
            return Task.Run(() =>
            {
                if (Directory.Exists(sourcePath))
                {
                    // TODO: Recursive copy for directories
                    // For now MVP just creates the folder
                    Directory.CreateDirectory(destPath);
                }
                else
                {
                    File.Copy(sourcePath, destPath, true);
                }
            });
        }

        public Task MoveItemAsync(string sourcePath, string destPath)
        {
            return Task.Run(() =>
            {
                if (Directory.Exists(sourcePath))
                {
                    Directory.Move(sourcePath, destPath);
                }
                else
                {
                    File.Move(sourcePath, destPath);
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
