using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Display.Style
{
    /// <summary>
    /// This class is used to create styles for conversation displays.
    /// </summary>
    [Serializable]
    public class ConversationDisplayStyle
    {
        /// <summary>
        /// A List of styles for conversation display panel images.
        /// </summary>
        [Header("Image Settings")]
        [SerializeField] public List<ImageStyleSettings> convoImageStyles = new List<ImageStyleSettings>();

        /// <summary>
        /// The style to use for the text of the conversation display.
        /// </summary>
        [Header("Conversation Text Settings")]
        [SerializeField] public TextStyleSettings convoTextStyle = new TextStyleSettings();

        /// <summary>
        /// The style to use for the character name panel.
        /// </summary>
        [Header("Character Name Settings")]
        [SerializeField] public CharacterNameStyle characterNameStyle = new CharacterNameStyle();
    }

    /// <summary>
    /// This class defines styling for the character name panel.
    /// </summary>
    [Serializable]
    public class CharacterNameStyle
    {
        /// <summary>
        /// The style to use for the character name's text.
        /// </summary>
        [SerializeField] public TextStyleSettings textStyleSettings = new TextStyleSettings();

        /// <summary>
        /// The style to use for the character name's image.
        /// </summary>
        [SerializeField] public ImageStyleSettings imageStyleSettings = new ImageStyleSettings();
    }
}