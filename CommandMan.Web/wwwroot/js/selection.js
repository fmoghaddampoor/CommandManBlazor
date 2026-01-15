window.setupSelection = (dotNetHelper, containerId) => {
    const container = document.getElementById(containerId);
    if (!container) return;

    let isSelecting = false;
    let startX = 0;
    let startY = 0;
    let selectionBox = null;
    
    // Prevent text selection during drag
    const preventTextSelection = (e) => {
        if (isSelecting) {
            e.preventDefault();
        }
    };

    container.addEventListener('mousedown', (e) => {
        // Only left click, and not on interactive elements like inputs/buttons
        if (e.button !== 0 || e.target.tagName === 'INPUT' || e.target.tagName === 'BUTTON' || e.target.tagName === 'A' || e.target.closest('tr')) return;
        
        isSelecting = true;
        startX = e.clientX; 
        startY = e.clientY; // Relative to viewport is easier for fixed overlay
        
        // Create selection box
        selectionBox = document.createElement('div');
        selectionBox.className = 'selection-box';
        selectionBox.style.left = startX + 'px';
        selectionBox.style.top = startY + 'px';
        selectionBox.style.width = '0px';
        selectionBox.style.height = '0px';
        document.body.appendChild(selectionBox);
        
        document.addEventListener('mousemove', onMouseMove);
        document.addEventListener('mouseup', onMouseUp);
        document.addEventListener('selectstart', preventTextSelection);
    });

    const onMouseMove = (e) => {
        if (!isSelecting) return;
        
        const currentX = e.clientX;
        const currentY = e.clientY;
        
        const left = Math.min(startX, currentX);
        const top = Math.min(startY, currentY);
        const width = Math.abs(currentX - startX);
        const height = Math.abs(currentY - startY);
        
        if (selectionBox) {
            selectionBox.style.left = left + 'px';
            selectionBox.style.top = top + 'px';
            selectionBox.style.width = width + 'px';
            selectionBox.style.height = height + 'px';
        }
        
        // Optional: Highlight items in real-time
        // checkIntersections(left, top, width, height);
    };

    const onMouseUp = (e) => {
        if (!isSelecting) return;
        
        isSelecting = false;
        
        // Finalize selection
        const rect = selectionBox.getBoundingClientRect();
        checkIntersections(rect.left, rect.top, rect.width, rect.height);
        
        // Cleanup
        if (selectionBox) {
            selectionBox.remove();
            selectionBox = null;
        }
        
        document.removeEventListener('mousemove', onMouseMove);
        document.removeEventListener('mouseup', onMouseUp);
        document.removeEventListener('selectstart', preventTextSelection);
    };

    const checkIntersections = (boxLeft, boxTop, boxWidth, boxHeight) => {
        const rows = container.querySelectorAll('tr[data-index]');
        const selectedIndices = [];
        
        const boxRight = boxLeft + boxWidth;
        const boxBottom = boxTop + boxHeight;

        rows.forEach(row => {
            const rect = row.getBoundingClientRect();
            
            // Check intersection
            const isIntersecting = !(rect.left > boxRight || 
                                   rect.right < boxLeft || 
                                   rect.top > boxBottom || 
                                   rect.bottom < boxTop);
            
            if (isIntersecting) {
                const index = parseInt(row.getAttribute('data-index'));
                if (!isNaN(index)) {
                    selectedIndices.push(index);
                }
            }
        });
        
        // Invoke dotnet method
        if (selectedIndices.length > 0 || boxWidth > 5 || boxHeight > 5) {
             dotNetHelper.invokeMethodAsync('OnSelectionChanged', selectedIndices, false); // false for replace, true for append? 
             // We'll mimic Windows Explorer: simple drag replaces, ctrl+drag appends.
             // But we don't have easy access to modifier keys in onMouseUp without storing them from event.
             // For now, let's assume replacement.
        }
    };
};
