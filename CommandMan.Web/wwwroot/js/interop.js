window.registerKeyboardHandler = (dotNetHelper) => {
    window.commandManKeyHandler = (e) => {
        // Skip if typing in an input or textarea
        if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') {
            return;
        }

        // Blocks F-keys (F1-F12), ArrowUp, ArrowDown, Tab, Enter
        const isFKey = /^F[1-9][0-2]?$/.test(e.key);
        const commandKeys = ["ArrowUp", "ArrowDown", "Tab", "Enter"];
        // Forward single character keys (a-z, 0-9, etc) for "seek-on-type"
        const isCharKey = e.key.length === 1 && !e.ctrlKey && !e.altKey;
        const isCtrlA = (e.key === 'a' || e.key === 'A') && e.ctrlKey;

        if (isFKey || commandKeys.includes(e.key) || isCharKey || isCtrlA) {
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

window.saveFavorites = (json) => {
    localStorage.setItem('favorites', json);
};

window.loadFavorites = () => {
    return localStorage.getItem('favorites') || "[]";
};

window.setHomeHelper = (helper) => {
    window.homeHelper = helper;
};

window.openAddFavoriteModal = () => {
    if (window.homeHelper) {
        window.homeHelper.invokeMethodAsync('OpenAddFavoriteModal');
    }
};

window.openManageFavoritesModal = () => {
    if (window.homeHelper) {
        window.homeHelper.invokeMethodAsync('OpenManageFavoritesModal');
    }
};

window.selectInputText = (element) => {
    if (element) {
        element.select();
    }
};

window.applyTheme = (themeClass) => {
    // Remove all previous theme classes
    const classes = document.documentElement.classList;
    for (let c of classes) {
        if (c.startsWith('theme-')) {
            document.documentElement.classList.remove(c);
        }
    }
    if (themeClass && themeClass !== 'theme-standard') {
        document.documentElement.classList.add(themeClass);
    }
};
