using System.Collections.Generic;
using CommandMan.Core.Entities;

namespace CommandMan.Web.Services
{
    public interface IClipboardService
    {
        List<FileSystemItem> Items { get; }
        bool IsCut { get; }
        void Copy(IEnumerable<FileSystemItem> items);
        void Cut(IEnumerable<FileSystemItem> items);
        void Clear();
    }
}
