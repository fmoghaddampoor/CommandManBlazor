using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using CommandMan.Core.Entities;

namespace CommandMan.Web.Services
{
    public class FavoritesService
    {
        private readonly IJSRuntime _js;
        private List<FavoriteItem> _favorites = new();

        public event Action? OnChange;

        public FavoritesService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task LoadAsync()
        {
            try
            {
                var json = await _js.InvokeAsync<string>("loadFavorites");
                if (!string.IsNullOrEmpty(json))
                {
                    _favorites = JsonSerializer.Deserialize<List<FavoriteItem>>(json) ?? new();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading favorites: {ex.Message}");
            }
        }

        public async Task SaveAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(_favorites);
                await _js.InvokeVoidAsync("saveFavorites", json);
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving favorites: {ex.Message}");
            }
        }

        public List<FavoriteItem> GetFavorites() => _favorites;

        public async Task AddFavorite(string name, string path)
        {
            _favorites.Add(new FavoriteItem { Name = name, Path = path });
            await SaveAsync();
        }

        public async Task UpdateFavorite(FavoriteItem item, string newName, string newPath)
        {
            item.Name = newName;
            item.Path = newPath;
            await SaveAsync();
        }

        public async Task RemoveFavorite(int index)
        {
            if (index >= 0 && index < _favorites.Count)
            {
                _favorites.RemoveAt(index);
                await SaveAsync();
            }
        }

        public async Task MoveUp(int index)
        {
            if (index > 0 && index < _favorites.Count)
            {
                var item = _favorites[index];
                _favorites.RemoveAt(index);
                _favorites.Insert(index - 1, item);
                await SaveAsync();
            }
        }

        public async Task MoveDown(int index)
        {
            if (index >= 0 && index < _favorites.Count - 1)
            {
                var item = _favorites[index];
                _favorites.RemoveAt(index);
                _favorites.Insert(index + 1, item);
                await SaveAsync();
            }
        }

        public async Task AddFolder(string name)
        {
            _favorites.Add(new FavoriteItem { Name = name, IsFolder = true });
            await SaveAsync();
        }

        public async Task MoveRight(FavoriteItem item, List<FavoriteItem> currentList)
        {
            int index = currentList.IndexOf(item);
            if (index > 0)
            {
                var targetFolder = currentList[index - 1];
                if (targetFolder.IsFolder)
                {
                    currentList.RemoveAt(index);
                    targetFolder.Children.Add(item);
                    await SaveAsync();
                }
            }
        }

        public async Task MoveLeft(FavoriteItem item, List<FavoriteItem> currentList, List<FavoriteItem>? parentList)
        {
            if (parentList != null)
            {
                int indexInParent = parentList.FindIndex(f => f.Children == currentList);
                if (indexInParent != -1)
                {
                    currentList.Remove(item);
                    parentList.Insert(indexInParent + 1, item);
                    await SaveAsync();
                }
            }
        }

        public async Task MoveToFolder(FavoriteItem item, List<FavoriteItem> sourceList, FavoriteItem targetFolder)
        {
            if (item != targetFolder && targetFolder.IsFolder && !IsDescendant(item, targetFolder))
            {
                sourceList.Remove(item);
                targetFolder.Children.Add(item);
                await SaveAsync();
            }
        }

        public async Task MoveToRoot(FavoriteItem item, List<FavoriteItem> sourceList)
        {
            if (_favorites != sourceList)
            {
                sourceList.Remove(item);
                _favorites.Add(item);
                await SaveAsync();
            }
        }

        private bool IsDescendant(FavoriteItem parent, FavoriteItem potentialDescendant)
        {
            if (parent.Children.Contains(potentialDescendant)) return true;
            foreach (var child in parent.Children)
            {
                if (child.IsFolder && IsDescendant(child, potentialDescendant)) return true;
            }
            return false;
        }

        public async Task RemoveFavorite(string name)
        {
            _favorites.RemoveAll(f => f.Name == name);
            await SaveAsync();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
