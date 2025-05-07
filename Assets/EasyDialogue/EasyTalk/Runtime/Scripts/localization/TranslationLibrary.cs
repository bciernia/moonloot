using EasyTalk.Nodes.Tags;
using EasyTalk.Nodes.Variable;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Localization
{
    /// <summary>
    /// Provides a collection of translations of text for various languages.
    /// </summary>
    [CreateAssetMenu(fileName = "Translation Library", menuName = "EasyTalk/Localization/Translation Library", order = 2)]
    [Serializable]
    public class TranslationLibrary : ScriptableObject
    {
        /// <summary>
        /// The original/default language for the translation library.
        /// </summary>
        [SerializeField]
        public string originalLanguage = "en";

        /// <summary>
        /// A list of TranslationSets, each containing translations for a specific language.
        /// </summary>
        [SerializeField]
        public List<TranslationSet> translationSets = new List<TranslationSet>();

        /// <summary>
        /// Returns the translation for the specified line of text and ISO-639 language code, if it exists.
        /// </summary>
        /// <param name="line">The line to translate.</param>
        /// <param name="targetLanguageCode">The ISO-639 language code to translate to.</param>
        /// <returns>The translated line, if found; otherwise null.</returns>
        public Translation GetTranslation(string line, string targetLanguageCode)
        {
            if(translationSets.Count == 0) { return null; }

            TranslationSet sourceSet = GetOrCreateOriginalTranslationSet();
            Translation lineTranslation = sourceSet.FindTranslation(line);

            TranslationSet targetSet = FindTranslationSetForLanguage(targetLanguageCode);

            if(lineTranslation != null && targetSet != null)
            {
                Translation translation = targetSet.GetTranslation(lineTranslation.id);
                if(translation != null) { return translation; }
            }

            Debug.LogWarning($"Line '{line}' is missing translation to {targetLanguageCode}");
            return lineTranslation;
        }

        /// <summary>
        /// Retrieves or creates a translation set for the default/original language of the library.
        /// </summary>
        /// <returns>A TranslationSet for the default/original language of the library.</returns>
        public TranslationSet GetOrCreateOriginalTranslationSet()
        {
            TranslationSet sourceSet = FindTranslationSetForLanguage(originalLanguage);

            if (sourceSet == null)
            {
                sourceSet = new TranslationSet();
                sourceSet.languageCode = originalLanguage;
                translationSets.Insert(0, sourceSet);
            }

            return sourceSet;
        }

        /// <summary>
        /// Returns true if the library contains a TranslationSet for the ISO-639 language code provided.
        /// </summary>
        /// <param name="languageCode">The ISO-639 language code to check for a translation set.</param>
        /// <returns>True if a TranslationSet is found for the specified ISO-639 language code.</returns>
        public bool HasTranslationSet(string languageCode)
        {
            foreach(TranslationSet translationSet in translationSets)
            {
                if(translationSet.languageCode.Equals(languageCode))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Add a TranslationSet for the ISO-639 language code specified if it doesn't exist in the library.
        /// </summary>
        /// <param name="languageCode">The ISO-639 language code to add a TranslationSet for.</param>
        /// <returns>The TranslationSet for the language code specified.</returns>
        public TranslationSet AddSecondaryTranslationSet(string languageCode)
        {
            TranslationSet translationSet = FindTranslationSetForLanguage(languageCode);

            if (translationSet == null)
            {
                translationSet = new TranslationSet();
                translationSet.languageCode = languageCode;
                translationSets.Add(translationSet);
            }

            TranslationSet originalTranslationSet = FindTranslationSetForLanguage(originalLanguage);
            translationSet.CopyStructure(originalTranslationSet);

            return translationSet;
        }

        /// <summary>
        /// Finds and returns the TranslationSet for the specified ISO-639 language code.
        /// </summary>
        /// <param name="languageCode">The ISO-639 language code to find a TranslationSet for.</param>
        /// <returns>The TranslationSet for the specified language, if it exists; null otherwise.</returns>
        public TranslationSet FindTranslationSetForLanguage(string languageCode)
        {
            foreach (TranslationSet translationSet in translationSets)
            {
                if (translationSet.languageCode.Equals(languageCode))
                {
                    return translationSet;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a List of untranslated Translations (lines) for the specified ISO-639 language code. Any empty entries in the found TranslationSet are assumed to be
        /// untranslated.
        /// </summary>
        /// <param name="languageCode">The ISO-639 language code of the TranslationSet to get untranslated lines for.</param>
        /// <returns>A List of untranslated lines for the language code specified.</returns>
        public List<Translation> GetUntranslatedLines(string languageCode)
        {
            TranslationSet sourceSet = FindTranslationSetForLanguage(originalLanguage);
            TranslationSet targetSet = FindTranslationSetForLanguage(languageCode);
            List<Translation> untranslatedLines = new List<Translation>();

            foreach(Translation sourceTranslation in sourceSet.translations)
            {
                Translation targetTranslation = targetSet.GetTranslation(sourceTranslation.id);

                if (targetTranslation == null || targetTranslation.text == null || targetTranslation.text.Length == 0)
                {
                    untranslatedLines.Add(sourceTranslation);
                }
            }

            return untranslatedLines;
        }

        /// <summary>
        /// Changes the original language of the library to the specified ISO-639 language code and adds a TranslationSet for it if one doesn't exist for that language.
        /// </summary>
        /// <param name="languageCode">The ISO-639 language code to change to.</param>
        public void SetOriginalLanguage(string languageCode)
        {
            if(!this.originalLanguage.Equals(languageCode))
            {
                bool foundSet = false;

                for(int i = 0; i < translationSets.Count; i++) 
                {
                    TranslationSet translationSet = translationSets[i];
                    if(translationSet.languageCode.Equals(languageCode))
                    {
                        translationSets.RemoveAt(i);
                        translationSets.Insert(0, translationSet);
                        foundSet = true;
                        break;
                    }
                }

                if(!foundSet)
                {
                    TranslationSet newSourceSet = new TranslationSet();
                    newSourceSet.languageCode = languageCode;
                    this.translationSets.Insert(0, newSourceSet);
                }

                this.originalLanguage = languageCode;
            }
        }
    }

    /// <summary>
    /// A TranslationSet contains lines of text which have been translated into a particular language.
    /// </summary>
    [Serializable]
    public class TranslationSet
    {
        /// <summary>
        /// The current counter of the translation set.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private int counter = 0;

        /// <summary>
        /// The ISO-639 language code for the translation set.
        /// </summary>
        [SerializeField]
        public string languageCode;

        /// <summary>
        /// A List of translated liens for the translation set.
        /// </summary>
        [SerializeField]
        public List<Translation> translations = new List<Translation>();

        /// <summary>
        /// Whether node tags and TextMeshPro/HTML markup tags should be removed from text prior to putting it into the list of translated lines.
        /// </summary>
        private bool preventTagInclusion = true;

        /// <summary>
        /// Checks the translation set to see if the text provided is in the current list of translations. If it isn't, a new translation entry is added for the text.
        /// </summary>
        /// <param name="text">The text to find/add a translation for.</param>
        /// <returns>The Translation for the translated line.</returns>
        public Translation AddOrFindTranslation(string text)
        {
            string finalText = text;
            if (preventTagInclusion)
            {
                finalText = RemoveAllTags(text);
            }

            Translation translation = FindTranslation(finalText);

            if (translation == null) 
            {
                translation = new Translation(counter, languageCode, finalText);
                this.translations.Add(translation);
                counter++;
            }

            return translation;
        }

        /// <summary>
        /// Attempts to add a new translation for the specified translation ID.
        /// </summary>
        /// <param name="id">The source ID the translation applies to.</param>
        /// <param name="text">The translated text.</param>
        /// <returns>The added Translation, or null if a translation couldn't be added.</returns>
        private Translation TryAddTranslation(int id, string text)
        {
            Translation translation = GetTranslation(id);

            if (translation == null)
            {
                string finalText = text;
                if (preventTagInclusion)
                {
                    finalText = RemoveAllTags(text);
                }

                translation = new Translation(id, languageCode, finalText);
                this.translations.Add(translation);
                if (id > counter)
                {
                    counter = id + 1;
                }

                return translation;
            }
            else
            {
                Debug.LogWarning("A translation already exists for ID:" + id + " Text:'" + text + "', Language = '" + languageCode + "'. " +
                    "Translation for '" + text + "' will not be created.");

                return null;
            }
        }

        /// <summary>
        /// Copies the structure of the provided TranslationSet, adding an entry with an identical ID for each translation.
        /// </summary>
        /// <param name="set">The translation set to copy the structure of.</param>
        public void CopyStructure(TranslationSet set)
        {
            for(int i = 0; i < set.translations.Count; i++)
            {
                Translation translation = set.translations[i];
                bool found = false;

                for(int j = 0; j < translations.Count; j++)
                {
                    if (translations[j].id == translation.id)
                    {
                        found = true;
                        break;
                    }
                }

                if(!found)
                {
                    translations.Add(new Translation(translation.id, languageCode, ""));
                    counter = Math.Max(counter + 1, translation.id + 1);
                }
            }
        }

        /// <summary>
        /// Sets the translation at the specified source ID to the value provided.
        /// </summary>
        /// <param name="id">The source ID the translation applies to.</param>
        /// <param name="text">The translated text to set.</param>
        public void SetTranslation(int id, string text)
        {
            bool translationFound = false;
            for(int i = 0; i < translations.Count; i++)
            {
                Translation translation = translations[i];
                if(translation.id == id)
                {
                    string finalText = text;
                    if (preventTagInclusion)
                    {
                        finalText = RemoveAllTags(text);
                    }

                    translation.text = finalText;
                    translationFound = true;
                    break;
                }
            }

            if(!translationFound)
            {
                TryAddTranslation(id, text);
            }
        }

        /// <summary>
        /// Returns the Translation for the specified ID, if it exists.
        /// </summary>
        /// <param name="id">The ID of the Translation to retrieve.</param>
        /// <returns>The Translation for the specified ID if it exists; null otherwise.</returns>
        public Translation GetTranslation(int id)
        {
            for (int i = 0; i < translations.Count; i++)
            {
                Translation translation = translations[i];
                if (translation.id == id)
                {
                    return translation;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns whether the translation set contains a translation with the text specified.
        /// </summary>
        /// <param name="text">The translation text to check.</param>
        /// <returns>True if the translation set contains the text provided; otherwise returns false.</returns>
        public bool ContainsTranslation(string text)
        {
            string finalText = text;
            if (preventTagInclusion)
            {
                finalText = RemoveAllTags(text);
            }

            foreach (Translation translation in translations)
            {
                if (translation.text.Equals(finalText))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds the Translation object attributed to the text specified, if it exists.
        /// </summary>
        /// <param name="text">The text to find a Translation object for.</param>
        /// <returns>The Translation object which contains the text specified, if it exists; otherwise returns null.</returns>
        public Translation FindTranslation(string text)
        {
            string finalText = text;
            if (preventTagInclusion)
            {
                finalText = RemoveAllTags(text);
            }

            for (int i = 0; i < translations.Count; i++)
            {
                if (translations[i].text.Equals(finalText))
                {
                    return translations[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Removes all node tags and TextMeshPro/HTML markup tags from the string provided.
        /// </summary>
        /// <param name="text">The string to remove tags from.</param>
        /// <returns>A modified version of the original string will all tags removed.</returns>
        public static string RemoveAllTags(string text)
        {
            string newText = RemoveTMPTags(text);
            newText = RemoveNodeTags(newText);
            return newText;
        }

        /// <summary>
        /// Removes TextMeshPro/HTML markup tags from the string provided.
        /// </summary>
        /// <param name="text">The string to remove tags from.</param>
        /// <returns>A modified version of the original string with all TextMeshPro/HTML markup tags removed.</returns>
        public static string RemoveTMPTags(string text)
        {
            return TMPTag.RemoveTags(text);
        }

        /// <summary>
        /// Removes all node tags from the string provided.
        /// </summary>
        /// <param name="text">The string to remove tags from.</param>
        /// <returns>A modified version of the original string with all node tags removed.</returns>
        public static string RemoveNodeTags(string text)
        {
            return NodeTag.RemoveTags(text);
        }

        /// <summary>
        /// Replaces all variable references in the provided string with numeric, indexed variable names. The new variable names are stored as keys in the variableNameMap provided and
        /// the original variable names are stored as the respective values.
        /// </summary>
        /// <param name="text">The text to index variable references on.</param>
        /// <param name="variableNameMap">A Dictionary to insert mappings of indexed variable names to old variable names into.</param>
        /// <returns>A modified version of the original string, with all variable names replaced by indexed variable names.</returns>
        public static string IndexVariableNames(string text, Dictionary<string, string> variableNameMap)
        {
            return NodeVariable.IndexVariablesNames(text, variableNameMap);
        }

        /// <summary>
        /// Replaces all variable references in the provided string with the respective variable names stored as values in the provided Dictionary. Each of the variable 
        /// names referenced in the string must match a key in the variableNameMap in order to be replaced.
        /// </summary>
        /// <param name="text">The string to replace variable names in.</param>
        /// <param name="variableNameMap">A Dictionary with current variable names as keys and replacement variable names as values.</param>
        /// <returns>A modified version of the original string, with all variable names replaced if they were found as keys in the Dictionary.</returns>
        public static string ReplaceVariables(string text, Dictionary<string, string> variableNameMap)
        {
            return NodeVariable.ReplaceVariablesNames(text, variableNameMap);
        }
    }

    /// <summary>
    /// A Translation contains a string which is a translation of text into a specific language, as well as other secondary information about that translation.
    /// </summary>
    [Serializable]
    public class Translation : IComparable<Translation>
    {
        /// <summary>
        /// The ID of the translation. This ID should be the same as the ID for the equivalent translation of other languages in the same library.
        /// </summary>
        [SerializeField]
        public int id;

        /// <summary>
        /// The ISO-639 language code of the translation.
        /// </summary>
        [SerializeField]
        public string language;

        /// <summary>
        /// The text of the translation.
        /// </summary>
        [SerializeField]
        public string text;

        /// <summary>
        /// Creates a new Translation.
        /// </summary>
        public Translation() { }

        /// <summary>
        /// Creates a new Translation with the specified ID and ISO-639 language code.
        /// </summary>
        /// <param name="id">The ID of the Translation.</param>
        /// <param name="languageCode">The ISO-639 language code to use.</param>
        public Translation(int id, string languageCode) : this(languageCode)
        {
            this.id = id;
        }

        /// <summary>
        /// Creates a new Translation with the specified ID, language code, and text.
        /// </summary>
        /// <param name="id">The ID of the Translation.</param>
        /// <param name="languageCode">The ISO-639 language code to use.</param>
        /// <param name="text">The text of the translation.</param>
        public Translation(int id, string languageCode, string text) : this(id, languageCode)
        {
            this.text = text;
        }

        /// <summary>
        /// Creates a new Translation with the specified ISO-639 language code.
        /// </summary>
        /// <param name="languageCode">The ISO-639 language code to use.</param>
        public Translation(string languageCode) : this()
        {
            this.language = languageCode;
        }

        /// <summary>
        /// Compares the Translation to another Translation.
        /// </summary>
        /// <param name="other">The other Translation to compare to.</param>
        /// <returns>The comparison value.</returns>
        public int CompareTo(Translation other)
        {
            if (this.id < other.id)
            {
                return -1;
            }

            return this.text.CompareTo(other.text);
        }
    }
}