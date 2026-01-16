(function() {
    const originalLog = console.log;
    const originalWarn = console.warn;
    const originalError = console.error;

    function sendLog(level, args) {
        // Convert args to string or object
        const message = args.map(arg => typeof arg === 'object' ? JSON.stringify(arg) : String(arg)).join(' ');
        
        // Use sendBeacon for reliability on unload, or fetch. Fetch is fine for general use.
        // We use check to avoid infinite loops if fetch fails and logs error.
        try {
            fetch('/api/log', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ level: level, message: message })
            }).catch(() => {}); // Ignore errors logging to avoid loop
        } catch (e) {}
    }

    console.log = function(...args) {
        originalLog.apply(console, args);
        sendLog('info', args);
    };

    console.warn = function(...args) {
        originalWarn.apply(console, args);
        sendLog('warn', args);
    };

    console.error = function(...args) {
        originalError.apply(console, args);
        sendLog('error', args);
    };
})();
