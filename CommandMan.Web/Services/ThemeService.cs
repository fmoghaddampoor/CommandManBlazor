using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace CommandMan.Web.Services
{
    public class ThemeService
    {
        private readonly ProtectedLocalStorage _localStorage;
        private readonly IJSRuntime _jsRuntime;
        private string _currentTheme = "standard";

        public event Action? OnThemeChanged;

        public ThemeService(ProtectedLocalStorage localStorage, IJSRuntime jsRuntime)
        {
            _localStorage = localStorage;
            _jsRuntime = jsRuntime;
        }

        public string CurrentTheme => _currentTheme;

        public async Task LoadThemeAsync()
        {
            try
            {
                var result = await _localStorage.GetAsync<string>("app_theme");
                if (result.Success && !string.IsNullOrEmpty(result.Value))
                {
                    _currentTheme = result.Value;
                    await ApplyThemeAsync(_currentTheme);
                }
            }
            catch
            {
                // Fallback or ignore
            }
        }

        public async Task SetThemeAsync(string theme)
        {
            if (_currentTheme != theme)
            {
                _currentTheme = theme;
                await _localStorage.SetAsync("app_theme", theme);
                await ApplyThemeAsync(theme);
                OnThemeChanged?.Invoke();
            }
        }

        private async Task ApplyThemeAsync(string theme)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("applyTheme", $"theme-{theme}");
            }
            catch
            {
                // Might fail during pre-rendering
            }
        }
    }
}
