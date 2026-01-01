/**
 * Cache clearing logic for CAST application
 * Clears browser cache, localStorage, and sessionStorage when pages are closed
 */

// Store page load time for tracking
const pageLoadTime = Date.now();

// Function to clear all cache
function clearAllCache() {
    try {
        // Clear localStorage
        if (typeof (Storage) !== "undefined") {
            localStorage.clear();
        }

        // Clear sessionStorage
        if (typeof (sessionStorage) !== "undefined") {
            sessionStorage.clear();
        }

        // Clear browser cache headers
        if (navigator.serviceWorker) {
            navigator.serviceWorker.getRegistrations().then(registrations => {
                registrations.forEach(registration => {
                    registration.unregister();
                });
            });
        }

        // Clear IndexedDB
        if (typeof (indexedDB) !== "undefined") {
            indexedDB.databases().then(dbs => {
                dbs.forEach(db => {
                    indexedDB.deleteDatabase(db.name);
                });
            });
        }

        console.log('Cache cleared successfully');
    } catch (error) {
        console.error('Error clearing cache:', error);
    }
}

// Clear cache when user leaves the page
window.addEventListener('beforeunload', function (e) {
    clearAllCache();
});

// Clear cache when page is unloaded
window.addEventListener('unload', function () {
    clearAllCache();
});

// Clear cache when tab is closed (pagehide event)
window.addEventListener('pagehide', function (e) {
    if (e.persisted === false) {
        clearAllCache();
    }
});

// Listen for visibility changes (when user switches tabs or closes window)
document.addEventListener('visibilitychange', function () {
    if (document.hidden) {
        // Page is hidden, clear cache
        clearAllCache();
    }
});

// Clear cache on browser back/forward navigation
window.addEventListener('pageshow', function (e) {
    if (e.persisted) {
        clearAllCache();
    }
});
