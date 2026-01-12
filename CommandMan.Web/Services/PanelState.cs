using System;
using System.Collections.Generic;
using CommandMan.Core.Entities;

namespace CommandMan.Web.Services
{
    public class PanelState
    {
        public string CurrentPath { get; set; } = string.Empty;
        public List<FileSystemItem> Items { get; set; } = new();
        public FileSystemItem? SelectedItem { get; set; }
        
        public event Action? OnChange;

        public void NotifyStateChanged() => OnChange?.Invoke();

        public void MoveSelection(int steps)
        {
            if (Items.Count == 0) return;
            
            // If nothing selected, select first
            if (SelectedItem == null)
            {
                SelectedItem = Items[0];
                NotifyStateChanged();
                return;
            }

            int index = Items.IndexOf(SelectedItem);
            // If selected item not found (souldn't happen), select first
            if (index == -1) 
            {
                SelectedItem = Items[0];
                NotifyStateChanged();
                return;
            }
            
            index += steps;
            if (index < 0) index = 0;
            if (index >= Items.Count) index = Items.Count - 1;
            
            SelectedItem = Items[index];
            NotifyStateChanged();
        }
    }
}
