using EasyTalk.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EasyTalk.Editor.Localization
{
    public class ETLocalizer
    {
        public static void ImportLocalizationFile(TranslationLibrary library)
        {
            EditorUtility.DisplayProgressBar("Import Localizations...", "Importing localizations...Please wait...", 0.0f);

            string chosenFile = EditorUtility.OpenFilePanel("Open Dialogue", "Assets/", "csv");

            if (chosenFile != null && chosenFile.Length > 0)
            {
                Dictionary<string, List<Translation>> translations = new Dictionary<string, List<Translation>>();
                StreamReader reader = new StreamReader(chosenFile);

                //Skip the first line
                reader.ReadLine();

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    //Parse the line.
                    int idxA = line.IndexOf(',');
                    int index = -1;

                    if (idxA > 0)
                    {
                        string indexString = line.Substring(0, idxA);
                        index = Int32.Parse(indexString);
                    }

                    if (index == -1) { continue; }

                    int idxB = line.IndexOf(',', idxA + 1);
                    string lang = null;
                    if (idxA > -1 && idxB > -1)
                    {
                        lang = line.Substring(idxA + 1, idxB - idxA - 1);
                    }

                    if (lang == null) { continue; }

                    string text = "";
                    if (idxB > -1 && line.Length > idxB + 1)
                    {
                        text = line.Substring(idxB + 1);
                    }

                    if (translations.ContainsKey(lang))
                    {
                        translations[lang].Add(new Translation(index, lang, text));
                    }
                    else
                    {
                        List<Translation> languageTranslations = new List<Translation>();
                        translations.Add(lang, languageTranslations);
                        languageTranslations.Add(new Translation(index, lang, text));
                    }
                }

                reader.Close();

                int languagesProcessed = 0;

                foreach (string lang in translations.Keys)
                {
                    EditorUtility.DisplayProgressBar("Import Localizations...", "Importing localizations for '" + lang + "'...Please wait...",
                        (((float)languagesProcessed) / translations.Keys.Count));

                    List<Translation> langTranslations = translations[lang];
                    TranslationSet translationSet = library.FindTranslationSetForLanguage(lang);
                    if (translationSet == null)
                    {
                        translationSet = library.AddSecondaryTranslationSet(lang);
                    }

                    foreach (Translation translation in langTranslations)
                    {
                        translationSet.SetTranslation(translation.id, translation.text);
                    }

                    languagesProcessed++;
                }
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.SetDirty(library);

            try 
            { 
                AssetDatabase.SaveAssetIfDirty(library); 
            }
            catch(Exception e)
            {
                Debug.LogWarning("Could not save translation library since it isn't in an asset file yet. If localizing from the node editor, " +
                    "you can save the dialogue and you won't see this message." + e.Message);
            }
        }

        public static void ExportLocalizationFile(TranslationLibrary library, List<string> languagesToExport, string filePrefix = "")
        {
            EditorUtility.DisplayProgressBar("Saving Localizations to File...", "Saving localizations...Please wait...", 0.0f);

            string savePath = EditorUtility.SaveFilePanelInProject("Create Localization File...", filePrefix + "_localization", "csv", "Save the currently open project to a new Dialogue asset.");

            if (savePath != null && savePath.Length > 0)
            {
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }

                StreamWriter writer = new StreamWriter(new FileStream(savePath, FileMode.Create));

                //Write the CSV header line.
                writer.WriteLine("id,language,text");

                TranslationSet originalSet = library.FindTranslationSetForLanguage(library.originalLanguage);
                int totalLinesToTranslate = originalSet.translations.Count * languagesToExport.Count;
                int linesProcessed = 0;

                foreach (Translation translation in originalSet.translations)
                {
                    //Write the line for each selected language.
                    foreach (string language in languagesToExport)
                    {
                        EditorUtility.DisplayProgressBar("Saving Localizations to File...", "Writing '" + language +
                            "' localization line for '" + translation.text + "'...Please wait...",
                            (((float)linesProcessed) / totalLinesToTranslate));

                        TranslationSet languageSet = library.FindTranslationSetForLanguage(language);
                        Translation languageTranslation = languageSet.GetTranslation(translation.id);
                        if (languageTranslation == null)
                        {
                            writer.WriteLine(translation.id + "," + language + ",");
                        }
                        else
                        {
                            writer.WriteLine(translation.id + "," + language + "," + languageTranslation.text);
                        }

                        linesProcessed++;
                    }
                }

                writer.Flush();
                writer.Close();
            }

            EditorUtility.ClearProgressBar();
        }
    }
}
