using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandMan.Core.Entities;

namespace CommandMan.Web.Services
{
    public interface IFavoritesService
    {
        event Action? OnChange;
        Task LoadAsync();
        Task SaveAsync();
        List<FavoriteItem> GetFavorites();
        Task AddFavorite(string name, string path);
        Task UpdateFavorite(FavoriteItem item, string newName, string newPath);
        Task RemoveFavorite(int index);
        Task RemoveFavorite(string name);
        Task MoveUp(int index);
        Task MoveDown(int index);
        Task AddFolder(string name);
        Task MoveRight(FavoriteItem item, List<FavoriteItem> currentList);
        Task MoveLeft(FavoriteItem item, List<FavoriteItem> currentList, List<FavoriteItem>? parentList);
        Task MoveToFolder(FavoriteItem item, List<FavoriteItem> sourceList, FavoriteItem targetFolder);
        Task MoveToRoot(FavoriteItem item, List<FavoriteItem> sourceList);
    }
}
