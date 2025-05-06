using System;
using UnityEngine;

namespace EasyTalk.Display.Style
{
    /// <summary>
    /// Defines a style which can be applied to a dialogue display.
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueStyle", menuName = "EasyTalk/Style/Dialogue Style", order = 9)]
    [Serializable]
    public class DialogueStyle : ScriptableObject
    {
        /// <summary>
        /// The style to use for the conversation display.
        /// </summary>
        [SerializeField]
        public ConversationDisplayStyle conversationStyle;

        /// <summary>
        /// The style to use for the option display.
        /// </summary>
        [SerializeField]
        public OptionDisplayStyle optionStyle;

        /// <summary>
        /// The style to use for the continue display.
        /// </summary>
        [SerializeField]
        public ContinueDisplayStyle continueStyle;
    }
}