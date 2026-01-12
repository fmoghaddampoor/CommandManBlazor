window.registerKeyboardHandler = (dotNetHelper) => {
    window.commandManKeyHandler = (e) => {
        // Prevent default for specific keys
        const blockedKeys = ["F3", "F4", "F5", "F6", "F7", "F8", "ArrowUp", "ArrowDown", "Tab", "Enter"];
        if (blockedKeys.includes(e.key)) {
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
