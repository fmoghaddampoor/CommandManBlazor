window.registerKeyboardHandler = (dotNetHelper) => {
    window.commandManKeyHandler = (e) => {
        // Blocks F-keys (F1-F12), ArrowUp, ArrowDown, Tab, Enter
        const isFKey = /^F[1-9][0-2]?$/.test(e.key);
        const commandKeys = ["ArrowUp", "ArrowDown", "Tab", "Enter"];

        if (isFKey || commandKeys.includes(e.key)) {
            e.preventDefault();
            dotNetHelper.invokeMethodAsync('HandleKeyDown', {
                key: e.key,
                shiftKey: e.shiftKey,
                ctrlKey: e.ctrlKey,
                altKey: e.altKey
            });
        }
    };
    window.addEventListener('keydown', window.commandManKeyHandler);
};

window.scrollToSelection = (element) => {
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }
};

window.unregisterKeyboardHandler = () => {
    if (window.commandManKeyHandler) {
        window.removeEventListener('keydown', window.commandManKeyHandler);
    }
};

// Panel path persistence
window.savePanelPath = (panelKey, path) => {
    try {
        localStorage.setItem(panelKey, path);
    } catch (e) {
        console.warn('Failed to save panel path:', e);
    }
};

window.loadPanelPath = (panelKey) => {
    try {
        return localStorage.getItem(panelKey) || null;
    } catch (e) {
        console.warn('Failed to load panel path:', e);
        return null;
    }
};
