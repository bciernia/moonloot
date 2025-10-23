using System.Collections.Generic;

namespace EasyTalk.Nodes.Tags
{
    /// <summary>
    /// A class used to store TextMeshPro tag information.
    /// </summary>
    public class TMPTag
    {
        /// <summary>
        /// The name/type of the TextMeshPro tag.
        /// </summary>
        public string name;

        /// <summary>
        /// The full text of the tag.
        /// </summary>
        public string fullTagText;

        /// <summary>
        /// Whether the tag is a closing tag.
        /// </summary>
        public bool isCloseTag = false;

        /// <summary>
        /// The index within the original string where this tag starts or started.
        /// </summary>
        public int startIdx;

        /// <summary>
        /// The index within the original string where this tag ends or ended.
        /// </summary>
        public int endIdx;

        /// <summary>
        /// Creates a new TMPTag object with the specified values.
        /// </summary>
        /// <param name="name">The name/type of the tag.</param>
        /// <param name="text">The full text of the tag.</param>
        /// <param name="isCloseTag">Whether the tag is a closing tag.</param>
        /// <param name="startIdx">The starting index of this tag in the original string it is/was in.</param>
        /// <param name="endIdx">The ending index of this tag in the original string it is/was in.</param>
        public TMPTag(string name, string text, bool isCloseTag, int startIdx, int endIdx)
        {
            this.name = name;
            this.fullTagText = text;
            this.isCloseTag = isCloseTag;
            this.startIdx = startIdx;
            this.endIdx = endIdx;
        }

        /// <summary>
        /// Searches for TextMeshPro/HTML markup tags in the provided string and creates TMPTag objects for those which are found.
        /// </summary>
        /// <param name="text">The string to find tags in.</param>
        /// <returns>A List of TMPTags which were found in the provided string.</returns>
        public static List<TMPTag> FindTags(string text)
        {
            bool seekingCloseArrow = false;
            int index = 0;
            List<TMPTag> tags = new List<TMPTag>();
            string tagText = "";
            bool isCloseTag = false;
            int startIdx = 0;
            int endIdx = 0;
            bool seekingTagName = true;
            string tagName = "";

            while (index < text.Length)
            {
                if (text[index] == '<')
                {
                    tagText = "<";
                    startIdx = index;
                    seekingCloseArrow = true;

                    if (index < text.Length - 1 && text[index + 1] == '/')
                    {
                        isCloseTag = true;
                    }
                }
                else if (text[index] == '>' && seekingCloseArrow)
                {
                    tagText += ">";
                    endIdx = index;
                    tags.Add(new TMPTag(tagName, tagText, isCloseTag, startIdx, endIdx));

                    seekingCloseArrow = false;
                    seekingTagName = true;
                    isCloseTag = false;
                    tagName = "";
                    tagText = "";
                }
                else if (text[index] == '=' && seekingCloseArrow)
                {
                    tagText += "=";
                    seekingTagName = false;
                }
                else if (seekingCloseArrow)
                {
                    tagText += text[index];

                    if (seekingTagName && text[index] != '/')
                    {
                        tagName += text[index];
                    }
                }

                index++;
            }

            return tags;
        }

        /// <summary>
        /// Removes  all TextMeshPro/HTML markup tags from the specified string.
        /// </summary>
        /// <param name="text">The string to remove tags from.</param>
        /// <returns>A modified version of the original string with all TextMeshPro/HTML markup tags removed.</returns>
        public static string RemoveTags(string text)
        {
            string newText = text;
            int startIndex = 0;
            int endIndex = 0;

            while (startIndex < newText.Length &&
                (startIndex = newText.IndexOf('<', startIndex)) != -1 &&
                startIndex + 1 < newText.Length &&
                (endIndex = newText.IndexOf('>', startIndex + 1)) != -1)
            {
                newText = newText.Substring(0, startIndex) + newText.Substring(endIndex + 1);
                startIndex = 0;
            }

            return newText;
        }
    }
}
