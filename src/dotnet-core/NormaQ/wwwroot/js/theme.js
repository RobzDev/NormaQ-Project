let isDark = false;

(function () {
    const saved = localStorage.getItem('theme') ?? 'dark';
    isDark = saved === 'dark';
    document.documentElement.classList.toggle('dark', isDark);
    document.addEventListener('DOMContentLoaded', () => {
        document.body.classList.toggle('dark-mode', isDark);
        document.body.classList.toggle('light-mode', !isDark);
    });
})();

function applyTheme(dark) {
    isDark = dark;
    document.documentElement.classList.toggle('dark', dark);
    if (document.body) {
        document.body.classList.toggle('dark-mode', dark);
        document.body.classList.toggle('light-mode', !dark);
    }
    localStorage.setItem('theme', dark ? 'dark' : 'light');

    const icon = document.getElementById('themeIcon');
    const label = document.getElementById('themeLabel');
    if (icon) icon.textContent = dark ? '☀️' : '🌙';
    if (label) label.textContent = dark ? 'Modo claro' : 'Modo oscuro';
}

function toggleTheme(event) {
    if (!document.startViewTransition) { applyTheme(!isDark); return; }
    const x = event?.clientX ?? innerWidth / 2;
    const y = event?.clientY ?? innerHeight / 2;
    const r = Math.hypot(Math.max(x, innerWidth - x), Math.max(y, innerHeight - y));
    const t = document.startViewTransition(() => applyTheme(!isDark));
    t.ready.then(() => {
        document.documentElement.animate(
            { clipPath: [`circle(0px at ${x}px ${y}px)`, `circle(${r}px at ${x}px ${y}px)`] },
            { duration: 800, easing: 'cubic-bezier(0.4,0,0.2,1)', pseudoElement: '::view-transition-new(root)' }
        );
    });
}