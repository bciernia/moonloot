namespace EasyTalk.Nodes.Tags
{
    /// <summary>
    /// A node tag which is used to set whether an option presented to the player is selectable or not.
    /// </summary>
    public class SelectableTag : NodeTag
    {
        /// <summary>
        /// Whether the option is selectable.
        /// </summary>
        public bool selectable;

        /// <summary>
        /// Creates a new SelectableTag.
        /// </summary>
        public SelectableTag() : base("selectable") { }

        /// <summary>
        /// Creates a new SelectableTag with the value provided.
        /// </summary>
        /// <param name="selectable">Whether the option is selectable.</param>
        public SelectableTag(bool selectable) : this()
        {
            this.selectable = selectable;
        }
    }
}
