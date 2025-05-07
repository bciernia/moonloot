using System;
using UnityEngine;

namespace EasyTalk.Display.Style
{
    /// <summary>
    /// This class is used to define styling for a directional option display.
    /// </summary>
    [Serializable]
    public class DirectionalOptionDisplayStyle
    {        
        /// <summary>
        /// The color to use for the main image.
        /// </summary>
        [SerializeField][ColorUsage(true)] public Color mainImageColor = Color.white;

        /// <summary>
        /// The color to use on a link when in 'normal' mode.
        /// </summary>
        [SerializeField][ColorUsage(true)] public Color normalColor = Color.white;

        /// <summary>
        /// The color to use on a link when in 'highlighted' mode.
        /// </summary>
        [SerializeField][ColorUsage(true)] public Color highlightColor = Color.white;

        /// <summary>
        /// The color to use on a link when in 'pressed' mode.
        /// </summary>
        [SerializeField][ColorUsage(true)] public Color pressedColor = Color.white;

        /// <summary>
        /// The color to use on a link when in 'disabled' mode.
        /// </summary>
        [SerializeField][ColorUsage(true)] public Color disabledColor = Color.white;
    }
}