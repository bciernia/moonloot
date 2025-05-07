using EasyTalk.Editor.Settings;
using EasyTalk.Localization;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Components
{
    public class ETLanguageToggle : Box
    {
        public LocalizableLanguage language;

        public Toggle toggle;

        public ETLanguageToggle(LocalizableLanguage language) : base()
        {
            this.language = language;

            this.style.flexDirection = FlexDirection.Row;
            this.style.paddingTop = 4.0f;

            toggle = new Toggle();
            toggle.AddToClassList("language-toggle");
            toggle.RegisterValueChangedCallback(ToggleChanged);
            Add(toggle);

            CreateLanguageLabel();
        }

        private void CreateLanguageLabel()
        {
            Label label = new Label(language.EnglishName + " - " + language.NativeName + " (" + language.LanguageCode + ")");
            label.AddToClassList("language-label");

            //Change the font if it's overriden for the current language
            foreach (LanguageFontOverride fontOverride in EasyTalkNodeEditor.Instance.EditorSettings.languageFontOverrides.overrides)
            {
                if (fontOverride.languageCode.Equals(language.LanguageCode))
                {
                    if (fontOverride.font != null)
                    {
                        label.style.unityFont = fontOverride.font;
                        label.style.unityFontDefinition = new StyleFontDefinition(fontOverride.font);
                    }
                }
            }

            Add(label);
        }

        private void ToggleChanged(ChangeEvent<bool> evt)
        {
            EasyTalkEditorSettings editorSettings = EasyTalkNodeEditor.Instance.EditorSettings;

            if (!toggle.value)
            {
                if (editorSettings.defaultLocalizationLanguages.Contains(language.LanguageCode))
                {
                    editorSettings.defaultLocalizationLanguages.Remove(language.LanguageCode);
                    EditorUtility.SetDirty(editorSettings);
                    AssetDatabase.SaveAssetIfDirty(editorSettings);
                }
            }
            else if (!editorSettings.defaultLocalizationLanguages.Contains(language.LanguageCode))
            {
                editorSettings.defaultLocalizationLanguages.Add(language.LanguageCode);
                EditorUtility.SetDirty(editorSettings);
                AssetDatabase.SaveAssetIfDirty(editorSettings);
            }
        }

        public bool IsSelected()
        {
            return toggle.value;
        }
    }
}