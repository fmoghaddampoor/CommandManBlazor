using System;

namespace CommandMan.Web.Services
{
    public interface IAppState
    {
        PanelState LeftPanel { get; }
        PanelState RightPanel { get; }
        PanelState ActivePanel { get; }
        void SetActivePanel(PanelState panel);
        event Action? OnChange;
    }
}
