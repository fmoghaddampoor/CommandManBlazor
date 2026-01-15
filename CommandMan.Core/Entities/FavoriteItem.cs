using System.Collections.Generic;

namespace CommandMan.Core.Entities
{
    public class FavoriteItem
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool IsFolder { get; set; }
        public List<FavoriteItem> Children { get; set; } = new();
    }
}
