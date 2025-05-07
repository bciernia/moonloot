using EasyTalk.Editor.Components;
using EasyTalk.Editor.Nodes;
using EasyTalk.Localization;
using EasyTalk.Nodes;
using System;
using UnityEditor;
using UnityEngine;
using EasyTalk.Nodes.Core;

namespace EasyTalk.Editor.Utils
{
    public class ETSessionHandler
    {
        public delegate void OnSessionStateLoaded();
        public OnSessionStateLoaded onSessionStateLoaded;

        /// <summary>
        /// Saves the current settings, including the Dialogue state and nodes, view settings (zoom level, pan amount etc.), Translation Library, and 
        /// file settings to the editor session state.
        /// </summary>
        public static void SaveSessionState()
        {
            EasyTalkNodeEditor editor = EasyTalkNodeEditor.Instance;
            ETNodeView nodeView = editor.NodeView;

            if (nodeView.GetNodes().Count > 0)
            {
                SaveDialogueToSession();
            }

            SaveViewToSession();
            SaveFileStateToSession();
            SaveTranslationLibraryToSession();
        }

        /// <summary>
        /// Save the current Translation Library to the editor session state.
        /// </summary>
        private static void SaveTranslationLibraryToSession()
        {
            EasyTalkNodeEditor editor = EasyTalkNodeEditor.Instance;

            if (editor != null)
            {
                //Serialize the Translation Library to JSON.
                string translationsJson = JsonUtility.ToJson(editor.TranslationLibrary);

                //Store the JSON in the session state.
                SessionState.SetString("et-translations", translationsJson);
            }
        }

        /// <summary>
        /// Saves the current file state (file location and whether there are unsaved changes) to the editor session state.
        /// </summary>
        private static void SaveFileStateToSession()
        {
            EasyTalkNodeEditor editor = EasyTalkNodeEditor.Instance;

            if (editor != null)
            {
                //Save the current file path.
                SessionState.SetString("et-current-file", editor.FileManager.CurrentFilePath);

                //Save whether there are unsaved changes.
                SessionState.SetBool("et-has-unsaved-changes", editor.hasUnsavedChanges);
            }
        }

        /// <summary>
        /// Save the current view settings of the pan zoom panel and node view to the editor session state.
        /// </summary>
        private static void SaveViewToSession()
        {
            EasyTalkNodeEditor editor = EasyTalkNodeEditor.Instance;
            ETNodeView nodeView = editor.NodeView;
            ETPanZoomPanel panZoomPanel = editor.PanZoomPanel;

            //Save the node view position.
            SessionState.SetVector3("et-view-pos", nodeView.transform.position);

            //Save the pan amount.
            Vector3 panAmount = panZoomPanel.PanAmount;
            SessionState.SetVector3("et-pan-amount", panAmount);

            //Save the zoom level.
            float zoomAmount = panZoomPanel.Zoom;
            SessionState.SetFloat("et-zoom", zoomAmount);
        }

        /// <summary>
        /// Saves the current Dialogue (or nodes) to the editor session state.
        /// </summary>
        private static void SaveDialogueToSession()
        {
            EasyTalkNodeEditor editor = EasyTalkNodeEditor.Instance;
            ETNodeView nodeView = editor.NodeView;

            //Create a new Dialogue object and add all of the nodes from the node view to it.
            Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue>();
            foreach (ETNode nmNode in nodeView.GetNodes())
            {
                dialogue.Nodes.Add(nmNode.CreateNode());
            }

            //Serialize the Dialogue to JSON.
            string json = JsonUtility.ToJson(dialogue);

            //Store nodes, view position, pan, and zoom
            SessionState.SetString("et-nodes", json);
            SessionState.SetInt("current-id", NodeUtils.CurrentID());
        }

        /// <summary>
        /// Loads the stored session state for the active editor session and updates the node editor based on that stored state.
        /// </summary>
        public void LoadSessionState()
        {
            EasyTalkNodeEditor editor = EasyTalkNodeEditor.Instance;
            ETNodeView nodeView = editor.NodeView;
            ETPanZoomPanel panZoomPanel = editor.PanZoomPanel;

            //Disable the ledger so that update events aren't registered when rebuilding the view.
            editor.Ledger.enabled = false;

            //Load the stored Dialogue asset.
            Dialogue dialogue = LoadDialogueForSession();

            if (dialogue != null)
            {
                //Display the retrieved Dialogue asset.
                PopulateNodeView(dialogue);

                //Load the stored file location and state and update the current file location to match.
                LoadFileStateForSession();

                //Load the stored view settings and update the pan zoom panel and node view.
                LoadViewSettingsFromSession();
            }

            //Load the stored translation library.
            LoadTranslationLibraryForSession();

            //Repaint the views.
            nodeView.MarkDirtyRepaint();
            panZoomPanel.RepaintBackgroundGrid();

            //Enable the ledger so that undo events can be registered.
            editor.Ledger.enabled = true;

            //Delete the stored session state since we no longer need it.
            ClearSessionState();

            //Invoke any session state load listeners.
            if (onSessionStateLoaded != null)
            {
                onSessionStateLoaded.Invoke();
            }
        }

        /// <summary>
        /// Removes all nodes from the node view and replaces them with the nodes of the specified Dialogue.
        /// </summary>
        /// <param name="dialogue">The Dialogue to present in the node view.</param>
        private static void PopulateNodeView(Dialogue dialogue)
        {
            EasyTalkNodeEditor editor = EasyTalkNodeEditor.Instance;

            if (editor != null)
            {
                ETNodeView nodeView = editor.NodeView;

                if (dialogue != null && nodeView != null)
                {
                    //Delete any nodes currently in the node view.
                    nodeView.DeleteAll();

                    //Load characters from the registry
                    EasyTalkNodeEditor.Instance.RefreshCharactersdLibraries();

                    //Add all of the nodes of the Dialogue into the node view and update their positions.
                    foreach (Node node in dialogue.Nodes)
                    {
                        ETNode nmNode = nodeView.CreateNode(node.NodeType, node);
                        nmNode.transform.position = new Vector3(node.XPosition, node.YPosition);
                    }
                }
            }
        }

        /// <summary>
        /// Loads previously stored settings for the pan zoom panel and node view and restores them.
        /// </summary>
        private static void LoadViewSettingsFromSession()
        {
            EasyTalkNodeEditor editor = EasyTalkNodeEditor.Instance;
            ETNodeView nodeView = editor.NodeView;
            ETPanZoomPanel panZoomPanel = editor.PanZoomPanel;

            if (nodeView != null && panZoomPanel != null)
            {
                //Load the view position, pan amount, and zoom level.
                Vector3 viewPos = SessionState.GetVector3("et-view-pos", Vector3.zero);
                Vector3 panAmount = SessionState.GetVector3("et-pan-amount", Vector3.zero);
                float zoomAmount = SessionState.GetFloat("et-zoom", 1.0f);

                if (panAmount == Vector3.zero && zoomAmount == 1.0f)
                {
                    //Pan and zoom to all nodes.
                    nodeView.PanAndZoomToAll();
                }
                else
                {
                    //Adjust view settings.
                    panZoomPanel.Zoom = zoomAmount;
                    panZoomPanel.PanAmount = panAmount;
                    nodeView.transform.position = viewPos;
                    nodeView.transform.scale = new Vector3(zoomAmount, zoomAmount, zoomAmount);
                    panZoomPanel.MarkDirtyRepaint();
                }
            }
        }

        /// <summary>
        /// Loads the settings for the open Dialogue file path of the active editor session, if one is stored.
        /// </summary>
        public static void LoadFileStateForSession()
        {
            EasyTalkNodeEditor editor = EasyTalkNodeEditor.Instance;

            if (editor != null)
            {
                //Set the current file path.
                string currentFIle = SessionState.GetString("et-current-file", "Assets/TempDialogue.asset");
                editor.FileManager.CurrentFilePath = currentFIle;

                //Update the "unsaved changes" state of the node editor.
                bool hasUnsavedChanged = SessionState.GetBool("et-has-unsaved-changes", false);
                if (hasUnsavedChanged)
                {
                    editor.NodesChanged(true);
                }
                else
                {
                    editor.SetChangesSaved();
                }
            }
        }

        /// <summary>
        /// Loads a Dialogue asset stored for the active editor session, if there is one.
        /// </summary>
        /// <returns>The loaded Dialogue asset which was retrieved from the session.</returns>
        public static Dialogue LoadDialogueForSession()
        {
            Dialogue dialogue = null;
            string json = SessionState.GetString("et-nodes", null);

            if (json != null && json.Length > 0)
            {
                try
                {
                    //Deserialize the Dialogue
                    dialogue = ScriptableObject.CreateInstance<Dialogue>();
                    JsonUtility.FromJsonOverwrite(json, dialogue);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            int currentId = SessionState.GetInt("current-id", 1000);
            NodeUtils.SetCurrentID(currentId);

            return dialogue;
        }

        /// <summary>
        /// Loads the translation library stored for the current editor session, if there is one.
        /// </summary>
        /// <returns>The Translation library which was retrieved from the session.</returns>
        public static TranslationLibrary LoadTranslationLibraryForSession()
        {
            TranslationLibrary translationLib = null;

            try
            {
                string translationJson = SessionState.GetString("et-translations", null);
                if (translationJson != null && translationJson.Length > 0)
                {
                    //Deserialize the translation library.
                    translationLib = ScriptableObject.CreateInstance<TranslationLibrary>();
                    JsonUtility.FromJsonOverwrite(translationJson, translationLib);

                    if (translationLib != null && EasyTalkNodeEditor.Instance != null)
                    {
                        //Set the translation library on the node editor instance.
                        EasyTalkNodeEditor.Instance.TranslationLibrary = translationLib;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("EasyTalk could not load translations from session state: " + e.Message);
            }

            return translationLib;
        }

        /// <summary>
        /// Deletes all of the stored session data for the EasyTalk node editor.
        /// </summary>
        public static void ClearSessionState()
        {
            SessionState.EraseString("et-nodes");
            SessionState.EraseVector3("et-view-pos");
            SessionState.EraseVector3("et-pan-amount");
            SessionState.EraseFloat("et-zoom");
            SessionState.EraseString("et-current-file");
            SessionState.EraseBool("et-has-unsaved-changes");
            SessionState.EraseString("et-translations");
            SessionState.EraseInt("current-id");
        }

        /// <summary>
        /// Returns whether there is stored node data for the current editor session.
        /// </summary>
        /// <returns>True if there is stored node data for the editor session; otherwise false.</returns>
        public static bool HasSavedSessionData()
        {
            string nodesJson = SessionState.GetString("et-nodes", null);
            if (nodesJson != null && nodesJson.Length > 0)
            {
                return true;
            }

            return false;
        }
    }
}