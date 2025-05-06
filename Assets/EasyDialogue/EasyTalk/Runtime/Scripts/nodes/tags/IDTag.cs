namespace EasyTalk.Nodes.Tags
{
    /// <summary>
    /// A node tag used to set the ID attributed to an item in a Dialogue asset, such as an option or line of dialogue in a conversation node.
    /// </summary>
    public class IDTag : NodeTag
    {
        /// <summary>
        /// The ID of the tagged item.
        /// </summary>
        public string id;

        /// <summary>
        /// Creates a new IDTag.
        /// </summary>
        public IDTag() : base("id") { }

        /// <summary>
        /// Creates a new IDTag with the specified ID.
        /// </summary>
        /// <param name="id">The character name.</param>
        public IDTag(string id) : this()
        {
            this.id = id;
        }
    }
}
