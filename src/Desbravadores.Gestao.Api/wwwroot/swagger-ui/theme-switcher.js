(function () {
  const darkThemeId = 'swagger-dark-theme';
  const darkThemeHref = 'https://cdn.jsdelivr.net/npm/swagger-ui-themes@3.0.1/themes/3.x/theme-monokai.css';

  function applyTheme(isDark) {
    const existingTheme = document.getElementById(darkThemeId);

    if (isDark) {
      if (!existingTheme) {
        const link = document.createElement('link');
        link.id = darkThemeId;
        link.rel = 'stylesheet';
        link.href = darkThemeHref;
        document.head.appendChild(link);
      }

      return;
    }

    if (existingTheme) {
      existingTheme.remove();
    }
  }

  const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
  applyTheme(mediaQuery.matches);

  if (typeof mediaQuery.addEventListener === 'function') {
    mediaQuery.addEventListener('change', (event) => applyTheme(event.matches));
  } else if (typeof mediaQuery.addListener === 'function') {
    mediaQuery.addListener((event) => applyTheme(event.matches));
  }
})();
