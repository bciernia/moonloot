using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Display.Style
{
    /// <summary>
    /// This class is used to define a frame style theme for dialogue displays.
    /// </summary>
    //[CreateAssetMenu(fileName = "Frame Theme", menuName = "EasyTalk/Style/Frame Theme")]
    public class FrameTheme : ScriptableObject
    {
        /// <summary>
        /// A List of image settings to use for the converstaion display's images.
        /// </summary>
        [SerializeField] public List<ImageStyleSettings> convoImageSettings = new List<ImageStyleSettings>();

        /// <summary>
        /// The settings to use for the character panel's image.
        /// </summary>
        [SerializeField] public ImageStyleSettings characterPanelImageSettings = new ImageStyleSettings();

        /// <summary>
        /// The settings to use for the continue display's image.
        /// </summary>
        [SerializeField] public ImageStyleSettings continueImageSettings = new ImageStyleSettings();

        /// <summary>
        /// The settings to use for the option buttons' images.
        /// </summary>
        [SerializeField] public ImageStyleSettings buttonImageSettings = new ImageStyleSettings();

        /// <summary>
        /// The settings to use for the images of the option display.
        /// </summary>
        [SerializeField] public List<ImageStyleSettings> optionImageSettings = new List<ImageStyleSettings>();
    }
}
