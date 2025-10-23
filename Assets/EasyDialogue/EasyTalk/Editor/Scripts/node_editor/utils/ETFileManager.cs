using EasyTalk.Editor.Nodes;
using EasyTalk.Localization;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using System;
using System.IO;

namespace EasyTalk.Editor.Utils
{
    public class ETFileManager
    {
        private string currentFilePath = string.Empty;

        private AutosaveMode autosaveMode = AutosaveMode.NONE;

        private Thread autosaveThread;

        private int autosaveDelayMs = 30000;

        private bool preventSaving = false;

        /// <summary>
        /// Creates a new Dialogue Asset file.
        /// </summary>
        public void NewFile()
        {
            //If there are unsaved changes, prompt the user to save them before continuing. Only continue if the user didn't choose "Cancel".
            if (PromptToSaveUnsavedChanges())
            {
                //Show a new file dialog.
                string newFilePath = EditorUtility.SaveFilePanelInProject("New Dialogue...", "NewDialogue", "asset", "Create a new Dialogue asset.");

                if (newFilePath != null && newFilePath.Length > 0)
                {
                    //Make sure files can't be saved while initializing the new file.
                    preventSaving = true;

                    //Delete all nodes in the current view.
                    EasyTalkNodeEditor.Instance.NodeView.DeleteAll();

                    //Load characters from the registry
                    EasyTalkNodeEditor.Instance.RefreshCharactersdLibraries();

                    //Create a new Entry node.
                    EasyTalkNodeEditor.Instance.NodeView.CreateNode(NodeType.ENTRY);

                    //Clear the session state
                    ETSessionHandler.ClearSessionState();

                    //Create a new Translation Library to use for translations on the new dialogue.
                    EasyTalkNodeEditor.Instance.TranslationLibrary = ScriptableObject.CreateInstance<TranslationLibrary>();

                    //Allow file saving again.
                    preventSaving = false;

                    //Set the current file path to the path of the new Dialogue asset.
                    CurrentFilePath = newFilePath;

                    //Mark the node editor as having no unsaved changes.
                    EasyTalkNodeEditor.Instance.SetChangesSaved();

                    //Save the new Dialogue asset.
                    SaveFile();
                }
            }
        }

        /// <summary>
        /// Prompt the user to open a Dialogue asset and open it in the node editor if they choose a file.
        /// </summary>
        public void OpenFile()
        {
            //Show an "Open Dialogue" prompt.
            string chosenFile = EditorUtility.OpenFilePanel("Open Dialogue", "Assets/", "asset");

            if (chosenFile != null && chosenFile.Length > 0 && chosenFile.Contains("/Assets/"))
            {
                //Remove the part of the path prior to "/Assets/"
                chosenFile = chosenFile.Substring(chosenFile.IndexOf("/Assets/") + 1);

                //Load the Dialogue Asset at the chosen asset path.
                LoadFile(chosenFile);
            }
        }

        /// <summary>
        /// Save the currently open Dialogue asset.
        /// </summary>
        public void SaveFile()
        {
            //If there is no file path set or we are not allowed to save, return.
            if (currentFilePath == null || currentFilePath.Length == 0 || preventSaving) { return; }

            //Prevent saving from happening while trying to save.
            preventSaving = true;

            //Saves the open Dialogue asset to the current file path.
            Dialogue dialogue = SaveOrCreateDialogue(EasyTalkNodeEditor.Instance.NodeView.GetNodes(), CurrentFilePath);

            //Save the Translation Library for the Dialogue asset.
            SaveTranslationLibrary(dialogue, CurrentFilePath);

            //Set the Dialogue asset to be dirty so changes will be saved.
            EditorUtility.SetDirty(dialogue);

            //Set the serialized object to be dirty.
            SerializedObject dialogueObj = new SerializedObject(dialogue);
            dialogueObj.SetIsDifferentCacheDirty();

            //Save the changes.
            AssetDatabase.SaveAssets();

            //Tell the node editor that changes have been saved.
            EasyTalkNodeEditor.Instance.SetChangesSaved();

            //Allow saving again.
            preventSaving = false;
        }

        /// <summary>
        /// Saves or creates a Dialogue asset to the specified file path. The content of the saved Dialogue will be the list of nodes provided.
        /// </summary>
        /// <param name="etNodes">The nodes which are contained in the Dialogue asset to be saved.</param>
        /// <param name="filePath">The path the Dialogue asset is to be saved to.</param>
        /// <returns>The Dialogue asset which was created/saved.</returns>
        public Dialogue SaveOrCreateDialogue(List<ETNode> etNodes, string filePath)
        {
            List<Node> nodes = new List<Node>();

            foreach(ETNode node in etNodes)
            {
                nodes.Add(node.CreateNode());
            }

            return SaveOrCreateDialogue(nodes, filePath);
        }

        /// <summary>
        /// Save the Dialogue stored in the active editor session if there is one.
        /// </summary>
        public static void SaveDialogueFromSession()
        {
            //Determine the stored file location.
            string currentFile = SessionState.GetString("et-current-file", "Assets/TempDialogue.asset");

            //Load the stored Dialogue asset.
            Dialogue dialogue = ETSessionHandler.LoadDialogueForSession();

            //Save the stored Dialogue asset (by creating a copy).
            dialogue = ETFileManager.SaveOrCreateDialogue(dialogue.Nodes, currentFile);

            //Load the stored Translation Library.
            TranslationLibrary translationLib = ETSessionHandler.LoadTranslationLibraryForSession();
            if (translationLib != null)
            {
                //Set the translation library on the Dialogue asset and save it.
                dialogue.TranslationLibrary = translationLib;
                EditorUtility.SetDirty(dialogue);
                ETFileManager.SaveTranslationLibrary(dialogue, currentFile);
            }

            //Set the dialogue and serialized object to be dirty.
            EditorUtility.SetDirty(dialogue);
            SerializedObject dialogueObj = new SerializedObject(dialogue);
            dialogueObj.SetIsDifferentCacheDirty();

            //Save the Dialogue asset.
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Save a Dialogue asset to the specified path, creating a new one if it doesn't exist. The saved Dialogue will be populated with the list of nodes provided.
        /// </summary>
        /// <param name="nodes">The nodes to populate the Dialogue asset with.</param>
        /// <param name="filePath">The file path to save the Dialogue asset to.</param>
        /// <returns>The saved Dialogue asset.</returns>
        public static Dialogue SaveOrCreateDialogue(List<Node> nodes, string filePath)
        {
            //Load the Dialogue asset at the specified file path, if it exists.
            Dialogue dialogue = AssetDatabase.LoadAssetAtPath<Dialogue>(filePath);
            bool fileExists = true;

            if (dialogue == null)
            {
                //The Dialogue doesn't exist, so create a new one.
                fileExists = false;
                dialogue = ScriptableObject.CreateInstance<Dialogue>();
            }

            //Clear the nodes of the Dialogue asset.
            dialogue.Nodes.Clear();

            //Set the max ID of the Dialogue asset.
            dialogue.MaxID = NodeUtils.CurrentID();

            //Populate the Dialogue asset with nodes.
            foreach (Node node in nodes)
            {
                dialogue.Nodes.Add(node);
            }

            EditorUtility.SetDirty(dialogue);

            if (!fileExists)
            {
                //Create a new Dialogue asset at the file location.
                AssetDatabase.CreateAsset(dialogue, filePath);
            }
            else
            {
                //Save the Dialogue asset.
                AssetDatabase.SaveAssetIfDirty(dialogue);
            }

            return dialogue;
        }

        /// <summary>
        /// Saves the Translation library of the Dialogue asset. If the Translation Library of the Dialogue asset is null, this method attempts to set it from the node editor instance prior to saving.
        /// </summary>
        /// <param name="dialogue">The Dialogue asset to save the Translation Library of.</param>
        /// <param name="dialogueAssetPath">The path of the Dialogue asset for which the Translation Library is being saved.</param>
        public static void SaveTranslationLibrary(Dialogue dialogue, string dialogueAssetPath)
        {
            try
            {
                int fileNameIdx = dialogueAssetPath.IndexOf(".asset");
                if (fileNameIdx != -1)
                {
                    if (EasyTalkNodeEditor.Instance != null && EasyTalkNodeEditor.Instance.TranslationLibrary != null)
                    {
                        TranslationLibrary translationLib;
                        float n = 0.0f;
                        if (dialogue.TranslationLibrary != null)
                        {
                            translationLib = dialogue.TranslationLibrary;
                            AssetDatabase.RemoveObjectFromAsset(dialogue.TranslationLibrary);
                        }
                        else
                        {
                            TranslationLibrary newLib = ScriptableObject.Instantiate<TranslationLibrary>(EasyTalkNodeEditor.Instance.TranslationLibrary);
                            newLib.name = "Translation Library";
                            translationLib = newLib;
                        }

                        foreach (ETNode node in EasyTalkNodeEditor.Instance.NodeView.GetNodes())
                        {
                            node.CreateLocalizations(translationLib);
                        }

                        foreach (string language in EasyTalkNodeEditor.Instance.EditorSettings.defaultLocalizationLanguages)
                        {
                            translationLib.AddSecondaryTranslationSet(language);
                        }

                        dialogue.TranslationLibrary = translationLib;
                        AssetDatabase.AddObjectToAsset(translationLib, dialogue);
                        EasyTalkNodeEditor.Instance.TranslationLibrary = translationLib;

                        EditorUtility.SetDirty(dialogue);
                    }

                    AssetDatabase.SaveAssetIfDirty(dialogue);
                }
            } catch(Exception e)
            {
                Debug.LogError("Failed to save translation library for dialogue: " + e);
            }
        }

        /// <summary>
        /// Open a Save As dialog to allow the user to save the current Dialogue asset to a new file location.
        /// </summary>
        public void SaveAs()
        {
            string savePath = EditorUtility.SaveFilePanelInProject("Save Dialogue to File...", "NewDialogue", "asset", "Save the currently open project to a new Dialogue asset.");

            if (savePath != null && savePath.Length > 0)
            {
                //Update the current working file path and save the Dialogue asset.
                CurrentFilePath = savePath;
                SaveFile();
            }
        }

        /// <summary>
        /// Prompt the user to save changes if there are unsaved changes to the current Dialogue asset.
        /// </summary>
        /// <returns>Returns false if the user canceled the operation. True is returned if the user saved or continued witout saving.</returns>
        public bool PromptToSaveUnsavedChanges()
        {
            if (EasyTalkNodeEditor.Instance.hasUnsavedChanges)
            {
                int saveOption = EditorUtility.DisplayDialogComplex("Save changes?", "The currently open file '" + CurrentFilePath + "' has unsaved changed... Would you like to save before opening another file?",
                    "Save", "Cancel", "Continue without Saving");

                if (saveOption == 0) //Chose to save, so save and return true.
                {
                    SaveFile();
                    return true;
                }
                else if (saveOption == 1) //Canceled operation, so return false.
                {
                    return false;
                }
                else //Reset to not have unsaved changes and continue by returning true.
                {
                    EasyTalkNodeEditor.Instance.SetChangesSaved();
                    return true;
                }
            }

            //There are no unsaved changes, so proceed.
            return true;
        }

        /// <summary>
        /// Loads the Dialogue asset at the specified file path.
        /// </summary>
        /// <param name="assetPath">The file path of the Dialogue asset to load into the node editor.</param>
        private void LoadFile(string assetPath)
        {
            //Return if there is no node view to load into.
            if (EasyTalkNodeEditor.Instance.NodeView == null) { return; }

            //Allow the user to save unsaved changes prior to loading the file, then proceed unless the user cancels.
            if (PromptToSaveUnsavedChanges())
            {
                //Prevent changes from being registered while the file is being loaded and prevent saving.
                EasyTalkNodeEditor.Instance.allowChangeRegistration = false;
                preventSaving = true;

                //Delete all nodes in the node view.
                EasyTalkNodeEditor.Instance.NodeView.DeleteAll();

                //Clear the stored session state.
                ETSessionHandler.ClearSessionState();

                //Load the Dialogue asset at the specified file path.
                Dialogue dialogue = AssetDatabase.LoadAssetAtPath<Dialogue>(assetPath);

                //Load the nodes of the loaded Dialogue asset into the node view.
                EasyTalkNodeEditor.Instance.LoadNodesIntoView(assetPath, dialogue);

                //Allow changes to be registered again and allow saving.
                EasyTalkNodeEditor.Instance.allowChangeRegistration = true;
                preventSaving = false;
            }
        }

        /// <summary>
        /// Start an autosave thread which periodically checks for unsaved changes and saves the current Dialogue asset when changes are detected.
        /// </summary>
        public void StartAutosaveThread()
        {
            if (autosaveThread == null)
            {
                autosaveThread = new Thread(() =>
                {
                    while (Thread.CurrentThread.IsAlive)
                    {
                        try
                        {
                            Thread.Sleep(autosaveDelayMs);
                            if (EasyTalkNodeEditor.Instance.hasUnsavedChanges)
                            {
                                EditorApplication.delayCall += SaveFile;
                            }
                        }
                        catch { }
                    }
                });

                autosaveThread.Start();
            }
        }

        /// <summary>
        /// Stops the autosave thread if it's running.
        /// </summary>
        public void StopAutosaveThread()
        {
            if (autosaveThread != null && autosaveThread.IsAlive)
            {
                autosaveThread.Interrupt();
                autosaveThread = null;
            }
        }

        /// <summary>
        /// Export the currently open Dialogue asset in JSON format.
        /// </summary>
        public void ExportJSON()
        {
            Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue>();

            //Populate the Dialogue asset with nodes.
            foreach (ETNode etNode in EasyTalkNodeEditor.Instance.NodeView.GetNodes())
            {
                dialogue.Nodes.Add(etNode.CreateNode());
            }

            //Set the max ID of the Dialogue asset.
            dialogue.MaxID = NodeUtils.CurrentID();

            string json = JsonUtility.ToJson(dialogue, true);

            string fileName = "Dialogue";

            if (currentFilePath.Contains("/") && currentFilePath.Contains("."))
            {
                fileName = currentFilePath.Substring(currentFilePath.LastIndexOf('/') + 1);
                fileName = fileName.Substring(0, fileName.LastIndexOf("."));
            }

            string savePath = EditorUtility.SaveFilePanelInProject("Export JSON...", fileName, "json", "Export the currently open Dialogue to JSON.");

            if (savePath != null && savePath.Length > 0)
            {
                if(File.Exists(savePath))
                {
                    File.Delete(savePath);
                }

                File.WriteAllText(savePath, json);
            }
        }

        /// <summary>
        /// Gets or sets the current working file path for the Dialogue asset being worked on.
        /// </summary>
        public string CurrentFilePath
        {
            get { return currentFilePath; }
            set
            {
                this.currentFilePath = value;
                EasyTalkNodeEditor.Instance.Toolbar.SetCurrentFile(value, EasyTalkNodeEditor.Instance.hasUnsavedChanges);
            }
        }

        /// <summary>
        /// Gets or sets the autosave mode used by the file manager.
        /// </summary>
        public AutosaveMode AutoSaveMode { get { return autosaveMode; } set { autosaveMode = value; } }

        /// <summary>
        /// Gets or sets the autosave delay (in milliseconds).
        /// </summary>
        public int AutoSaveDelayMS { get { return autosaveDelayMs; } set { autosaveDelayMs = value; } }

        /// <summary>
        /// Defines autosave modes which the file manager can use.
        /// </summary>
        public enum AutosaveMode { NONE, TIMED, ON_CHANGES }
    }
}
