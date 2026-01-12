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
        Task DeleteItemAsync(string path);
        Task CopyItemAsync(string sourcePath, string destPath);
        Task MoveItemAsync(string sourcePath, string destPath);
        string GetParentPath(string path);
    }
}
