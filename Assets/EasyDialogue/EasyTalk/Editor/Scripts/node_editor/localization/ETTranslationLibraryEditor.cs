using EasyTalk.Localization;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyTalk.Editor.Localization
{
    [CustomEditor(typeof(TranslationLibrary))]
    public class ETTranslationLibraryEditor : UnityEditor.Editor
    {
        private static string languageFilter = "ALL";
        private string searchText = "";

        private static Color colorA = new Color(0.1f, 0.1f, 0.1f);
        private static Color colorB = new Color(0.3f, 0.1f, 0.1f);

        private static Vector2 scrollPos = Vector2.zero;

        private static Translation selectedTranslation;

        public override void OnInspectorGUI()
        {
            TranslationLibrary library = (TranslationLibrary)target;

            if(GUILayout.Button(new GUIContent("Import...")))
            {
                ETLocalizer.ImportLocalizationFile(library);
            }

            if(GUILayout.Button(new GUIContent("Export...")))
            {
                List<string> languagesToExport = new List<string>();
                if(languageFilter.Equals("ALL"))
                {
                    foreach(TranslationSet set in library.translationSets)
                    {
                        languagesToExport.Add(set.languageCode);
                    }
                }
                else
                {
                    languagesToExport.Add(languageFilter.ToLower());
                }
                ETLocalizer.ExportLocalizationFile(library, languagesToExport);
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Origin Language", GUILayout.MaxWidth(100.0f));

            //Create a dropdown for changing the original language.
            if (EditorGUILayout.DropdownButton(new GUIContent(library.originalLanguage.ToUpper()), FocusType.Passive))
            {
                GenericMenu languageDropdown = new GenericMenu();

                foreach (TranslationSet set in library.translationSets)
                {
                    string lang = set.languageCode.ToUpper();
                    languageDropdown.AddItem(new GUIContent(lang), languageFilter.Equals(lang), SetOriginalLanguage, lang);
                }
                languageDropdown.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Filter", GUILayout.MaxWidth(100.0f));

            //Create a language dropdown filter
            if(EditorGUILayout.DropdownButton(new GUIContent(languageFilter), FocusType.Passive))
            {
                GenericMenu languageDropdown = new GenericMenu();
                languageDropdown.AddItem(new GUIContent("ALL"), languageFilter.Equals("ALL"), SetLanguageFilter, "ALL");

                foreach (TranslationSet set in library.translationSets)
                {
                    string lang = set.languageCode.ToUpper();
                    languageDropdown.AddItem(new GUIContent(lang), languageFilter.Equals(lang), SetLanguageFilter, lang);
                }
                languageDropdown.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
            }

            EditorGUILayout.EndHorizontal();

            //Create a search field
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Search"), GUILayout.MaxWidth(60.0f));
            searchText = EditorGUILayout.TextField(searchText);
            EditorGUILayout.EndHorizontal();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            //Create a list of lines
            if(languageFilter.Equals("ALL"))
            {
                int counter = 0;

                foreach(TranslationSet translationSet in library.translationSets)
                {
                    Rect setArea = EditorGUILayout.BeginVertical();
                    EditorGUI.DrawRect(setArea, (counter % 2 == 0) ? colorA : colorB);
                    EditorGUILayout.LabelField(translationSet.languageCode);

                    EditorGUI.indentLevel++;
                    ListTranslationsForSet(translationSet, library);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.EndVertical();
                    counter++;
                }
            }
            else
            {
                TranslationSet translationSet = library.FindTranslationSetForLanguage(languageFilter.ToLower());
                ListTranslationsForSet(translationSet, library);
            }

            EditorGUILayout.EndScrollView();
        }

        private void ListTranslationsForSet(TranslationSet translationSet, TranslationLibrary library)
        {
            for (int i = 0; i < translationSet.translations.Count; i++)
            {
                Translation translation = translationSet.translations[i];

                if (searchText != null && searchText.Length > 0)
                {
                    if (!translation.text.ToLower().Contains(searchText.ToLower()))
                    {
                        continue;
                    }
                }

                Rect itemRect = EditorGUILayout.BeginHorizontal();

                Color textColor = new Color(0.8f, 0.8f, 0.8f);

                if (translationSet.languageCode.Equals(library.originalLanguage))
                {
                    if (GUILayout.Button(new GUIContent("-"), GUILayout.MaxWidth(20.0f)))
                    {
                        //Remove the line from all translation sets.
                        translationSet.translations.RemoveAt(i);

                        //Remove the translation from all other sets.
                        foreach (TranslationSet set in library.translationSets)
                        {
                            if (set != translationSet && set.translations.Count > i)
                            {
                                set.translations.RemoveAt(i);
                            }
                        }

                        EditorUtility.SetDirty(library);
                        i--;
                    }
                }

                EditorGUILayout.LabelField(new GUIContent("" + translation.id), GUILayout.MaxWidth(50.0f));

                if (translationSet.languageCode.Equals(library.originalLanguage))
                {
                    EditorGUI.BeginDisabledGroup(true);
                }

                string translationText = EditorGUILayout.TextField(translation.text);
                if (!translationText.Equals(translation.text))
                {
                    translation.text = translationText;
                    EditorUtility.SetDirty(library);
                }

                if (translationSet.languageCode.Equals(library.originalLanguage))
                {
                    EditorGUI.EndDisabledGroup();
                }

                if (translationSet.languageCode.Equals(library.originalLanguage))
                {
                    if (i > 0)
                    {
                        if (GUILayout.Button(new GUIContent("\u2191"), GUILayout.MaxWidth(20.0f)))
                        {
                            for (int n = 0; n < library.translationSets.Count; n++)
                            {
                                TranslationSet changedSet = library.translationSets[n];
                                Translation changedTranslation = changedSet.translations[i];
                                changedSet.translations.RemoveAt(i);
                                changedSet.translations.Insert(i - 1, changedTranslation);
                            }

                            EditorUtility.SetDirty(library);
                            i--;
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField(new GUIContent(""), GUILayout.MaxWidth(20.0f));
                    }

                    if (i < translationSet.translations.Count - 1)
                    {
                        if (GUILayout.Button(new GUIContent("\u2193"), GUILayout.MaxWidth(20.0f)))
                        {
                            for(int n = 0; n < library.translationSets.Count; n++)
                            {
                                TranslationSet changedSet = library.translationSets[n];
                                Translation changedTranslation = changedSet.translations[i];
                                changedSet.translations.RemoveAt(i);
                                changedSet.translations.Insert(i + 1, changedTranslation);
                            }

                            EditorUtility.SetDirty(library);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField(new GUIContent(""), GUILayout.MaxWidth(20.0f));
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void SetLanguageFilter(object language)
        {
            languageFilter = language.ToString();
        }

        private void SetOriginalLanguage(object language)
        {
            TranslationLibrary library = (TranslationLibrary)target;
            library.SetOriginalLanguage(language.ToString().ToLower());

            EditorUtility.SetDirty(library);
        }
    }

    public class TranslationLineDisplayRect
    {
        public Rect rect;
        public Translation translation;

        public TranslationLineDisplayRect(Rect rect, Translation translation)
        {
            this.rect = rect;
            this.translation = translation;
        }
    }
}
