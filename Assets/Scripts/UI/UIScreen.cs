using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace UIFramework.Core
{
    /// <summary>
    /// Base class for every UI screen in your game.
    ///
    /// Lifecycle called by UIManager:
    ///   OnShow   — screen becomes the active top screen.
    ///   OnHide   — screen is popped from the stack entirely.
    ///   OnPause  — a new screen was pushed on top; this one is now beneath it.
    ///   OnResume — the screen on top was hidden; this one is active again.
    ///
    /// Visibility is controlled via CanvasGroup (alpha + raycasts) so the
    /// GameObject stays active and coroutines / event listeners keep running.
    ///
    /// Override OnThemeApplied(UIThemeData) to react to theme changes.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIScreen : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────────
        [Header("Identity")]
        [Tooltip("Unique ID used by UIManager.ShowScreen(id). Auto-filled from GameObject name.")]
        [SerializeField] private string screenId;

        [Tooltip("Show this screen at startup without an explicit ShowScreen call.")]
        [SerializeField] private bool visibleOnStart = false;

        [Header("Screen Events")]
        public UnityEvent onShow;
        public UnityEvent onHide;
        public UnityEvent onPause;
        public UnityEvent onResume;

        // ── Runtime ───────────────────────────────────────────────────────────────
        private CanvasGroup _canvasGroup;
        private bool        _visible;

        // ── Properties ────────────────────────────────────────────────────────────
        public string ScreenId       => screenId;
        public bool   VisibleOnStart => visibleOnStart;
        public bool   IsVisible      => _visible;

        // ── Unity Lifecycle ───────────────────────────────────────────────────────
        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        // ── Lifecycle Methods (called by UIManager) ───────────────────────────────

        /// <summary>Screen becomes the active, interactable top screen.</summary>
        public virtual void OnShow()
        {
            gameObject.SetActive(true);
            SetCanvasGroup(alpha: 1f, interactable: true, blocksRaycasts: true);
            _visible = true;
            onShow.Invoke();
        }

        /// <summary>Screen is removed from the stack — no longer rendered or interactive.</summary>
        public virtual void OnHide()
        {
            _visible = false;
            onHide.Invoke();
            SetCanvasGroup(alpha: 0f, interactable: false, blocksRaycasts: false);
        }

        /// <summary>Another screen was pushed on top; this screen stays in the stack but idles.</summary>
        public virtual void OnPause()
        {
            SetCanvasGroup(alpha: 0f, interactable: false, blocksRaycasts: false);
            onPause.Invoke();
        }

        /// <summary>The screen above was removed; this screen is interactive again.</summary>
        public virtual void OnResume()
        {
            SetCanvasGroup(alpha: 1f, interactable: true, blocksRaycasts: true);
            _visible = true;
            onResume.Invoke();
        }

        // ── Theme ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Called by UIManager when a new theme is applied.
        /// Override this to update custom visual elements.
        /// </summary>
        public virtual void OnThemeApplied(UIThemeData theme) { }

        // ── Convenience ───────────────────────────────────────────────────────────

        /// <summary>Ask UIManager to close this screen (pop from stack).</summary>
        public void Close()
        {
            if (UIManager.IsInitialised)
                UIManager.Instance.HideTopScreen();
        }

        // ── Internal ──────────────────────────────────────────────────────────────
        private void SetCanvasGroup(float alpha, bool interactable, bool blocksRaycasts)
        {
            if (_canvasGroup == null) return;

            _canvasGroup.DOFade(alpha, 0.5f);
            // _canvasGroup.alpha          = alpha;
            _canvasGroup.interactable   = interactable;
            _canvasGroup.blocksRaycasts = blocksRaycasts;
        }

        // ── Editor Helper ─────────────────────────────────────────────────────────
#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(screenId))
                screenId = gameObject.name;
        }
#endif
    }
}
