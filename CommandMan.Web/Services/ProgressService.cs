using System;

namespace CommandMan.Web.Services
{
    public class ProgressService
    {
        public bool IsVisible { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public double Progress { get; private set; }
        public string Message { get; private set; } = string.Empty;

        public event Action? OnChange;

        public void Start(string title)
        {
            Title = title;
            Progress = 0;
            Message = string.Empty;
            IsVisible = true;
            NotifyStateChanged();
        }

        public void Update(double progress, string message = "")
        {
            Progress = progress;
            if (!string.IsNullOrEmpty(message)) Message = message;
            NotifyStateChanged();
        }

        public void Stop()
        {
            IsVisible = false;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
