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
        Task CopyItemAsync(string sourcePath, string destPath, Action<double>? onProgress = null);
        Task MoveItemAsync(string sourcePath, string destPath, Action<double>? onProgress = null);
        Task ZipItemsAsync(List<string> sourcePaths, string destinationZipPath, System.IO.Compression.CompressionLevel compressionLevel);
        Task OpenFileAsync(string path);
        string GetParentPath(string path);
    }
}
