using TMPro;
using UIFramework.Core;
using UIFramework.Localization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIFramework.Components
{
    /// <summary>
    /// Reusable UI Button — no DOTween, no animation curves.
    /// Visual feedback is instant colour swaps driven by a simple state machine.
    ///
    /// States:  Normal → Hovered → Pressed → (back to Hovered / Normal on release)
    ///          Disabled — blocks all pointer events.
    ///
    /// Events (wired in Inspector or subscribed in code):
    ///   onClick       — pointer clicked (not fired when disabled)
    ///   onHoverEnter  — pointer entered bounds
    ///   onHoverExit   — pointer left bounds
    ///   onPressed     — pointer button pressed down
    ///   onReleased    — pointer button released
    ///
    /// Localisation:
    ///   Set localizationKey in the Inspector or via code. The label refreshes
    ///   automatically whenever LocalizationManager fires OnLanguageChanged.
    /// </summary>
    [AddComponentMenu("UIFramework/UI Button")]
    public class UIButton : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        // ── Button State ──────────────────────────────────────────────────────────
        public enum ButtonState { Normal, Hovered, Pressed, Disabled }

        // ── Inspector — References ────────────────────────────────────────────────
        [Header("References")]
        [Tooltip("Background Image whose colour changes with state. Auto-found if left empty.")]
        [SerializeField] private Image backgroundImage;

        [Tooltip("TMP label. Auto-found in children if left empty.")]
        [SerializeField] private TextMeshProUGUI label;

        [Tooltip("Optional icon image (child Image that is not the background).")]
        [SerializeField] private Image iconImage;

        // ── Inspector — Colours ───────────────────────────────────────────────────
        [Header("Background Colours")]
        [SerializeField] private Color colorNormal   = new Color(0.20f, 0.20f, 0.20f, 1f);
        [SerializeField] private Color colorHovered  = new Color(0.35f, 0.35f, 0.35f, 1f);
        [SerializeField] private Color colorPressed  = new Color(0.12f, 0.12f, 0.12f, 1f);
        [SerializeField] private Color colorDisabled = new Color(0.20f, 0.20f, 0.20f, 0.40f);

        [Header("Label Colours")]
        [SerializeField] private Color labelNormal   = Color.white;
        [SerializeField] private Color labelHovered  = Color.white;
        [SerializeField] private Color labelPressed  = new Color(0.85f, 0.85f, 0.85f, 1f);
        [SerializeField] private Color labelDisabled = new Color(1f, 1f, 1f, 0.38f);

        // ── Inspector — Behaviour ────────────────────────────────────────────────
        [Header("Behaviour")]
        [SerializeField] private bool interactable = true;

        [Tooltip("Localisation key for the label. Leave empty to manage the label text manually.")]
        [SerializeField] private string localizationKey;

        // ── Inspector — Events ────────────────────────────────────────────────────
        [Header("Events")]
        [Tooltip("Fired when the button is clicked. Not fired when disabled.")]
        public UnityEvent onClick;

        [Tooltip("Fired when the pointer enters the button area.")]
        public UnityEvent onHoverEnter;

        [Tooltip("Fired when the pointer leaves the button area.")]
        public UnityEvent onHoverExit;

        [Tooltip("Fired when the pointer is pressed down on the button.")]
        public UnityEvent onPressed;

        [Tooltip("Fired when the pointer is released over the button.")]
        public UnityEvent onReleased;

        // ── Runtime ───────────────────────────────────────────────────────────────
        private ButtonState _state     = ButtonState.Normal;
        private bool        _isHovered = false;

        // ── Properties ────────────────────────────────────────────────────────────
        public ButtonState CurrentState => _state;

        /// <summary>Get or set interactability. Transitions to Disabled / Normal immediately.</summary>
        public bool Interactable
        {
            get => interactable;
            set
            {
                interactable = value;
                TransitionTo(value ? ButtonState.Normal : ButtonState.Disabled);
            }
        }

        /// <summary>Directly set the label text (bypasses localisation key).</summary>
        public string LabelText
        {
            get => label != null ? label.text : string.Empty;
            set { if (label != null) label.text = value; }
        }

        /// <summary>Set a new localisation key and refresh the label immediately.</summary>
        public string LocalizationKey
        {
            get => localizationKey;
            set { localizationKey = value; RefreshLabel(); }
        }

        // ── Unity Lifecycle ───────────────────────────────────────────────────────
        private void Awake()
        {
            ResolveReferences();
            TransitionTo(interactable ? ButtonState.Normal : ButtonState.Disabled);
        }

        private void OnEnable()
        {
            if (LocalizationManager.IsInitialised)
                LocalizationManager.Instance.OnLanguageChanged += RefreshLabel;

            RefreshLabel();
        }

        private void OnDisable()
        {
            if (LocalizationManager.IsInitialised)
                LocalizationManager.Instance.OnLanguageChanged -= RefreshLabel;
        }

        // ── Pointer Handlers ─────────────────────────────────────────────────────
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!interactable) return;
            _isHovered = true;
            TransitionTo(ButtonState.Hovered);
            onHoverEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!interactable) return;
            _isHovered = false;
            TransitionTo(ButtonState.Normal);
            onHoverExit?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!interactable) return;
            TransitionTo(ButtonState.Pressed);
            onPressed?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!interactable) return;
            TransitionTo(_isHovered ? ButtonState.Hovered : ButtonState.Normal);
            onReleased?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable) return;
            onClick?.Invoke();
        }

        // ── State Machine ─────────────────────────────────────────────────────────

        /// <summary>Instantly apply the visual colours for a given state.</summary>
        private void TransitionTo(ButtonState newState)
        {
            _state = newState;

            Color bg  = newState switch
            {
                ButtonState.Normal   => colorNormal,
                ButtonState.Hovered  => colorHovered,
                ButtonState.Pressed  => colorPressed,
                ButtonState.Disabled => colorDisabled,
                _                    => colorNormal,
            };

            Color lbl = newState switch
            {
                ButtonState.Normal   => labelNormal,
                ButtonState.Hovered  => labelHovered,
                ButtonState.Pressed  => labelPressed,
                ButtonState.Disabled => labelDisabled,
                _                    => labelNormal,
            };

            if (backgroundImage != null) backgroundImage.color = bg;
            if (label != null)           label.color           = lbl;
        }

        // ── Theme ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Apply colours from a UIThemeData asset.
        /// Called automatically by UIScreen.OnThemeApplied if the button is inside a UIScreen.
        /// </summary>
        public void ApplyTheme(UIThemeData theme)
        {
            if (theme == null) return;

            colorNormal   = theme.buttonNormal;
            colorHovered  = theme.buttonHovered;
            colorPressed  = theme.buttonPressed;
            colorDisabled = theme.buttonDisabled;

            labelNormal   = theme.labelNormal;
            labelHovered  = theme.labelHovered;
            labelPressed  = theme.labelPressed;
            labelDisabled = theme.labelDisabled;

            TransitionTo(_state); // Re-apply current state with new colours
        }

        // ── Localisation ──────────────────────────────────────────────────────────
        private void RefreshLabel()
        {
            if (string.IsNullOrEmpty(localizationKey)) return;
            if (!LocalizationManager.IsInitialised)    return;
            if (label == null)                         return;

            label.text = LocalizationManager.Instance.Get(localizationKey);
        }

        // ── Public Utilities ──────────────────────────────────────────────────────

        /// <summary>Trigger a click from code (respects interactable flag).</summary>
        public void SimulateClick()
        {
            if (!interactable) return;
            onClick?.Invoke();
        }

        /// <summary>Assign a sprite to the icon image.</summary>
        public void SetIcon(Sprite sprite)
        {
            if (iconImage == null) return;
            iconImage.sprite  = sprite;
            iconImage.enabled = sprite != null;
        }

        // ── Helpers ───────────────────────────────────────────────────────────────
        private void ResolveReferences()
        {
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();

            if (label == null)
                label = GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);

            if (iconImage == null)
            {
                // Find the first child Image that isn't the background
                foreach (var img in GetComponentsInChildren<Image>(includeInactive: true))
                {
                    if (img != backgroundImage) { iconImage = img; break; }
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Preview colour changes in Edit mode without entering Play mode
            ResolveReferences();
            TransitionTo(interactable ? ButtonState.Normal : ButtonState.Disabled);
        }
#endif
    }
}
