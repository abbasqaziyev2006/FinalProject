/**
 * Dark Mode Theme Manager
 * Handles dark mode toggle and persistence
 * Improved to avoid innerHTML replacements and provide accessible animated toggle
 */

class DarkModeTheme {
    constructor() {
        this.STORAGE_KEY = 'theme-preference';
        this.DARK_CLASS = 'dark-mode';
        this.LIGHT_CLASS = 'light-mode';
        this.SYSTEM_PREF = 'system';
        this.init();
    }

    init() {
        this.loadTheme();
        this.attachToggleListener();
        this.watchSystemPreference();
    }

    /**
     * Load saved theme preference or fall back to system
     */
    loadTheme() {
        const savedTheme = localStorage.getItem(this.STORAGE_KEY);

        if (savedTheme === 'dark') {
            this.enableDarkMode();
        } else if (savedTheme === 'light') {
            this.enableLightMode();
        } else {
            this.applySystemPreference();
        }

        this.updateToggleUI();
    }

    /**
     * Apply current system preference if no stored override
     */
    applySystemPreference() {
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            this.enableDarkMode();
        } else {
            this.enableLightMode();
        }
    }

    enableDarkMode() {
        const doc = document.documentElement;
        doc.classList.add(this.DARK_CLASS);
        doc.classList.remove(this.LIGHT_CLASS);
        document.body.classList.add(this.DARK_CLASS);
        document.body.classList.remove(this.LIGHT_CLASS);
        doc.setAttribute('data-theme', 'dark');
        localStorage.setItem(this.STORAGE_KEY, 'dark');
    }

    enableLightMode() {
        const doc = document.documentElement;
        doc.classList.add(this.LIGHT_CLASS);
        doc.classList.remove(this.DARK_CLASS);
        document.body.classList.add(this.LIGHT_CLASS);
        document.body.classList.remove(this.DARK_CLASS);
        doc.setAttribute('data-theme', 'light');
        localStorage.setItem(this.STORAGE_KEY, 'light');
    }

    /**
     * Toggle theme
     */
    toggle() {
        const isDark = document.documentElement.classList.contains(this.DARK_CLASS);
        if (isDark) {
            this.enableLightMode();
        } else {
            this.enableDarkMode();
        }
        this.updateToggleUI();
        this.dispatchThemeChangeEvent();
    }

    /**
     * Attach click listeners
     */
    attachToggleListener() {
        const singleBtn = document.getElementById('theme-toggle-btn');
        if (singleBtn) {
            singleBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.toggle();
            });
        }

        // Support multiple buttons via data attribute
        document.querySelectorAll('[data-toggle-theme]').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                this.toggle();
            });
        });
    }

    /**
     * Watch system preference changes when user hasn't explicitly chosen
     */
    watchSystemPreference() {
        if (!window.matchMedia) return;
        const mq = window.matchMedia('(prefers-color-scheme: dark)');
        mq.addEventListener('change', (e) => {
            const savedTheme = localStorage.getItem(this.STORAGE_KEY);
            if (!savedTheme || savedTheme === this.SYSTEM_PREF) {
                if (e.matches) {
                    this.enableDarkMode();
                } else {
                    this.enableLightMode();
                }
                this.updateToggleUI();
            }
        });
    }

    /**
     * Accessible UI update for toggle button
     * No innerHTML rewrite (icons are static in markup)
     */
    updateToggleUI() {
        const isDark = document.documentElement.classList.contains(this.DARK_CLASS);
        const btn = document.getElementById('theme-toggle-btn');
        if (!btn) return;

        // Animation class (optional pulse)
        btn.classList.add('theme-toggle--animating');
        setTimeout(() => btn.classList.remove('theme-toggle--animating'), 450);

        btn.setAttribute('aria-pressed', isDark ? 'true' : 'false');
        btn.setAttribute('aria-label', isDark ? 'Switch to light mode' : 'Switch to dark mode');
        btn.title = isDark ? 'Switch to light mode' : 'Switch to dark mode';
    }

    /**
     * Fire a custom event when theme changes
     */
    dispatchThemeChangeEvent() {
        const event = new CustomEvent('themechange', {
            detail: {
                isDark: document.documentElement.classList.contains(this.DARK_CLASS),
                theme: this.getCurrentTheme()
            }
        });
        window.dispatchEvent(event);
    }

    getCurrentTheme() {
        return document.documentElement.classList.contains(this.DARK_CLASS) ? 'dark' : 'light';
    }
}

// Initialize once DOM ready (layout adds early inline script to avoid FOUC)
document.addEventListener('DOMContentLoaded', () => {
    window.darkModeTheme = new DarkModeTheme();
});