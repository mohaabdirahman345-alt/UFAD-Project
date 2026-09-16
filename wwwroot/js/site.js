// UFAD Garowe — interaction helpers & Dark Mode theme manager.

function updateThemeUI(theme) {
    document.querySelectorAll('.theme-toggle-icon.icon-sun').forEach(function (el) {
        if (theme === 'dark') { el.classList.add('d-none'); } else { el.classList.remove('d-none'); }
    });
    document.querySelectorAll('.theme-toggle-icon.icon-moon').forEach(function (el) {
        if (theme === 'dark') { el.classList.remove('d-none'); } else { el.classList.add('d-none'); }
    });
    document.querySelectorAll('.themeLabelText').forEach(function (el) {
        el.textContent = theme === 'dark' ? 'Dark' : 'Light';
    });
}

function toggleTheme() {
    var currentTheme = document.documentElement.getAttribute('data-theme') === 'dark' ? 'dark' : 'light';
    var newTheme = currentTheme === 'dark' ? 'light' : 'dark';
    document.documentElement.setAttribute('data-theme', newTheme);
    localStorage.setItem('theme', newTheme);
    updateThemeUI(newTheme);
}

document.addEventListener('DOMContentLoaded', function () {
    var currentTheme = document.documentElement.getAttribute('data-theme') || 'light';
    updateThemeUI(currentTheme);

    // Confirm before any destructive action (delete portfolio item, etc.)
    document.querySelectorAll('[data-confirm]').forEach(function (element) {
        element.addEventListener('submit', function (event) {
            var message = element.getAttribute('data-confirm');
            if (!window.confirm(message)) {
                event.preventDefault();
            }
        });
    });

    // Reveal elements with fade-in when visible
    var io = null;
    if ('IntersectionObserver' in window) {
        io = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-visible');
                    io.unobserve(entry.target);
                }
            });
        }, { rootMargin: '0px 0px -8px 0px', threshold: 0.08 });

        document.querySelectorAll('.fade-in').forEach(function (el) { io.observe(el); });
    } else {
        // Fallback: make visible immediately
        document.querySelectorAll('.fade-in').forEach(function (el) { el.classList.add('is-visible'); });
    }

    // FAQ accordion — click to expand/collapse answers
    document.querySelectorAll('.faq-question').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var item = btn.closest('.faq-item');
            var isOpen = item.classList.contains('open');
            // Close all others
            document.querySelectorAll('.faq-item.open').forEach(function (openItem) {
                openItem.classList.remove('open');
                openItem.querySelector('.faq-question').setAttribute('aria-expanded', 'false');
            });
            // Toggle clicked
            if (!isOpen) {
                item.classList.add('open');
                btn.setAttribute('aria-expanded', 'true');
            }
        });
    });

    // Mobile nav hamburger toggle
    var navToggle = document.getElementById('navMobileToggle');
    var navMenu = document.getElementById('siteNavMenu');
    if (navToggle && navMenu) {
        navToggle.addEventListener('click', function () {
            var expanded = navToggle.getAttribute('aria-expanded') === 'true';
            navToggle.setAttribute('aria-expanded', String(!expanded));
            navMenu.classList.toggle('open', !expanded);
        });
        // Close nav when a link is clicked
        navMenu.querySelectorAll('a').forEach(function (link) {
            link.addEventListener('click', function () {
                navMenu.classList.remove('open');
                navToggle.setAttribute('aria-expanded', 'false');
            });
        });
    }

    // Active nav link detection
    var currentPath = window.location.pathname;
    document.querySelectorAll('.site-nav-item').forEach(function (link) {
        var href = link.getAttribute('href');
        if (href && !href.startsWith('#') && (href === currentPath || (href !== '/' && currentPath.startsWith(href)))) {
            link.classList.add('active');
        }
    });
});

