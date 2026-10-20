window.todoEffects = {
  pulse(el) {
    if (!el) return;
    el.classList.remove('clicked');
    void el.offsetWidth;
    el.classList.add('clicked');
    setTimeout(() => el.classList.remove('clicked'), 450);
  },
  setTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);
  },
};
