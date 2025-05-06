using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Display.Style
{
    /// <summary>
    /// This class is used to define color schemes for changing the colors of dialogue displays.
    /// </summary>
    [CreateAssetMenu(fileName = "Color Theme", menuName = "EasyTalk/Style/Color Theme", order = 10)]
    public class ColorTheme : ScriptableObject
    {
        /// <summary>
        /// The main color of the theme.
        /// </summary>
        [SerializeField] public Color color = Color.white;

        /// <summary>
        /// A complimentary color to the main color.
        /// </summary>
        [SerializeField] public Color complimentary1 = Color.white;

        /// <summary>
        /// A complimentary color to the main color.
        /// </summary>
        [SerializeField] public Color complimentary2 = Color.white;

        /// <summary>
        /// A contrasting color to the main color.
        /// </summary>
        [SerializeField] public Color contrastColor1 = Color.white;

        /// <summary>
        /// A contrasting color to the main color.
        /// </summary>
        [SerializeField] public Color contrastColor2 = Color.white;

        /// <summary>
        /// The color to use on the conversation display's text.
        /// </summary>
        [SerializeField] public Color convoTextColor = Color.white;

        /// <summary>
        /// The colors to use on each image in the conversation display.
        /// </summary>
        [SerializeField] public List<Color> convoImageColors;

        /// <summary>
        /// The color to use on the character name text.
        /// </summary>
        [SerializeField] public Color characterNameTextColor = Color.white;

        /// <summary>
        /// The color to use on the character panel image.
        /// </summary>
        [SerializeField] public Color characterPanelColor = Color.white;

        /// <summary>
        /// The colors to use on each image in the option display's main panel.
        /// </summary>
        [SerializeField] public List<Color> optionImageColors;

        /// <summary>
        /// The color to use for option buttons' text when in 'normal' mode.
        /// </summary>
        [SerializeField] public Color buttonTextNormalColor = Color.white;

        /// <summary>
        /// The color to use for option buttons' text when in 'pressed' mode.
        /// </summary>
        [SerializeField] public Color buttonTextPressedColor = Color.white;

        /// <summary>
        /// The color to use for option buttons' text when in 'disabled' mode.
        /// </summary>
        [SerializeField] public Color buttonTextDisabledColor = Color.white;

        /// <summary>
        /// The color to use for option buttons' text when in 'highlighted' mode.
        /// </summary>
        [SerializeField] public Color buttonTextHighlightColor = Color.white;

        /// <summary>
        /// The color to use for option buttons' images when in 'normal' mode.
        /// </summary>
        [SerializeField] public Color buttonNormalColor = Color.white;

        /// <summary>
        /// The color to use for option buttons' images when in 'pressed' mode.
        /// </summary>
        [SerializeField] public Color buttonPressedColor = Color.white;

        /// <summary>
        /// The color to use for option buttons' images when in 'disabled' mode.
        /// </summary>
        [SerializeField] public Color buttonDisabledColor = Color.white;

        /// <summary>
        /// The color to use for option buttons' images when in 'highlighted' mode.
        /// </summary>
        [SerializeField] public Color buttonHighlightColor = Color.white;

        /// <summary>
        /// The color to use on the continue text when in 'normal' mode.
        /// </summary>
        [SerializeField] public Color continueTextNormalColor = Color.white;

        /// <summary>
        /// The color to use on the continue text when in 'pressed' mode.
        /// </summary>
        [SerializeField] public Color continueTextPressedColor = Color.white;

        /// <summary>
        /// The color to use on the continue text when in 'disabled' mode.
        /// </summary>
        [SerializeField] public Color continueTextDisabledColor = Color.white;

        /// <summary>
        /// The color to use on the continue text when in 'highlighted' mode.
        /// </summary>
        [SerializeField] public Color continueTextHighlightColor = Color.white;

        /// <summary>
        /// The color to use on the continue background image when in 'normal' mode.
        /// </summary>
        [SerializeField] public Color continueButtonNormalColor = Color.white;

        /// <summary>
        /// The color to use on the continue background image when in 'pressed' mode.
        /// </summary>
        [SerializeField] public Color continueButtonPressedColor = Color.white;

        /// <summary>
        /// The color to use on the continue background image when in 'disabled' mode.
        /// </summary>
        [SerializeField] public Color continueButtonDisabledColor = Color.white;

        /// <summary>
        /// The color to use on the continue background image when in 'highlighted' mode.
        /// </summary>
        [SerializeField] public Color continueButtonHighlightColor = Color.white;
    }
}