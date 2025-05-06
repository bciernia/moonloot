using System;
using UnityEngine;
using UnityEngine.UI;

namespace EasyTalk.Display.Style
{
    /// <summary>
    /// This class defines styling for an image component.
    /// </summary>
    [Serializable]
    public class ImageStyleSettings
    {
        /// <summary>
        /// Whether the image is enabled.
        /// </summary>
        [SerializeField] public bool enabled = true;

        /// <summary>
        /// The sprite to use.
        /// </summary>
        [SerializeField] public Sprite sprite;

        /// <summary>
        /// The color to use.
        /// </summary>
        [SerializeField][ColorUsage(true)] public Color color = Color.white;

        /// <summary>
        /// The display mode to use for the image.
        /// </summary>
        [SerializeField] public Image.Type imageType = Image.Type.Simple;

        /// <summary>
        /// The pixels per unit multiplier to use.
        /// </summary>
        [SerializeField] public float pixelsPerUnit = 1.0f;
    }
}
