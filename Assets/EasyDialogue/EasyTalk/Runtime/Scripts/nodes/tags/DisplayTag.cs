namespace EasyTalk.Nodes.Tags
{
    /// <summary>
    /// A node tag used to indicate whether an option should be presented to the player.
    /// </summary>
    public class DisplayTag : NodeTag
    {
        /// <summary>
        /// Whether to display the option.
        /// </summary>
        public bool display;

        /// <summary>
        /// Creates a new DisplayTag.
        /// </summary>
        public DisplayTag() : base("display") { }

        /// <summary>
        /// Creates a new DisplayTag with the specified value.
        /// </summary>
        /// <param name="display">Whether the option should be displayed.</param>
        public DisplayTag(bool display) : this()
        {
            this.display = display;
        }
    }
}