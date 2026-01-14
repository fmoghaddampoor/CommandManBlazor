// Drag and Drop Interop for CommandMan
window.setupDragDrop = function (dotNetHelper, elementId) {
    const panel = document.getElementById(elementId);
    if (!panel) {
        console.warn('[D&D Debug] JS: Panel not found for id', elementId);
        return;
    }

    panel.addEventListener('dragover', function (e) {
        e.preventDefault(); // Required to allow drop
        panel.classList.add('drag-over');
    });

    panel.addEventListener('dragleave', function (e) {
        panel.classList.remove('drag-over');
    });

    panel.addEventListener('drop', async function (e) {
        e.preventDefault();
        panel.classList.remove('drag-over');
        console.log('[D&D Debug] JS: Drop event on', elementId);
        // Notify Blazor
        try {
            await dotNetHelper.invokeMethodAsync('OnDropFromJS');
        } catch (err) {
            console.error('[D&D Debug] JS: Error invoking OnDropFromJS', err);
        }
    });

    console.log('[D&D Debug] JS: Setup complete for', elementId);
};
