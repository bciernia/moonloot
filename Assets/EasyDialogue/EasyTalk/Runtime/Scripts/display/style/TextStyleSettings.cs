using System;

#if TEXTMESHPRO_INSTALLED
using TMPro;
#endif

using UnityEngine;

namespace EasyTalk.Display.Style
{
    /// <summary>
    /// This class defines styling for text elements.
    /// </summary>
    [Serializable]
    public class TextStyleSettings
    {
        /// <summary>
        /// Whether the text is enabled.
        /// </summary>
        [SerializeField] public bool enabled = true;

#if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// The TextMeshPro font to use.
        /// </summary>
        [SerializeField] public TMP_FontAsset tmpFont;
#endif

        /// <summary>
        /// The font to use.
        /// </summary>
        [SerializeField] public Font font;

        /// <summary>
        /// The color to use for the text.
        /// </summary>
        [SerializeField] public Color color;

        /// <summary>
        /// The font size to use.
        /// </summary>
        [SerializeField] public float fontSize = 24.0f;

        /// <summary>
        /// Whether the font should be automatically resized for the best fit of the text.
        /// </summary>
        [SerializeField] public bool autoSizeFont = true;

        /// <summary>
        /// The minimum font size to use when in auto-size mode.
        /// </summary>
        [SerializeField] public float minFontSize = 8.0f;

        /// <summary>
        /// The maximum font size to use when in auto-size mode.
        /// </summary>
        [SerializeField] public float maxFontSize = 72.0f;

        /// <summary>
        /// Gets or sets the standard Font.
        /// </summary>
        public Font StandardFont
        {
            get { return this.font; }
            set { this.font = value; }
        }

#if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// Gets or sets the TextMeshPro Font.
        /// </summary>
        public TMP_FontAsset TMPFont
        {
            get { return tmpFont; }
            set { tmpFont = value; }
        }
#endif
    }
}