using System;
using UnityEngine;

namespace EasyTalk.Display.Style
{
    /// <summary>
    /// This class defines styling for dialogue buttons.
    /// </summary>
    [Serializable]
    public class ButtonStyleSettings
    {
        /// <summary>
        /// The color to use for the button's image when the button is in 'normal' mode.
        /// </summary>
        [SerializeField][ColorUsage(true)] public Color normalImageColor = Color.white;

        /// <summary>
        /// The color to use for the button's image when the button is in 'highlighted' mode.
        /// </summary>
        [SerializeField][ColorUsage(true)] public Color highlightedImageColor = Color.white;

        /// <summary>
        /// The color to use for the button's image when the button is in 'pressed' mode.
        /// </summary>
        [SerializeField][ColorUsage(true)] public Color pressedImageColor = Color.white;

        /// <summary>
        /// The color to use for the button's image when the button is in 'disabled' mode.
        /// </summary>
        [SerializeField][ColorUsage(true)] public Color disabledImageColor = Color.white;

        /// <summary>
        /// The color to use for the button's text when the button is in 'normal' mode.
        /// </summary>
        [SerializeField][ColorUsage(true)] public Color normalTextColor = Color.white;

        /// <summary>
        /// The color to use for the button's text when the button is in 'highlighted' mode.
        /// </summary>
        [SerializeField][ColorUsage(true)] public Color highlightedTextColor = Color.white;

        /// <summary>
        /// The color to use for the button's text when the button is in 'pressed' mode.
        /// </summary>
        [SerializeField][ColorUsage(true)] public Color pressedTextColor = Color.white;

        /// <summary>
        /// The color to use for the button's text when the button is in 'disabled' mode.
        /// </summary>
        [SerializeField][ColorUsage(true)] public Color disabledTextColor = Color.white;
    }
}
