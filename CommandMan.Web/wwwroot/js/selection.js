window.setupSelection = (dotNetHelper, containerId) => {
    const container = document.getElementById(containerId);
    if (!container) return;

    // Find the file-list div inside the container
    const fileList = container.querySelector('.file-list');
    if (!fileList) return;

    let isSelecting = false;
    let startX = 0;
    let startY = 0;
    let selectionBox = null;
    let dragOccurred = false; // Track if a drag selection happened

    // Prevent text selection during drag
    const preventTextSelection = (e) => {
        if (isSelecting) {
            e.preventDefault();
        }
    };

    // Prevent click event after drag selection
    fileList.addEventListener('click', (e) => {
        if (dragOccurred) {
            e.stopPropagation();
            e.preventDefault();
            dragOccurred = false;
        }
    }, true); // Use capture phase to intercept before Blazor

    fileList.addEventListener('mousedown', (e) => {
        // Only left click
        if (e.button !== 0) return;

        // Skip if clicking on interactive elements
        if (e.target.tagName === 'INPUT' || e.target.tagName === 'BUTTON' || e.target.tagName === 'A') return;

        // Skip if clicking directly on a table row (single click selection handles this)
        const clickedRow = e.target.closest('tr[data-index]');
        if (clickedRow) return;

        isSelecting = true;
        dragOccurred = false;
        startX = e.clientX;
        startY = e.clientY;

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

        // Highlight items in real-time
        highlightIntersections(left, top, width, height);
    };

    const onMouseUp = (e) => {
        if (!isSelecting) return;

        isSelecting = false;

        // Finalize selection
        const rect = selectionBox.getBoundingClientRect();

        // Mark that a drag occurred if the selection box was actually dragged
        if (rect.width > 5 || rect.height > 5) {
            dragOccurred = true;
        }

        checkIntersections(rect.left, rect.top, rect.width, rect.height);

        // Cleanup
        if (selectionBox) {
            selectionBox.remove();
            selectionBox = null;
        }

        // Clear all highlights
        clearHighlights();

        document.removeEventListener('mousemove', onMouseMove);
        document.removeEventListener('mouseup', onMouseUp);
        document.removeEventListener('selectstart', preventTextSelection);
    };

    const highlightIntersections = (boxLeft, boxTop, boxWidth, boxHeight) => {
        const rows = fileList.querySelectorAll('tr[data-index]');

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
                row.classList.add('selection-highlight');
            } else {
                row.classList.remove('selection-highlight');
            }
        });
    };

    const clearHighlights = () => {
        const rows = fileList.querySelectorAll('tr[data-index]');
        rows.forEach(row => row.classList.remove('selection-highlight'));
    };

    const checkIntersections = (boxLeft, boxTop, boxWidth, boxHeight) => {
        const rows = fileList.querySelectorAll('tr[data-index]');
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
            dotNetHelper.invokeMethodAsync('OnSelectionChanged', selectedIndices, false);
        }
    };
};
