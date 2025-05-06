using EasyTalk.Editor.Localization;
using EasyTalk.Editor.Nodes;
using EasyTalk.Editor.Settings;
using EasyTalk.Editor.Utils;
using EasyTalk.Localization;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static EasyTalk.Editor.Utils.ETTranslator;

namespace EasyTalk.Editor.Components
{
    public class ETLocalizationCreationPanel : Box
    {
        private ETTextField projectNameField;

        private ETTextField accessTokenField;

        private ETDropdownField languageDropdown;

        private Button translateButton;

        private ETLanguageList languageList;

        private string accessToken;

        private bool translationComplete = false;
        private float currentProgress = 0.0f;
        private string progressString = "";
        private bool translationCanceled = false;
        private Thread translationThread;

        public ETLocalizationCreationPanel() : base()
        {
            this.AddToClassList("localization-panel");
            EditorUtility.ClearProgressBar();
            //Create original language dropdown
            CreateOriginalLanguageArea();

            //Create area for choosing supported languages
            languageList = new ETLanguageList();
            Add(languageList);

            //Create the panel for translating
            CreateTranslationArea();

            //Create buttons for importing/creating localization files
            CreateLocalizationArea();

            EditorApplication.delayCall += PopulateSavedFieldsFromSettings;
        }

        private void CreateOriginalLanguageArea()
        {
            Box originalLangBox = new Box();
            originalLangBox.style.marginBottom = 4.0f;
            originalLangBox.style.flexShrink = 0;
            originalLangBox.style.flexDirection = FlexDirection.Row;

            Label originalLangLabel = new Label("Original Language: ");
            originalLangLabel.style.alignSelf = Align.Center;
            originalLangBox.Add(originalLangLabel);

            languageDropdown = CreateLanguageDropdown();
            originalLangBox.Add(languageDropdown);

            Button closeButton = new Button();
            closeButton.Add(new Label("x"));
            closeButton.clicked += Hide;
            originalLangBox.Add(closeButton);

            Add(originalLangBox);
            
        }

        private ETDropdownField CreateLanguageDropdown()
        {
            ETDropdownField dropdown = new ETDropdownField();
            dropdown.RegisterValueChangedCallback(SourceLanguageChanged);
            dropdown.style.flexGrow = 1;
            dropdown.style.flexShrink = 1;

            foreach (LocalizableLanguage language in EasyTalkNodeEditor.Instance.EditorSettings.localizableLanguages.Languages)
            {
                dropdown.choices.Add(language.EnglishName + " - " + language.NativeName + " (" + language.LanguageCode + ")");
            }

            dropdown.choices.Sort();
            return dropdown;
        }

        private void CreateTranslationArea()
        {
            CreateAccessTokenBox();

            Box projectBox = new Box();
            projectBox.style.flexDirection = FlexDirection.Row;
            projectBox.style.marginBottom = 4.0f;
            projectBox.style.flexShrink = 0;

            projectNameField = CreateProjectNameField();
            projectBox.Add(projectNameField);

            translateButton = CreateTranslateButton();
            projectBox.Add(translateButton);

            Add(projectBox);
        }

        private ETTextField CreateProjectNameField()
        {
            ETTextField projectNameField = new ETTextField("Enter Google Project Name...");
            projectNameField.NotifyOfChanges = false;
            projectNameField.PublishUndoableActions = false;
            projectNameField.style.flexGrow = 1;
            projectNameField.style.flexShrink = 1;
            projectNameField.style.alignSelf = Align.Center;
            projectNameField.RegisterCallback<FocusOutEvent>(ProjectNameChanged);
            projectNameField.RegisterCallback<InputEvent>(delegate { EnableOrDisableTranslateButton(); });

            return projectNameField;
        }

        private Button CreateTranslateButton()
        {
            Button translateButton = new Button();
            translateButton.SetEnabled(false);
            translateButton.style.justifyContent = Justify.Center;
            translateButton.clicked += StartTranslation;
            translateButton.Add(new Label("Translate"));

            return translateButton;
        }

        private void CreateAccessTokenBox()
        {
            Box serviceKeyBox = new Box();
            serviceKeyBox.style.flexDirection = FlexDirection.Row;
            serviceKeyBox.style.flexShrink = 0;
            serviceKeyBox.style.marginBottom = 4.0f;

            CreateAccessTokenField();
            serviceKeyBox.Add(accessTokenField);

            Add(serviceKeyBox);
        }

        private void CreateAccessTokenField()
        {
            accessTokenField = new ETTextField("Enter Access Token...");
            accessTokenField.NotifyOfChanges = false;
            accessTokenField.PublishUndoableActions = false;
            accessTokenField.style.flexGrow = 1;
            accessTokenField.style.flexShrink = 1;
            accessTokenField.style.alignSelf = Align.Center;
            accessTokenField.RegisterCallback<FocusOutEvent>(AccessTokenChanged);
            accessTokenField.RegisterCallback<InputEvent>(delegate { EnableOrDisableTranslateButton(); });
        }

        private void EnableOrDisableTranslateButton()
        {
            string sourceLangCode = GetSourceLanguageCode();

            if (projectNameField.value != null && projectNameField.value.Length > 0 &&
                 sourceLangCode != null && sourceLangCode.Length > 0)
            {
                translateButton.SetEnabled(true);
            }
            else
            {
                translateButton.SetEnabled(false);
            }
        }

        private void CreateLocalizationArea()
        {
            Button createLocalizationFileButton = new Button();
            createLocalizationFileButton.style.flexShrink = 0;
            createLocalizationFileButton.clicked += CreateLocalizationFile;
            createLocalizationFileButton.Add(new Label("Create Localization File..."));
            Add(createLocalizationFileButton);

            Button importLocalizationButton = new Button();
            importLocalizationButton.style.flexShrink = 0;
            importLocalizationButton.clicked += ImportLocalizationFile;
            importLocalizationButton.Add(new Label("Import Localizations..."));
            Add(importLocalizationButton);
        }

        private void CreateLocalizationFile()
        {
            TranslationLibrary library = EasyTalkNodeEditor.Instance.TranslationLibrary;

            //Populate the translation library with the current states of the nodes.
            CreateLocalizationsForNodes(library);

            List<string> languages = languageList.GetSelectedLanguages();
            AddTranslationSetsForLanguages(library, languages);

            //Export the localizations.
            ETLocalizer.ExportLocalizationFile(library, languages, GetPreferredLocalizationFilePrefix());
        }

        private string GetPreferredLocalizationFilePrefix()
        {
            //Find a file prefix based on the currently open dialogue file.
            string fileName = "";
            string filePath = EasyTalkNodeEditor.Instance.FileManager.CurrentFilePath;

            if (filePath != null && filePath.Length > 0)
            {
                int startIdx = filePath.LastIndexOf('/') + 1;
                if (startIdx >= 0) { filePath = filePath.Substring(startIdx); }

                if (filePath.Contains(".")) { filePath = filePath.Substring(0, filePath.IndexOf('.')); }

                fileName = filePath;
            }

            return fileName;
        }

        private void ImportLocalizationFile()
        {
            ETLocalizer.ImportLocalizationFile(EasyTalkNodeEditor.Instance.TranslationLibrary);
            EasyTalkNodeEditor.Instance.NodesChanged();
        }

        private void AddTranslationSetsForLanguages(TranslationLibrary library, List<string> languages)
        {
            foreach(string language in languages) 
            {
                library.AddSecondaryTranslationSet(language);
            }
        }

        private void AccessTokenChanged(FocusOutEvent evt)
        {
            EnableOrDisableTranslateButton();
        }

        private void ProjectNameChanged(FocusOutEvent evt) 
        {
            EasyTalkEditorSettings editorSettings = EasyTalkNodeEditor.Instance.EditorSettings;

            if (editorSettings.googleTranslateProjectId == null || !editorSettings.googleTranslateProjectId.Equals(projectNameField.value))
            {
                editorSettings.googleTranslateProjectId = projectNameField.value;
                AssetDatabase.SaveAssetIfDirty(editorSettings);
            }

            EnableOrDisableTranslateButton();
        }

        private void SourceLanguageChanged(ChangeEvent<string> evt)
        {
            EasyTalkEditorSettings editorSettings = EasyTalkNodeEditor.Instance.EditorSettings;

            string sourceLangCode = GetSourceLanguageCode();
            TranslationLibrary library = EasyTalkNodeEditor.Instance.TranslationLibrary;
            if(library != null && sourceLangCode != null)
            {
                library.SetOriginalLanguage(sourceLangCode);
            }

            if (editorSettings.defaultOriginalLanguage == null || !editorSettings.defaultOriginalLanguage.Equals(sourceLangCode))
            {
                editorSettings.defaultOriginalLanguage = sourceLangCode;
                EditorUtility.SetDirty(editorSettings);
                AssetDatabase.SaveAssetIfDirty(editorSettings);
            }

            EnableOrDisableTranslateButton();
        }

        private string GetSourceLanguageCode()
        {
            try
            {
                string originalLanguage = languageDropdown.value;
                int startIdx = originalLanguage.LastIndexOf('(') + 1;
                int endIdx = originalLanguage.LastIndexOf(')') - startIdx;
                string langCode = originalLanguage.Substring(startIdx, endIdx);
                return langCode;
            } catch
            {
                Debug.LogWarning("There was an issue retrieving the selected language from the localization dropdown in the EasyTalk Node Editor. " +
                    "Make sure the default language is set correctly by going to Language -> Set Writing Language -> (Choose the language you are writing in). " +
                    "For now, English will be used...");
                SetOriginalLanguageDropdown("en");
                return "en";
            }
        }

        public void SetOriginalLanguageDropdown(string languageCode)
        {
            for (int i = 0; i < languageDropdown.choices.Count; i++)
            {
                string choice = languageDropdown.choices[i];

                if (choice.Contains("(" + languageCode + ")"))
                {
                    languageDropdown.value = choice;
                    break;
                }
            }
        }

        private void PopulateSavedFieldsFromSettings()
        {
            EasyTalkEditorSettings editorSettings = EasyTalkNodeEditor.Instance.EditorSettings;

            if (editorSettings.defaultOriginalLanguage != null && editorSettings.defaultOriginalLanguage.Length > 0)
            {
                //Set the original language dropdown.
                SetOriginalLanguageDropdown(editorSettings.defaultOriginalLanguage);
            }
            else
            {
                SetOriginalLanguageDropdown("en");
            }

            if (editorSettings.googleTranslateProjectId != null && editorSettings.googleTranslateProjectId.Length > 0)
            {
                projectNameField.value = editorSettings.googleTranslateProjectId;
            }

            EnableOrDisableTranslateButton();
        }

        private void CreateLocalizationsForNodes(TranslationLibrary library)
        {
            List<ETNode> nodes = EasyTalkNodeEditor.Instance.NodeView.GetNodes();
            foreach (ETNode node in nodes)
            {
                SetProgress("Creating localizations for " + node.GetType() + "...", currentProgress);
                node.CreateLocalizations(library);
            }
        }

        private void StartTranslation()
        {
            //Retreive the Google access token
            accessToken = GetAccessToken();

            //Create a thread to monitor the translation process
            Thread monitorThread = CreateTranslationMonitorThread();

            //Create a thread to handle the translations
            translationThread = CreateTranslationThread();

            //Start the monitor and translation threads
            monitorThread.Start();
            translationThread.Start();
        }

        private Thread CreateTranslationMonitorThread()
        {
            Thread translationMonitor = new Thread(() =>
            {
                //Reset the flags which indicate the current status of the translation operation
                translationCanceled = false;
                translationComplete = false;

                //Wait for the translation process to be canceled or completed.
                while (!translationCanceled && !translationComplete)
                {
                    EditorApplication.delayCall += delegate {
                        translationCanceled = EditorUtility.DisplayCancelableProgressBar("Translating Dialogue...",
                            progressString, currentProgress);
                    };

                    Thread.Sleep(1000);
                }

                //If the translation process was canceled, stop the translation thread.
                if (translationCanceled)
                {
                    translationThread.Interrupt();
                    translationThread.Abort();
                }

                //Re-enable the translation button and clear the progress bar.
                EditorApplication.delayCall += delegate
                {
                    EditorUtility.ClearProgressBar();
                    translateButton.SetEnabled(true);
                };
            });

            translationMonitor.Name = "Dialogue Translation Process Monitor Thread";
            return translationMonitor;
        }

        private Thread CreateTranslationThread()
        {
            Thread translationThread = new Thread(() =>
            {
                EditorApplication.delayCall += delegate { translateButton.SetEnabled(false); };

                Translate();

                EditorApplication.delayCall += delegate
                {
                    EditorUtility.ClearProgressBar();
                    EasyTalkNodeEditor.Instance.NodesChanged();
                    translateButton.SetEnabled(true);
                };
            });

            translationThread.Name = "Dialogue Translation Thread";
            return translationThread;
        }

        private string GetAccessToken()
        {
            //Try to get a Google Access Token string by calling the 'gcloud' client from the command line/terminal.
            if(accessTokenField.value == null || accessTokenField.value.Length == 0)
            {
                string accessToken = ETTranslator.GetGoogleAccessToken();
                if(accessToken != null && accessToken.Length > 0)
                {
                    return accessToken;
                }
            }

            //Return whatever value is in the field.
            return accessTokenField.value;
        }

        private void Translate()
        {
            translationComplete = false;
            SetProgress("Creating localizations...", currentProgress);

            //Try to get the access token used for Google Translate API
            if(accessToken == null || accessToken.Length == 0)
            {
                Debug.LogWarning("Cannot translate. No access token was provided or could be obtained.");
                translationComplete = true;
                return;
            }

            //Get all of the lines of text from the dialogue asset.
            TranslationLibrary library = EasyTalkNodeEditor.Instance.TranslationLibrary;
            CreateLocalizationsForNodes(library);

            SetProgress("Finding untranslated lines...", 0.2f);

            //Do the actual translation
            Translate(library, accessToken);

            SetProgress("Translation complete.", 1.0f);

            translationComplete = true;
        }

        private void Translate(TranslationLibrary library, string accessToken)
        {
            SetProgress("Batch processing translations...", 0.3f);
            string sourceLangCode = GetSourceLanguageCode();

            //Create a List for each language we're translating to, based on what lines have not already been translated.
            int totalLines;
            int totalTranslatedLines = 0;
            Dictionary<string, List<Translation>> untranslatedLinesMap = GetUntranslatedLines(library, out totalLines);

            if (totalLines == 0)
            {
                Debug.Log("There was no text found which needs to be translated.");
                SetProgress("No lines need to be translated.", 1.0f);
            }
            else
            {
                SetProgress("Translating lines..." + totalLines, currentProgress);

                //Create batches of 100 lines and retrieve the translations.
                foreach (string langCode in untranslatedLinesMap.Keys)
                {
                    TranslationSet translationSet = library.FindTranslationSetForLanguage(langCode);
                    List<Translation> neededTranslations = untranslatedLinesMap[langCode];
                    int idx = 0;

                    SetProgress("Needed translations: " + neededTranslations.Count, currentProgress);

                    while (idx < neededTranslations.Count)
                    {
                        SetProgress("Translating from '" + sourceLangCode + "' to '" + langCode + "' (" + ((idx / 100) + 1) + "/" +
                            ((neededTranslations.Count / 100) + 1) + ")...", currentProgress);

                        List<string> linesToTranslate = new List<string>();
                        int translationIdx = idx;
                        int batchCount = 0;

                        Dictionary<int, Dictionary<string, string>> replacedVariables = new Dictionary<int, Dictionary<string, string>>();

                        while (batchCount < 100 && idx < neededTranslations.Count)
                        {
                            //Convert the variables names to numbers so that they won't be translated if they include words
                            Dictionary<string, string> replacedVariableNames = new Dictionary<string, string>();
                            string modifiedText = TranslationSet.IndexVariableNames(neededTranslations[idx].text, replacedVariableNames);

                            replacedVariables.Add(batchCount, replacedVariableNames);

                            if (modifiedText != null && modifiedText.Length > 0)
                            {
                                linesToTranslate.Add(modifiedText);
                            }
                            else
                            {
                                linesToTranslate.Add("N/A");
                            }

                            batchCount++;
                            idx++;
                        }

                        GoogleTranslationResult result = ETTranslator.Translate(projectNameField.value, accessToken, sourceLangCode, langCode, linesToTranslate);
                        if (result != null)
                        {
                            batchCount = 0;
                            foreach (GoogleTranslation translation in result.translations)
                            {
                                SetProgress("Translating from '" + library.originalLanguage + "' to '" + langCode + "' Please wait...", ((float)totalTranslatedLines) / totalLines);

                                //Replace the modified variables names back with the original names.
                                string finalText = TranslationSet.ReplaceVariables(translation.translatedText, replacedVariables[batchCount]);
                                translationSet.SetTranslation(neededTranslations[translationIdx].id, finalText);
                                totalTranslatedLines++;
                                translationIdx++;
                                batchCount++;
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Translation could not be completed for " + langCode);
                        }
                    }
                }
            }
        }

        private Dictionary<string, List<Translation>> GetUntranslatedLines(TranslationLibrary library, out int totalLines)
        {
            Dictionary<string, List<Translation>> untranslatedLinesMap = new Dictionary<string, List<Translation>>();
            totalLines = 0;

            foreach (string language in languageList.GetSelectedLanguages())
            {
                if (!library.HasTranslationSet(language))
                {
                    library.AddSecondaryTranslationSet(language);
                }

                SetProgress("Getting untranslated lines for " + language, currentProgress);
                List<Translation> untranslatedLines = library.GetUntranslatedLines(language);
                untranslatedLinesMap.Add(language, untranslatedLines);
                totalLines++;
            }

            SetProgress("Finished getting untranslated lines...", 0.35f);

            return untranslatedLinesMap;
        }

        private void SetProgress(string progressString, float progress)
        {
            this.progressString = progressString;
            this.currentProgress = progress;
        }

        public void Hide()
        {
            this.style.visibility = Visibility.Hidden;
        }

        public void Show()
        {
            this.style.visibility = Visibility.Visible;
        }
    }
}
