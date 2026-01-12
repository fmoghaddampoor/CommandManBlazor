using System;

namespace CommandMan.Core.Entities
{
    public abstract class FileSystemItem
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
        
        public abstract long Size { get; }
        public abstract string Extension { get; }
    }

    public class FileItem : FileSystemItem
    {
        public long FileSize { get; set; }
        
        public override long Size => FileSize;
        public override string Extension => System.IO.Path.GetExtension(Name);
    }

    public class DirectoryItem : FileSystemItem
    {
        public override long Size => 0; // Directories don't show size in standard view
        public override string Extension => "<DIR>";
    }
}
