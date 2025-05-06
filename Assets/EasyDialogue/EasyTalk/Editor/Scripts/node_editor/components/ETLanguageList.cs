using EasyTalk.Editor.Settings;
using EasyTalk.Localization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Components
{
    public class ETLanguageList : Box
    {
        private List<ETLanguageToggle> languageToggles = new List<ETLanguageToggle>();

        public ETLanguageList()
        {
            CreateLanguageToggles();

            EasyTalkEditorSettings editorSettings = EasyTalkNodeEditor.Instance.EditorSettings;

            if (editorSettings.defaultLocalizationLanguages != null && editorSettings.defaultLocalizationLanguages.Count > 0)
            {
                InitalizeLanguageToggles(editorSettings.defaultLocalizationLanguages);
            }
        }

        public List<string> GetSelectedLanguages()
        {
            //Write lines for each of the toggled on languages.
            List<string> languagesToExport = new List<string>();

            foreach (ETLanguageToggle languageToggle in languageToggles)
            {
                if (languageToggle.IsSelected())
                {
                    languagesToExport.Add(languageToggle.language.LanguageCode);
                }
            }

            return languagesToExport;
        }

        private void CreateLanguageToggles()
        {
            ScrollView scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            scrollView.style.flexGrow = 1;
            scrollView.style.marginBottom = 4.0f;
            scrollView.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);

            foreach (LocalizableLanguage language in EasyTalkNodeEditor.Instance.EditorSettings.localizableLanguages.Languages)
            {
                ETLanguageToggle languageBox = new ETLanguageToggle(language);
                languageToggles.Add(languageBox);
                scrollView.Add(languageBox);
            }

            Add(scrollView);
        }

        private void InitalizeLanguageToggles(List<string> enabledLanguageCodes)
        {
            foreach (string languageCode in enabledLanguageCodes)
            {
                //Check the toggle for the language and add it to the list.
                foreach (ETLanguageToggle languageToggle in languageToggles)
                {
                    if (languageToggle.language.LanguageCode.Equals(languageCode))
                    {
                        languageToggle.toggle.value = true;
                        break;
                    }
                }
            }
        }
    }
}
