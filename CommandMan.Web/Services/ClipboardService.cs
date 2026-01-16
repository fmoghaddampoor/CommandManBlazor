using System.Collections.Generic;
using CommandMan.Core.Entities;

namespace CommandMan.Web.Services
{
    public class ClipboardService : IClipboardService
    {
        public List<FileSystemItem> Items { get; private set; } = new();
        public bool IsCut { get; private set; }

        public void Copy(IEnumerable<FileSystemItem> items)
        {
            Items.Clear();
            Items.AddRange(items);
            IsCut = false;
        }

        public void Cut(IEnumerable<FileSystemItem> items)
        {
            Items.Clear();
            Items.AddRange(items);
            IsCut = true;
        }

        public void Clear()
        {
            Items.Clear();
            IsCut = false;
        }
    }
}
