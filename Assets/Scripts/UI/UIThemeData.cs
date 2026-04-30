using UnityEngine;

namespace UIFramework.Core
{
    /// <summary>
    /// ScriptableObject that stores the visual palette for the entire UI.
    /// Create one via  Assets → Create → UIFramework → Theme Data.
    /// Assign it to UIManager in the Inspector; it is pushed to every
    /// registered UIScreen and UIButton automatically.
    /// </summary>
    [CreateAssetMenu(menuName = "UIFramework/Theme Data", fileName = "UIThemeData")]
    public class UIThemeData : ScriptableObject
    {
        [Header("Button Background States")]
        public Color buttonNormal   = new Color(0.20f, 0.20f, 0.20f, 1f);
        public Color buttonHovered  = new Color(0.35f, 0.35f, 0.35f, 1f);
        public Color buttonPressed  = new Color(0.12f, 0.12f, 0.12f, 1f);
        public Color buttonDisabled = new Color(0.20f, 0.20f, 0.20f, 0.40f);

        [Header("Button Label States")]
        public Color labelNormal   = Color.white;
        public Color labelHovered  = Color.white;
        public Color labelPressed  = new Color(0.85f, 0.85f, 0.85f, 1f);
        public Color labelDisabled = new Color(1f, 1f, 1f, 0.38f);

        [Header("Screen Background")]
        public Color screenBackground = new Color(0.08f, 0.08f, 0.08f, 1f);

        [Header("Accent")]
        [Tooltip("Highlight or focus colour used by custom screen elements.")]
        public Color accent = new Color(0.18f, 0.52f, 1.00f, 1f);
    }
}
