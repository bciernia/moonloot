namespace EasyTalk.Nodes.Tags
{
    /// <summary>
    /// A node tag used to mark a line of dialogue for automatic continuation.
    /// </summary>
    public class AutoplayTag : NodeTag
    {
        /// <summary>
        /// Whether the line should use the delay set on this tag, rather than the automatically calcualted dialogue line delay.
        /// </summary>
        public bool overrideDelay = false;
        
        /// <summary>
        /// The delay, in seconds, to use when overriding the dialogue line delay.
        /// </summary>
        public float delay = -1.0f;

        /// <summary>
        /// Creates a new autoplay tag.
        /// </summary>
        public AutoplayTag() : base("autoplay") { }

        /// <summary>
        /// Creates a new autoplay tag which overrides the automatically calculated delay.
        /// </summary>
        /// <param name="delay"></param>
        public AutoplayTag(float delay) : this()
        {
            this.delay = delay;
            overrideDelay = true;
        }
    }
}
