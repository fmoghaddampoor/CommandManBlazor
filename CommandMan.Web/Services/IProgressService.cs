using System;

namespace CommandMan.Web.Services
{
    public interface IProgressService
    {
        bool IsVisible { get; }
        string Title { get; }
        double Progress { get; }
        string Message { get; }
        event Action? OnChange;
        void Start(string title);
        void Update(double progress, string message = "");
        void Stop();
    }
}
