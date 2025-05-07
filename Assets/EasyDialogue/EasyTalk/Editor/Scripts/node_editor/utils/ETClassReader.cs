using EasyTalk.Editor.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace EasyTalk.Editor.Utils
{
    public class ETClassReader
    {
        private static Regex regex = new Regex(@"[^a-zA-Z0-9.][\s\s+\[\]]");

        /// <summary>
        /// Reads the C# file at the specified path and returns a list of all class names found.
        /// </summary>
        /// <param name="filePath">The file path to read from.</param>
        /// <returns>A list of all C# class names in the specified file.</returns>
        public static List<string> ReadClasses(string filePath)
        {
            List<string> classNames = new List<string>();

            try
            {
                StreamReader reader = new StreamReader(new FileStream(filePath, FileMode.Open));
                try
                {
                    string fileContent = reader.ReadToEnd();
                    fileContent = regex.Replace(fileContent, " ");

                    List<int> namespaceIndices = new List<int>(fileContent.AllIndexesOf("namespace"));

                    List<int> classIndices = new List<int>(fileContent.AllIndexesOf("public class"));
                    for (int i = 0; i < classIndices.Count; i++)
                    {
                        int indexOfClassName = classIndices[i] + 13;
                        int endIndex = fileContent.IndexOf(" ", indexOfClassName);

                        string className = fileContent.Substring(indexOfClassName, endIndex - indexOfClassName);

                        int namespaceKeywordIdx = -1;

                        if (namespaceIndices != null)
                        {
                            for (int j = 0; j < namespaceIndices.Count; j++)
                            {
                                if (namespaceIndices[j] > classIndices[i])
                                {
                                    break;
                                }

                                namespaceKeywordIdx = namespaceIndices[j];
                            }
                        }

                        if (namespaceKeywordIdx >= 0)
                        {
                            int nsNameIdx = namespaceKeywordIdx + 10;
                            int spaceIdx = fileContent.IndexOf(' ', nsNameIdx);
                            string ns = fileContent.Substring(nsNameIdx, spaceIdx - nsNameIdx);
                            className = ns + "." + className;
                        }

                        className = className.Replace("\n", "").Replace("\r", "");

                        classNames.Add(className);
                    }
                }
                catch { }

                reader.Close();
            }
            catch { }

            return classNames;
        }
    }
}