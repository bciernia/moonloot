using System.Collections.Generic;

namespace EasyTalk.Editor.Extensions
{
    public static class Extensions
    {
        public static List<int> AllIndexesOf(this string text, string value)
        {
            List<int> indexes = new List<int>();
            int index = 0;

            while (index < text.Length && (index = text.IndexOf(value, index)) >= 0)
            {
                indexes.Add(index);
                index += value.Length;
            }

            return indexes;
        }
    }
}
