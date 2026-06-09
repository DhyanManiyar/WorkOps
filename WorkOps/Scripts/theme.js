/**
 * WorkOps theme — light / dark mode (localStorage)
 */
(function () {
    'use strict';

    var STORAGE_KEY = 'wo-theme';
    var DARK = 'dark';
    var LIGHT = 'light';

    function getPreferred() {
        try {
            var stored = localStorage.getItem(STORAGE_KEY);
            if (stored === DARK || stored === LIGHT) return stored;
        } catch (e) { /* ignore */ }
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return DARK;
        }
        return LIGHT;
    }

    function applyTheme(theme) {
        var root = document.documentElement;
        if (theme === DARK) {
            root.setAttribute('data-theme', DARK);
        } else {
            root.removeAttribute('data-theme');
        }
        document.querySelectorAll('.wo-theme-toggle').forEach(function (btn) {
            btn.setAttribute('aria-label', theme === DARK ? 'Switch to light mode' : 'Switch to dark mode');
            btn.setAttribute('title', theme === DARK ? 'Light mode' : 'Dark mode');
        });
    }

    function saveTheme(theme) {
        try {
            localStorage.setItem(STORAGE_KEY, theme);
        } catch (e) { /* ignore */ }
    }

    function toggleTheme() {
        var next = document.documentElement.getAttribute('data-theme') === DARK ? LIGHT : DARK;
        applyTheme(next);
        saveTheme(next);
    }

    function initToggleButtons() {
        document.querySelectorAll('.wo-theme-toggle').forEach(function (btn) {
            if (btn.getAttribute('data-wo-theme-bound')) return;
            btn.setAttribute('data-wo-theme-bound', '1');
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                toggleTheme();
            });
        });
    }

    applyTheme(getPreferred());

    document.addEventListener('DOMContentLoaded', function () {
        initToggleButtons();
        if (typeof lucide !== 'undefined' && lucide.createIcons) {
            lucide.createIcons();
        }
    });

    window.WorkOpsTheme = {
        toggle: toggleTheme,
        set: function (theme) {
            applyTheme(theme === DARK ? DARK : LIGHT);
            saveTheme(theme === DARK ? DARK : LIGHT);
        },
        get: function () {
            return document.documentElement.getAttribute('data-theme') === DARK ? DARK : LIGHT;
        }
    };
})();
