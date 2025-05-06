namespace EasyTalk.Nodes.Tags
{
    /// <summary>
    /// A node tag which is used to change the target conversation display for a line of dialogue.
    /// </summary>
    public class TargetTag : NodeTag
    {
        /// <summary>
        /// The Display ID of the target conversation display.
        /// </summary>
        public string target;

        /// <summary>
        /// Creates a new TargetTag.
        /// </summary>
        public TargetTag() : base("target") { }

        /// <summary>
        /// Creates a new TargetTag with the provided target (display ID).
        /// </summary>
        /// <param name="target">The Display ID of the target conversation display.</param>
        public TargetTag(string target) : this()
        {
            this.target = target;
        }
    }
}
