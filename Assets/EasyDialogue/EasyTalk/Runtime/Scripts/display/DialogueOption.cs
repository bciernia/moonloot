namespace EasyTalk.Display
{
    /// <summary>
    /// This class is a representation of a dialogue option presented to a player.
    /// </summary>
    public class DialogueOption
    {
        /// <summary>
        /// GEts or sets the ID attributed to the Dialogue Option.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Gets or sets the text of the option.
        /// </summary>
        public string OptionText { get; set; } = "...";

        /// <summary>
        /// The evaluated text value of the option, immediately prior to when it was translated.
        /// </summary>
        public string PreTranslationText { get; set; }

        /// <summary>
        /// Gets or sets the index of the option (as it occurs in the original list of options).
        /// </summary>
        public int OptionIndex { get; set; } = -1;

        /// <summary>
        /// Gets or sets whether the option is to be displayed.
        /// </summary>
        public bool IsDisplayed { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the option is selectable.
        /// </summary>
        public bool IsSelectable { get; set; } = true;
    }
}