const THEME_KEY = 'it-assets-theme';

function getPreferredTheme() {
    const storedTheme = localStorage.getItem(THEME_KEY);
    if (storedTheme) {
        return storedTheme;
    }
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
}

function setTheme(theme) {
    if (theme === 'auto') {
        theme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }
    document.documentElement.setAttribute('data-bs-theme', theme);
    
    const themeSelect = document.getElementById('themeSelect');
    if (themeSelect) {
        themeSelect.value = theme;
    }
}

function changeTheme(theme) {
    localStorage.setItem(THEME_KEY, theme);
    setTheme(theme);
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

function toggleSidebar() {
    const page = document.querySelector('.page');
    if (page) {
        page.classList.toggle('sidebar-hidden');
    }
}
