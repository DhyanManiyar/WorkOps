/**
 * WorkOps UI — modern.js
 * Sidebar, toasts, loading overlay (preserves existing IDs)
 */
(function () {
    'use strict';

    function initSidebar() {
        var sidebar = document.getElementById('woSidebar') || document.querySelector('.wo-sidebar, .sidebar');
        var backdrop = document.getElementById('woSidebarBackdrop');
        var toggleBtn = document.getElementById('woSidebarToggle');

        if (!sidebar) return;

        function openSidebar() {
            sidebar.classList.add('is-open');
            if (backdrop) backdrop.classList.add('is-visible');
            document.body.style.overflow = 'hidden';
        }

        function closeSidebar() {
            sidebar.classList.remove('is-open');
            if (backdrop) backdrop.classList.remove('is-visible');
            document.body.style.overflow = '';
        }

        function toggleSidebar() {
            if (sidebar.classList.contains('is-open')) closeSidebar();
            else openSidebar();
        }

        if (toggleBtn) {
            toggleBtn.addEventListener('click', toggleSidebar);
        }

        if (backdrop) {
            backdrop.addEventListener('click', closeSidebar);
        }

        window.addEventListener('resize', function () {
            if (window.innerWidth > 992) closeSidebar();
        });

        sidebar.querySelectorAll('.wo-nav-item, .nav-link-custom').forEach(function (link) {
            link.addEventListener('click', function () {
                if (window.innerWidth <= 992) closeSidebar();
            });
        });
    }

    function showToast(message, type) {
        var container = document.getElementById('woToastContainer');
        if (!container || !message) return;

        var toast = document.createElement('div');
        toast.className = 'wo-toast wo-toast--' + (type || 'info');
        toast.setAttribute('role', 'alert');
        toast.textContent = message;
        container.appendChild(toast);

        setTimeout(function () {
            toast.style.opacity = '0';
            toast.style.transform = 'translateX(16px)';
            toast.style.transition = 'opacity 0.3s, transform 0.3s';
            setTimeout(function () {
                if (toast.parentNode) toast.parentNode.removeChild(toast);
            }, 300);
        }, 5000);
    }

    function initToastsFromPage() {
        var success = document.getElementById('woTempSuccess');
        var error = document.getElementById('woTempError');
        if (success && success.value) showToast(success.value, 'success');
        if (error && error.value) showToast(error.value, 'error');
    }

    function parseServerDate(value) {
        if (!value) return null;
        if (typeof value === 'string') {
            var msMatch = /\/Date\((\d+)\)\//.exec(value);
            if (msMatch) {
                var ms = parseInt(msMatch[1], 10);
                if (!isNaN(ms)) return new Date(ms);
            }
            var dt = new Date(value);
            return isNaN(dt.getTime()) ? null : dt;
        }
        var fallback = new Date(value);
        return isNaN(fallback.getTime()) ? null : fallback;
    }

    function initLoadingOverlay() {
        var overlay = document.getElementById('loadingOverlay');
        if (!overlay) return;
        var progress = document.getElementById('woLoadingProgress');
        var hint = document.getElementById('woLoadingHint');
        var timer = null;
        var steps = [18, 34, 52, 68, 82, 93];
        var hints = [
            'Preparing your workspace',
            'Loading modules',
            'Retrieving data',
            'Finalizing UI'
        ];
        var hintIdx = 0;

        window.showLoading = function () {
            overlay.style.display = 'flex';
            overlay.classList.add('is-visible');
            if (progress) progress.style.width = '16%';
            if (hint) hint.textContent = hints[0];
            hintIdx = 1;
            if (timer) clearInterval(timer);
            timer = setInterval(function () {
                if (progress) {
                    var current = parseInt((progress.style.width || '16').replace('%', ''), 10);
                    var next = steps.find(function (v) { return v > current; }) || 94;
                    progress.style.width = next + '%';
                }
                if (hint && hintIdx < hints.length) {
                    hint.textContent = hints[hintIdx++];
                }
            }, 500);
        };

        window.hideLoading = function () {
            if (timer) {
                clearInterval(timer);
                timer = null;
            }
            if (progress) progress.style.width = '100%';
            setTimeout(function () {
                if (progress) progress.style.width = '16%';
                if (hint) hint.textContent = hints[0];
            }, 180);
            overlay.style.display = 'none';
            overlay.classList.remove('is-visible');
        };

        document.addEventListener('submit', function (e) {
            var form = e.target;
            if (form && form.tagName === 'FORM' && !form.hasAttribute('data-no-loading')) {
                window.showLoading();
            }
        });

        if (window.jQuery) {
            var ajaxCount = 0;
            jQuery(document).ajaxStart(function () {
                if (++ajaxCount === 1) window.showLoading();
            });
            jQuery(document).ajaxStop(function () {
                if (--ajaxCount <= 0) {
                    ajaxCount = 0;
                    window.hideLoading();
                }
            });
        }
    }

    function initConfirmModal() {
        var modalEl = document.getElementById('woConfirmModal');
        if (!modalEl || typeof bootstrap === 'undefined') return;

        var modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        var titleEl = document.getElementById('woConfirmTitle');
        var messageEl = document.getElementById('woConfirmMessage');
        var okBtn = document.getElementById('woConfirmOk');
        var cancelBtn = document.getElementById('woConfirmCancel');
        var pending = null;

        function cleanup() {
            pending = null;
            okBtn.classList.remove('btn-danger', 'btn-success', 'btn-primary');
            okBtn.classList.add('btn-primary');
        }

        cancelBtn.addEventListener('click', function () {
            if (pending && pending.reject) pending.reject(false);
            cleanup();
        });

        modalEl.addEventListener('hidden.bs.modal', function () {
            if (pending && !pending.confirmed && pending.reject) pending.reject(false);
            cleanup();
        });

        okBtn.addEventListener('click', function () {
            if (!pending) return;
            var resolve = pending.resolve;
            var action = pending.action;
            pending.confirmed = true;
            if (action) action();
            if (resolve) resolve(true);
            cleanup();
            modal.hide();
        });

        window.woConfirm = function (options) {
            options = options || {};
            return new Promise(function (resolve, reject) {
                titleEl.textContent = options.title || 'Confirm action';
                messageEl.textContent = options.message || 'Are you sure you want to continue?';
                okBtn.textContent = options.confirmText || 'Confirm';
                cancelBtn.textContent = options.cancelText || 'Cancel';

                okBtn.classList.remove('btn-primary', 'btn-danger', 'btn-success');
                var variant = options.variant || 'primary';
                okBtn.classList.add(variant === 'danger' ? 'btn-danger' : variant === 'success' ? 'btn-success' : 'btn-primary');

                pending = { resolve: resolve, reject: reject, action: options.onConfirm || null };
                modal.show();
                if (typeof lucide !== 'undefined') lucide.createIcons();
            });
        };

        function isSubmitTrigger(el) {
            if (!el) return false;
            var tag = el.tagName;
            if (tag === 'BUTTON' && (el.type === 'submit' || el.getAttribute('type') === 'submit')) return true;
            if (tag === 'INPUT' && el.type === 'submit') return true;
            return false;
        }

        function submitConfirmedForm(form) {
            if (!form) return;
            if (typeof form.checkValidity === 'function' && !form.checkValidity()) {
                form.reportValidity();
                return;
            }
            // Native submit bypasses submit-event listeners (avoids confirm loop).
            setTimeout(function () {
                HTMLFormElement.prototype.submit.call(form);
            }, 0);
        }

        document.addEventListener('click', function (e) {
            var trigger = e.target.closest('[data-wo-confirm]');
            if (!trigger) return;

            var message = trigger.getAttribute('data-wo-confirm');
            if (!message) return;

            // Submit buttons are confirmed on the form submit event (preserves submitter).
            if (isSubmitTrigger(trigger) && trigger.closest('form')) return;

            e.preventDefault();
            e.stopPropagation();

            var title = trigger.getAttribute('data-wo-confirm-title') || 'Confirm action';
            var variant = trigger.getAttribute('data-wo-confirm-variant') || 'primary';
            var confirmText = trigger.getAttribute('data-wo-confirm-ok') || 'Confirm';

            woConfirm({
                title: title,
                message: message,
                variant: variant,
                confirmText: confirmText,
                onConfirm: function () {
                    if (trigger.tagName === 'A' && trigger.href) {
                        window.location.href = trigger.href;
                    }
                }
            });
        }, true);

        document.addEventListener('submit', function (e) {
            var form = e.target;
            if (!form || form.tagName !== 'FORM') return;
            if (form.getAttribute('data-wo-skip-confirm') === '1') {
                form.removeAttribute('data-wo-skip-confirm');
                return;
            }
            var submitter = e.submitter;
            if (!submitter) return;
            var message = submitter.getAttribute('data-wo-confirm');
            if (!message) return;
            e.preventDefault();
            e.stopPropagation();

            woConfirm({
                title: submitter.getAttribute('data-wo-confirm-title') || 'Confirm action',
                message: message,
                variant: submitter.getAttribute('data-wo-confirm-variant') || 'primary',
                confirmText: submitter.getAttribute('data-wo-confirm-ok') || 'Confirm',
                onConfirm: function () {
                    submitConfirmedForm(form);
                }
            });
        }, true);

    }

    function initNotifications() {
        var badge = document.getElementById('woNotifyBadge');
        var list = document.getElementById('woNotifyList');
        var clearBtn = document.getElementById('woNotifyClear');
        if (!badge || !list) return;

        function render(items, count) {
            if (count > 0) {
                badge.style.display = 'inline-flex';
                badge.textContent = count > 99 ? '99+' : String(count);
            } else {
                badge.style.display = 'none';
            }

            if (!items || !items.length) {
                list.innerHTML = '<div class="wo-notify-empty">No new notifications</div>';
                return;
            }

            list.innerHTML = items.map(function (n) {
                var title = (n.title || '').replace(/[<>&]/g, '');
                var msg = (n.message || '').replace(/[<>&]/g, '');
                var time = (n.timeAgo || '').replace(/[<>&]/g, '');
                var linkStart = n.link ? '<a href="' + n.link + '" class="text-decoration-none">' : '';
                var linkEnd = n.link ? '</a>' : '';
                return linkStart + '<div class="wo-notify-item"><div class="wo-notify-item__title">' + title +
                    '</div><div class="wo-notify-item__message">' + msg +
                    '</div><div class="wo-notify-item__time">' + time + '</div></div>' + linkEnd;
            }).join('');
        }

        function fetchUnread() {
            fetch('/Notification/GetUnread', { credentials: 'same-origin' })
                .then(function (r) { return r.json(); })
                .then(function (res) {
                    if (res && res.success) render(res.items || [], res.count || 0);
                })
                .catch(function () { });
        }

        if (clearBtn) {
            clearBtn.addEventListener('click', function (e) {
                e.preventDefault();
                fetch('/Notification/MarkAllRead', {
                    method: 'POST',
                    credentials: 'same-origin',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                }).then(function () { fetchUnread(); });
            });
        }

        fetchUnread();
        setInterval(fetchUnread, 30000);
    }

    function initLucide() {
        if (typeof lucide !== 'undefined' && lucide.createIcons) {
            lucide.createIcons();
        }
    }

    function initBackForwardGuard() {
        if (!document.body.classList.contains('wo-app')) return;
        window.addEventListener('pageshow', function (e) {
            if (e.persisted) window.location.reload();
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        initSidebar();
        initToastsFromPage();
        initLoadingOverlay();
        initConfirmModal();
        initLucide();
        initBackForwardGuard();
        initNotifications();
    });

    window.WorkOpsUI = {
        showToast: showToast,
        showLoading: function () { if (window.showLoading) window.showLoading(); },
        hideLoading: function () { if (window.hideLoading) window.hideLoading(); },
        confirm: function (opts) { return window.woConfirm ? window.woConfirm(opts) : Promise.resolve(false); },
        parseServerDate: parseServerDate
    };
})();
