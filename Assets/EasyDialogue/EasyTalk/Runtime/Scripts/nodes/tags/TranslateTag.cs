namespace EasyTalk.Nodes.Tags
{
    /// <summary>
    /// A node tag used to specify whether text should be translated or not.
    /// </summary>
    public class TranslateTag : NodeTag
    {
        /// <summary>
        /// Whether the text should be translated.
        /// </summary>
        public bool translate;

        /// <summary>
        /// Creates a  new TranslateTag.
        /// </summary>
        public TranslateTag() : base("translate") { }

        /// <summary>
        /// Creates a new TranslateTag with the value specified.
        /// </summary>
        /// <param name="translate">Whether the text should be translated.</param>
        public TranslateTag(bool translate) : this()
        {
            this.translate = translate;
        }
    }
}