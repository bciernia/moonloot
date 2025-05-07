using UnityEngine;

namespace EasyTalk.Display
{
    /// <summary>
    /// Defines methods for conversation display listeners, which listen for changes to conversation displays.
    /// </summary>
    public class ConversationDisplayListener : MonoBehaviour
    {
        /// <summary>
        ///  When set to true, debug logging will be shown for each method called on the listener.
        /// </summary>
        [SerializeField]
        protected bool debugEnabled = false;

        /// <summary>
        /// Called whenever the character name is updated on a converation display.
        /// </summary>
        /// <param name="characterName">The derived/translated name of the character.</param>
        /// <param name="sourceName">The original/source name of the character.</param>
        public virtual void OnCharacterNameUpdated(string characterName, string sourceName)
        {
            if (debugEnabled) { Debug.Log("Convo Display -> Character Name Updated to '" + characterName + "' a.k.a. '" + sourceName + "'"); }
        }

        /// <summary>
        /// Called whenever the text of a conversation display is updated.
        /// </summary>
        /// <param name="text">The text which was set on the conversation display.</param>
        public virtual void OnConversationTextUpdated(string text)
        {
            if (debugEnabled) { Debug.Log("Convo Display -> Converation Text Updated to '" + text + "'"); }
        }

        /// <summary>
        /// Called whenever the converation display is reset.
        /// </summary>
        public virtual void OnReset()
        {
            if (debugEnabled) { Debug.Log("Convo Display -> Reset()"); }
        }
    }
}
