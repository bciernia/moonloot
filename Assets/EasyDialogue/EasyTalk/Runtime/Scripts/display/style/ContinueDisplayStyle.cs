using System;
using UnityEngine;

namespace EasyTalk.Display.Style
{
    /// <summary>
    /// This class is used to define style settings for a continue display.
    /// </summary>
    [Serializable]
    public class ContinueDisplayStyle
    {
        /// <summary>
        /// The styling to use for the image of the continue display.
        /// </summary>
        [SerializeField] public ImageStyleSettings imageStyleSettings = new ImageStyleSettings();

        /// <summary>
        /// The styling to use on the continue button.
        /// </summary>
        [SerializeField] public ButtonStyleSettings buttonStyleSettings = new ButtonStyleSettings();

        /// <summary>
        /// The styling to use on the continue text.
        /// </summary>
        [SerializeField] public TextStyleSettings textStyleSettings = new TextStyleSettings();
    }
}
