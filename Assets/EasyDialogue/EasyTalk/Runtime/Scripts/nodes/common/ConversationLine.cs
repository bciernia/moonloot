using UnityEngine;

namespace EasyTalk.Nodes.Common
{
    /// <summary>
    /// This class is a representation of a line of dialogue in the EasyTalk dialogue system.
    /// </summary>
    public class ConversationLine
    {
        /// <summary>
        /// GEts or sets the ID attributed to the line of dialogue.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Gets or sets the full text of the line of dialogue.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The evaluated text value of the line of dialogue, immediately prior to when it was translated.
        /// </summary>
        public string PreTranslationText { get; set; }

        /// <summary>
        /// Gets or sets the audio clip associated with the line of dialogue.
        /// </summary>
        public AudioClip AudioClip { get; set; }

        /// <summary>
        /// Gets or sets the key value associated with the line of dialogue. The key value can be anything and can be used to perform various actions when a line of dialogue is reached.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets a flag which indicates that there is not an option directly after this line, so there is likely another line of text to display.
        /// </summary>
        public bool PrecedesOption { get; set; } = false;

        /// <summary>
        /// Gets or sets the display ID of the conversation display to target and display this dialogue text on.
        /// </summary>
        public string Target { get; set; } = null;

        /// <summary>
        /// Gets or sets the original (untranslated) name of the character speaking this line of dialogue.
        /// </summary>
        public string OriginalCharacterName { get; set; }

        /// <summary>
        /// Gets or sets the translated name of the character speaking this line of dialogue.
        /// </summary>
        public string TranslatedCharacterName { get; set; } = null;

        /// <summary>
        /// Gets or sets whether the line of dialogue is being played in AUTOPLAY mode.
        /// </summary>
        public bool AutoPlay { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the line overrides the automatically calculated autoplay line delay.
        /// </summary>
        public bool OverrideAutoplayDelay { get; set; } = false;

        /// <summary>
        /// Gets or sets the delay to use for the line when it is autoplayed.
        /// </summary>
        public float AutoPlayDelay { get; set; } = 3.0f;

        /// <summary>
        /// Gets or sets the minimal amount of time that the line of dialogue will be displayed when in AUTOPLAY mode (unless skipped).
        /// </summary>
        public float PlayTime { get; set; } = 0.0f;

        /// <summary>
        /// Gets or sets the icon ID which is to be displayed when the line of dialogue is shown.
        /// </summary>
        public string IconID { get; set; } = null;

        /// <summary>
        /// Gets or sets the text display mode to use when displaying the line.
        /// </summary>
        public TextDisplayMode TextDisplayMode { get; set; } = TextDisplayMode.REPLACE;
    }

    public enum TextDisplayMode
    {
        REPLACE, APPEND
    }
}