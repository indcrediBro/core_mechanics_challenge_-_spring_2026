using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.Core
{
    /// <summary>
    /// Central UI manager.  Place one instance in your bootstrap/persistent scene.
    ///
    /// Features
    /// ────────
    /// • Stack-based navigation  (ShowScreen / HideTopScreen / ClearAll / GoBack).
    /// • Register screens by string ID or direct reference.
    /// • Global theme propagation via UIThemeData ScriptableObject.
    /// • C# events for screen shown / hidden / theme changed.
    ///
    /// Quick start
    /// ───────────
    ///   UIManager.Instance.ShowScreen("MainMenu");
    ///   UIManager.Instance.HideTopScreen();
    ///   UIManager.Instance.ApplyTheme(myTheme);
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        // ── Inspector ─────────────────────────────────────────────────────────────
        [Header("Theme")]
        [Tooltip("Optional default theme applied to all screens on startup.")]
        [SerializeField] private UIThemeData defaultTheme;

        [Header("Screen Registry")]
        [Tooltip("Screens already present in the scene. They will be registered automatically.")]
        [SerializeField] private List<UIScreen> sceneScreens = new();

        // ── Runtime state ─────────────────────────────────────────────────────────
        private readonly Dictionary<string, UIScreen> _registry = new();
        private readonly Stack<UIScreen>               _stack    = new();
        private UIThemeData                            _activeTheme;

        // ── Events ────────────────────────────────────────────────────────────────
        public event Action<UIScreen>     OnScreenShown;
        public event Action<UIScreen>     OnScreenHidden;
        public event Action<UIThemeData>  OnThemeChanged;

        // ── Properties ────────────────────────────────────────────────────────────
        public UIScreen    TopScreen    => _stack.Count > 0 ? _stack.Peek() : null;
        public UIThemeData ActiveTheme  => _activeTheme;
        public int         StackDepth   => _stack.Count;

        // ── Unity Lifecycle ───────────────────────────────────────────────────────
        protected override void Awake()
        {
            base.Awake();   // Singleton setup + DontDestroyOnLoad

            // Register all scene screens and hide them to start
            foreach (var screen in sceneScreens)
                RegisterScreen(screen);

            // Apply default theme before any screen is shown
            if (defaultTheme != null)
                ApplyTheme(defaultTheme);

            // Show screens that are flagged as visible on start
            foreach (var screen in sceneScreens)
            {
                if (screen.VisibleOnStart)
                    ShowScreen(screen, addToStack: true);
            }
        }

        // ── Registration ──────────────────────────────────────────────────────────

        /// <summary>
        /// Register a screen so it can be shown by ID.
        /// Called automatically for screens listed in the Inspector;
        /// call manually for screens spawned at runtime (e.g. from prefabs).
        /// </summary>
        public void RegisterScreen(UIScreen screen)
        {
            if (screen == null)
            {
                Debug.LogWarning("[UIManager] Tried to register a null screen.");
                return;
            }

            if (_registry.ContainsKey(screen.ScreenId))
            {
                Debug.LogWarning($"[UIManager] Screen '{screen.ScreenId}' is already registered.");
                return;
            }

            _registry[screen.ScreenId] = screen;

            // Apply active theme immediately
            if (_activeTheme != null)
                screen.OnThemeApplied(_activeTheme);

            // Start hidden unless VisibleOnStart (handled in Awake after all screens are registered)
            screen.OnHide();

            Debug.Log($"[UIManager] Registered screen: {screen.ScreenId}");
        }

        /// <summary>
        /// Unregister a screen — e.g. when it belongs to a scene that is unloading.
        /// </summary>
        public void UnregisterScreen(UIScreen screen)
        {
            if (screen == null) return;
            _registry.Remove(screen.ScreenId);
        }

        // ── Navigation ────────────────────────────────────────────────────────────

        /// <summary>Push and show a screen by string ID.</summary>
        public void ShowScreen(string screenId, bool addToStack = true)
        {
            if (!_registry.TryGetValue(screenId, out var screen))
            {
                Debug.LogWarning($"[UIManager] Screen '{screenId}' not found in registry.");
                return;
            }

            ShowScreen(screen, addToStack);
        }

        /// <summary>Push and show a screen by direct reference.</summary>
        public void ShowScreen(UIScreen screen, bool addToStack = true)
        {
            if (screen == null) return;

            // Pause the current top screen
            if (_stack.Count > 0 && _stack.Peek() != screen)
                _stack.Peek().OnPause();

            if (addToStack)
                _stack.Push(screen);

            screen.OnShow();
            OnScreenShown?.Invoke(screen);
        }

        /// <summary>
        /// Pop and hide the top screen.
        /// The screen beneath it (if any) is resumed automatically.
        /// </summary>
        public void HideTopScreen()
        {
            if (_stack.Count == 0)
            {
                Debug.LogWarning("[UIManager] HideTopScreen called on an empty stack.");
                return;
            }

            var top = _stack.Pop();
            top.OnHide();
            OnScreenHidden?.Invoke(top);

            if (_stack.Count > 0)
                _stack.Peek().OnResume();
        }

        /// <summary>
        /// Navigate back — alias for HideTopScreen.
        /// Handy to wire to an Android back-button or an in-game Back button.
        /// </summary>
        public void GoBack() => HideTopScreen();

        /// <summary>Hide every screen and clear the stack.</summary>
        public void ClearAll()
        {
            while (_stack.Count > 0)
            {
                var screen = _stack.Pop();
                screen.OnHide();
                OnScreenHidden?.Invoke(screen);
            }
        }

        /// <summary>Clear the stack, then immediately show the given screen.</summary>
        public void ReplaceAll(string screenId)
        {
            ClearAll();
            ShowScreen(screenId);
        }

        // ── Theme ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Apply a new theme to all registered screens and buttons.
        /// Fires OnThemeChanged so any subscriber can react.
        /// </summary>
        public void ApplyTheme(UIThemeData theme)
        {
            if (theme == null)
            {
                Debug.LogWarning("[UIManager] ApplyTheme called with a null theme.");
                return;
            }

            _activeTheme = theme;

            foreach (var screen in _registry.Values)
                screen.OnThemeApplied(theme);

            OnThemeChanged?.Invoke(theme);
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        /// <summary>Returns true and the screen reference if the ID is registered.</summary>
        public bool TryGetScreen(string screenId, out UIScreen screen)
            => _registry.TryGetValue(screenId, out screen);

        /// <summary>True if the screen with this ID is currently the top of the stack.</summary>
        public bool IsTopScreen(string screenId)
            => _stack.Count > 0 && _stack.Peek().ScreenId == screenId;

        /// <summary>True if this screen ID exists anywhere in the stack.</summary>
        public bool IsInStack(string screenId)
        {
            foreach (var s in _stack)
                if (s.ScreenId == screenId) return true;
            return false;
        }
    }
}
