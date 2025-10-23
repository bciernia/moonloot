using System;
using System.Collections.Generic;

namespace EasyTalk.Nodes.Tags
{
    /// <summary>
    /// Base class implementation defining common features for node tags which modify how dialogue nodes are processed during gameplay.
    /// </summary>
    public class NodeTag
    {
        /// <summary>
        /// The name of the tag.
        /// </summary>
        public string tagName;

        /// <summary>
        /// Creates a new NodeTag with the specified tag name.
        /// </summary>
        /// <param name="tagName">The tag name.</param>
        public NodeTag(string tagName)
        {
            this.tagName = tagName;
        }

        /// <summary>
        /// Extracts all node tags from the provided string and stores them in a Dictionary, where the key is the tag name and the value is the node tag itself.
        /// </summary>
        /// <param name="text">The string to extract tags from.</param>
        /// <param name="tags">A Dictionary to store extracted node tags in.</param>
        /// <returns>A version of the original string with all node tags removed.</returns>
        public static string ExtractTags(string text, Dictionary<string, NodeTag> tags)
        {
            string newText = text;
            int startIndex = 0;
            int endIndex = 0;

            while ((startIndex = newText.IndexOf('[')) != -1 &&
                (endIndex = newText.IndexOf(']', startIndex + 1)) != -1)
            {
                string tagText = newText.Substring(startIndex + 1, endIndex - startIndex - 1);
                newText = newText.Substring(0, startIndex) + newText.Substring(endIndex + 1);

                int splitIndex = tagText.IndexOf(':');
                if (splitIndex != -1)
                {
                    string tagName = tagText.Substring(0, splitIndex);
                    string value = tagText.Substring(splitIndex + 1);

                    NodeTag tag = CreateTag(tagName, value);
                    if (tag != null && !tags.ContainsKey(tag.tagName))
                    {
                        tags.Add(tag.tagName, tag);
                    }
                }
                else
                {
                    NodeTag tag = CreateTag(tagText);
                    if(tag != null && !tags.ContainsKey(tag.tagName))
                    {
                        tags.Add(tag.tagName, tag);
                    }
                }
            }

            return newText;
        }

        /// <summary>
        /// Creates and returns a version of the provided string with all of the node tags removed.
        /// </summary>
        /// <param name="text">The string to remove node tags from.</param>
        /// <returns>A version of the original string with all node tags removed.</returns>
        public static string RemoveTags(string text)
        {
            string newText = text;
            int startIndex = 0;
            int endIndex = 0;

            while (startIndex < newText.Length &&
                (startIndex = newText.IndexOf('[', startIndex)) != -1 &&
                startIndex + 1 < newText.Length &&
                (endIndex = newText.IndexOf(']', startIndex + 1)) != -1)
            {
                newText = newText.Substring(0, startIndex) + newText.Substring(endIndex + 1);
                startIndex = 0;
            }

            return newText;
        }

        /// <summary>
        /// Extracts the specified node tag type from the provided string and stores it in the NodeTag specified.
        /// </summary>
        /// <param name="text">The string to extract a node tag from.</param>
        /// <param name="tagName">The name of the tag type to extract.</param>
        /// <param name="tag">The NodeTag to store the tag's value(s) in.</param>
        /// <returns>A version of the original string with the specified node tag removed.</returns>
        public static string ExtractTag(string text, string tagName, out NodeTag tag)
        {
            string newText = text;
            tag = null;

            int tagStartIndex = newText.IndexOf("[" + tagName);
            int tagEndIndex = newText.IndexOf("]", tagStartIndex + 1);
            if (tagStartIndex != -1 && tagEndIndex != -1)
            {
                string tagText = newText.Substring(tagStartIndex + 1, tagEndIndex - tagStartIndex - 1);
                newText = newText.Substring(0, tagStartIndex) + newText.Substring(tagEndIndex + 1);

                int splitIndex = tagText.IndexOf(':');
                if (splitIndex != -1)
                {
                    string value = tagText.Substring(splitIndex + 1);
                    tag = CreateTag(tagName, value);
                }
                else
                {
                    tag = CreateTag(tagName);
                }
            }

            return newText;
        }

        public static NodeTag CreateTag(string tagName)
        {
            NodeTag tag = null;

            if(tagName.Equals("append"))
            {
                tag = new AppendTag();
            }
            else if(tagName.Equals("autoplay"))
            {
                tag = new AutoplayTag();
            }

            return tag;
        }

        /// <summary>
        /// Creates a new NodeTag of the specified type with the value provided.
        /// </summary>
        /// <param name="tagName">The name of the tag type to create.</param>
        /// <param name="value">The value to set on the created node tag.</param>
        /// <returns>The created NodeTag.</returns>
        public static NodeTag CreateTag(string tagName, string value)
        {
            NodeTag tag = null;

            if (tagName.Equals("key"))
            {
                tag = new KeyTag(value);
            }
            else if (tagName.Equals("name"))
            {
                tag = new NameTag(value);
            }
            else if (tagName.Equals("target"))
            {
                tag = new TargetTag(value);
            }
            else if (tagName.Equals("display"))
            {
                tag = new DisplayTag(Convert.ToBoolean(value));
            }
            else if (tagName.Equals("selectable"))
            {
                tag = new SelectableTag(Convert.ToBoolean(value));
            }
            else if (tagName.Equals("translate"))
            {
                tag = new TranslateTag(Convert.ToBoolean(value));
            }
            else if(tagName.Equals("id"))
            {
                tag = new IDTag(value);
            }
            else if(tagName.Equals("autoplay"))
            {
                float delay = 1.0f;
                float.TryParse(value, out delay);
                tag = new AutoplayTag(delay);
            }

            return tag;
        }
    }
}
