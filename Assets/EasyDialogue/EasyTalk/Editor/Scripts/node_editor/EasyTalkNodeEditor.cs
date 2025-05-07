using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading;
using UnityEditor.Callbacks;
using EasyTalk.Editor.Components;
using EasyTalk.Editor.Ledger;
using EasyTalk.Localization;
using EasyTalk.Editor.Settings;
using EasyTalk.Editor.Utils;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes;
using static EasyTalk.Editor.Utils.ETFileManager;
using EasyTalk.Character;

namespace EasyTalk.Editor
{
    public class EasyTalkNodeEditor : EditorWindow
    {
        /// <summary>
        /// The node editor instance.
        /// </summary>
        private static EasyTalkNodeEditor instance;

        /// <summary>
        /// The node view used to display/edit nodes.
        /// </summary>
        private ETNodeView nodeView;

        /// <summary>
        /// The pan zoom panel which contains the node view and provides pan/zoom functionality.
        /// </summary>
        private ETPanZoomPanel panZoomPanel;

        /// <summary>
        /// The undo/redo ledger.
        /// </summary>
        private ETLedger ledger = new ETLedger();

        /// <summary>
        /// The toolbar.
        /// </summary>
        private ETToolbar toolbar;

        /// <summary>
        /// The file manager used to open and save files.
        /// </summary>
        private ETFileManager fileManager = new ETFileManager();

        /// <summary>
        /// The session handler used to keep the node editor state available through domain reloads.
        /// </summary>
        private ETSessionHandler sessionHandler = new ETSessionHandler();

        /// <summary>
        /// The mouse cursor to use.
        /// </summary>
        private MouseCursor cursor = MouseCursor.Arrow;

        /// <summary>
        /// The Drag and Drop mode to display.
        /// </summary>
        private DragAndDropVisualMode dndMode = DragAndDropVisualMode.None;

        /// <summary>
        /// A label used to indicate when the UI or Dialogue asset is loading.
        /// </summary>
        private Label loadingLabel;

        /// <summary>
        /// The find panel used for searching for nodes.
        /// </summary>
        private NodeSearchPanel searchPanel;

        /// <summary>
        /// The settings panel used for modifying more advanced settings for nodes.
        /// </summary>
        private ETSettingsPanel settingsPanel;

        /// <summary>
        /// The localization panel used to create localization files and do translations.
        /// </summary>
        private ETLocalizationCreationPanel localizationPanel;

        /// <summary>
        /// The Translation Library for the current Dialogue asset.
        /// </summary>
        private TranslationLibrary translationLibrary;

        /// <summary>
        /// Various editor settings.
        /// </summary>
        private EasyTalkEditorSettings editorSettings;

        /// <summary>
        /// Delay for reloading the session state after a domain reload occurs (UI needs time to rebuild)
        /// </summary>
        private static int uiReloadSessionDelay = 1000;

        /// <summary>
        /// Delay for loading nodes from a dialogue asset after opening the editor (UI needs time to build)
        /// </summary>
        private static int dialogueLoadDelay = 100;

        /// <summary>
        /// Controls whether unsaved changes can be registered on the node editor.
        /// </summary>
        public bool allowChangeRegistration = false;

        /// <inheritdoc/>
        [MenuItem("Tools/EasyTalk/EasyTalk Node Editor")]
        static void OpenEditor()
        {
            EasyTalkNodeEditor etEditor = GetWindow<EasyTalkNodeEditor>();
            etEditor.position = new Rect(new Vector2(0.0f, 100.0f), new Vector2(800.0f, 600.0f));
            etEditor.titleContent = new GUIContent("Easy Talk");
            etEditor.titleContent.image = Resources.Load<Texture>("images/icons/nodes_title_icon");
            etEditor.saveChangesMessage = "You have unsaved changes to a Dialogue in the EasyTalk Dialogue Node Editor. Would you like to save?";
            instance = etEditor;
        }

        /// <summary>
        /// Gets the node editor instance.
        /// </summary>
        public static EasyTalkNodeEditor Instance { get { return instance; } }

        /// <summary>
        /// 
        /// </summary>
        public void CreateGUI()
        {
            instance = this;
            this.allowChangeRegistration = false;
            this.hasUnsavedChanges = false;
            this.wantsMouseEnterLeaveWindow = true;

            SetCursor(MouseCursor.Arrow);

            this.translationLibrary = ScriptableObject.CreateInstance<TranslationLibrary>();

            LoadSettings();

            toolbar = new ETToolbar();
            rootVisualElement.Add(toolbar);

            nodeView = new ETNodeView();
            panZoomPanel = new ETPanZoomPanel(nodeView);
            rootVisualElement.Add(panZoomPanel);

            localizationPanel = new ETLocalizationCreationPanel();
            localizationPanel.Hide();
            rootVisualElement.Add(localizationPanel);

            CreateLoadingLabel();
            sessionHandler.onSessionStateLoaded += HideLoadingLabel;

            //Wait an extra frame for events to propagate before allowing changes to be registered.
            sessionHandler.onSessionStateLoaded += delegate { EditorApplication.delayCall += delegate { this.allowChangeRegistration = true; }; };

            rootVisualElement.pickingMode = PickingMode.Ignore;
            rootVisualElement.style.flexDirection = FlexDirection.ColumnReverse;
            nodeView.StretchToParentSize();

            searchPanel = new NodeSearchPanel();
            rootVisualElement.Add(searchPanel);

            settingsPanel = new ETSettingsPanel();
            rootVisualElement.Add(settingsPanel);
            settingsPanel.Hide();

            toolbar.BringToFront();

            if(ETSessionHandler.HasSavedSessionData()) 
            {
                StartSessionReloadThread();
            }
            else 
            {
                HideLoadingLabel(); 
            }

            EditorApplication.delayCall += delegate 
            { 
                ChangeLanguage(editorSettings.defaultOriginalLanguage);
                RefreshCharactersdLibraries();
            };
        }

        public void RefreshCharactersdLibraries()
        {
            characterLibraries.Clear();
            string[] characterLibraryAssets = AssetDatabase.FindAssets("t:CharacterLibrary");

            foreach (string guid in characterLibraryAssets)
            {
                string libraryPath = AssetDatabase.GUIDToAssetPath(guid);

                if(libraryPath.Contains("EasyTalk/Demo"))
                {
                    continue; //If you want to use character libraries from the EasyTalk/Demo folder in the node editor, comment out this line.
                }

                CharacterLibrary library = AssetDatabase.LoadAssetAtPath<CharacterLibrary>(libraryPath);
                characterLibraries.Add(library);
                AddCharactersToNodeView(library);
            }
        }

        private List<CharacterLibrary> characterLibraries = new List<CharacterLibrary>();

        /// <summary>
        /// Loads the names for registered characters from the dialogue registry into the node view.
        /// </summary>
        public void AddCharactersToNodeView(CharacterLibrary characterLibrary)
        {
            foreach (string charName in GetUniqueCharacterNameList())
            {
                nodeView.AddCharacter(charName);
            }
        }

        public List<string> GetUniqueCharacterNameList()
        {
            List<CharacterLibrary> charLibs = EasyTalkNodeEditor.Instance.CharacterLibraries;
            List<string> characterNames = new List<string>();

            foreach (CharacterLibrary library in charLibs)
            {
                foreach (CharacterDefinition character in library.Characters)
                {
                    if (!characterNames.Contains(character.CharacterName))
                    {
                        characterNames.Add(character.CharacterName);
                    }
                }
            }

            return characterNames;
        }

        public List<CharacterLibrary> CharacterLibraries
        {
            get { return this.characterLibraries; }
        }

        /// <summary>
        /// Loads the node editor settings to use.
        /// </summary>
        private void LoadSettings()
        {
            editorSettings = Resources.Load<EasyTalkEditorSettings>("settings/EasyTalk Editor Settings");

            foreach (StyleSheet stylesheet in editorSettings.stylesheets)
            {
                rootVisualElement.styleSheets.Add(stylesheet);
            }

            uiReloadSessionDelay = editorSettings.uiReloadSessionDelay;
            dialogueLoadDelay = editorSettings.dialogueLoadDelay;

            fileManager.AutoSaveMode = editorSettings.autosaveMode;
            fileManager.AutoSaveDelayMS = editorSettings.autosaveDelayMs;
        }

        /// <summary>
        /// Starts a thread which reloads the stored session state.
        /// </summary>
        private void StartSessionReloadThread()
        {
            Thread sessionReloadThread = new Thread(() =>
            {
                try
                {
                    Thread.Sleep(uiReloadSessionDelay);
                    EditorApplication.delayCall += ReloadSession;
                }
                catch { }
            });

            sessionReloadThread.Name = "Easy Talk Node Editor Session Reload Thread";
            sessionReloadThread.Start();
        }

        /// <summary>
        /// Hides the loading label.
        /// </summary>
        private void HideLoadingLabel()
        {
            loadingLabel.style.visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Creates a loading label to be shown on top of the node editor when the editor is loading.
        /// </summary>
        private void CreateLoadingLabel()
        {
            loadingLabel = new Label("Loading...");
            loadingLabel.style.top = new StyleLength(new Length(50.0f, LengthUnit.Percent));
            loadingLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            loadingLabel.style.position = Position.Absolute;
            loadingLabel.style.alignSelf = Align.Center;
            loadingLabel.style.fontSize = 42.0f;
            rootVisualElement.Add(loadingLabel);
        }

        /// <summary>
        /// Registers changes on the node editor. If autosave is set to ON_CHANGES, the file will be saved, otherwise the node editor is marked as having unsaved changes.
        /// </summary>
        /// <param name="force">If true, changes will be registered regardless of whether allowChangeRegistration is true.</param>
        public void NodesChanged(bool force = false)
        {
            if(!allowChangeRegistration && !force) { return; }

            if (fileManager.AutoSaveMode == AutosaveMode.ON_CHANGES && toolbar.IsAutosaveEnabled)
            {
                fileManager.SaveFile();
            }
            else
            {
                this.hasUnsavedChanges = true;
                toolbar.SetCurrentFile(fileManager.CurrentFilePath, this.hasUnsavedChanges);
            }
        }

        /// <summary>
        /// Calls Update on the pan/zoom panel every frame.
        /// </summary>
        private void Update()
        {
            if (panZoomPanel != null)
            {
                panZoomPanel.Update();
            }
        }

        /// <summary>
        /// Returns the audio volume set on the node editor.
        /// </summary>
        /// <returns>The audio volume the node editor should use when playing sounds.</returns>
        public float GetAudioVolume()
        {
            return toolbar.GetAudioVolume();
        }

        /// <summary>
        /// Shows the search panel.
        /// </summary>
        public void ShowSearchPanel()
        {
            searchPanel.Show();
            EditorApplication.delayCall += searchPanel.SearchField.Focus;
        }

        /// <summary>
        /// Shows the localization panel.
        /// </summary>
        public void ShowLocalizationPanel()
        {
            localizationPanel.Show();
        }

        /// <summary>
        /// Changes the source language of the current Dialogue to the specified ISO-639 language.
        /// </summary>
        /// <param name="languageCode">The ISO-639 language code to use.</param>
        public void ChangeLanguage(string languageCode)
        {
            if (languageCode != null)
            {
                //Set the language dropdown of the localization panel
                localizationPanel.SetOriginalLanguageDropdown(languageCode);

                LanguageFontOverride fontOverride = editorSettings.languageFontOverrides.GetOverrideForLanguage(languageCode);

                if (fontOverride != null)
                {
                    //Update the fonts and text used in the node editor UI
                    editorSettings.localizedFont = fontOverride.font;
                    EditorUtility.SetDirty(editorSettings);
                    AssetDatabase.SaveAssetIfDirty(editorSettings);

                    AdjustFonts(nodeView, editorSettings.localizedFont);
                }
                else
                {
                    editorSettings.localizedFont = null;
                    EditorUtility.SetDirty(editorSettings);
                    AssetDatabase.SaveAssetIfDirty(editorSettings);

                    AdjustFonts(nodeView, editorSettings.defaultFont);
                }
            }
        }

        /// <summary>
        /// Changes the fonts of the specified Visual Element and all of its children to the specified font.
        /// </summary>
        /// <param name="element">The Visual Element to recursively change.</param>
        /// <param name="font">The font to use.</param>
        private void AdjustFonts(VisualElement element, Font font)
        {
            foreach(VisualElement child in element.Children())
            {
                if(child is Label || child is TextField || child is ETEditableDropdown || child is DropdownField)
                {
                    child.style.unityFont = font;
                    child.style.unityFontDefinition = new StyleFontDefinition(font);
                }

                AdjustFonts(child, font);
            }
        }

        /// <summary>
        /// Sets the mouse cursor to use.
        /// </summary>
        /// <param name="cursor">The cursor to use.</param>
        public void SetCursor(MouseCursor cursor)
        {
            this.cursor = cursor;
        }

        /// <summary>
        /// Gets the node view used to display nodes.
        /// </summary>
        public ETNodeView NodeView { get { return nodeView; } }

        /// <summary>
        /// Gets the pan/zoom panel which provides panning/zooming functionality.
        /// </summary>
        public ETPanZoomPanel PanZoomPanel { get { return panZoomPanel; } }

        /// <summary>
        /// Updates the mouse cursor and handles when the mouse enters or leaves the node editor. Also handles updating the cursor based on the current drag and drop mode.
        /// </summary>
        private void OnGUI()
        {
            EditorGUIUtility.AddCursorRect(new Rect(0, 0, rootVisualElement.contentRect.width, rootVisualElement.contentRect.height), cursor);

            if (Event.current.type == EventType.MouseEnterWindow || Event.current.type == EventType.MouseLeaveWindow)
            {
                if (nodeView != null) { nodeView.WindowEnteredOrExited(); }
            }

            DragAndDrop.visualMode = dndMode;
        }

        /// <summary>
        /// Sets the drag and drop mode to use.
        /// </summary>
        /// <param name="mode">The drag and drop mode to use.</param>
        public void SetDragAndDropMode(DragAndDropVisualMode mode)
        {
            dndMode = mode;
        }

        /// <summary>
        /// Loads the nodes of the provided Dialogue into the node view and sets the current file path to the path specified.
        /// </summary>
        /// <param name="assetPath">The file path of the Dialogue asset.</param>
        /// <param name="dialogue">The Dialogue asset to load into the node editor.</param>
        public void LoadNodesIntoView(string assetPath, Dialogue dialogue)
        {
            if (fileManager.PromptToSaveUnsavedChanges())
            {
                allowChangeRegistration = false;
                ledger.enabled = false;
                ledger.Clear();
                
                NodeView.DeleteAll();

                EditorUtility.DisplayProgressBar("Opening Dialogue", "Loading dialogue from '" + assetPath + "'...", 0.0f);

                //Load character names from the registry into the node view
                RefreshCharactersdLibraries();

                if (dialogue != null)
                {
                    int numLoaded = 0;
                    foreach (Node node in dialogue.Nodes)
                    {
                        EditorUtility.DisplayProgressBar("Opening Dialogue", "Creating " + node.NodeType + " node...", (((float)numLoaded) / dialogue.Nodes.Count));
                        NodeView.CreateNode(node.NodeType, node);
                        numLoaded++;
                    }

                    NodeUtils.SetCurrentID(dialogue.MaxID + 1);
                    fileManager.CurrentFilePath = assetPath;
                }

                SetTranslationLibrary(dialogue);

                NodeView.PanAndZoomToAll();

                EditorApplication.delayCall += delegate
                {
                    SetChangesSaved();
                };

                ledger.enabled = true;
                allowChangeRegistration = true;
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// Sets the Translation Library of the node editor to that of the provided Dialogue. If the Dialogue doesn't have a translation library, this method will create one and assign it to the Dialogue.
        /// </summary>
        /// <param name="dialogue">The Dialogue to use.</param>
        private void SetTranslationLibrary(Dialogue dialogue)
        {
            if (dialogue != null && dialogue.TranslationLibrary != null)
            {
                this.translationLibrary = dialogue.TranslationLibrary;
            }
            else
            {
                this.translationLibrary = ScriptableObject.CreateInstance<TranslationLibrary>();
                dialogue.TranslationLibrary = this.translationLibrary;
            }
        }

        /// <summary>
        /// Saves any unsaved changes to the current Dialogue asset.
        /// </summary>
        public override void SaveChanges()
        {
            if (fileManager != null && NodeView != null)
            {
                fileManager.SaveFile();
                ETSessionHandler.ClearSessionState();
            }
            else
            {
                //Handles edge case when the Unity Editor is closed but there are unsaved changes and the node editor isn't fully loaded.
                ETFileManager.SaveDialogueFromSession();
            }

            base.SaveChanges();
        }

        /// <summary>
        /// Sets the node editor so that it isn't indicating that it has unsaved changes.
        /// </summary>
        public void SetChangesSaved()
        {
            this.hasUnsavedChanges = false;

            if (toolbar != null && fileManager != null)
            {
                toolbar.SetCurrentFile(fileManager.CurrentFilePath, this.hasUnsavedChanges);
            }
        }

        /// <inheritdoc/>
        public override void DiscardChanges()
        {
            ETSessionHandler.ClearSessionState();
            base.DiscardChanges();
        }

        /// <summary>
        /// Opens the specified Dialogue asset in the EasyTalk Node Editor.
        /// </summary>
        /// <param name="instanceID">The instance ID of the Dialogue asset to load.</param>
        /// <returns>Returns true if the dialogue was opened successfully.</returns>
        [OnOpenAsset(1)]
        public static bool OpenDialogueEditor(int instanceID)
        {
            string assetPath = AssetDatabase.GetAssetPath(instanceID);
            Dialogue dialogue = AssetDatabase.LoadAssetAtPath<Dialogue>(assetPath);

            if (dialogue != null)
            {
                EasyTalkNodeEditor editorWindow;

                if (!EditorWindow.HasOpenInstances<EasyTalkNodeEditor>())
                {
                    OpenEditor();
                }

                editorWindow = EditorWindow.GetWindow<EasyTalkNodeEditor>();

                if (EasyTalkNodeEditor.instance != null)
                {
                    if(!EasyTalkNodeEditor.instance.fileManager.PromptToSaveUnsavedChanges())
                    {
                        return false;
                    }
                }

                if (!SaveUnsavedSessionChanges())
                {
                    return false;
                }

                ETSessionHandler.ClearSessionState();
                editorWindow.SetChangesSaved();

                EditorWindow.FocusWindowIfItsOpen<EasyTalkNodeEditor>();
                
                if (editorWindow != null)
                {
                    LoadNodesFromAsset(assetPath, dialogue, editorWindow);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Prompts the user to save unsaved changes which are stored in the session state if they exist, and saves upon confirmation.
        /// </summary>
        /// <returns>Returns true if the user chooses to save or continue. If the user cancels the current operation, this method returns false.</returns>
        private static bool SaveUnsavedSessionChanges()
        {
            if (ETSessionHandler.HasSavedSessionData())
            {
                bool hasUnsavedChanges = SessionState.GetBool("et-has-unsaved-changes", false);

                if (hasUnsavedChanges)
                {
                    string currentFile = SessionState.GetString("et-current-file", null);
                    int saveOption = EditorUtility.DisplayDialogComplex("Save changes?", "The currently open file '" + currentFile + "' has unsaved changed. " +
                        "Would you like to save before opening another file?", "Save", "Cancel", "Continue without Saving");

                    if (saveOption == 0)
                    {
                        ETFileManager.SaveDialogueFromSession();
                    }
                    else if (saveOption == 1)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Loads the nodes of the specified Dialogue into the node view and sets the current file path to the path provided.
        /// </summary>
        /// <param name="assetPath">The working file path of the Dialogue asset.</param>
        /// <param name="dialogue">The Dialogue asset to load.</param>
        /// <param name="editorWindow">The EasyTalkNodeEditor window to use.</param>
        private static void LoadNodesFromAsset(string assetPath, Dialogue dialogue, EasyTalkNodeEditor editorWindow)
        {
            Thread dialogueNodeLoadingThread = new Thread(() =>
            {
                try
                {
                    Thread.Sleep(dialogueLoadDelay);
                    EditorApplication.delayCall += delegate { editorWindow.LoadNodesIntoView(assetPath, dialogue); };
                }
                catch { }
            });

            dialogueNodeLoadingThread.Name = "Dialogue Asset Loading Thread";
            dialogueNodeLoadingThread.Start();
        }

        /// <summary>
        /// Copy the current node selection to the clipboard.
        /// </summary>
        public void Copy()
        {
            NodeView.CopySelection();
        }

        /// <summary>
        /// Paste the current clipboard to the node view.
        /// </summary>
        public void Paste()
        {
            NodeView.PasteClipboard();
        }

        /// <summary>
        /// Delete all selected nodes.
        /// </summary>
        public void DeleteSelected()
        {
            nodeView.DeleteNodes(true);
        }

        /// <summary>
        /// Zoom in on the node view.
        /// </summary>
        public void ZoomIn()
        {
            panZoomPanel.ZoomIn();
        }

        /// <summary>
        /// Zoom out on the node view.
        /// </summary>
        public void ZoomOut()
        {
            panZoomPanel.ZoomOut();
        }

        /// <summary>
        /// Pan and zoom to view all currently selected nodes.
        /// </summary>
        public void ViewSelected()
        {
            NodeView.PanAndZoomToSelected();
        }

        /// <summary>
        /// Pan and zoom to view all nodes.
        /// </summary>
        public void ViewAll()
        {
            NodeView.PanAndZoomToAll();
        }

        /// <summary>
        /// Select all nodes.
        /// </summary>
        public void SelectAll()
        {
            nodeView.SelectAllNodes();
        }

        /// <summary>
        /// Deselect all nodes.
        /// </summary>
        public void DeselectAll()
        {
            nodeView.DeselectAllNodes();
        }

        /// <summary>
        /// Invert the current node selection.
        /// </summary>
        public void InvertSelection()
        {
            nodeView.InvertSelection();
        }

        /// <summary>
        /// Create a new node of the specified type and add it to the view.
        /// </summary>
        /// <param name="nodeType">The type of node to create.</param>
        public void CreateNode(NodeType nodeType)
        {
            nodeView.CreateNode(nodeType);
        }

        /// <summary>
        /// Undo the most recent action.
        /// </summary>
        public void Undo()
        {
            Ledger.Undo(nodeView);
        }

        /// <summary>
        /// Redo the last undone action.
        /// </summary>
        public void Redo()
        {
            Ledger.Redo(nodeView);
        }

        /// <summary>
        /// Set the save changes message.
        /// </summary>
        private void OnEnable()
        {
            this.saveChangesMessage = "You have unsaved changes to a Dialogue in the EasyTalk Dialogue Node Editor. Would you like to save?";
        }

        /// <summary>
        /// Store the current node editor state to the Unity editor session state and stop the autosave thread if it's running.
        /// </summary>
        private void OnDisable()
        {
            if (NodeView != null && loadingLabel.style.visibility == Visibility.Hidden)
            {
                ETSessionHandler.SaveSessionState();
            }

            fileManager.StopAutosaveThread();
        }

        /// <summary>
        /// Clear the stored session state if there is oen.
        /// </summary>
        private void OnDestroy()
        {
            if (NodeView != null)
            {
                ETSessionHandler.ClearSessionState();
            }
        }

        /// <summary>
        /// Reload the stored session state if there is one and restart the autosave thread if set to do so.
        /// </summary>
        private void ReloadSession()
        {
            if (NodeView != null)
            {
                loadingLabel.style.visibility = Visibility.Visible;
                EditorApplication.delayCall += sessionHandler.LoadSessionState;

                if (toolbar.IsAutosaveEnabled && fileManager.AutoSaveMode == AutosaveMode.TIMED)
                {
                    fileManager.StartAutosaveThread();
                }
            }
        }

        /// <summary>
        /// Gets the ledger used to undo/redo actions.
        /// </summary>
        public ETLedger Ledger { get { return ledger; } }

        /// <summary>
        /// Gets the Translation Library currently being used for the working Dialogue asset.
        /// </summary>
        public TranslationLibrary TranslationLibrary 
        { 
            get { return this.translationLibrary; } 
            set { this.translationLibrary = value; }
        }

        /// <summary>
        /// Gets the EasyTalk menu toolbar.
        /// </summary>
        public ETToolbar Toolbar { get { return toolbar; } }

        /// <summary>
        /// Gets the Settings panel for the editor.
        /// </summary>
        public ETSettingsPanel SettingsPanel { get { return settingsPanel; } }

        /// <summary>
        /// Gets the file manager.
        /// </summary>
        public ETFileManager FileManager { get { return fileManager; } }

        /// <summary>
        /// Gets the EasyTalk editor settings being used.
        /// </summary>
        public EasyTalkEditorSettings EditorSettings { get { return this.editorSettings; } }

        /// <summary>
        /// Finds and returns the Visual Element at the specified mouse position.
        /// </summary>
        /// <param name="position">The local node view position to retrieve a component for.</param>
        /// <returns>The Visual Element at the specified position.</returns>
        public VisualElement GetElementAtMouse(Vector3 position)
        {
            return NodeView.panel.Pick(NodeView.ChangeCoordinatesTo(NodeView.parent.parent.parent, position));
        }

        /// <summary>
        /// Retrieves all Visual Elements at the specified mouse position.
        /// </summary>
        /// <param name="position">The local node view position to retrieve a component for.</param>
        /// <returns>A List of all Visual Elements at the specified position.</returns>
        public List<VisualElement> GetElementsAtMouse(Vector3 position)
        {
            List<VisualElement> elements = new List<VisualElement>();
            NodeView.panel.PickAll(NodeView.ChangeCoordinatesTo(NodeView.parent.parent.parent, position), elements);
            return elements;
        }
    }
}