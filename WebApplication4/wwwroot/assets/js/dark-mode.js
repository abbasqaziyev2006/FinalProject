/**
 * Enhanced Dark Mode Theme Manager
 * Handles dark mode toggle, persistence, and proper initialization
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
        // Apply theme immediately to prevent flash
        this.applyThemeImmediately();

        // Wait for DOM to be ready for interactive features
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => {
                this.initializeInteractive();
            });
        } else {
            this.initializeInteractive();
        }
    }

    /**
     * Apply theme immediately (called even before DOM ready)
     */
    applyThemeImmediately() {
        const savedTheme = localStorage.getItem(this.STORAGE_KEY);

        if (savedTheme === 'dark') {
            this.enableDarkMode(true);
        } else if (savedTheme === 'light') {
            this.enableLightMode(true);
        } else {
            this.applySystemPreference(true);
        }
    }

    /**
     * Initialize interactive features after DOM ready
     */
    initializeInteractive() {
        this.updateToggleUI();
        this.attachToggleListener();
        this.watchSystemPreference();
        this.forceStyleApplication();
    }

    /**
     * Force style application to stubborn elements
     */
    forceStyleApplication() {
        const isDark = document.documentElement.classList.contains(this.DARK_CLASS);

        if (isDark) {
            // Force background colors on common containers
            const containerSelectors = [
                'body',
                '.container',
                '.product-card',
                '.pc__img-wrapper',
                '.bg-white',
                '.bg-light'
            ];

            containerSelectors.forEach(selector => {
                const elements = document.querySelectorAll(selector);
                elements.forEach(el => {
                    el.setAttribute('data-dark-mode-applied', 'true');
                });
            });

            // Force text colors on product cards and filters
            const textSelectors = [
                '.product-card .pc__title',
                '.product-card h6',
                '.product-card h5',
                '.product-card h4',
                '.product-card a',
                '.product-card .product-card__title',
                '.product-card .pc__price',
                '.product-card .product-card__price',
                '.aside a',
                '.aside li',
                '.aside label',
                '.aside h3',
                '.aside h4',
                '.aside h5',
                '.widget a',
                '.widget li',
                '.widget label',
                '.widget h3',
                '.widget h4',
                '.widget h5',
                '.widget-title',
                '.list-unstyled li',
                '.list-unstyled a'
            ];

            textSelectors.forEach(selector => {
                const elements = document.querySelectorAll(selector);
                elements.forEach(el => {
                    if (selector.includes('.aside') || selector.includes('.widget') || selector.includes('.list-unstyled')) {
                        el.style.setProperty('color', '#e8e8e8', 'important');
                    } else {
                        el.style.setProperty('color', '#f0f0f0', 'important');
                    }
                });
            });
        } else {
            // Remove forced styles in light mode
            const textSelectors = [
                '.product-card .pc__title',
                '.product-card h6',
                '.product-card h5',
                '.product-card h4',
                '.product-card a',
                '.product-card .product-card__title',
                '.product-card .pc__price',
                '.product-card .product-card__price',
                '.aside a',
                '.aside li',
                '.aside label',
                '.aside h3',
                '.aside h4',
                '.aside h5',
                '.widget a',
                '.widget li',
                '.widget label',
                '.widget h3',
                '.widget h4',
                '.widget h5',
                '.widget-title',
                '.list-unstyled li',
                '.list-unstyled a'
            ];

            textSelectors.forEach(selector => {
                const elements = document.querySelectorAll(selector);
                elements.forEach(el => {
                    el.style.removeProperty('color');
                });
            });
        }
    }

    /**
     * Apply current system preference if no stored override
     */
    applySystemPreference(skipUI = false) {
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            this.enableDarkMode(skipUI);
        } else {
            this.enableLightMode(skipUI);
        }
    }

    enableDarkMode(skipUI = false) {
        const doc = document.documentElement;
        const body = document.body;

        doc.classList.add(this.DARK_CLASS);
        doc.classList.remove(this.LIGHT_CLASS);
        body.classList.add(this.DARK_CLASS);
        body.classList.remove(this.LIGHT_CLASS);
        doc.setAttribute('data-theme', 'dark');

        localStorage.setItem(this.STORAGE_KEY, 'dark');

        if (!skipUI) {
            this.updateToggleUI();
            this.forceStyleApplication();
        }
    }

    enableLightMode(skipUI = false) {
        const doc = document.documentElement;
        const body = document.body;

        doc.classList.add(this.LIGHT_CLASS);
        doc.classList.remove(this.DARK_CLASS);
        body.classList.add(this.LIGHT_CLASS);
        body.classList.remove(this.DARK_CLASS);
        doc.setAttribute('data-theme', 'light');

        localStorage.setItem(this.STORAGE_KEY, 'light');

        if (!skipUI) {
            this.updateToggleUI();
            // Ensure any inline 'dark-mode' forced styles are removed immediately
            this.forceStyleApplication();
        }
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

        this.dispatchThemeChangeEvent();
    }

    /**
     * Attach click listeners to toggle buttons
     */
    attachToggleListener() {
        // Single button by ID
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

        // Support any element with class
        document.querySelectorAll('.theme-toggle-btn').forEach(btn => {
            if (btn.id !== 'theme-toggle-btn') { // Avoid duplicate listener
                btn.addEventListener('click', (e) => {
                    e.preventDefault();
                    this.toggle();
                });
            }
        });
    }

    /**
     * Watch system preference changes
     */
    watchSystemPreference() {
        if (!window.matchMedia) return;

        const mq = window.matchMedia('(prefers-color-scheme: dark)');

        const handleChange = (e) => {
            const savedTheme = localStorage.getItem(this.STORAGE_KEY);

            // Only apply system preference if user hasn't set explicit preference
            if (!savedTheme || savedTheme === this.SYSTEM_PREF) {
                if (e.matches) {
                    this.enableDarkMode();
                } else {
                    this.enableLightMode();
                }
            }
        };

        // Modern browsers
        if (mq.addEventListener) {
            mq.addEventListener('change', handleChange);
        }
        // Fallback for older browsers
        else if (mq.addListener) {
            mq.addListener(handleChange);
        }
    }

    /**
     * Update toggle button UI with animation
     */
    updateToggleUI() {
        const isDark = document.documentElement.classList.contains(this.DARK_CLASS);
        const buttons = [
            document.getElementById('theme-toggle-btn'),
            ...document.querySelectorAll('[data-toggle-theme]'),
            ...document.querySelectorAll('.theme-toggle-btn')
        ].filter(Boolean);

        buttons.forEach(btn => {
            // Add animation class
            btn.classList.add('theme-toggle--animating');
            setTimeout(() => btn.classList.remove('theme-toggle--animating'), 450);

            // Update ARIA attributes
            btn.setAttribute('aria-pressed', isDark ? 'true' : 'false');
            btn.setAttribute('aria-label', isDark ? 'Switch to light mode' : 'Switch to dark mode');
            btn.title = isDark ? 'Switch to light mode' : 'Switch to dark mode';
        });
    }

    /**
     * Fire custom event when theme changes
     */
    dispatchThemeChangeEvent() {
        const isDark = document.documentElement.classList.contains(this.DARK_CLASS);

        const event = new CustomEvent('themechange', {
            detail: {
                isDark: isDark,
                theme: this.getCurrentTheme()
            },
            bubbles: true
        });

        window.dispatchEvent(event);
        document.dispatchEvent(event);
    }

    /**
     * Get current theme
     */
    getCurrentTheme() {
        return document.documentElement.classList.contains(this.DARK_CLASS) ? 'dark' : 'light';
    }

    /**
     * Public API method to manually set theme
     */
    setTheme(theme) {
        if (theme === 'dark') {
            this.enableDarkMode();
        } else if (theme === 'light') {
            this.enableLightMode();
        } else if (theme === 'system') {
            localStorage.setItem(this.STORAGE_KEY, this.SYSTEM_PREF);
            this.applySystemPreference();
        }
    }
}

// Initialize immediately to prevent FOUC
(function () {
    // Create instance as early as possible
    window.darkModeTheme = new DarkModeTheme();
})();

// Also expose for manual initialization if needed
window.DarkModeTheme = DarkModeTheme;