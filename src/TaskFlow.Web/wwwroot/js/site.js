/* ==========================================================================
   TaskFlow - site.js
   Global, cross-page behavior: theme toggle, mobile sidebar toggle, and
   the notification bell's periodic AJAX refresh. Page-specific logic
   (Kanban drag/drop, calendar, charts) lives in its own file per view.
   ========================================================================== */

(function () {
    'use strict';

    // ---- CSRF token helper for AJAX POST requests ----
    // Program.cs configures AddAntiforgery with HeaderName = "X-CSRF-TOKEN",
    // and _AntiForgeryTokenMeta.cshtml renders the token as a <meta> tag;
    // every fetch() POST in this file (and any page-specific script) should
    // include this header so [ValidateAntiForgeryToken] actions succeed.
    window.getCsrfToken = function () {
        var meta = document.querySelector('meta[name="csrf-token"]');
        return meta ? meta.getAttribute('content') : '';
    };

    window.postJson = function (url, body) {
        return fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest',
                'X-CSRF-TOKEN': window.getCsrfToken()
            },
            body: body ? JSON.stringify(body) : null
        });
    };

    // ---- Dark / Light mode ----
    const THEME_KEY = 'taskflow-theme';
    const htmlEl = document.documentElement;
    const themeToggleBtn = document.getElementById('themeToggle');

    function applyTheme(theme) {
        htmlEl.setAttribute('data-bs-theme', theme);
        if (themeToggleBtn) {
            const icon = themeToggleBtn.querySelector('i');
            if (icon) {
                icon.className = theme === 'dark' ? 'fa-solid fa-sun' : 'fa-solid fa-moon';
            }
        }
    }

    const savedTheme = localStorage.getItem(THEME_KEY) ||
        (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
    applyTheme(savedTheme);

    if (themeToggleBtn) {
        themeToggleBtn.addEventListener('click', function () {
            const current = htmlEl.getAttribute('data-bs-theme');
            const next = current === 'dark' ? 'light' : 'dark';
            applyTheme(next);
            localStorage.setItem(THEME_KEY, next);
        });
    }

    // ---- Mobile sidebar toggle ----
    const sidebarToggleBtn = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');
    if (sidebarToggleBtn && sidebar) {
        sidebarToggleBtn.addEventListener('click', function () {
            sidebar.classList.toggle('show');
        });
    }

    // ---- Notification bell polling ----
    const notificationBell = document.getElementById('notificationBell');
    const notificationCount = document.getElementById('notificationCount');
    const notificationList = document.getElementById('notificationList');

    function timeAgo(dateString) {
        const seconds = Math.floor((new Date() - new Date(dateString)) / 1000);
        if (seconds < 60) return 'just now';
        const minutes = Math.floor(seconds / 60);
        if (minutes < 60) return minutes + 'm ago';
        const hours = Math.floor(minutes / 60);
        if (hours < 24) return hours + 'h ago';
        return Math.floor(hours / 24) + 'd ago';
    }

    function renderNotifications(items) {
        if (!notificationList) return;

        if (!items || items.length === 0) {
            notificationList.innerHTML = '<div class="text-center text-muted p-3">No notifications yet.</div>';
            return;
        }

        notificationList.innerHTML = items.map(function (n) {
            return '<div class="notification-item ' + (n.isRead ? '' : 'unread') + '" data-id="' + n.id + '">' +
                '<div class="fw-semibold">' + n.title + '</div>' +
                '<div class="text-muted">' + n.message + '</div>' +
                '<div class="text-muted small mt-1">' + timeAgo(n.createdAt) + '</div>' +
                '</div>';
        }).join('');
    }

    function refreshNotifications() {
        if (!notificationBell) return;

        fetch('/Notifications/GetRecent', { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(function (res) { return res.ok ? res.json() : null; })
            .then(function (data) {
                if (!data) return;

                if (notificationCount) {
                    if (data.unreadCount > 0) {
                        notificationCount.textContent = data.unreadCount > 9 ? '9+' : data.unreadCount;
                        notificationCount.classList.remove('d-none');
                    } else {
                        notificationCount.classList.add('d-none');
                    }
                }

                renderNotifications(data.items);
            })
            .catch(function () { /* silent - notification polling is best-effort */ });
    }

    if (notificationBell) {
        refreshNotifications();
        setInterval(refreshNotifications, 60000); // poll every 60s

        notificationBell.addEventListener('click', function () {
            // Mark-all-as-read shortly after opening, so the badge clears
            // once the user has actually seen the list.
            setTimeout(function () {
                window.postJson('/Notifications/MarkAllRead').then(refreshNotifications).catch(function () {});
            }, 1500);
        });
    }

    // ---- Highlight active sidebar link based on current path ----
    document.querySelectorAll('.sidebar-nav a').forEach(function (link) {
        if (link.getAttribute('href') && window.location.pathname.startsWith(link.getAttribute('href')) && link.getAttribute('href') !== '/') {
            link.classList.add('active');
        }
    });
})();
