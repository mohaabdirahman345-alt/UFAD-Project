// UFAD Garowe — interaction helpers & Dark Mode theme manager.

var UFAD_SOMALI_DICT = {
    "Home": "Hoyga",
    "Find Experts": "Raadi Khubaro",
    "Why UFAD": "Maxaa UFAD",
    "FAQ": "Su'aalo & Jawaabo",
    "My Dashboard": "Gudigeyga",
    "Admin Panel": "Guddiga Maamulka",
    "Service Provider Dashboard": "Guddiga Adeeg-bixiyaha",
    "Log In": "Soo Gal",
    "Sign In": "Soo Gal",
    "Sign Up": "Is Diiwaangeli",
    "Join the Directory": "Ku Biir Diiwaanka",
    "Log Out": "Ka Bax",
    "Public Site": "Bogga Guud",
    "About": "Ku Saabsan",
    "Portfolio": "Shaqooyinkii Hore",
    "Reviews": "Faallooyinka",
    "Write Review": "Qor Faallo",
    "Leave a Review": "Reeb Faallo",
    "Your Rating": "Qiimeyntaada",
    "Your Review": "Faalladaada",
    "Post Review": "Dir Faallada",
    "Contact Information": "Macluumaadka Xiriirka",
    "Phone": "Telefoon",
    "Phone & WhatsApp": "Telefoon & WhatsApp",
    "Email": "Iimayl",
    "Location": "Goobta",
    "Chat on WhatsApp": "Kula hadal WhatsApp",
    "Message": "Farriin",
    "Save": "Keydi",
    "Saved": "La Keydiyay",
    "Verified": "Xaqiijisan",
    "Available": "Diyaar ah",
    "Available Now": "Hadda Diyaar ah",
    "years of experience": "sano oo waayo-aragnimo ah",
    "Profile Views": "Daawashada Profile-ka",
    "Profile Complete": "Dhameystirka Profile-ka",
    "Report Profile": "Soo Sheeg Profile-kan",
    "Report this Profile": "Soo Sheeg Profile-kan",
    "Find the right expert, right here.": "Ka hel khabiirka saxda ah halkan.",
    "All categories": "Dhammaan qaybaha",
    "Search": "Raadi",
    "Apply Filters": "Codso Shaandhada",
    "Filter": "Shaandhee",
    "Keyword": "Eray Furaha",
    "Service category": "Qaybta adeegga",
    "Address": "Cinwaanka",
    "Minimum rating": "Qiimeynta ugu hooseysa",
    "Availability": "Helitaanka",
    "Any address": "Cinwaan kasta",
    "Any rating": "Qiimeyn kasta",
    "Any availability": "Xilli kasta",
    "Overview": "Guudmar",
    "Directory": "Diiwaanka",
    "Oversight": "Kormeerka",
    "Account": "Koontada",
    "Settings": "Hagaajinta",
    "Notifications": "Ogeysiisyada",
    "Profile Approvals": "Oggolaanshaha Profile-ka",
    "Service Requests": "Codsiyada Adeegga",
    "Categories": "Qaybaha",
    "Skills": "Xirfadaha",
    "Manage Users": "Maamul Isticmaalayaasha",
    "Reports": "Warbixinnada",
    "Saved Providers": "Adeeg-bixiyayaasha La Keydiyay",
    "My Requests": "Codsiyadeyda",
    "Completed Jobs": "Shaqooyinka Dhammaaday",
    "Manage Portfolio": "Maamul Shaqooyinkaaga",
    "Professional CV / Resume": "CV / Resume-ga Xirfadeed",
    "My Profile": "Profile-kayga",
    "Total Service Providers": "Wadarta Adeeg-bixiyayaasha",
    "Total Clients": "Wadarta Macaamiisha",
    "Pending Approvals": "Sugaya Oggolaansho",
    "Pending Requests": "Codsiyada Sugaya",
    "Approved": "La Oggolaaday",
    "Rejected": "La Diiday",
    "Pending": "Sugaya",
    "Accepted": "La Aqbalay",
    "Declined": "La Diiday",
    "Completed": "Dhammaaday",
    "Cancelled": "La Kansalay",
    "Active": "Shaqeynaya",
    "Deactivated": "La Joojiyay",
    "Deactivate": "Jooji",
    "Activate": "Shaqsii",
    "Reset Password": "Dib u Deji Furaha",
    "Appearance": "Muuqaalka",
    "Translation": "Turjumaadda",
    "Profile Picture": "Sawirka Profile-ka",
    "Change password": "Beddel Furaha Sirta ah",
    "Current password": "Furaha hadda",
    "New password": "Furaha cusub",
    "Confirm new password": "Xaqiiji furaha cusub",
    "Update Password": "Cusbooneysii Furaha",
    "Upload Picture": "Soo Geli Sawir",
    "Choose photo": "Dooro sawir",
    "Upload CV": "Soo Geli CV",
    "Choose CV / Resume": "Dooro CV / Resume",
    "No CV uploaded yet.": "Weli CV lama soo gelin.",
    "No portfolio items yet": "Weli shaqooyin hore lama gelin",
    "Excellent": "Aad u Fiican",
    "Good": "Fiican",
    "Average": "Dhexdhexaad",
    "Below Average": "Ka Hooseeya Dhexdhexaad",
    "Poor": "Liita"
};

function walkTextNodes(node, callback) {
    if (node.nodeType === Node.TEXT_NODE) {
        callback(node);
    } else if (node.nodeType === Node.ELEMENT_NODE) {
        var tag = node.tagName.toLowerCase();
        if (tag !== 'script' && tag !== 'style' && tag !== 'textarea') {
            for (var child = node.firstChild; child; child = child.nextSibling) {
                walkTextNodes(child, callback);
            }
        }
    }
}

function applyLanguage(lang) {
    document.querySelectorAll('.language-selector').forEach(function (sel) {
        sel.value = lang;
    });

    if (lang === 'so') {
        walkTextNodes(document.body, function (node) {
            if (node._enOriginal === undefined) {
                node._enOriginal = node.nodeValue;
            }
            var text = node._enOriginal.trim();
            if (UFAD_SOMALI_DICT[text]) {
                node.nodeValue = node._enOriginal.replace(text, UFAD_SOMALI_DICT[text]);
            }
        });

        // Translate inputs / buttons
        document.querySelectorAll('input[placeholder]').forEach(function (input) {
            if (input._enPlaceholder === undefined) {
                input._enPlaceholder = input.getAttribute('placeholder');
            }
            var ph = input._enPlaceholder.trim();
            if (UFAD_SOMALI_DICT[ph]) {
                input.setAttribute('placeholder', UFAD_SOMALI_DICT[ph]);
            }
        });
    } else {
        // Restore English
        walkTextNodes(document.body, function (node) {
            if (node._enOriginal !== undefined) {
                node.nodeValue = node._enOriginal;
            }
        });

        document.querySelectorAll('input[placeholder]').forEach(function (input) {
            if (input._enPlaceholder !== undefined) {
                input.setAttribute('placeholder', input._enPlaceholder);
            }
        });
    }
}

function changeLanguage(lang) {
    localStorage.setItem('ufad_lang', lang);
    applyLanguage(lang);
}

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

    var currentLang = localStorage.getItem('ufad_lang') || 'en';
    applyLanguage(currentLang);

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

