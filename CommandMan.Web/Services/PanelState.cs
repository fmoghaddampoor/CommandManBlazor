using System;
using System.Collections.Generic;
using CommandMan.Core.Entities;

namespace CommandMan.Web.Services
{
    public class PanelState
    {
        public string CurrentPath { get; set; } = string.Empty;
        public List<FileSystemItem> Items { get; set; } = new();
        public bool IsLoading { get; set; }
        public HashSet<FileSystemItem> SelectedItems { get; set; } = new();
        public FileSystemItem? SelectedItem { get; set; } // The "active" item/cursor
        private FileSystemItem? _pivotItem; // Shift-click start point
        
        public string SortColumn { get; set; } = "Name";
        public bool SortAscending { get; set; } = true;

        public event Action? OnChange;

        public void NotifyStateChanged() => OnChange?.Invoke();

        public void Sort(string column = "")
        {
            if (!string.IsNullOrEmpty(column))
            {
                if (SortColumn == column)
                {
                    SortAscending = !SortAscending;
                }
                else
                {
                    SortColumn = column;
                    SortAscending = true;
                }
            }

            Items.Sort((a, b) =>
            {
                // Rule: Directories always at top
                if (a is DirectoryItem && b is FileItem) return -1;
                if (a is FileItem && b is DirectoryItem) return 1;

                int result = SortColumn switch
                {
                    "Extension" => string.Compare(a.Extension, b.Extension, StringComparison.OrdinalIgnoreCase),
                    "Date modified" => a.LastModified.CompareTo(b.LastModified),
                    "Size" => a.Size.CompareTo(b.Size),
                    "File version" => string.Compare(a.FileVersion, b.FileVersion, StringComparison.OrdinalIgnoreCase),
                    _ => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)
                };

                return SortAscending ? result : -result;
            });

            NotifyStateChanged();
        }

        public void SelectItem(FileSystemItem? item, bool clearExisting = true, bool toggle = false, bool range = false)
        {
            if (item == null)
            {
                SelectedItems.Clear();
                SelectedItem = null;
                _pivotItem = null;
                NotifyStateChanged();
                return;
            }

            if (range && _pivotItem != null && Items.Contains(_pivotItem))
            {
                int start = Items.IndexOf(_pivotItem);
                int end = Items.IndexOf(item);
                if (start > end) { int t = start; start = end; end = t; }
                
                SelectedItems.Clear();
                for (int i = start; i <= end; i++)
                {
                    SelectedItems.Add(Items[i]);
                }
            }
            else if (toggle)
            {
                if (SelectedItems.Contains(item))
                    SelectedItems.Remove(item);
                else
                    SelectedItems.Add(item);
                
                _pivotItem = item;
            }
            else
            {
                if (clearExisting)
                {
                    SelectedItems.Clear();
                    SelectedItems.Add(item);
                }
                else
                {
                    SelectedItems.Add(item);
                }
                _pivotItem = item;
            }

            SelectedItem = item;
            NotifyStateChanged();
        }

        public void MoveSelection(int steps, bool range = false)
        {
            if (Items.Count == 0) return;
            
            int index = SelectedItem != null ? Items.IndexOf(SelectedItem) : -1;
            
            if (index == -1) 
            {
                SelectItem(Items[0]);
                return;
            }
            
            int nextIndex = index + steps;
            if (nextIndex < 0) nextIndex = 0;
            if (nextIndex >= Items.Count) nextIndex = Items.Count - 1;
            
            var nextItem = Items[nextIndex];

            if (range)
            {
                // Shift + Arrow
                SelectItem(nextItem, range: true);
            }
            else
            {
                SelectItem(nextItem);
            }
        }

        public void SeekItem(string key)
        {
            if (Items.Count == 0 || string.IsNullOrEmpty(key)) return;

            // Find all items starting with the key
            var matches = Items
                .Where(i => i.Name.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matches.Count == 0) return;

            FileSystemItem nextMatch;
            if (SelectedItem != null && matches.Contains(SelectedItem))
            {
                // Already on a match, find the next one for cycling
                int currentIndex = matches.IndexOf(SelectedItem);
                int nextMatchIndex = (currentIndex + 1) % matches.Count;
                nextMatch = matches[nextMatchIndex];
            }
            else
            {
                // Not on a match, pick the first one
                nextMatch = matches[0];
            }

            SelectItem(nextMatch);
        }
    }
}
