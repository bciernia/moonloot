using EasyTalk.Localization;
using EasyTalk.Nodes.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static EasyTalk.Editor.Utils.ETFileManager;

namespace EasyTalk.Editor.Components
{
    public class ETToolbar : Toolbar
    {
        private Label currentFileLabel;

        private Toggle autosaveToggle;

        private Slider volumeSlider;

        private bool isAutoSaveEnabled = false;

        public ETToolbar() : base()
        {
            Add(BuildFileMenu());
            Add(new ToolbarSpacer());
            Add(BuildViewMenu());
            Add(new ToolbarSpacer());
            Add(BuildEditMenu());
            Add(new ToolbarSpacer());
            Add(BuildSelectionMenu());
            Add(new ToolbarSpacer());
            Add(BuildCreateMenu());
            Add(new ToolbarSpacer());
            Add(BuildLanguageMenu());
            Add(new ToolbarSpacer());
            Add(CreateVolumeSlider());
            Add(new ToolbarSpacer());

            if (EasyTalkNodeEditor.Instance.FileManager.AutoSaveMode != AutosaveMode.NONE)
            {
                Add(CreateAutosaveToggle());
                Add(new ToolbarSpacer());
            }

            Add(CreateFileLabel());
        }

        private ToolbarMenu BuildFileMenu()
        {
            ToolbarMenu fileMenu = new ToolbarMenu();
            fileMenu.Remove(fileMenu.Q(null, "unity-toolbar-menu__arrow"));
            fileMenu.AddToClassList("toolbar-menu");
            fileMenu.text = "File";
            fileMenu.menu.AppendAction("New Dialogue ^n", delegate { EasyTalkNodeEditor.Instance.FileManager.NewFile(); });
            fileMenu.menu.AppendSeparator();
            fileMenu.menu.AppendAction("Open Dialogue ^o", delegate { EasyTalkNodeEditor.Instance.FileManager.OpenFile(); });
            fileMenu.menu.AppendSeparator();
            fileMenu.menu.AppendAction("Save ^s", delegate { EasyTalkNodeEditor.Instance.FileManager.SaveFile(); });
            fileMenu.menu.AppendAction("Save As...", delegate { EasyTalkNodeEditor.Instance.FileManager.SaveAs(); });
            fileMenu.menu.AppendSeparator();
            fileMenu.menu.AppendAction("Export JSON...", delegate { EasyTalkNodeEditor.Instance.FileManager.ExportJSON(); });
            return fileMenu;
        }

        private ToolbarMenu BuildEditMenu()
        {
            ToolbarMenu editMenu = new ToolbarMenu();
            editMenu.Remove(editMenu.Q(null, "unity-toolbar-menu__arrow"));
            editMenu.AddToClassList("toolbar-menu");
            editMenu.text = "Edit";
            editMenu.menu.AppendAction("Undo ^z", delegate { EasyTalkNodeEditor.Instance.Undo(); });
            editMenu.menu.AppendAction("Redo ^y", delegate { EasyTalkNodeEditor.Instance.Redo(); });
            editMenu.menu.AppendSeparator();
            editMenu.menu.AppendAction("Copy ^c", delegate { EasyTalkNodeEditor.Instance.Copy(); });
            editMenu.menu.AppendAction("Paste ^v", delegate { EasyTalkNodeEditor.Instance.Paste(); });
            editMenu.menu.AppendSeparator();
            editMenu.menu.AppendAction("Delete Selected _delete", delegate { EasyTalkNodeEditor.Instance.DeleteSelected(); });
            return editMenu;
        }

        private ToolbarMenu BuildViewMenu()
        {
            ToolbarMenu viewMenu = new ToolbarMenu();
            viewMenu.Remove(viewMenu.Q(null, "unity-toolbar-menu__arrow"));
            viewMenu.AddToClassList("toolbar-menu");
            viewMenu.text = "View";
            viewMenu.menu.AppendAction("Find... ^f", delegate { EasyTalkNodeEditor.Instance.ShowSearchPanel(); });
            viewMenu.menu.AppendSeparator();
            viewMenu.menu.AppendAction("Zoom In ^+", delegate { EasyTalkNodeEditor.Instance.ZoomIn(); });
            viewMenu.menu.AppendAction("Zoom Out ^-", delegate { EasyTalkNodeEditor.Instance.ZoomOut(); });
            viewMenu.menu.AppendSeparator();
            viewMenu.menu.AppendAction("View Selected ^x", delegate { EasyTalkNodeEditor.Instance.ViewSelected(); });
            viewMenu.menu.AppendSeparator();
            viewMenu.menu.AppendAction("View All ^#x", delegate { EasyTalkNodeEditor.Instance.ViewAll(); });
            return viewMenu;
        }

        private ToolbarMenu BuildSelectionMenu()
        {
            ToolbarMenu selectMenu = new ToolbarMenu();
            selectMenu.Remove(selectMenu.Q(null, "unity-toolbar-menu__arrow"));
            selectMenu.AddToClassList("toolbar-menu");
            selectMenu.text = "Select";
            selectMenu.menu.AppendAction("Select All ^a", delegate { EasyTalkNodeEditor.Instance.SelectAll(); });
            selectMenu.menu.AppendAction("Deselect All #a", delegate { EasyTalkNodeEditor.Instance.DeselectAll(); });
            selectMenu.menu.AppendAction("Invert Selection ^i", delegate { EasyTalkNodeEditor.Instance.InvertSelection(); });
            return selectMenu;
        }

        private ToolbarMenu BuildCreateMenu()
        {
            ToolbarMenu createMenu = new ToolbarMenu();
            createMenu.Remove(createMenu.Q(null, "unity-toolbar-menu__arrow"));
            createMenu.AddToClassList("toolbar-menu");
            createMenu.text = "Create";
            createMenu.menu.AppendAction("Common/Conversation Node", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.CONVO); });
            createMenu.menu.AppendAction("Common/Option Node", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.OPTION); });
            createMenu.menu.AppendAction("Common/Option Modifier", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.OPTION_MOD); });
            createMenu.menu.AppendAction("Common/Entry Node", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.ENTRY); });
            createMenu.menu.AppendAction("Common/Exit Node", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.EXIT); });
            createMenu.menu.AppendAction("Common/Story Node", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.STORY); });

            createMenu.menu.AppendAction("Flow/Jump-In", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.JUMPIN); });
            createMenu.menu.AppendAction("Flow/Jump-Out", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.JUMPOUT); });
            createMenu.menu.AppendAction("Flow/Path Select", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.PATH_SELECT); });
            createMenu.menu.AppendAction("Flow/Random Path", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.RANDOM); });
            createMenu.menu.AppendAction("Flow/Sequence", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.SEQUENCE); });
            createMenu.menu.AppendAction("Flow/Pause", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.PAUSE); });
            createMenu.menu.AppendAction("Flow/Wait", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.WAIT); });
            createMenu.menu.AppendAction("Flow/Goto", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.GOTO); });

            createMenu.menu.AppendAction("Logic/Bool Logic", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.BOOL_LOGIC); });
            createMenu.menu.AppendAction("Logic/Build String", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.BUILD_STRING); });
            createMenu.menu.AppendAction("Logic/Math Function", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.MATH); });
            createMenu.menu.AppendAction("Logic/Number Compare", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.NUMBER_COMPARE); });
            createMenu.menu.AppendAction("Logic/String Compare", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.STRING_COMPARE); });
            createMenu.menu.AppendAction("Logic/Value Select", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.VALUE_SELECT); });
            createMenu.menu.AppendAction("Logic/Conditional Value", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.CONDITIONAL_VALUE); });
            createMenu.menu.AppendAction("Logic/Trigger Script", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.TRIGGER); });

            createMenu.menu.AppendAction("Variable/Variable Getter", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.GET_VARIABLE_VALUE); });
            createMenu.menu.AppendAction("Variable/Variable Setter", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.SET_VARIABLE_VALUE); });
            createMenu.menu.AppendAction("Variable/Bool Variable", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.BOOL_VARIABLE); });
            createMenu.menu.AppendAction("Variable/Float Variable", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.FLOAT_VARIABLE); });
            createMenu.menu.AppendAction("Variable/Int Variable", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.INT_VARIABLE); });
            createMenu.menu.AppendAction("Variable/String Variable", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.STRING_VARIABLE); });

            createMenu.menu.AppendAction("Uility/Show", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.SHOW); });
            createMenu.menu.AppendAction("Uility/Hide", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.HIDE); });
            createMenu.menu.AppendAction("Uility/Player Input", delegate { EasyTalkNodeEditor.Instance.CreateNode(NodeType.PLAYER_INPUT); });

            return createMenu;
        }

        private ToolbarMenu BuildLanguageMenu()
        {
            ToolbarMenu languageMenu = new ToolbarMenu();
            languageMenu.Remove(languageMenu.Q(null, "unity-toolbar-menu__arrow"));
            languageMenu.AddToClassList("toolbar-menu");
            languageMenu.text = "Language";

            foreach (LocalizableLanguage language in EasyTalkNodeEditor.Instance.EditorSettings.localizableLanguages.Languages)
            {
                string menuItemText = language.EnglishName + " - " + language.NativeName + " (" + language.LanguageCode + ")";
                languageMenu.menu.AppendAction("Set Writing Language/" + menuItemText, delegate { EasyTalkNodeEditor.Instance.ChangeLanguage(language.LanguageCode); });
            }

            languageMenu.menu.AppendAction("Localization...", delegate { EasyTalkNodeEditor.Instance.ShowLocalizationPanel(); });

            return languageMenu;
        }

        private VisualElement CreateAutosaveToggle()
        {
            autosaveToggle = new Toggle("Autosave?");
            autosaveToggle.AddToClassList("autosave-toggle");
            autosaveToggle.value = EditorPrefs.GetBool("et-autosave", true);
            autosaveToggle.RegisterCallback<ChangeEvent<bool>>(OnAutosaveOptionChanged);
            return autosaveToggle;
        }

        private void OnAutosaveOptionChanged(ChangeEvent<bool> evt)
        {
            EditorPrefs.SetBool("et-autosave", autosaveToggle.value);

            isAutoSaveEnabled = autosaveToggle.value;

            if (EasyTalkNodeEditor.Instance.FileManager.AutoSaveMode == AutosaveMode.TIMED)
            {
                if (IsAutosaveEnabled)
                {
                    EasyTalkNodeEditor.Instance.FileManager.StartAutosaveThread();
                }
                else
                {
                    EasyTalkNodeEditor.Instance.FileManager.StopAutosaveThread();
                }
            }
        }

        private VisualElement CreateFileLabel()
        {
            currentFileLabel = new Label(EasyTalkNodeEditor.Instance.FileManager.CurrentFilePath);
            currentFileLabel.AddToClassList("current-file-status-label");
            return currentFileLabel;
        }

        private VisualElement CreateVolumeSlider()
        {
            volumeSlider = new Slider("Volume: ", 0.0f, 100.0f);
            volumeSlider.name = "volume-slider";
            volumeSlider.value = EditorPrefs.GetFloat("et-volume", 20.0f);
            volumeSlider.RegisterCallback<ChangeEvent<float>>(OnVolumeChanged);
            return volumeSlider;
        }

        public void OnVolumeChanged(ChangeEvent<float> evt)
        {
            EditorPrefs.SetFloat("et-volume", volumeSlider.value);
        }

        public float GetAudioVolume()
        {
            return volumeSlider.value / 100.0f;
        }

        public bool IsAutosaveEnabled
        {
            get { return isAutoSaveEnabled; }
        }

        public void SetCurrentFile(string filePath, bool hasUnsavedChanges)
        {
            if (hasUnsavedChanges)
            {
                currentFileLabel.text = filePath + "* - Unsaved Changes";
                currentFileLabel.style.color = Color.red;
            }
            else
            {
                currentFileLabel.text = filePath;
                currentFileLabel.style.color = Color.green;
            }
        }
    }
}
