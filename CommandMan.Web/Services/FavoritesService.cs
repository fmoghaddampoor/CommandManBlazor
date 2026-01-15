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

        public async Task UpdateFavorite(int index, string name)
        {
            if (index >= 0 && index < _favorites.Count)
            {
                _favorites[index].Name = name;
                await SaveAsync();
            }
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

        public async Task RemoveFavorite(string name)
        {
            _favorites.RemoveAll(f => f.Name == name);
            await SaveAsync();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
