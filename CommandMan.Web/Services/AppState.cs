using System;

namespace CommandMan.Web.Services
{
    public class AppState : IAppState
    {
        public PanelState LeftPanel { get; } = new();
        public PanelState RightPanel { get; } = new();
        
        public PanelState ActivePanel { get; private set; }

        public AppState()
        {
            ActivePanel = LeftPanel;
        }

        public void SetActivePanel(PanelState panel)
        {
            if (ActivePanel != panel)
            {
                ActivePanel = panel;
                NotifyStateChanged();
            }
        }

        public event Action? OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
