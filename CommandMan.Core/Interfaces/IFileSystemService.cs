using System.Collections.Generic;
using System.Threading.Tasks;
using CommandMan.Core.Entities;

namespace CommandMan.Core.Interfaces
{
    public interface IFileSystemService
    {
        Task<List<FileSystemItem>> GetDirectoryContentAsync(string path);
        Task<List<FileSystemItem>> GetDrivesAsync();
        Task<bool> ExistsAsync(string path);
        Task CreateDirectoryAsync(string path);
        Task CreateFileAsync(string path);
        Task DeleteItemAsync(string path);
        Task RenameItemAsync(string sourcePath, string newPath);
        Task CopyItemAsync(string sourcePath, string destPath, Action<double>? onProgress = null);
        Task MoveItemAsync(string sourcePath, string destPath, Action<double>? onProgress = null);
        Task ZipItemsAsync(List<string> sourcePaths, string destinationZipPath, System.IO.Compression.CompressionLevel compressionLevel);
        Task UnzipItemAsync(string zipFilePath, string destinationPath);
        Task OpenFileAsync(string path);
        Task ShowInExplorerAsync(string path);
        string GetParentPath(string path);
    }
}
