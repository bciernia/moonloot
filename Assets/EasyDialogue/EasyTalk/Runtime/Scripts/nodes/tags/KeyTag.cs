namespace EasyTalk.Nodes.Tags
{
    /// <summary>
    /// A node tag used to pass along a custom key value which can be used by other scripts.
    /// </summary>
    public class KeyTag : NodeTag
    {
        /// <summary>
        /// The key value to use.
        /// </summary>
        public string keyValue;

        /// <summary>
        /// Creates a new KeyTag.
        /// </summary>
        public KeyTag() : base("key") { }

        /// <summary>
        /// Creates a new KeyTag with the specified value.
        /// </summary>
        /// <param name="keyValue">The key value.</param>
        public KeyTag(string keyValue) : this()
        {
            this.keyValue = keyValue;
        }
    }
}
