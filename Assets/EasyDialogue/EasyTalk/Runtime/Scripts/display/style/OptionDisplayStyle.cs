using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Display.Style
{
    /// <summary>
    /// This class allows styling to be defined for an option display's components.
    /// </summary>
    [Serializable]
    public class OptionDisplayStyle
    {
        /// <summary>
        /// A List of image styles to use for option display images.
        /// </summary>
        [Header("Option Image Settings")]
        [SerializeField] public List<ImageStyleSettings> optionPanelImageStyles = new List<ImageStyleSettings>();

        /// <summary>
        /// The style to apply to the option panel's buttons.
        /// </summary>
        [Header("Option Button Settings")]
        [SerializeField] public OptionButtonStyle optionButtonStyle = new OptionButtonStyle();

        /// <summary>
        /// Styling to use for directional option displays.
        /// </summary>
        [Header("Directional Element Settings")]
        [SerializeField] public DirectionalOptionDisplayStyle directionalStyle = new DirectionalOptionDisplayStyle();
    }

    /// <summary>
    /// This class defines styling for option panel dialogue button components.
    /// </summary>
    [Serializable]
    public class OptionButtonStyle
    {
        /// <summary>
        /// The style to use on the button's image.
        /// </summary>
        [SerializeField]
        public ImageStyleSettings imageStyleSettings = new ImageStyleSettings();

        /// <summary>
        /// The style to use on the button.
        /// </summary>
        [SerializeField]
        public ButtonStyleSettings buttonStyleSettings = new ButtonStyleSettings();

        /// <summary>
        /// The style to use on the button's text.
        /// </summary>
        [SerializeField]
        public TextStyleSettings textStyleSettings = new TextStyleSettings();
        
        /// <summary>
        /// The sound to use when hovered over.
        /// </summary>
        [SerializeField] 
        public AudioClip hoverSound;
    }
}