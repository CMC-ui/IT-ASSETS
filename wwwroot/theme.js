const THEME_KEY = 'it-assets-theme';

function getPreferredTheme() {
    const storedTheme = localStorage.getItem(THEME_KEY);
    if (storedTheme) {
        return storedTheme;
    }
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
}

function setTheme(theme) {
    const isDark = theme === 'auto' ? window.matchMedia('(prefers-color-scheme: dark)').matches : theme === 'dark';
    document.documentElement.setAttribute('data-bs-theme', isDark ? 'dark' : 'light');
    
    const toggleSwitch = document.getElementById('themeToggleSwitch');
    if (toggleSwitch) {
        toggleSwitch.checked = isDark;
    }
}

function toggleTheme() {
    const current = document.documentElement.getAttribute('data-bs-theme');
    const next = current === 'dark' ? 'light' : 'dark';
    localStorage.setItem(THEME_KEY, next);
    setTheme(next);
}

// Apply immediately on script load to prevent flashing
setTheme(getPreferredTheme());

// Listen for system theme changes if no explicit preference is set
window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
    if (!localStorage.getItem(THEME_KEY)) {
        setTheme(getPreferredTheme());
    }
});

function downloadFileFromBase64(filename, base64) {
    const link = document.createElement('a');
    link.download = filename;
    link.href = 'data:text/csv;charset=utf-8;base64,' + base64;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}
