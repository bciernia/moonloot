namespace EasyTalk.Nodes.Tags
{
    /// <summary>
    /// A node tag used to set the character name during dialogue playback.
    /// </summary>
    public class NameTag : NodeTag
    {
        /// <summary>
        /// The name of the character to use.
        /// </summary>
        public string name;

        /// <summary>
        /// The ID of the icon to show (if applicable).
        /// </summary>
        public string iconId;

        /// <summary>
        /// Creates a new NameTag.
        /// </summary>
        public NameTag() : base("name") { }

        /// <summary>
        /// Creates a new NameTag with the specified character name.
        /// </summary>
        /// <param name="name">The character name.</param>
        public NameTag(string name) : this()
        {
            if(name.Contains(','))
            {
                string[] values = name.Split(',');
                this.name = values[0];
                this.iconId = values[1];
            }
            else
            {
                this.name = name;
            }
        }
    }
}
